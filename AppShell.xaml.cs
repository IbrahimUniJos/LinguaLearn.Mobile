namespace LinguaLearn.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("/main", typeof(MainPage));

        }
    }
}
