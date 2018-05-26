using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace PetitGeo30.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "À propos";

            OpenWebCommand = new Command(() => Device.OpenUri(new Uri("https://www.geocaching.com/play")));
        }

        public ICommand OpenWebCommand { get; }
    }
}