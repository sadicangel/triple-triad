namespace TripleTriad.Bridge;

public abstract record GameCommand(string ClientRequestId);

public sealed record PlayCardCommand(
	string CardInstanceId,
	int BoardSlotIndex,
	string ClientRequestId) : GameCommand(ClientRequestId);

public static class GameCommandFactory
{
	public static PlayCardCommand CreatePlayCardCommand(
		CardSnapshot card,
		BoardCellSnapshot boardCell,
		string? clientRequestId = null)
	{
		if (!card.IsPlayable)
			throw new InvalidOperationException("Only playable cards can create play-card commands.");

		if (!boardCell.CanDrop || boardCell.Card is not null)
			throw new InvalidOperationException("The selected board slot cannot accept a card.");

		return new PlayCardCommand(
			card.CardInstanceId,
			boardCell.Index,
			clientRequestId ?? Guid.NewGuid().ToString("N"));
	}
}
