using MonoGame.Extended.Tweening;
using TripleTriad.Components;
using TripleTriad.Objects;
using TripleTriad.Services;

namespace TripleTriad.Scenes;

public sealed class GameScene
    : IScene
{
    private readonly CardProvider _cardProvider;
    private readonly OrthographicCamera _camera;
    private readonly InputListenerComponent _inputListener;
    private readonly Board _board;
    private readonly DragDrop _dragDrop;
    private readonly Flip _flip;
    private readonly Hover _hover;

    public GameScene(
        CardProvider cardProvider,
        OrthographicCamera camera,
        InputListenerComponent inputListener)
    {
        _cardProvider = cardProvider;
        _camera = camera;
        _inputListener = inputListener;
        var cards = Enumerable
            .Range(1, 110)
            .Select((n, i) => cardProvider.CreateCard(n, new Vector2(i % 11 * 256, i / 11 * 256)))
            .ToArray();
        _board = new Board(_cardProvider, [.. Random.Shared.GetItems(cards, 5)], [.. Random.Shared.GetItems(cards, 5)]);
        _dragDrop = new(camera, inputListener, _board);
        _flip = new();
        _hover = new(camera, inputListener, _board);
    }

    public void Update(GameTime gameTime)
    {
        //const float PixelsPerSecond = 500f;
        //var position = camera.Position;
        //if (inputListener.KeyboardState.IsKeyDown(Keys.Right))
        //    position.X += gameTime.GetElapsedSeconds() * PixelsPerSecond;
        //if (inputListener.KeyboardState.IsKeyDown(Keys.Left))
        //    position.X -= gameTime.GetElapsedSeconds() * PixelsPerSecond;
        //if (inputListener.KeyboardState.IsKeyDown(Keys.Down))
        //    position.Y += gameTime.GetElapsedSeconds() * PixelsPerSecond;
        //if (inputListener.KeyboardState.IsKeyDown(Keys.Up))
        //    position.Y -= gameTime.GetElapsedSeconds() * PixelsPerSecond;
        //camera.Position = position;

        _dragDrop.Update(gameTime);
        _flip.Update(gameTime);
        _hover.Update(gameTime);

        // TODO: Testing only.
        if (_camera.Contains(_inputListener.MouseState.Position) is ContainmentType.Contains)
        {
            if (_inputListener.MouseState.WasButtonPressed(MouseButton.Left))
            {
                if (_board.LeftHand.OrderBy(card => card.LayerDepth).FirstOrDefault(card => card.Border.Contains(_inputListener.MouseState.Position)) is Card card)
                {
                    _flip.Flip360(card);
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _board.Draw(spriteBatch);
    }

    private sealed class DragDrop(OrthographicCamera camera, InputListenerComponent inputListener, Board board)
    {
        private Card? _card;
        private Vector2 _originalPosition;
        private float _originalDepthLayer;

        public void Update(GameTime gameTime)
        {
            _ = gameTime;

            // Dragging
            if (_card is not null)
            {
                if (inputListener.MouseState.IsButtonDown(MouseButton.Left))
                {
                    _card.Position -= inputListener.MouseState.DeltaPosition.ToVector2();
                }
                else if (board.Cells.FirstOrDefault(cell => cell.Card is null && cell.Border.Contains(inputListener.MouseState.Position)) is Cell cell)
                {
                    cell.Card = _card;
                    board.RightHand.Remove(_card);
                    _card.Position = cell.Position;
                    _card.LayerDepth = _originalDepthLayer;
                    _card.IsInUse = false;
                    _card = null;
                }
                else
                {
                    _card.Position = _originalPosition;
                    _card.LayerDepth = _originalDepthLayer;
                    _card.IsInUse = false;
                    _card = null;
                }
            }
            else if (camera.Contains(inputListener.MouseState.Position) is ContainmentType.Contains)
            {
                if (inputListener.MouseState.IsButtonDown(MouseButton.Left))
                {
                    if (board.RightHand.OrderBy(card => card.LayerDepth).FirstOrDefault(card => card.Border.Contains(inputListener.MouseState.Position)) is Card card)
                    {
                        if (!card.IsInUse)
                        {
                            _card = card;
                            _originalPosition = _card.Position;
                            _originalDepthLayer = _card.LayerDepth;
                            _card.IsInUse = true;
                            _card.LayerDepth = .1f;
                        }
                    }
                }
            }
        }
    }

    private sealed class Flip
    {
        private readonly Tweener _tweener = new();

        public bool IsFlipped { get; private set; }

        public void Update(GameTime gameTime) => _tweener.Update(gameTime.GetElapsedSeconds());

        public void Flip180(Card card)
        {
            if (!card.IsInUse)
            {
                var originalDepthLayer = card.LayerDepth;
                card.IsInUse = true;
                card.LayerDepth = .1f;
                _tweener
                    .TweenTo(card, static target => target.Scale, new Vector2(0f, 1.25f), .15f)
                    .Easing(EasingFunctions.SineIn)
                    .OnEnd(_ =>
                    {
                        IsFlipped = !IsFlipped;
                        _tweener
                            .TweenTo(card, static target => target.Scale, new Vector2(1f, 1f), .15f)
                            .Easing(EasingFunctions.SineOut)
                            .OnEnd(_ =>
                            {
                                card.LayerDepth = originalDepthLayer;
                                card.IsInUse = false;
                            });
                    });
            }
        }

        public void Flip360(Card card)
        {
            if (!card.IsInUse)
            {
                var originalDepthLayer = card.LayerDepth;
                card.IsInUse = true;
                card.LayerDepth = .1f;
                _tweener
                    .TweenTo(card, static target => target.Scale, new Vector2(0f, 1.25f), .15f)
                    .Easing(EasingFunctions.SineIn)
                    .OnEnd(_ =>
                    {
                        IsFlipped = true;
                        _tweener
                            .TweenTo(card, static target => target.Scale, new Vector2(1f, 1.25f), .15f)
                            .Easing(EasingFunctions.SineOut)
                            .OnEnd(_ =>
                            {
                                _tweener
                                    .TweenTo(card, static target => target.Scale, new Vector2(0f, 1.25f), .15f)
                                    .Easing(EasingFunctions.SineIn)
                                    .OnEnd(_ =>
                                    {
                                        IsFlipped = false;
                                        _tweener
                                            .TweenTo(card, static target => target.Scale, new Vector2(1f, 1), .15f)
                                            .Easing(EasingFunctions.SineOut)
                                            .OnEnd(_ =>
                                            {
                                                card.LayerDepth = originalDepthLayer;
                                                card.IsInUse = false;
                                            });
                                    });
                            });
                    });
            }
        }
    }

    private sealed class Hover(OrthographicCamera camera, InputListenerComponent inputListener, Board board)
    {
        public void Update(GameTime gameTime)
        {
            _ = gameTime;

            if (camera.Contains(inputListener.MouseState.Position) is ContainmentType.Contains)
            {
                board.RightHand.ForEach(card => card.IsHighlighted = false);
                if (board.RightHand.OrderBy(card => card.LayerDepth).FirstOrDefault(card => card.Border.Contains(inputListener.MouseState.Position)) is Card card)
                {
                    card.IsHighlighted = true;
                }
            }
        }
    }
}
