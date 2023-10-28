using Microsoft.Maui.Graphics;

namespace TripleTriad.Controls;
public sealed class OutlinedDrawable : IDrawable
{
    public string? Text { get; set; }
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(OutlinedDrawable));

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if(!String.IsNullOrWhiteSpace(Text))
        {
            canvas.DrawString(Text, 0, 0, HorizontalAlignment.Left);
        }
    }
}
