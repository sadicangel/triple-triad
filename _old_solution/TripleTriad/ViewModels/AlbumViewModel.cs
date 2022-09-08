using Microsoft.UI;
using TripleTriad.Repositories;
using TripleTriad.ViewModels.Explicit;

namespace TripleTriad.ViewModels;

public sealed class AlbumViewModel : BaseViewModel
{
    private readonly ICardRepository _repository;

    public IEnumerable<CardViewModel> Cards
    {
        get => _repository.FindAll().Select(card => new CardViewModel
        {
            Model = card,
            Color = Colors.AliceBlue
        });
    }

    public AlbumViewModel(ICardRepository repository)
    {
        _repository = repository;
    }
}