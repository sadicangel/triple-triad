using MonoGame.Extended.Tweening;
using TripleTriad.Core;

namespace TripleTriad.Objects;

public sealed class Card
{
    private readonly CardData _data;
    private readonly Texture2DAtlas _atlas;

    private readonly Texture2DRegion _card;
    private readonly Texture2DRegion _fill;
    private readonly Texture2DRegion _back;

    private readonly Texture2DRegion _w;
    private readonly Texture2DRegion _n;
    private readonly Texture2DRegion _e;
    private readonly Texture2DRegion _s;

    private readonly Texture2DRegion _t;

    private readonly FlipAnimation _flipAnimation;

    public Card(CardData data, Texture2DAtlas atlas)
    {
        _data = data;
        _atlas = atlas;

        _card = _atlas.GetRegion($"card_{_data.Number}");
        _back = _atlas.GetRegion("fill_0");
        _fill = _atlas.GetRegion("fill_1");

        _w = _atlas.GetRegion($"val_W_{_data.W:X1}");
        _n = _atlas.GetRegion($"val_N_{_data.N:X1}");
        _e = _atlas.GetRegion($"val_E_{_data.E:X1}");
        _s = _atlas.GetRegion($"val_S_{_data.S:X1}");
        _t = _atlas.GetRegion($"elem_{_data.Element}");

        // TODO: Probably assign this through an enum instead.
        Color = (_data.Number % 2 == 0 ? Color.DarkRed : Color.DarkBlue) with { A = 64 };

        _flipAnimation = new FlipAnimation(this);
    }

    public Vector2 Position { get; set; }

    public Vector2 Scale { get; set; } = Vector2.One;

    public Color Color { get; set; }

    public Vector2 Origin => new(_fill.Size.Width * .5f, _fill.Size.Height * .5f);

    public float LayerDepth { get; set; } = 0f;

    public Rectangle Border => _fill.Bounds with { Location = Position.ToPoint() };

    public void Flip()
    {
        _flipAnimation.Animate();
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _flipAnimation.Update(gameTime);

        var position = Position + Origin;

        if (_flipAnimation.IsFlipped is false)
        {
            spriteBatch.Draw(_fill, position, Color, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_card, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_w, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_n, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_e, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_s, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_t, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
        }
        else
        {
            spriteBatch.Draw(_back, position, Color.White, 0f, Origin, Scale, SpriteEffects.None, 0f);
        }
    }

    private sealed class FlipAnimation(Card card)
    {
        private readonly Tweener _tweener = new();
        private bool _isAnimating;

        public bool IsFlipped { get; private set; }

        public void Update(GameTime gameTime)
        {
            _tweener.Update(gameTime.GetElapsedSeconds());
        }

        public void Animate()
        {
            if (!_isAnimating)
            {
                _isAnimating = true;
                _tweener
                    .TweenTo(card, static card => card.Scale, new Vector2(0f, 1.25f), .15f)
                    .Easing(EasingFunctions.SineIn)
                    .OnEnd(_ =>
                    {
                        IsFlipped = true;
                        _tweener
                            .TweenTo(card, static card => card.Scale, new Vector2(1f, 1.25f), .15f)
                            .Easing(EasingFunctions.SineOut)
                            .OnEnd(_ =>
                            {
                                _tweener
                                    .TweenTo(card, static card => card.Scale, new Vector2(0f, 1.25f), .15f)
                                    .Easing(EasingFunctions.SineIn)
                                    .OnEnd(_ =>
                                    {
                                        IsFlipped = false;
                                        _tweener
                                            .TweenTo(card, static card => card.Scale, new Vector2(1f, 1), .15f)
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
}
