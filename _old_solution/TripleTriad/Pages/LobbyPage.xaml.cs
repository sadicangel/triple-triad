using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using System.Collections;
using TripleTriad.Models;
using TripleTriad.ViewModels;
namespace TripleTriad.Pages;

public sealed partial class LobbyPage : Page
{
    private LobbyViewModel ViewModel { get; }

    public LobbyPage()
    {
        InitializeComponent();

        DataContext = ViewModel = App.GetService<LobbyViewModel>();
        ViewModel.View = this;
    }

    public static IEnumerable GetValues(string typeName)
    {
        return typeName switch
        {
            nameof(MatchRules) => Enum.GetValues<MatchRules>().Skip(1),
            nameof(BoardRules) => Enum.GetValues<BoardRules>().Skip(1),
            nameof(TradeRules) => Enum.GetValues<TradeRules>(),
            _ => throw new InvalidOperationException()
        };
    }

    private void MessageTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !String.IsNullOrWhiteSpace(MessageTextBox.Text))
            ViewModel.SendMessageCommand.Execute(MessageTextBox.Text);
    }
}
