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
using You.In.GeocodeService;
using You.In.RouteService;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls.Maps.Platform;

namespace You.In
{
    public partial class MapPage2 : PhoneApplicationPage
    {        
        private double lattitude;
        private double longitude;
        private string location="location";
        ApplicationBarIconButton getRoute;
        GeoCoordinateWatcher watcher;
        double _user_latitude;
        double _user_longitude;


        public MapPage2()
        {
            InitializeComponent();
            map1.Loaded += new RoutedEventHandler(map1_Loaded);
            pin.Visibility = Visibility.Visible;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 0.8;

            getRoute = new ApplicationBarIconButton(new Uri("/Images/appbar.directions.png", UriKind.Relative));
            getRoute.Click += new EventHandler(getRoute_Click);
            getRoute.Text = "direction";
            getRoute.IsEnabled = false;
            ApplicationBar.Buttons.Add(getRoute);
            // The watcher variable was previously declared as type GeoCoordinateWatcher. 
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // Use high accuracy.
                watcher.MovementThreshold = 20; // Use MovementThreshold to ignore noise in the signal.
                watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            }
            watcher.Start();
        }

        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                // Use the Position property of the GeoCoordinateWatcher object to get the current location.
                GeoCoordinate co = watcher.Position.Location;
                _user_latitude = co.Latitude;
                _user_longitude = co.Longitude;
                //Stop the Location Service to conserve battery power.
                getRoute.IsEnabled = true;
                watcher.Stop();
            }
        }

        private void CalculateRoute()
        {
            // Create the service variable and set the callback method using the CalculateRouteCompleted property.
            RouteService.RouteServiceClient routeService = new RouteService.RouteServiceClient("BasicHttpBinding_IRouteService");
            routeService.CalculateRouteCompleted += new EventHandler<RouteService.CalculateRouteCompletedEventArgs>(routeService_CalculateRouteCompleted);

            // Set the token.
            RouteService.RouteRequest routeRequest = new RouteService.RouteRequest();
            routeRequest.Credentials = new Credentials();
            routeRequest.Credentials.ApplicationId = ((ApplicationIdCredentialsProvider)map1.CredentialsProvider).ApplicationId;

            // Return the route points so the route can be drawn.
            routeRequest.Options = new RouteService.RouteOptions();
            routeRequest.Options.RoutePathType = RouteService.RoutePathType.Points;

            // Set the waypoints of the route to be calculated using the Geocode Service results stored in the geocodeResults variable.
            routeRequest.Waypoints = new System.Collections.ObjectModel.ObservableCollection<RouteService.Waypoint>();
            RouteService.Waypoint waypoint = new RouteService.Waypoint();
           // waypoint.Description = "Shit City";
            waypoint.Location = new Location();
            waypoint.Location.Latitude = _user_latitude;
            waypoint.Location.Longitude = _user_longitude;

            RouteService.Waypoint waypoint2 = new RouteService.Waypoint();
           // waypoint2.Description = "Shit City";
            waypoint2.Location = new Location();
            waypoint2.Location.Latitude = lattitude;
            waypoint2.Location.Longitude = longitude;

            routeRequest.Waypoints.Add(waypoint);
            routeRequest.Waypoints.Add(waypoint2);

            // Make the CalculateRoute asnychronous request.
            routeService.CalculateRouteAsync(routeRequest);
        }

        private void routeService_CalculateRouteCompleted(object sender, RouteService.CalculateRouteCompletedEventArgs e)
        {

            // If the route calculate was a success and contains a route, then draw the route on the map.
            if ((e.Result.ResponseSummary.StatusCode == RouteService.ResponseStatusCode.Success) & (e.Result.Result.Legs.Count != 0))
            {
                // Set properties of the route line you want to draw.
                Color routeColor = Colors.Orange;
                SolidColorBrush routeBrush = new SolidColorBrush(routeColor);
                MapPolyline routeLine = new MapPolyline();
                routeLine.Locations = new LocationCollection();
                routeLine.Stroke = routeBrush;
                routeLine.Opacity = 0.65;
                routeLine.StrokeThickness = 5.0;

                //// Retrieve the route points that define the shape of the route.
                foreach (Location p in e.Result.Result.RoutePath.Points)
                {
                    Location t = new Location();
                    t.Latitude = p.Latitude;
                    t.Longitude = p.Longitude;
                    routeLine.Locations.Add(t);
                }

                //// Add a map layer in which to draw the route.
                MapLayer myRouteLayer = new MapLayer();
                map1.Children.Add(myRouteLayer);

                Pushpin pin2 = new Pushpin();
                pin2.Visibility = Visibility.Visible;
                pin2.Location = new GeoCoordinate(_user_latitude, _user_longitude);
                pin2.Content = "your location";
                map1.Children.Add(pin2);

                //// Add the route line to the new layer.
                myRouteLayer.Children.Add(routeLine);
                showMsgBox("Distance(Miles) from your current location: "+ e.Result.Result.Summary.Distance.ToString());
            }
        }

        private void showMsgBox(String msg)
        {
            distanceBlock.Text = msg;
            rectangle1.Visibility = Visibility.Visible; 
        }

        void getRoute_Click(object sender, EventArgs e)
        {
            if (watcher.Status == GeoPositionStatus.Ready)
            {
                CalculateRoute();
            }
            else
            {
                MessageBox.Show("Sorry we could not detect your current location.");
            }
        }

        void map1_Loaded(object sender, RoutedEventArgs e)
        {
            GeoCoordinate g = new GeoCoordinate(lattitude, longitude);
            map1.Center = g;
            map1.ZoomLevel = 16.0;
            pin.Visibility = Visibility.Visible;
            pin.Location = g;
            pin.Content = location;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("lat"))
            {
                lattitude = double.Parse(this.NavigationContext.QueryString["lat"]);
            }
            if (this.NavigationContext.QueryString.ContainsKey("long"))
            {
                longitude= double.Parse(this.NavigationContext.QueryString["long"]);
            }
            if (this.NavigationContext.QueryString.ContainsKey("location"))
            {
                location = this.NavigationContext.QueryString["location"];
            }
        }

        #region Event Handlers



        #endregion

    }
}