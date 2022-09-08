using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace TripleTriad.ViewModels.Explicit;

public abstract class BaseViewModel<TModel, TView> : BaseViewModel<TModel>
    where TModel : new()
    where TView : UserControl
{
    private TView? _view;

    public TView View { get => _view ?? throw new InvalidOperationException("View cannot be null"); set => SetPropertyNotNull(ref _view, value, OnViewChanged); }

    protected virtual void OnViewChanged(TView? view) { }

    protected void RunOnUIThread(DispatcherQueueHandler callback)
    {
        if (!View.DispatcherQueue.TryEnqueue(callback))
            throw new InvalidOperationException(nameof(RunOnUIThread));
    }
}
