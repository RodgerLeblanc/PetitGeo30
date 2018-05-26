using Newtonsoft.Json;
using PetitGeo30.Helpers;
using PetitGeo30.Models;
using PetitGeo30.Services.Cloudant;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PetitGeo30.ViewModels
{
    public class PlayViewModel : BaseViewModel
    {
        private DateTime _lastUserHeading = DateTime.MinValue;
        private Xamarin.Essentials.SensorSpeed _speed;

        public PlayViewModel()
        {
            _speed = Xamarin.Essentials.SensorSpeed.Ui;

            Title = "Jouer";

            AppearingCommand = new Command(async () => await OnAppearing());
            DisappearingCommand = new Command(async () => await OnDisappearing());
            UpdateGeoCacheCommand = new Command(async () => await UpdateGeoCache());

            GeoCachePosition = new Position();
            UserPosition = new Position();

            PropertyChanged += PlayViewModel_PropertyChanged;
        }

        public Command AppearingCommand { get; set; }

        public Command DisappearingCommand { get; set; }

        public Command UpdateGeoCacheCommand { get; set; }

        public Position GeoCachePosition { get; set; }

        public Position UserPosition { get; set; }

        public double UserHeading { get; set; }

        public double DegreeBearing { get; set; }

        public double CacheDirection { get; set; }

        public double Distance { get; set; }

        public int CompassInterval { get; set; } = 1000;

        private async Task OnAppearing()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            SettingsModel settings = GetSettings();
            GeoCacheModel lastGeoCache = settings.GeoCache;

            if (!IsLastGeoCacheValid(lastGeoCache))
            {
                lastGeoCache = await UpdateLastGeoCache(lastGeoCache, settings);
            }

            if (lastGeoCache == null)
            {
                lastGeoCache = new GeoCacheModel();
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                GeoCachePosition = new Position(lastGeoCache.GeoCacheLatitude, lastGeoCache.GeoCacheLongitude);
            });

            await StartListeningGps();
            StartListeningCompass();

            //SleepMode.ActivateAutoSleepMode(false);

            IsBusy = false;
        }

        private async Task OnDisappearing()
        {
            //SleepMode.ActivateAutoSleepMode(true);

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                await StopListeningGps();
                StopListeningCompass();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateGeoCache()
        {
            var lastGeoCache = await UpdateLastGeoCache(null, GetSettings());
            if (lastGeoCache == null)
            {
                lastGeoCache = new GeoCacheModel();
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                GeoCachePosition = new Position(lastGeoCache.GeoCacheLatitude, lastGeoCache.GeoCacheLongitude);
            });
        }

        public SettingsModel GetSettings()
        {
            try
            {
                return JsonConvert.DeserializeObject<SettingsModel>(Settings.GeneralSettings) ?? new SettingsModel();
            }
            catch (Exception)
            {
                return new SettingsModel();
            }
        }

        private bool IsLastGeoCacheValid(GeoCacheModel lastGeoCache)
        {
            if (lastGeoCache == null || lastGeoCache.GeoCacheLatitude == 0 || lastGeoCache.GeoCacheLongitude == 0)
                return false;

            DateTimeOffset geoCacheHiddenTimestamp =
                DateTimeOffset.FromUnixTimeSeconds(lastGeoCache.GeoCacheHiddenTimestamp);

            return DateTimeOffset.Now - geoCacheHiddenTimestamp < TimeSpan.FromDays(6);
        }

        private async Task<GeoCacheModel> UpdateLastGeoCache(GeoCacheModel lastGeoCache, SettingsModel settings)
        {
            try
            {
                lastGeoCache = await (new DocumentService()).GetDocument();
                lastGeoCache.GeoCacheHiddenTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                settings.GeoCache = lastGeoCache;

                Settings.GeneralSettings = JsonConvert.SerializeObject(settings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                lastGeoCache = new GeoCacheModel();
            }

            return lastGeoCache;
        }

        private async Task StartListeningGps()
        {
            if (CrossGeolocator.Current.IsListening)
                return;

            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    {
                        await Application.Current.MainPage.DisplayAlert("Need location", "Gunna need that location", "OK");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Location))
                        status = results[Permission.Location];
                }

                if (status == PermissionStatus.Granted)
                {
                    CrossGeolocator.Current.PositionChanged += PositionChanged;
                    CrossGeolocator.Current.PositionError += PositionError;
                    await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 1, true);
                }
                else if (status != PermissionStatus.Unknown)
                {
                    await Application.Current.MainPage.DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Location exception", ex.Message, "OK");
            }
        }

        private async Task StopListeningGps()
        {
            if (!CrossGeolocator.Current.IsListening)
                return;

            CrossGeolocator.Current.PositionChanged -= PositionChanged;
            CrossGeolocator.Current.PositionError -= PositionError;
            await CrossGeolocator.Current.StopListeningAsync();
        }

        private void StartListeningCompass()
        {
            try
            {
                if (Compass.IsMonitoring)
                    Compass.Stop();
                else
                    Compass.Start(_speed);
            }
            catch (FeatureNotSupportedException)
            {
                // Feature not supported on device
            }
            catch (Exception)
            {
                // Some other exception has occured
            }


            //if (!CrossCompass.Current.IsSupported)
            //    return;

            //CrossCompass.Current.CompassChanged += Current_CompassChanged;
            //CrossCompass.Current.Start();
        }

        private void StopListeningCompass()
        {
            //if (!CrossCompass.Current.IsSupported)
            //    return;

            //CrossCompass.Current.CompassChanged -= Current_CompassChanged;
            //CrossCompass.Current.Stop();
        }

        private void PositionChanged(object sender, PositionEventArgs e)
        {
            var position = e.Position;

            Device.BeginInvokeOnMainThread(() =>
            {
                UserPosition = position;

                if (UserPosition != null && GeoCachePosition != null)
                {
                    DegreeBearing = BearingCalculator.DegreeBearing(
                        UserPosition.Latitude,
                        UserPosition.Longitude,
                        GeoCachePosition.Latitude,
                        GeoCachePosition.Longitude);

                    Distance = DistanceCalculator.DistanceTo(
                        UserPosition.Latitude,
                        UserPosition.Longitude,
                        GeoCachePosition.Latitude,
                        GeoCachePosition.Longitude);
                }
            });
        }

        private void PositionError(object sender, PositionErrorEventArgs e)
        {
            Debug.WriteLine(e.Error);
            //Handle event here for errors
        }

        //private void Current_CompassChanged(object sender, CompassChangedEventArgs e)
        //{
        //    //if (DateTime.Now - _lastUserHeading < TimeSpan.FromMilliseconds(CompassInterval))
        //    //    return;

        //    //_lastUserHeading = DateTime.Now;

        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        UserHeading = e.Heading;
        //        CacheDirection = DegreeBearing - UserHeading;
        //    });
        //}

        private void PlayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GeoCachePosition) || e.PropertyName == nameof(UserPosition))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                });
            }
        }
    }
}