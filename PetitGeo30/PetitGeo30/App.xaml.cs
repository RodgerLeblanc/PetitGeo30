
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using PetitGeo30.Views;
using Xamarin.Forms;

namespace PetitGeo30
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            AppCenter.Start(
                "ios=47c99d12-a6ba-4039-82e6-7ce6cf392bd1;" +
                "android=7d21f0dc-fc2d-41c2-9c9f-dabb2693c261;" +
                "uwp=cead7b58-52ac-4778-8ebb-dd7dc7a0c705;",
                typeof(Analytics),
                typeof(Crashes));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
