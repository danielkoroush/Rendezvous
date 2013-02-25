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
using You.In;
using System.ComponentModel;
using Microsoft.Phone.Tasks;
using System.Collections;
using System.Device.Location;
using System.Windows.Media.Imaging;

namespace You.In
{
    public partial class Events_Page : PhoneApplicationPage
    {
        public static String Lat = String.Empty;
        public static String Lng = String.Empty;

        const String newEventText = "new event";
        private FQLCall fql = new FQLCall();
        private Groupon groupn = new Groupon();

        GeoCoordinateWatcher watcher;
        ApplicationBarIconButton newEvent;
        static long suggestion_uid = -1;

        BackgroundWorker getFriends = new BackgroundWorker();
        BackgroundWorker getSuggestions = new BackgroundWorker();
        BackgroundWorker x;
        BackgroundWorker addFriends = new BackgroundWorker();
        static BackgroundWorker getDeals;

        static List<User> friends = new List<User>();
        static bool _populated_events = false;
        static bool _populated_deals = false;
        static bool _loading_friendsEvent = false;
        static bool _foundDeals = false;
        public static long uid;
        static int index = 0;

        public Events_Page()
        {
            _populated_events = false;
            SupportedOrientations = SupportedPageOrientation.Portrait;
            InitializeComponent();

            Visibility v = (Visibility)Resources["PhoneLightThemeVisibility"];
            if (v != System.Windows.Visibility.Visible)
            {
                Uri uri = new Uri("/Images/powered_by_groupon2.png", UriKind.Relative); // Resource 
                System.Windows.Media.Imaging.BitmapImage imgSource = new System.Windows.Media.Imaging.BitmapImage(uri);
                grouponLogo.Source = imgSource;
            }

          // // this.grouponLogo.Source
          //  // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.FriendsList.SelectionChanged += new SelectionChangedEventHandler(FriendsList_SelectionChanged);
            this.FriendsList.MouseLeftButtonDown += new MouseButtonEventHandler(FriendsList_MouseLeftButtonDown);
            //  fql_SuggestionsFinished(null, null);
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // Use high accuracy.
            watcher.MovementThreshold = 20; // Use MovementThreshold to ignore noise in the signal.
            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            if (!_populated_deals)
            {
                dealListBox.Items.Add(new Deal("Loading deals please wait."));
                watcher.Start();
            }
            else
            {
                groupn_Finished(null, null);
            }
        }

        void FriendsList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            index = 1;
            NavigationService.Navigate(new Uri("/FriendSelector.xaml", UriKind.Relative));
        }

        private void Summarize(IList items)
        {
        }

        // Get a friend events when selection changes
        void FriendsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListPicker lp = (ListPicker)sender;
            if (lp != null)
            {
                User user = (User)lp.SelectedItem;
               if (user != null && suggestion_uid != user.Uid)
                {
                    fql.SuggestionsFinished += new FQLCall.SuggestionsEventHandler(fql_SuggestionsFinished);
                    suggestion_uid = user.Uid;
                    //getSuggestions.WorkerSupportsCancellation = true;
                    //if (getSuggestions.IsBusy)
                    //{
                    //    getSuggestions.CancelAsync();
                    //}
                    getSuggestions.DoWork += new DoWorkEventHandler(getSuggestions_DoWork);

                    SuggestionListBox.Items.Clear();
                   FacebookData.Open_Events = new Dictionary<long, Event>();
                    SuggestionListBox.Items.Add(new ItemViewModel() { EventTitle = "Loading ...", EventDate = String.Empty });
                    showProgressBar(true);
                   getSuggestions.RunWorkerAsync();

                }
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            AppSettings.BackToLogin = true;
            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            fql.Finished += new FQLCall.FBEventHandler(fql_Finished);
            fql.UpcomingEventsFinished += new FQLCall.UpcomingEventsEventHandler(fql_Finished);
            fql.FriendsFinished += new FQLCall.FriendsEventHandler(fql_FriendsFinished);
            groupn.Finished += new Groupon.DealsFinshedEventHandler(groupn_Finished);
            try
            {
                if (this.NavigationContext.QueryString.ContainsKey("refresh"))
                {
                    App.ViewModel.Items.Clear();
                    FacebookData.LoadedEvents = false;
                    App.ViewModel.IsDataLoaded = false;
                    refreshMenuItem_Click(null, null);
                    return;
                }
                if (!_populated_events)
                {
                    x = new BackgroundWorker();
                    x.WorkerSupportsCancellation = true;
                    x.DoWork += new DoWorkEventHandler(x_DoWork);
                    x.RunWorkerAsync();
                }

                if (FacebookData.LoadedFriends)
                {
                    if (this.NavigationContext.QueryString.ContainsKey("uid"))
                    {
                        string temp = this.NavigationContext.QueryString["uid"] as string;
                        if (temp == "1")
                        {
                            this.menu.SelectedIndex = index;
                            this.FriendsList.Items.Add(FacebookData.friends[uid]);
                        }
                    }
                    else
                    {
                        this.FriendsList.Items.Add(getRandomFriend());
                    }
                }
            }
            finally
            {
                Pivot_SelectionChanged(null, null);
            }
        }

