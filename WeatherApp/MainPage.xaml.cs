using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WeatherApp.Resources;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace WeatherApp {
    public partial class MainPage : PhoneApplicationPage {
        // Constructor
        public MainPage() {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent")) {
                // User has opted in or out of Location
                GetLocation();
                return;
            }
            else {
                MessageBoxResult result =
                    MessageBox.Show("This app accesses your phone's location. Is that ok?",
                    "Location",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK) {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                    GetLocation();
                }
                else {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        private async void GetLocation() {
            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true) {
                // The user has opted out of Location.
                //display error message
                return;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                string lat  = geoposition.Coordinate.Latitude.ToString("0.000000");
                string longitude = geoposition.Coordinate.Longitude.ToString("0.000000");
                GetWeather(lat, longitude);
            }
            catch (Exception ex) {
                if ((uint)ex.HResult == 0x80004004) {
                    // the application does not have the right capability or the location master switch is off
                   // StatusTextBlock.Text = "location  is disabled in phone settings.";
                }
                //else
                {
                    // something else happened acquring the location
                }
            }
        }

        private async void GetWeather(string lat, string lo) {
            string url = "http://api.wunderground.com/api/c32ba2c7772e748b/conditions/q/" + lat + "," + lo + ".json";

            Uri uri = new Uri(url);
            HttpClient client = new HttpClient();
            string data = await client.GetStringAsync(uri);

            Match match = Regex.Match(data, @"""temp_f"":([\d\.]+)", RegexOptions.IgnoreCase);
            if (match.Success) {
                string key = match.Groups[1].Value;
                TemperatureBlock.Text = key + "° F";
            }
            else {
                return;
            }

            match = Regex.Match(data, @"""weather"":""([^""]+)", RegexOptions.IgnoreCase);
            if (match.Success) {
                string key = match.Groups[1].Value;
                WeatherBlock.Text = key;
            }
            else {
                return;
            }

            match = Regex.Match(data, @"""full"":""([^""]+)", RegexOptions.IgnoreCase);
            if (match.Success) {
                string key = match.Groups[1].Value;
                CityBlock.Text = key;
            }
            else {
                return;
            }


        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem)
        private void OneShotLocation_Click(object sender, RoutedEventArgs e) {
            this.GetLocation();
        }
    }


}