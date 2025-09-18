using LinguaLearn.Mobile.ViewModels;

namespace LinguaLearn.Mobile.Views;

public partial class SeedPage : ContentPage
{
    public SeedPage(SeedViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
