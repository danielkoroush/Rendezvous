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
using Microsoft.Phone.Shell;
using System.Device.Location;
using Microsoft.Phone.Controls;
using System.IO;

namespace You.In
{
    public partial class ChooseCity : PhoneApplicationPage
    {
        ApplicationBarIconButton create;
        ApplicationBarIconButton cancel;
        
        String latitude = String.Empty;
        String longitude = String.Empty;
        GeoCoordinateWatcher watcher;

        public delegate void GeoLocationHandler(object sender, EventArgs e);
        public static event GeoLocationHandler GeoLocation;

        public virtual void OnGeoLocation(EventArgs e)
        {
            if (GeoLocation != null)
                GeoLocation(null, e);
            if (String.IsNullOrEmpty(ChoosePlacePivot.latitude) || String.IsNullOrEmpty(ChoosePlacePivot.longitude))
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(String.Format("Sorry we could not find geographic coordinate for {0}.", ChoosePlacePivot.location));
                });
            }
            else
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        NavigationService.Navigate(new Uri("/ChoosePlacePivot.xaml?choosecity=1", UriKind.Relative));
                    }
                    catch (InvalidOperationException) { }
                });
            }

        }

        public ChooseCity()
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

            create = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative));
            create.Text = "accept";
            create.Click += new EventHandler(create_Click);

            cancel = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancel.Text = "cancel";
            cancel.Click += new EventHandler(cancel_Click);

            create.IsEnabled = false;
            ApplicationBar.Buttons.Add(create);
            ApplicationBar.Buttons.Add(cancel);
            this.CityName.Focus();
        }
        void showProgressBar(bool show)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                if (show)
                {
                    progressBar1.IsIndeterminate = true;                    
                    progressBar1.Visibility = Visibility.Visible;
                }
                else
                {                    
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
                ChoosePlacePivot.latitude = co.Latitude.ToString("0.000");
                ChoosePlacePivot.longitude = co.Longitude.ToString("0.000");
                watcher.Stop();
                create.IsEnabled = true;
                this.CityName.Text = "current location";
                OnGeoLocation(EventArgs.Empty);
            }
            else if (e.Status != GeoPositionStatus.Initializing)
            {
                MessageBox.Show("Sorry, we couldn't detect your location. :(");
            }
        }
               
        void cancel_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        void create_Click(object sender, EventArgs e)
        {
            this.Focus();
            ChoosePlacePivot.location = CityName.Text;
            NavigationService.Navigate(new Uri("/ChoosePlacePivot.xaml?choosecity=1", UriKind.Relative));            
        }

        private void LocationGPS_Checked(object sender, RoutedEventArgs e)
        {
            this.CityName.IsEnabled = false;
            this.CityName.Text = "";
            
            showProgressBar(true);
            watcher.Start();
        }


        private void LocationGPS_Unchecked(object sender, RoutedEventArgs e)
        {
            this.CityName.Text = "";
            this.CityName.IsEnabled = true;
            create.IsEnabled = false;
        }

        private void _KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
                ChoosePlacePivot.location = CityName.Text;
                // NavigationService.Navigate(new Uri("/ChoosePlacePivot.xaml?choosecity=1", UriKind.Relative));   
            }
        }

        /// <summary>
        /// this will call the google API for getting the long/lat given a city name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CityName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(this.CityName.Text))
            {
                create.IsEnabled = true;
                Paco.Application.Place.GetLongLat(this.CityName.Text, CityResponseCallback);
            }            
        }

        private void CityResponseCallback(IAsyncResult ar)
        {
            string json;
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    json = streamReader1.ReadToEnd();   
                }
                Paco.Application.Place.GeoCodeResponseCallback(json, out ChoosePlacePivot.latitude, out ChoosePlacePivot.longitude);                
                OnGeoLocation(EventArgs.Empty);
        }
		        private void CityName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(CityName.Text))
                create.IsEnabled = true;
            else
                create.IsEnabled = false;
        }
    }
}