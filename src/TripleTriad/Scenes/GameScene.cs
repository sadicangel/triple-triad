using System.Diagnostics;
using fennecs;
using TripleTriad.Components;
using TripleTriad.Components.Tags;
using TripleTriad.Services;
using TripleTriad.Systems;

namespace TripleTriad.Scenes;

public sealed class GameScene : Scene
{
    private readonly CardDataProvider _cardDataProvider;
    private readonly OrthographicCamera _camera;
    private readonly World _world;

    //private readonly Board _board;
    //private readonly DragDrop _dragDrop;
    //private readonly Flip _flip;
    //private readonly Hover _hover;

    public GameScene(
        CardDataProvider cardDataProvider,
        OrthographicCamera camera)
    {
        _cardDataProvider = cardDataProvider;
        _camera = camera;
        _world = new World();

        _world.Entity()
            .Add<Card>()
            .Add<CardValues>(null!)
            .Add<CardTextures>(null!)
            .Add<CardState>()
            .Add<Transform>(new Transform() with { Scale = Vector2.One })
            .Add<Color>()
            .Spawn(110);

        var i = 0;
        _world
            .Query<CardValues, CardTextures, CardState, Transform>()
            .Has<Card>()
            .Stream()
            .For((ref CardValues values, ref CardTextures textures, ref CardState state, ref Transform transform) =>
        {
            values = _cardDataProvider.GetValues(i + 1);
            textures = _cardDataProvider.GetTextures(i + 1);
            transform.Position = new Vector2(i % 11 * 256, i / 11 * 256);
            state.Color = i % 2 == 0 ? Color.DarkRed : Color.DarkBlue;
            ++i;
        });

        _world.Query<Card>().Stream().For((ref Card card) => Debug.WriteLine(card));

        //_board = new Board(_cardProvider, [.. Random.Shared.GetItems(cards, 5)], [.. Random.Shared.GetItems(cards, 5)]);
        //_dragDrop = new(camera, _board);
        //_flip = new();
        //_hover = new(camera, _board);
    }

    public override void Update(GameTime gameTime)
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

        //_dragDrop.Update(gameTime);
        //_flip.Update(gameTime);
        //_hover.Update(gameTime);

