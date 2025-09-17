using System.Windows.Input;

namespace LinguaLearn.Mobile.Components;

public partial class QuickActionCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(QuickActionCard), string.Empty, propertyChanged: OnTitleChanged);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(QuickActionCard), string.Empty, propertyChanged: OnIconChanged);

    public static readonly BindableProperty CardBackgroundColorProperty =
        BindableProperty.Create(nameof(CardBackgroundColor), typeof(Color), typeof(QuickActionCard), Colors.Transparent, propertyChanged: OnBackgroundColorChanged);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(QuickActionCard), Colors.Black, propertyChanged: OnTextColorChanged);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(QuickActionCard), null, propertyChanged: OnCommandChanged);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Color CardBackgroundColor
    {
        get => (Color)GetValue(CardBackgroundColorProperty);
        set => SetValue(CardBackgroundColorProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public QuickActionCard()
    {
        InitializeComponent();
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is QuickActionCard card)
        {
            card.TitleLabel.Text = newValue?.ToString() ?? string.Empty;
        }
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is QuickActionCard card)
        {
            card.IconLabel.Text = newValue?.ToString() ?? string.Empty;
        }
    }

    private static void OnBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is QuickActionCard card && newValue is Color color)
        {
            card.ActionCard.BackgroundColor = color;
        }
    }

    private static void OnTextColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is QuickActionCard card && newValue is Color color)
        {
            card.TitleLabel.TextColor = color;
        }
    }

    private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is QuickActionCard card)
        {
            card.TapGesture.Command = newValue as ICommand;
        }
    }
}