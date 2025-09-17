using LinguaLearn.Mobile.Views;
using LinguaLearn.Mobile.Views.Onboarding;

namespace LinguaLearn.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("/main", typeof(MainPage));
            Routing.RegisterRoute("/onboarding", typeof(OnboardingPage));
            Routing.RegisterRoute("/home", typeof(UserHomepagePage));

        }
    }
}
