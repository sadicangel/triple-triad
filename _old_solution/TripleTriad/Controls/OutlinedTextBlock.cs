using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI;

namespace TripleTriad.Controls;

public sealed class OutlinedTextBlock : UserControl
{
    private CanvasControl? _canvas;
    private long _tokenForeground;
    private long _tokenFontSize;
    private long _tokenFontFamily;
    private long _tokenFontStyle;
    private long _tokenFontWeight;
    private long _tokenHorizontalContentAlignment;
    private const string _category = "Common";

    [Category(_category)]
    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public static readonly DependencyProperty TextColorProperty =
        DependencyProperty.Register(nameof(TextColor), typeof(Color), typeof(OutlinedTextBlock), new PropertyMetadata(Colors.White, OnPropertyChanged));

    [Category(_category)]
    public Color OutlineColor { get => (Color)GetValue(OutlineColorProperty); set => SetValue(OutlineColorProperty, value); }

    public static readonly DependencyProperty OutlineColorProperty =
        DependencyProperty.Register(nameof(OutlineColor), typeof(Color), typeof(OutlinedTextBlock), new PropertyMetadata(Colors.Black, OnPropertyChanged));

    [Category(_category)]
    public CanvasWordWrapping TextWrapping { get => (CanvasWordWrapping)GetValue(TextWrappingProperty); set => SetValue(TextWrappingProperty, value); }
    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.Register(nameof(TextWrapping), typeof(CanvasWordWrapping), typeof(OutlinedTextBlock), new PropertyMetadata(CanvasWordWrapping.NoWrap, OnPropertyChanged));

    [Category(_category)]
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(OutlinedTextBlock), new PropertyMetadata(string.Empty, OnPropertyChanged));


    [Category(_category)]
    public double OutlineThickness { get => (double)GetValue(OutlineThicknessProperty); set => SetValue(OutlineThicknessProperty, value); }
    public static readonly DependencyProperty OutlineThicknessProperty =
        DependencyProperty.Register(nameof(OutlineThickness), typeof(double), typeof(OutlinedTextBlock), new PropertyMetadata(4D, OnPropertyChanged));

    private void OnPropertyChanged2(DependencyObject sender, DependencyProperty dp)
    {
        _canvas?.Invalidate();
    }

    public OutlinedTextBlock()
    {
        Loaded += OutlinedTextBlock_Loaded;
        Unloaded += OutlinedTextBlock_Unloaded;
        
    }

    private void OutlinedTextBlock_Loaded(object sender, RoutedEventArgs e)
    {
        _canvas = new CanvasControl();
        _canvas.Draw += OnDraw;
        Content = _canvas;

        _tokenForeground = RegisterPropertyChangedCallback(ForegroundProperty, OnPropertyChanged2);
        _tokenFontSize = RegisterPropertyChangedCallback(FontSizeProperty, OnPropertyChanged2);
        _tokenFontFamily = RegisterPropertyChangedCallback(FontFamilyProperty, OnPropertyChanged2);
        _tokenFontStyle = RegisterPropertyChangedCallback(FontStyleProperty, OnPropertyChanged2);
        _tokenFontWeight = RegisterPropertyChangedCallback(FontWeightProperty, OnPropertyChanged2);
        _tokenHorizontalContentAlignment = RegisterPropertyChangedCallback(HorizontalContentAlignmentProperty, OnPropertyChanged2);
    }

    private void OutlinedTextBlock_Unloaded(object sender, RoutedEventArgs e)
    {
        // Explicitly remove references to allow the Win2D controls to get garbage collected
        if (_canvas != null)
        {
            _canvas.RemoveFromVisualTree();
            _canvas = null;

            UnregisterPropertyChangedCallback(ForegroundProperty, _tokenForeground);
            UnregisterPropertyChangedCallback(FontSizeProperty, _tokenFontSize);
            UnregisterPropertyChangedCallback(FontFamilyProperty, _tokenFontFamily);
            UnregisterPropertyChangedCallback(FontStyleProperty, _tokenFontStyle);
            UnregisterPropertyChangedCallback(FontWeightProperty, _tokenFontWeight);
            UnregisterPropertyChangedCallback(HorizontalContentAlignmentProperty, _tokenHorizontalContentAlignment);
        }
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OutlinedTextBlock instance && instance._canvas != null)
            instance._canvas.Invalidate();
    }


    private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        //Outlined text
        using var textLayout = CreateTextLayout(args.DrawingSession, sender.Size);
        var offset = (float)(OutlineThickness / 2);
        using (var geometry = CanvasGeometry.CreateText(textLayout))
        {
            using var dashedStroke = new CanvasStrokeStyle() { DashStyle = CanvasDashStyle.Solid };
            args.DrawingSession.DrawGeometry(geometry, offset, offset, OutlineColor, (float)OutlineThickness, dashedStroke);
        }
        args.DrawingSession.DrawTextLayout(textLayout, offset, offset, TextColor);
        InvalidateMeasure();
    }


    protected override Size MeasureOverride(Size availableSize)
    {
        // CanvasTextLayout cannot cope with infinite sizes, so we change infinite to some-large-value.
        if (double.IsInfinity(availableSize.Width))
        {
            availableSize.Width = 6000;
        }
        if (double.IsInfinity(availableSize.Height))
        {
            availableSize.Height = 6000;
        }

        var device = CanvasDevice.GetSharedDevice();

        using var layout = CreateTextLayout(device, availableSize);
        var bounds = layout.LayoutBounds;
        var desiredSize = new Size(Math.Min(availableSize.Width, bounds.Width + ExpandAmount),
                                   Math.Min(availableSize.Height, bounds.Height + ExpandAmount));

        if (_canvas is not null)
            _canvas.Measure(desiredSize);

        return desiredSize;


        // https://github.com/Microsoft/Win2D-Samples/blob/master/ExampleGallery/GlowTextCustomControl.cs
    }

    private CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator, Size size)
    {
        var format = new CanvasTextFormat()
        {
            HorizontalAlignment = GetCanvasHorizontalAlignemnt(),
            //VerticalAlignment = GetCanvasVerticalAlignment()
            FontSize = (float)FontSize,
            FontFamily = FontFamily.Source,
            FontStyle = FontStyle,
            FontWeight = FontWeight,
            WordWrapping = TextWrapping,
        };

        return new CanvasTextLayout(resourceCreator, Text, format, (float)size.Width, (float)size.Height);
    }


    private double ExpandAmount => OutlineThickness * 2; // { get { return Math.Max(GlowAmount, MaxGlowAmount) * 4; } }

    private CanvasHorizontalAlignment GetCanvasHorizontalAlignemnt()
    {
        return HorizontalContentAlignment switch
        {
            HorizontalAlignment.Center => CanvasHorizontalAlignment.Center,
            HorizontalAlignment.Left => CanvasHorizontalAlignment.Left,
            HorizontalAlignment.Right => CanvasHorizontalAlignment.Right,
            _ => CanvasHorizontalAlignment.Left,
        };
    }
}