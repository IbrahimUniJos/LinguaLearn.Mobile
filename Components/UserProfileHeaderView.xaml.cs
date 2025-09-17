using LinguaLearn.Mobile.Models;

namespace LinguaLearn.Mobile.Components;

public partial class UserProfileHeaderView : ContentView
{
    public static readonly BindableProperty UserProfileProperty =
        BindableProperty.Create(nameof(UserProfile), typeof(UserProfile), typeof(UserProfileHeaderView), null);

    public UserProfile UserProfile
    {
        get => (UserProfile)GetValue(UserProfileProperty);
        set => SetValue(UserProfileProperty, value);
    }

    public UserProfileHeaderView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is UserProfile profile)
        {
            UserProfile = profile;
        }
    }
}