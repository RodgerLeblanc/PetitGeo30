using PetitGeo30.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PetitGeo30.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayPage : ContentPage
    {
        private PlayViewModel _viewModel;

        public PlayPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new PlayViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel?.AppearingCommand?.CanExecute(null) == true)
            {
                _viewModel.AppearingCommand.Execute(null);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (_viewModel?.DisappearingCommand?.CanExecute(null) == true)
            {
                _viewModel.DisappearingCommand.Execute(null);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return base.OnBackButtonPressed();
        }
    }
}