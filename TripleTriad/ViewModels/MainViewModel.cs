using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using TripleTriad.Models;
using TripleTriad.Pages;
using TripleTriad.Repositories;
using TripleTriad.Services;
using TripleTriad.ViewModels.Explicit;

namespace TripleTriad.ViewModels;

public sealed class MainViewModel : BaseViewModel<object, MainPage>
{
    private readonly INavigationService _navigation;
    private CancellationTokenSource? _cancellationSource;
    private ContentDialog? _contentDialog;

    private CancellationTokenSource CancellationSource
    {
        get
        {
            if (_cancellationSource is not null)
            {
                using (_cancellationSource)
                    _cancellationSource.Cancel();
            }
            _cancellationSource = new CancellationTokenSource();
            return _cancellationSource;
        }
    }

    public IAsyncRelayCommand<string> HostCommand { get; }

    public IAsyncRelayCommand<string> JoinCommand { get; }

    public PlayerViewModel Player { get; }

    [NotNull] public ITripleTriadUser? User { get; private set; }

    public MainViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        HostCommand = new AsyncRelayCommand<string>(HostAsync);
        JoinCommand = new AsyncRelayCommand<string>(JoinAsync);
        Player = new PlayerViewModel
        {
            Model = new Player { Name = "FUCKER" }
        };
    } 

    public async Task HostAsync(string? port)
    {
        if (!Int32.TryParse(port, out var portValue))
            throw new InvalidOperationException("Invalid port");
        Player.Color = Colors.DarkGreen;
        Player.IsLeft = true;
        var server = ITripleTriadServer.Create(Player.Model, portValue);
        User = server;
        User.PlayerConnected += Server_PlayerConnected;
        _contentDialog = new ContentDialog
        {
            XamlRoot = View.XamlRoot,
            Title = "Hosting..",
            Content = "Waiting for connection..",
            CloseButtonText = "Cancel",
        };
        _contentDialog.ShowAsync().GetAwaiter();
        await server.HostAsync(CancellationSource.Token);
    }

    private void Server_PlayerConnected(object? sender, Player e)
    {
        RunOnUIThread(() =>
        {
            _contentDialog?.Hide();
            _navigation.NavigateTo<LobbyViewModel>();
        });
    }

    public async Task JoinAsync(string? address)
    {
        if (String.IsNullOrEmpty(address))
            throw new InvalidOperationException("Invalid address");
        Player.Color = Colors.DarkRed;
        Player.IsLeft = false;
        var client = ITripleTriadClient.Create(Player.Model, address);
        User = client;
        using var tokenSource = new CancellationTokenSource();
        _contentDialog = new ContentDialog
        {
            XamlRoot = View.XamlRoot,
            Title = "Joining..",
            Content = "Connecting to host..",
            CloseButtonText = "Cancel",
        };
        _contentDialog.ShowAsync().GetAwaiter();
        await client.JoinAsync(tokenSource.Token);
        _contentDialog.Hide();
        _navigation.NavigateTo<LobbyViewModel>();
    }
}