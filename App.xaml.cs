using HorusStudio.Maui.MaterialDesignControls;
using LinguaLearn.Mobile.Extensions;
using Microsoft.Extensions.Configuration;

namespace LinguaLearn.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MaterialDesignControls.InitializeComponents();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
           return new Window(new AppShell());
        }

       
    }
}