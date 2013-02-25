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
using Paco.Application;
using System.Device.Location;

namespace You.In
{
    public partial class ChoosePlace : PhoneApplicationPage
    {
        GraphApiPlaces p = new GraphApiPlaces();        
        private const string search_msg="Search for a place...";
        bool checkin = false;
        String latitude = String.Empty;
        String longitude = String.Empty;
        static bool more = false;
        GeoCoordinateWatcher watcher;

        public ChoosePlace()
        {
            InitializeComponent();
            this.listBox1.GotFocus += new RoutedEventHandler(listBox1_GotFocus);
            showProgressBar(true);
            p.Changed += new GraphApiPlaces.ChangedEventHandler(p_Changed);            
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // Use high accuracy.
                watcher.MovementThreshold = 20; // Use MovementThreshold to ignore noise in the signal.
                watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            }
            watcher.Start();

        }

        void listBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (listBox1.Items.Count > 0)
            {
                p.GetMoreResult();
            }
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("checkin"))
            {
                checkin = true;
            }
        }
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
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

        void accepted_Click(object sender, EventArgs e)
        {
            String parameter = string.Empty;
            if (listBox1.SelectedItem != null)
            {
                parameter = "pname=" + listBox1.SelectedItem.ToString();
                FacebookData.choosenPlace = GraphApiPlaces.FoundPlaces[listBox1.SelectedItem.ToString()];
            }
            if (checkin)
            {                
                NavigationService.Navigate(new Uri("/Checkin_Page.xaml?" + parameter, UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/Create3.xaml?" + parameter, UriKind.Relative));
            }
        }

        void cancelled_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
        /// <summary>
        /// once the places has been loaded display them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void p_Changed(object sender, EventArgs e)
        {
            //this.Dispatcher.BeginInvoke(() =>
            //{
            //    foreach (Place p in GraphApiPlaces.FoundMorePlaces.Values)
            //    {
            //        listBox1.Items.Add(new ItemViewModel { EventTitle = p.Name, EventAddress =String.Format("Distance: {0} Miles", p.GetDistance(double.Parse(latitude), double.Parse(longitude)).ToString("0.000")) });
            //    }
            //    showProgressBar(false);
            //});
            //more = false;
        }

        private void PlaceTitle_GotFocus(object sender, RoutedEventArgs e)
        {           
            if (PlaceTitle.Text.Equals(search_msg))
            {
                PlaceTitle.Text = String.Empty;
            }
            PlaceTitle.FontStyle = FontStyles.Normal;
            PlaceTitle.Foreground = new SolidColorBrush(Colors.Black);
        }


        private void PlaceTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                p.Changed += new GraphApiPlaces.ChangedEventHandler(p_Changed);
                p.GetPlaces(PlaceTitle.Text, latitude, longitude, AppSettings.TokenSetting,false);
                listBox1.Focus();
            }
        }

        private void PlaceTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(PlaceTitle.Text))
            {
                PlaceTitle.Text = search_msg;
                PlaceTitle.FontStyle = FontStyles.Italic;
                PlaceTitle.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }


        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {

            if (e.Status == GeoPositionStatus.Ready)
            {
                // Use the Position property of the GeoCoordinateWatcher object to get the current location.
                GeoCoordinate co = watcher.Position.Location;
                latitude = co.Latitude.ToString("0.000");
                longitude = co.Longitude.ToString("0.000");                
                p.GetPlaces(String.Empty, latitude, longitude, AppSettings.TokenSetting,false);
                watcher.Stop();
            }
            else if (e.Status!=GeoPositionStatus.Initializing)
            {
                MessageBox.Show("Sorry, we couldn't detect your current location :(");
                NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
            }
        }

        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!more && listBox1.Items.Count > 0)
            {
                more = true;
                p.GetMoreResult();
            }
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String parameter = string.Empty;
            if (listBox1.SelectedItem != null)
            {
                ItemViewModel i = (ItemViewModel) listBox1.SelectedItem;
                parameter = "pname=" + i.EventTitle;
                FacebookData.choosenPlace = GraphApiPlaces.FoundPlaces[i.EventTitle];
            }
            if (checkin)
            {
                // NavigationService.Navigate(new Uri("/PivotPage1.xaml?" + parameter, UriKind.Relative));
                NavigationService.Navigate(new Uri("/Checkin_Page.xaml?" + parameter, UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/Create3.xaml?" + parameter, UriKind.Relative));
            }
        }
    }
}