        // TODO: Testing only.
        //if (_camera.Contains(Input.Mouse.Position) is ContainmentType.Contains)
        //{
        //    if (Input.Mouse.WasButtonPressed(MouseButton.Left))
        //    {
        //        if (_board.LeftHand.OrderBy(card => card.LayerDepth).FirstOrDefault(card => card.Border.Contains(Input.Mouse.Position)) is Card card)
        //        {
        //            _flip.Flip360(card);
        //        }
        //    }
        //}
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        //_board.Draw(spriteBatch);
        _world
            .Query<CardTextures, CardState, Transform, Color>()
            .Has<Card>()
            .Stream()
            .For(spriteBatch, Renderer.Render);
    }

    //private sealed class DragDrop(OrthographicCamera camera, Board board)
    //{
    //    private Card? _card;
    //    private Vector2 _originalPosition;
    //    private float _originalDepthLayer;

    //    public void Update(GameTime gameTime)
    //    {
    //        _ = gameTime;

    //        // Dragging
    //        if (_card is not null)
    //        {
    //            if (Input.Mouse.IsButtonDown(MouseButton.Left))
    //            {
    //                _card.Position -= Input.Mouse.DeltaPosition.ToVector2();
    //            }
    //            else if (board.Cells.FirstOrDefault(cell => cell.Card is null && cell.Border.Contains(Input.Mouse.Position)) is Cell cell)
    //            {
    //                cell.Card = _card;
    //                board.RightHand.Remove(_card);
    //                _card.Position = cell.Position;
    //                _card.LayerDepth = _originalDepthLayer;
    //                _card.IsInUse = false;
    //                _card = null;
    //            }
    //            else
    //            {
    //                _card.Position = _originalPosition;
    //                _card.LayerDepth = _originalDepthLayer;
    //                _card.IsInUse = false;
    //                _card = null;
    //            }
    //        }
    //        else if (camera.Contains(Input.Mouse.Position) is ContainmentType.Contains)
    //        {
    //            if (Input.Mouse.IsButtonDown(MouseButton.Left))
    //            {
    //                if (board.RightHand.OrderBy(card => card.LayerDepth).FirstOrDefault(card => card.Border.Contains(Input.Mouse.Position)) is Card card)
    //                {
    //                    if (!card.IsInUse)
    //                    {
    //                        _card = card;
    //                        _originalPosition = _card.Position;
    //                        _originalDepthLayer = _card.LayerDepth;
    //                        _card.IsInUse = true;
    //                        _card.LayerDepth = .1f;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    //private sealed class Flip
    //{
    //    private readonly Tweener _tweener = new();

    //    public bool IsFlipped { get; private set; }

    //    public void Update(GameTime gameTime) => _tweener.Update(gameTime.GetElapsedSeconds());

    //    public void Flip180(Card card)
    //    {
    //        if (!card.IsInUse)
    //        {
    //            var originalDepthLayer = card.LayerDepth;
    //            card.IsInUse = true;
    //            card.LayerDepth = .1f;
    //            _tweener
    //                .TweenTo(card, static target => target.Scale, new Vector2(0f, 1.25f), .15f)
    //                .Easing(EasingFunctions.SineIn)
    //                .OnEnd(_ =>
    //                {
    //                    IsFlipped = !IsFlipped;
    //                    _tweener
    //                        .TweenTo(card, static target => target.Scale, new Vector2(1f, 1f), .15f)
    //                        .Easing(EasingFunctions.SineOut)
    //                        .OnEnd(_ =>
    //                        {
    //                            card.LayerDepth = originalDepthLayer;
    //                            card.IsInUse = false;
    //                        });
    //                });
    //        }
    //    }

    //    public void Flip360(Card card)
    //    {
    //        if (!card.IsInUse)
    //        {
    //            var originalDepthLayer = card.LayerDepth;
    //            card.IsInUse = true;
    //            card.LayerDepth = .1f;
    //            _tweener
    //                .TweenTo(card, static target => target.Scale, new Vector2(0f, 1.25f), .15f)
    //                .Easing(EasingFunctions.SineIn)
    //                .OnEnd(_ =>
    //                {
    //                    IsFlipped = true;
    //                    _tweener
    //                        .TweenTo(card, static target => target.Scale, new Vector2(1f, 1.25f), .15f)
    //                        .Easing(EasingFunctions.SineOut)
    //                        .OnEnd(_ =>
    //                        {
    //                            _tweener
    //                                .TweenTo(card, static target => target.Scale, new Vector2(0f, 1.25f), .15f)
    //                                .Easing(EasingFunctions.SineIn)
    //                                .OnEnd(_ =>
    //                                {
    //                                    IsFlipped = false;
    //                                    _tweener
    //                                        .TweenTo(card, static target => target.Scale, new Vector2(1f, 1), .15f)
    //                                        .Easing(EasingFunctions.SineOut)
    //                                        .OnEnd(_ =>
    //                                        {
    //                                            card.LayerDepth = originalDepthLayer;
    //                                            card.IsInUse = false;
    //                                        });
    //                                });
    //                        });
    //                });
    //        }
    //    }
    //}

    //private sealed class Hover(OrthographicCamera camera, Board board)
    //{
    //    public void Update(GameTime gameTime)
    //    {
    //        _ = gameTime;

    //        if (camera.Contains(Input.Mouse.Position) is ContainmentType.Contains)
    //        {
    //            board.RightHand.ForEach(card => card.IsHighlighted = false);
    //            if (board.RightHand.OrderBy(card => card.LayerDepth).FirstOrDefault(card => card.Border.Contains(Input.Mouse.Position)) is Card card)
    //            {
    //                card.IsHighlighted = true;
    //            }
    //        }
    //    }
    //}
}
