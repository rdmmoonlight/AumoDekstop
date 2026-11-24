
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop.Shell.Ribbon;
public sealed partial class RibbonButton : UserControl
{
    public static readonly DependencyProperty GlyphProperty =
        DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(RibbonButton), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(RibbonButton), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CommandIdProperty =
        DependencyProperty.Register(nameof(CommandId), typeof(string), typeof(RibbonButton), new PropertyMetadata(string.Empty));

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string CommandId
    {
        get => (string)GetValue(CommandIdProperty);
        set => SetValue(CommandIdProperty, value);
    }

    public event RoutedEventHandler? Click;

    public RibbonButton()
    {
        InitializeComponent();
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        Click?.Invoke(this, e);
    }
}
