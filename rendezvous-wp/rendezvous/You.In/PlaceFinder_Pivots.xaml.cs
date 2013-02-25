using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Paco.Application;
using System.Device.Location;

namespace You.In
{
    public partial class PlaceFinder_Pivots : PhoneApplicationPage
    {
        GeoCoordinateWatcher watcher;
        String latitude = String.Empty;
        String longitude = String.Empty;
        GeoLocationSearch _search;




        public PlaceFinder_Pivots()
        {
            InitializeComponent();
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // Use high accuracy.
                watcher.MovementThreshold = 20; // Use MovementThreshold to ignore noise in the signal.
                watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            }
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            _search = new GeoLocationSearch("toronto");
            _search.GeoLocationFound += new GeoLocationSearch.GeoLocationHandler(_search_GeoLocationFound);
            _search.GetGeoCoordinate();
        }

        void _search_GeoLocationFound(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
 {
     textBlock3.Text = _search.City.Latitude + "   " + _search.City.Longitude;
 });
        }

        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                // Use the Position property of the GeoCoordinateWatcher object to get the current location.
                GeoCoordinate co = watcher.Position.Location;
                latitude = co.Latitude.ToString("0.000");
                longitude = co.Longitude.ToString("0.000");
              //  p.GetPlaces(String.Empty, latitude, longitude, AppSettings.TokenSetting);
                watcher.Stop();
            }
            else if (e.Status != GeoPositionStatus.Initializing)
            {
                MessageBox.Show("Sorry we could not detect your current location.");
                NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
            }
        }

        private void image1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _search = new GeoLocationSearch(textBox2.Text);
            _search.GeoLocationFound+=new GeoLocationSearch.GeoLocationHandler(_search_GeoLocationFound);
            _search.GetGeoCoordinate();
        }
    }
}