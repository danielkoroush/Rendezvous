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
    public partial class ChoosePlacePivot : PhoneApplicationPage
    {
        GraphApiPlaces p = new GraphApiPlaces();
        private const string no_restult_msg = "Sorry, couldn't find any results...";
        static bool checkin = false;
        public static String srchTxt = String.Empty;
        public static String latitude = String.Empty;
        public static String longitude = String.Empty;
        public static string location = "current location";
        bool _seachquery = false;
        GeoCoordinateWatcher watcher;
        ApplicationBarIconButton newPlace;
        ApplicationBarIconButton cancel;

        public ChoosePlacePivot()
        {
            InitializeComponent();
            showProgressBar(true);

            newPlace = new ApplicationBarIconButton(new Uri("/Images/appbar.add.rest.png", UriKind.Relative));
            newPlace.Text = "custom";
            newPlace.Click += new EventHandler(newPlace_Click);

            cancel = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancel.Text = "cancel";
            cancel.Click += new EventHandler(cancel_Click);
            
            ApplicationBar = new ApplicationBar();            
            ApplicationBar.Buttons.Add(newPlace);            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // Use high accuracy.
            watcher.MovementThreshold = 20; // Use MovementThreshold to ignore noise in the signal.
            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);

            p.Changed += new GraphApiPlaces.ChangedEventHandler(p_Changed);
            p.CitySearch += new GraphApiPlaces.CitySearchEventHandler(p_CitySearch);

            if (this.NavigationContext.QueryString.ContainsKey("choosecity"))
            {
                ChooseCity.GeoLocation += new ChooseCity.GeoLocationHandler(ChooseCity_GeoLocation);
                listBox2.Items.Clear();
                this.TopPivot.SelectedIndex = 1; 
            }
            if (GraphApiPlaces.FoundPlaces != null && GraphApiPlaces.FoundPlaces.Count > 0)
            {
                p_Changed(null, null);
            }
            else
            {
                    watcher.Start();
            }

            if (this.NavigationContext.QueryString.ContainsKey("checkin"))
            {
                checkin = true;
                ApplicationBar.IsVisible = false;                
            }

            PlaceTitle.Text = srchTxt;              
        }

        void ChooseCity_GeoLocation(object sender, EventArgs e)
        {                      
            if (String.IsNullOrEmpty(latitude) || String.IsNullOrEmpty(longitude))
            {

                showProgressBar(false);
            }
            else
            {
                p.GetPlaces(String.IsNullOrEmpty(srchTxt) ? String.Empty : srchTxt, latitude, longitude, AppSettings.TokenSetting, true);
                showProgressBar(true);
            }   
        }

        void p_CitySearch(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                listBox2.Items.Clear();
                if (GraphApiPlaces.FoundPlaces != null && GraphApiPlaces.FoundPlaces.Count > 0)
                {
                    foreach (Place p in GraphApiPlaces.FoundPlaces.Values)
                    {
                        listBox2.Items.Add(new ItemViewModel { EventTitle = p.Name, EventAddress = String.Format("Distance: {0} Miles", p.GetDistance(double.Parse(latitude), double.Parse(longitude)).ToString("0.000")) });
                    }
                }
                else
                {
                    listBox2.Items.Add(new ItemViewModel { EventTitle = no_restult_msg });
                }
                showProgressBar(false);
            });
        }

        void cancel_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        void newPlace_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/CustomPlace.xaml", UriKind.Relative));   
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

        /// <summary>
        /// once the places has been loaded display them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void p_Changed(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                listBox2.Items.Clear();
                if (GraphApiPlaces.FoundPlaces!=null && GraphApiPlaces.FoundPlaces.Count > 0)
                {
                    foreach (Place p in GraphApiPlaces.FoundPlaces.Values)
                    {
                        listBox2.Items.Add(new ItemViewModel { EventTitle = p.Name, EventAddress = String.Format("Distance: {0} Miles", String.IsNullOrEmpty(latitude) ? " " : p.GetDistance(double.Parse(latitude), double.Parse(longitude)).ToString("0.000")) });
                    }
                }
                else
                {
                    listBox2.Items.Add(new ItemViewModel { EventTitle = no_restult_msg });
                }

                showProgressBar(false);
            });
        }

        private void PlaceTitle_GotFocus(object sender, RoutedEventArgs e)
        {           
            PlaceTitle.FontStyle = FontStyles.Normal;
            PlaceTitle.Foreground = new SolidColorBrush(Colors.Black);
        }


        private void PlaceTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
                srchTxt = PlaceTitle.Text;
                Image_MouseLeftButtonDown(null, null);
            }
        }

        private void PlaceTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(srchTxt))
            {
                PlaceTitle.FontStyle = FontStyles.Italic;
                PlaceTitle.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                listBox2.Items.Clear();
                if (String.IsNullOrEmpty(latitude) || String.IsNullOrEmpty(longitude))
                {
                    listBox2.Items.Add(new ItemViewModel { EventTitle = no_restult_msg });
                }
                else
                {
                    p.GetPlaces(srchTxt, latitude, longitude, AppSettings.TokenSetting, true);
                    showProgressBar(true);
                    listBox2.Focus();
                }
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
                if (!_seachquery)
                {
                    p.GetPlaces(String.Empty, latitude, longitude, AppSettings.TokenSetting, false);
                }
                else
                {
                    _seachquery = false;
                    p.GetPlaces(srchTxt, latitude, longitude, AppSettings.TokenSetting, true);
                }
                showProgressBar(true);
                watcher.Stop();
            }
            else if (e.Status!=GeoPositionStatus.Initializing)
            {
                MessageBox.Show("Sorry, we couldn't detect your current location :(");
                NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
            }
        }


        private void listBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String parameter = string.Empty;
            if (listBox2.SelectedItem != null)
            {
                ItemViewModel i = (ItemViewModel)listBox2.SelectedItem;
                if (!i.EventTitle.Equals(no_restult_msg))
                {
                Create3.Place = i.EventTitle;
                FacebookData.choosenPlace = GraphApiPlaces.FoundPlaces[i.EventTitle];
                }
            }
            if (checkin)
            {                
                NavigationService.Navigate(new Uri("/Checkin_Page.xaml", UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/Create3.xaml", UriKind.Relative));
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {            
            base.OnBackKeyPress(e);
        }

        private void location_MouseEnter(object sender, MouseEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ChooseCity.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {      
        }
       

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ChooseCity.xaml", UriKind.Relative));
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            srchTxt = PlaceTitle.Text;
            if (!String.IsNullOrEmpty(latitude))
            {
                GraphApiPlaces.FoundPlaces = new Dictionary<string, Place>();
                p.GetPlaces(srchTxt, latitude, longitude, AppSettings.TokenSetting, true);
            }
            else
            {
                _seachquery = true;
                watcher.Start();
            }
            showProgressBar(true);
        }
    }
}