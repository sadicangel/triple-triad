//using CommunityToolkit.Mvvm.Messaging;
//using System.Diagnostics;
//using TripleTriad.Domain.Enums;
//using TripleTriad.Domain.Events;
//using TripleTriad.Domain.Interfaces;

//namespace TripleTriad.Domain.Aggregates;

//public sealed partial class ClientBoard : Board, ITripleTriadListener
//{
//    public ClientBoard(IMessenger messenger, Side side) : base(messenger)
//    {
//        IsActive = true;
//        Side = side;
//    }

//    public Side Side { get; }

//    public void Receive(GameStartedEvent message)
//    {
//        Rules = message.Data.Rules;
//        LeftPlayer = message.Data.LeftPlayer;
//        RightPlayer = message.Data.RightPlayer;
//        LeftHand = new(message.Data.LeftHand);
//        RightHand = new(message.Data.RightHand);
//        Cells = message.Data.Cells;
//        ActiveSide = message.Data.ActiveSide;
//        Debug.WriteLine($"{Side}: Game started. Starting side: {ActiveSide}");
//    }

//    public void Receive(CardPlayedEvent message)
//    {
//        var cell = Cells[message.Data.Move.CellIndex];
//        var card = ActiveHand[message.Data.Move.HandIndex];
//        ActiveHand.RemoveAt(message.Data.Move.HandIndex);
//        cell.Owner = ActivePlayer;
//        cell.Card = card;
//        Debug.WriteLine($"{Side}: {ActiveSide}'s card at {message.Data.Move.HandIndex} played on cell {message.Data.Move.CellIndex}");
//    }

//    public void Receive(CardsFlippedEvent message)
//    {
//        foreach (var flipped in message.Data.CardsFlipped)
//        {
//            if (flipped.Rules != Enums.BoardRules.None)
//                Debug.WriteLine($"{Side}: Card at {flipped.CellIndex} flipped. Rules '{flipped.Rules}' activated");
//            else
//                Debug.WriteLine($"{Side}: Card at {flipped.CellIndex} flipped");
//            Cells[flipped.CellIndex].Owner = ActivePlayer;
//        }
//    }

//    public void Receive(GameOverEvent message)
//    {
//        IsGameOver = true;
//        Winner = message.Data.Winner;
//        Debug.WriteLine($"{Side}: GameOver. Winner: {Winner.Side}");
//    }

//    public void Receive(ActiveSideChangedEvent message)
//    {
//        var old = ActiveSide;
//        ActiveSide = message.Data.ActiveSide;
//        Debug.WriteLine($"{Side}: Side changed: {old} => {ActiveSide}");
//    }
//}
