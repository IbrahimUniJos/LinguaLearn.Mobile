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
            Routing.RegisterRoute("lessonDetails", typeof(LessonDetailsPage));
            Routing.RegisterRoute("lessonPlayer", typeof(LessonPlayerPage));
            Routing.RegisterRoute("lessonComplete", typeof(LessonCompletePage));
            Routing.RegisterRoute("seed", typeof(SeedPage));
        }
    }
}
