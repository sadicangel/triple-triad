using System.Collections.Concurrent;
using Godot;
using TripleTriad.Contracts;
using TripleTriad.Data;
using TripleTriad.Sessions;

namespace TripleTriad.Bridge;

public partial class GameSessionBridge : Node
{
	private readonly ConcurrentQueue<GameEvent> _pendingEvents = new();
	private readonly ConcurrentQueue<ConnectionStateChange> _pendingConnectionStates = new();
	private readonly CancellationTokenSource _sessionLifetime = new();
	private bool _isExiting;
	private long _nextConnectionSequence;
	private IGameSession _session = null!;

	[Signal] public delegate void game_event_raisedEventHandler(GameEventResource gameEvent);

	[Signal] public delegate void connection_state_changedEventHandler(ConnectionStateResource connectionState);

	[Export] public bool RevealOpponentHand { get; set; } = true;

	[Export] public bool AutoPlayOpponent { get; set; } = true;

	public MatchSnapshotResource? CurrentSnapshot { get; private set; }

	public override async void _Ready()
	{
		var catalog = CardCatalog.Load(ProjectSettings.GlobalizePath("res://assets/triple_triad/cards.json"));
		var flow = GetNodeOrNull<GameFlowBridge>("/root/GameFlowBridge");
		_session = flow?.GetActiveGameSession()
			?? CreateFallbackSession(catalog);
		_ = PumpSessionEventsAsync(_sessionLifetime.Token);

		try
		{
			EnqueueConnectionState(SessionConnectionState.Connecting);
			var snapshot = await _session.StartAsync(_sessionLifetime.Token);
			CurrentSnapshot ??= MatchSnapshotResource.FromModel(snapshot);
			EnqueueConnectionState(_session.ConnectionState);
		}
		catch (OperationCanceledException) when (_sessionLifetime.IsCancellationRequested) { }
		catch (Exception ex)
		{
			EnqueueConnectionState(SessionConnectionState.Failed, ex.Message);
		}
	}

	public override void _ExitTree()
	{
		_isExiting = true;
		_sessionLifetime.Cancel();

		_sessionLifetime.Dispose();
	}

	public MatchSnapshotResource? get_current_snapshot() => CurrentSnapshot;

	public void submit_play_card(string cardInstanceId, int boardSlotIndex, string clientRequestId)
	{
		if (_session is null)
			return;

		_ = SendPlayCardAsync(cardInstanceId, boardSlotIndex, clientRequestId);
	}

	private async Task SendPlayCardAsync(string cardInstanceId, int boardSlotIndex, string clientRequestId)
	{
		try
		{
			await _session.SendCommandAsync(
				new PlayCardCommand(
					cardInstanceId,
					boardSlotIndex,
					clientRequestId),
				_sessionLifetime.Token);
		}
		catch (OperationCanceledException) when (_sessionLifetime.IsCancellationRequested) { }
		catch (Exception ex)
		{
			EnqueueConnectionState(SessionConnectionState.Failed, ex.Message);
		}
	}

	private async Task PumpSessionEventsAsync(CancellationToken cancellationToken)
	{
		try
		{
			await foreach (var gameEvent in _session.SubscribeEventsAsync(cancellationToken))
				EnqueueSessionEvent(gameEvent);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
		catch (Exception ex)
		{
			EnqueueConnectionState(SessionConnectionState.Failed, ex.Message);
		}
	}

	private void EnqueueSessionEvent(GameEvent gameEvent)
	{
		if (_isExiting)
			return;

		_pendingEvents.Enqueue(gameEvent);
		CallDeferred(nameof(DrainSessionEvents));
	}

	public void DrainSessionEvents()
	{
		while (_pendingEvents.TryDequeue(out var gameEvent))
			ApplySessionEvent(gameEvent);
	}

	private void ApplySessionEvent(GameEvent gameEvent)
	{
		var resource = GameEventResource.FromModel(gameEvent);
		CurrentSnapshot = resource.Snapshot ?? MatchSnapshotResource.FromModel(gameEvent.Snapshot);
		resource.Snapshot ??= CurrentSnapshot;
		EmitSignal(SignalName.game_event_raised, resource);
	}

	private void EnqueueConnectionState(SessionConnectionState state, string? reason = null)
	{
		if (_isExiting)
			return;

		_pendingConnectionStates.Enqueue(new ConnectionStateChange(state, reason));
		CallDeferred(nameof(DrainConnectionStates));
	}

	public void DrainConnectionStates()
	{
		while (_pendingConnectionStates.TryDequeue(out var change))
			EmitConnectionState(change.State, change.Reason);
	}

	private void EmitConnectionState(SessionConnectionState state, string? reason)
	{
		var resource = new ConnectionStateResource
		{
			State = state.ToString(),
			Reason = reason ?? string.Empty,
			Sequence = ++_nextConnectionSequence,
		};

		EmitSignal(SignalName.connection_state_changed, resource);
	}

	private IGameSession CreateFallbackSession(CardCatalog catalog)
	{
		var session = new LocalGameSession(catalog, Seat.Blue, RevealOpponentHand);
		if (AutoPlayOpponent)
			_ = RunSeatControllerAsync(new AiSeatController(Seat.Red), session, _sessionLifetime.Token);

		return session;
	}

	private async Task RunSeatControllerAsync(
		ISeatController controller,
		IGameSession session,
		CancellationToken cancellationToken)
	{
		try
		{
			await controller.RunAsync(session, cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
		catch (Exception ex)
		{
			EnqueueConnectionState(SessionConnectionState.Failed, ex.Message);
		}
	}

	private readonly record struct ConnectionStateChange(SessionConnectionState State, string? Reason);
}
