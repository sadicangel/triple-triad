using MonoGame.Extended.Tweening;

namespace TripleTriad.Animations;

public sealed class FlipAnimation<T>(T target) where T : class, IAnimationTarget
{
    private readonly Tweener _tweener = new();
    private bool _isAnimating;

    public bool IsFlipped { get; private set; }

    public void Update(GameTime gameTime) => _tweener.Update(gameTime.GetElapsedSeconds());

    public void Flip180()
    {
        if (!_isAnimating)
        {
            _isAnimating = true;
            _tweener
                .TweenTo(target, static target => target.Scale, new Vector2(0f, 1.25f), .15f)
                .Easing(EasingFunctions.SineIn)
                .OnEnd(_ =>
                {
                    IsFlipped = !IsFlipped;
                    _tweener
                        .TweenTo(target, static target => target.Scale, new Vector2(1f, 1f), .15f)
                        .Easing(EasingFunctions.SineOut)
                        .OnEnd(_ =>
                        {
                            _isAnimating = false;
                        });
                });
        }
    }

    public void Flip360()
    {
        if (!_isAnimating)
        {
            _isAnimating = true;
            _tweener
                .TweenTo(target, static target => target.Scale, new Vector2(0f, 1.25f), .15f)
                .Easing(EasingFunctions.SineIn)
                .OnEnd(_ =>
                {
                    IsFlipped = true;
                    _tweener
                        .TweenTo(target, static target => target.Scale, new Vector2(1f, 1.25f), .15f)
                        .Easing(EasingFunctions.SineOut)
                        .OnEnd(_ =>
                        {
                            _tweener
                                .TweenTo(target, static target => target.Scale, new Vector2(0f, 1.25f), .15f)
                                .Easing(EasingFunctions.SineIn)
                                .OnEnd(_ =>
                                {
                                    IsFlipped = false;
                                    _tweener
                                        .TweenTo(target, static target => target.Scale, new Vector2(1f, 1), .15f)
                                        .Easing(EasingFunctions.SineOut)
                                        .OnEnd(_ =>
                                        {
                                            _isAnimating = false;
                                        });
                                });
                        });
                });
        }
    }
}
