using TripleTriad.Repositories;
using TripleTriad.ViewModels.Explicit;

namespace TripleTriad.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private readonly ICardRepository _repository;
    public BoardViewModel Board { get; } = new();

    public MainViewModel(ICardRepository repository)
    {
        _repository = repository;
        //for(int i = 0; i < 9; ++i)
        //{
        //    var random = Random.Shared.Next(110);
        //    var id = $"CardE01T{random / 11 + 1:D2}N{random + 1:D3}V1";
        //    var card = _repository.FindById(id);
        //    Board.Cells.Add(new CellViewModel
        //    {
        //        Card = new CardViewModel
        //        {
        //            Model = card!,
        //            Color = Random.Shared.Next(2) == 0 ? Player1.Color : Player2.Color,
        //        }
        //    });
        //}
        for (int i = 0; i < 5; ++i)
        {
            var random = Random.Shared.Next(110);
            var id = $"CardE01T{random / 11 + 1:D2}N{random + 1:D3}V1";
            var card = _repository.FindById(id);
            Board.LeftHand.Add(new CardViewModel
            {
                Model = card!,
                Color = Board.LeftPlayer.Color,
                HandIndex = i
            });
        }
        for (int i = 0; i < 5; ++i)
        {
            var random = Random.Shared.Next(110);
            var id = $"CardE01T{random / 11 + 1:D2}N{random + 1:D3}V1";
            var card = _repository.FindById(id);
            Board.RightHand.Add(new CardViewModel
            {
                Model = card!,
                Color = Board.RightPlayer.Color,
                HandIndex = i
            });
        }
        Board.IsLeftActive = Random.Shared.Next(2) == 0;
    }
}