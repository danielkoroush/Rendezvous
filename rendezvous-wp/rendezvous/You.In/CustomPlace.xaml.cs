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
using Microsoft.Phone.Shell;
using System.Device.Location;
using Paco.Application;

namespace You.In
{
    public partial class CustomPlace : PhoneApplicationPage
    {
        ApplicationBarIconButton create;
        ApplicationBarIconButton cancel;
        bool checkin = false;
        String latitude = String.Empty;
        String longitude = String.Empty;
        GeoCoordinateWatcher watcher;

        public CustomPlace()
        {
            InitializeComponent();
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // Use high accuracy.
                watcher.MovementThreshold = 20; // Use MovementThreshold to ignore noise in the signal.
                watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            }

            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            create = new ApplicationBarIconButton(new Uri("/Images/appbar.save.rest.png", UriKind.Relative));
            create.Text = "save";
            create.Click += new EventHandler(create_Click);

            cancel = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancel.Text = "cancel";
            cancel.Click += new EventHandler(cancel_Click);


            ApplicationBar.Buttons.Add(create);
            ApplicationBar.Buttons.Add(cancel);
        }
        void showProgressBar(bool show)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                if (show)
                {
                    progressBar1.IsIndeterminate = true;
                    //SyncMsg.Visibility = Visibility.Visible;
                    progressBar1.Visibility = Visibility.Visible;
                }
                else
                {
                    //SyncMsg.Visibility = Visibility.Collapsed;
                    progressBar1.Visibility = Visibility.Collapsed;
                }
            });
        }
        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            showProgressBar(false);
            if (e.Status == GeoPositionStatus.Ready)
            {
                // Use the Position property of the GeoCoordinateWatcher object to get the current location.
                GeoCoordinate co = watcher.Position.Location;
                latitude = co.Latitude.ToString("0.000");
                longitude = co.Longitude.ToString("0.000");
                watcher.Stop();
                this.PlaceAddress.Text = "current location";//latitude + " " + longitude;
            }
            else if (e.Status != GeoPositionStatus.Initializing)
            {
                MessageBox.Show("Sorry, we couldn't detect your current location :(");
            }

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("checkin"))
            {
                checkin = true;
            }
        }

        void cancel_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        void create_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(PlaceName.Text.Trim()))
            {
                MessageBox.Show("Please enter a place...");
                return;
            }

            String parameter = string.Empty;
            Place p;
            if (LocationGPS.IsChecked.Value && watcher != null)
            {
                p = new Place(PlaceName.Text, float.Parse(watcher.Position.Location.Latitude.ToString()), float.Parse(watcher.Position.Location.Longitude.ToString()), String.Empty);
            }
            else
            {
                p = new Place(PlaceName.Text, 0, 0, String.IsNullOrEmpty(PlaceAddress.Text) ? String.Empty : PlaceAddress.Text);
            }
            FacebookData.choosenPlace = p;
            if (!String.IsNullOrEmpty(PlaceName.Text))
            {
                parameter = "pname=" + PlaceName.Text;
                Create3.Place = PlaceName.Text;
            }

            NavigationService.Navigate(new Uri("/Create3.xaml", UriKind.Relative));
        }

        private void LocationGPS_Checked(object sender, RoutedEventArgs e)
        {
            this.PlaceAddress.IsEnabled = false;
            this.PlaceAddress.Text = "";
            showProgressBar(true);
            watcher.Start();
        }


        private void LocationGPS_Unchecked(object sender, RoutedEventArgs e)
        {
            this.PlaceAddress.Text = "";
            this.PlaceAddress.IsEnabled = true;
        }

        private void _KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Focus();
        }
    }
}