        private User getRandomFriend()
        {
            Random random = new Random();
            int temp = random.Next(0, FacebookData.friends.Count-1);
            return FacebookData.friends.Values.ToArray<User>()[temp];
        }

        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            try
            {
                if (e.Status == GeoPositionStatus.Ready)
                {
                    // Use the Position property of the GeoCoordinateWatcher object to get the current location.
                    GeoCoordinate co = watcher.Position.Location;
                    Lat = co.Latitude.ToString("0.000");
                    Lng = co.Longitude.ToString("0.000");
                    watcher.Stop();
                    showProgressBar(true);
                    getDeals = new BackgroundWorker();
                    getDeals.WorkerSupportsCancellation = true;
                    getDeals.DoWork += new DoWorkEventHandler(getDeals_DoWork);
                    getDeals.RunWorkerAsync();
                }
                else if (e.Status != GeoPositionStatus.Initializing)
                {
                    Lat = "0";
                    Lng = "0"; 
                }
            }
            finally
            {

            }
        }

        void groupn_Finished(object sender, EventArgs e)
        {
            try
            {
                //Show the DEALS
                this.Dispatcher.BeginInvoke(() =>
    {
        dealListBox.Items.Clear();

        if (Groupon.Deals.Count > 0)
        {
            _foundDeals = true;
            foreach (Deal d in Groupon.Deals)
            {
                dealListBox.Items.Add(d);
            }
        }
        else
        {
            _foundDeals = false;
            Deal d = new Deal("Sorry we found no deals near you.");
            dealListBox.Items.Add(d);
        }
    });
            }
            finally
            {
                _populated_deals = true;
                Pivot_SelectionChanged(null, null);
            }
        }

        void getDeals_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!_populated_deals)
            {
                if (!getDeals.CancellationPending)
                {
                    groupn.getDeals(Lat, Lng);
                }
            }
            else
            {
                groupn_Finished(null, null);
            }
        }

        void logoutMenuItem_Click(object sender, EventArgs e)
        {
            if (getFriends != null)
            {
                try
                {
                    getFriends.CancelAsync();
                }
                catch (Exception) { }
            }
            if (x != null)
            {
                try
                {
                    x.CancelAsync();
                }
                catch (Exception) { }
            }
            if (getSuggestions != null && getSuggestions.IsBusy)
            {
                try
                {
                    getSuggestions.CancelAsync();
                }
                catch (Exception) { }
            }

            AppSettings.TokenSetting = String.Empty;
            AppSettings.LogOut();

            SuggestionListBox.Items.Clear();
            App.ViewModel.Items.Clear();
            NavigationService.Navigate(new Uri("/MainPage.xaml?logout=1", UriKind.Relative));
        }

        void refreshMenuItem_Click(object sender, EventArgs e)
        {
            FacebookData.User_Events = new Dictionary<long, Event>();
            FacebookData.Checkins = new Dictionary<long, Checkin>();
            FacebookData.LoadedEvents = false;
            FacebookData.LoadedCheckins = false;
            App.ViewModel.IsDataLoaded = false;
            _populated_events = false;

            BackgroundWorker x = new BackgroundWorker();
            x.DoWork += new DoWorkEventHandler(x_DoWork);
            x.RunWorkerAsync();

            watcher.Start();
        }


        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            newEvent = new ApplicationBarIconButton(new Uri("/Images/appbar.newevent.rest.png", UriKind.Relative));
            newEvent.Click += new EventHandler(ApplicationBarIconButton_Click);
            newEvent.Text = newEventText;

            ApplicationBarMenuItem refreshMenuItem = new ApplicationBarMenuItem("refresh");
            refreshMenuItem.Click += new EventHandler(refreshMenuItem_Click);

            ApplicationBarMenuItem logoutMenuItem = new ApplicationBarMenuItem("logout");
            logoutMenuItem.Click += new EventHandler(logoutMenuItem_Click);

            ApplicationBarMenuItem settingMenuItem = new ApplicationBarMenuItem("settings");
            settingMenuItem.Click += new EventHandler(settingMenuItem_Click);

            ApplicationBar.Buttons.Add(newEvent);
            ApplicationBar.MenuItems.Add(refreshMenuItem);
            ApplicationBar.MenuItems.Add(settingMenuItem);
            ApplicationBar.MenuItems.Add(logoutMenuItem);

            getFriends = new BackgroundWorker();
            getFriends.WorkerSupportsCancellation = true;
            getFriends.DoWork += new DoWorkEventHandler(getFriends_DoWork);
            getFriends.RunWorkerAsync();
        }


        void settingMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        void getSuggestions_DoWork(object sender, DoWorkEventArgs e)
        {
            _loading_friendsEvent = true;
            fql.GetSuggestions(AppSettings.TokenSetting, suggestion_uid);
        }

        void getFriends_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!FacebookData.LoadedFriends)
            {
                fql.GetFriends(AppSettings.TokenSetting);
            }
        }

        void x_DoWork(object sender, DoWorkEventArgs e)
        {
            fql.Finished += new FQLCall.FBEventHandler(fql_Finished);
            fql.UpcomingEventsFinished += new FQLCall.UpcomingEventsEventHandler(fql_Finished);
            if (!FacebookData.LoadedEvents)
            {
                fql.GetUpcomingEvents(AppSettings.TokenSetting);
            }
            else if (!_populated_events)
            {
                fql_Finished(null, null);
            }
        }

        void fql_SuggestionsFinished(object sender, EventArgs e)
        {
            try
            {
                if (!getSuggestions.CancellationPending && FacebookData.Open_Events != null)
                {
                    List<Event> suggestionEvents = FacebookData.Open_Events.Values.ToList<Event>();
                    suggestionEvents.Sort();
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        SuggestionListBox.Items.Clear();
                        if (suggestionEvents.Count > 0)
                        {
                            for (int i = 0; i < suggestionEvents.Count; i++)
                            {
                                Event temp = suggestionEvents[i];
                                if (temp.GetAssociatedFriends() != null)
                                {
                                    int assocFriendCount = temp.GetAssociatedFriends().Count;
                                    ItemViewModel tmp = new ItemViewModel()
                                    {
                                        EventTitle = temp.Name,
                                        EventDate = Utility.ConvertEpochToUtc(temp.StartTime).ToString("ddd, MMM dd"),
                                        EventTime = Utility.ConvertEpochToUtc(temp.StartTime).ToString("t"),
                                        EventAddress = temp.Location,
                                        Eid = temp.ID,
                                        PicSource = temp.Picture
                                    };
                                    try
                                    {
                                        if (assocFriendCount > 0) tmp.EventConfirmed = FacebookData.friends[temp.GetAssociatedFriends().Keys.ElementAt(0)].Name;
                                        if (assocFriendCount > 1) tmp.EventConfirmed += String.Format(" + {0} others...", assocFriendCount - 1);
                                        SuggestionListBox.Items.Add(tmp);
                                    }
                                    catch (KeyNotFoundException) { }
                                }
                            }
                        }
                        else
                        {
                            SuggestionListBox.Items.Add(new ItemViewModel() { EventTitle = "No Upcoming Event.", EventDate = String.Empty });
                        }
                    });
                }
            }
            finally
            {
                _loading_friendsEvent = false;
                Pivot_SelectionChanged(null, null);
            }
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

        void fql_FriendsFinished(object sender, EventArgs e)
        {
            FacebookData.LoadedFriends = true;
            this.Dispatcher.BeginInvoke(() =>
            {
                this.FriendsList.Items.Add(getRandomFriend());
            });
        }

        void fql_Finished(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                App.ViewModel.Items.Clear();
                App.ViewModel.LoadData(FacebookData.User_Events);
                App.ViewModel.IsDataLoaded = true;
            });
            FacebookData.LoadedEvents = true;
            _populated_events = true;
            Pivot_SelectionChanged(null, null);
        }

        private void FirstListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemViewModel item = ((ListBox)sender).SelectedItem as ItemViewModel;
            if (item != null && item.Eid != 0)
            {
                index = 0;
                NavigationService.Navigate(new Uri("/Event_Pivots.xaml?eid=" + item.Eid, UriKind.Relative));
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (((ApplicationBarIconButton)sender).Text.Equals(newEventText))
            {

                NavigationService.Navigate(new Uri("/Create3.xaml", UriKind.Relative));
            }
            else
            {
                if (AppSettings.GeoLocation)
                {
                    NavigationService.Navigate(new Uri("/ChoosePlacePivot.xaml?checkin=1", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show("Location services has been turned off. Please go to the settings page to turn on the location services.");
                }
            }
        }


        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
    {
        if (menu.SelectedIndex == 0)
        {
            showProgressBar(!_populated_events);
        }
        else if (menu.SelectedIndex == 2)
        {
            showProgressBar(!_populated_deals);
        }
        else if (menu.SelectedIndex == 1)
        {
            showProgressBar(_loading_friendsEvent);
        }
    });
        }

        private void dealListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_foundDeals)
            {
                index = 2;
                int id = ((ListBox)sender).SelectedIndex;
                NavigationService.Navigate(new Uri("/Deal_Pivot.xaml?id=" + id, UriKind.Relative));
            }
        }

        private void SuggestionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemViewModel item = ((ListBox)sender).SelectedItem as ItemViewModel;
            if (item != null && item.Eid != 0)
            {
                if (FacebookData.Open_Events.ContainsKey(item.Eid))
                {
                    index = 0;
                    NavigationService.Navigate(new Uri("/Event_Pivots.xaml?eid=" + item.Eid, UriKind.Relative));
                }
            }
        }

    }
}