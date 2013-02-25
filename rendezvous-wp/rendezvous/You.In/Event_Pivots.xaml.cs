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
using System.Windows.Navigation;
using Paco.Application;
using System.Windows.Controls.Primitives;
using System.Device.Location;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace You.In
{
    public partial class Event_Pivots : PhoneApplicationPage
    {
        bool newPageInstance = false;
        string pageDataObject;
        private Event ev;
       // private string userStatusImage;
        List<User> _attendingList;
        List<User> _declinedList;
        List<User> _tentativeList;
        List<User> _noreplyList;
       // List<long> _friendsList;

        private bool _attending_loaded = false;
        private bool _declined_loaded = false;
        private bool _tentative_loaded = false;
        private bool _noreply_loaded = false;
        
        bool _friends_loaded = false;
        bool _is_suggestion_event = false;
        ApplicationBarIconButton acceptEvent;
        ApplicationBarIconButton tentativeEvent;
        ApplicationBarIconButton cancelEvent;

        Dictionary<long, RSVP> _myFriends = new Dictionary<long, RSVP>();
        GraphAPI api;
        RSVP request;

        public Event_Pivots()
        {
            InitializeComponent();
            newPageInstance = true;
            SupportedOrientations = SupportedPageOrientation.Portrait;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            acceptEvent = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative));
            acceptEvent.Click += new EventHandler(acceptEvent_Click);
            acceptEvent.Text = "accept";
            ApplicationBar.Buttons.Add(acceptEvent);

            tentativeEvent = new ApplicationBarIconButton(new Uri("/Images/appbar.questionmark.rest.png", UriKind.Relative));
            tentativeEvent.Click += new EventHandler(tentativeEvent_Click);
            tentativeEvent.Text = "tentative";

            ApplicationBar.Buttons.Add(tentativeEvent);

            cancelEvent = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancelEvent.Click += new EventHandler(cancelEvent_Click);
            cancelEvent.Text = "decline";

            ApplicationBar.Buttons.Add(cancelEvent);

            ApplicationBarMenuItem txtMenuItem = new ApplicationBarMenuItem("send via text");
            txtMenuItem.Click += new EventHandler(txtMenuItem_Click);

            ApplicationBarMenuItem emailMenuItem = new ApplicationBarMenuItem("send via e-mail");
            emailMenuItem.Click += new EventHandler(emailMenuItem_Click);

            ApplicationBar.MenuItems.Add(txtMenuItem);
            ApplicationBar.MenuItems.Add(emailMenuItem);            

            api = new GraphAPI();            
            api.RSVPSent += new GraphAPI.RSVPEventHandler(api_RSVPSent);

        }

        void emailMenuItem_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.Body = "Check out this event...\n" + String.Format("http://www.facebook.com/event.php?eid={0}", ev.ID);
            emailComposeTask.Subject = "Event";
            emailComposeTask.Show();
        }

        void txtMenuItem_Click(object sender, EventArgs e)
        {
            SmsComposeTask smsComposeTask = new SmsComposeTask();
            smsComposeTask.Body = "Check out this event...\n" + String.Format("http://www.facebook.com/event.php?eid={0}", ev.ID);
            smsComposeTask.Show();
        }

        void api_RSVPSent(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(api.rsvp_reply);
                // need to disable all the buttons since facebook is not letting you update the user status anymore
                if (!api.rsvp_posted)
                {
                    acceptEvent.IsEnabled = false;
                    cancelEvent.IsEnabled = false;
                    tentativeEvent.IsEnabled = false;
                }
                else
                {
                    try
                    {
                        FacebookData.User_Events[ev.ID].RSVP = request;
                    }
                    catch (Exception)
                    {
                        FacebookData.Open_Events[ev.ID].RSVP = request;
                    }
                }
            });
        }

        void set_appIcons(RSVP rsvp)
        {
            if (rsvp == RSVP.attending)
            {
                acceptEvent.IsEnabled = false;
                cancelEvent.IsEnabled = true;
                tentativeEvent.IsEnabled = true;
                userstatus.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Images/accept.png", UriKind.Relative));
            }
            else if (rsvp == RSVP.declined)
            {
                acceptEvent.IsEnabled = true;
                cancelEvent.IsEnabled = false;
                tentativeEvent.IsEnabled = true;
                userstatus.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Images/declined.png", UriKind.Relative));
            }
            else if (rsvp == RSVP.maybe)
            {
                acceptEvent.IsEnabled = true;
                cancelEvent.IsEnabled = true;
                tentativeEvent.IsEnabled = false;
                userstatus.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Images/maybe.png", UriKind.Relative));
            }
        }

        void cancelEvent_Click(object sender, EventArgs e)
        {
            request = RSVP.declined;
            api.RSVPEvent(AppSettings.TokenSetting, ev.ID, RSVP.declined);
            set_appIcons(RSVP.declined);
        }

        void tentativeEvent_Click(object sender, EventArgs e)
        {
            request = RSVP.maybe;
            api.RSVPEvent(AppSettings.TokenSetting, ev.ID, RSVP.maybe);
            set_appIcons(RSVP.maybe);
        }

        void acceptEvent_Click(object sender, EventArgs e)
        {
            request = RSVP.attending;
            api.RSVPEvent(AppSettings.TokenSetting, ev.ID, RSVP.attending);
            set_appIcons(RSVP.attending);
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.State["PreservingPageState"] = true;
           
            // Set newPageInstance back to false. It will be set back to true if the constructor is called again.
            newPageInstance = false;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // If this is a new page instance, the data must be retrieved in some way
            // If not, the page was already in memory and the data already exists
            if (newPageInstance)
            {                
                // if the application member variable is empty, call the method that loads data.
                if ((Application.Current as You.In.App).AppDataObject == null)
                {
                  //  GetDataAsync();
                }
                else
                {
                    // Otherwise set the page's data object from the application member variable
                    pageDataObject = (Application.Current as You.In.App).AppDataObject;
                    SetData(pageDataObject);
                }
            }

            if (this.NavigationContext.QueryString.ContainsKey("eid"))
            {
                long eid = long.Parse(this.NavigationContext.QueryString["eid"]);
                ImageBrush brush = new ImageBrush();
                try
                {                    
                    ev = FacebookData.User_Events[eid];                    
                }
                catch (KeyNotFoundException)
                {
                    ev = FacebookData.Open_Events[eid];
                    _is_suggestion_event = true;
                }
                ev.LoadedGuests = false;
                ev.Invitees = new Dictionary<RSVP, List<User>>();
                GetData();                
                
                if (ev.RSVP == RSVP.maybe)
                {
                    tentativeEvent.IsEnabled = false;
                    userstatus.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Images/maybe.png", UriKind.Relative));
                }
                else if (ev.RSVP == RSVP.declined)
                {
                    cancelEvent.IsEnabled = false;
                    userstatus.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Images/declined.png", UriKind.Relative));
                }
                else if (ev.RSVP == RSVP.not_replied)
                {
                    userstatus.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Images/noreply.png", UriKind.Relative));                
                }
                else
                {
                    acceptEvent.IsEnabled = false;
                    userstatus.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Images/accept.png", UriKind.Relative));                    
                }
                this.PageTitle.Title = ev.Name;
                this.Time.Text = Utility.ConvertEpochToUtc(ev.StartTime).ToString("ddd M/dd hh:mm tt");
                this.EndTime.Text = Utility.ConvertEpochToUtc(ev.EndTime).ToString("ddd M/dd hh:mm tt");
                if (!String.IsNullOrEmpty(ev.Description))
                {
                    this.Description.Text = ev.Description;
                }
                else
                {
                    this.Description.Visibility = Visibility.Collapsed;
                    this.DetailsLabel.Visibility = Visibility.Collapsed;
                }
                if (!String.IsNullOrEmpty(ev.Picture))
                    this.EventPic.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(ev.Picture, UriKind.RelativeOrAbsolute));
                else
                    this.EventPic.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("Images/event_thumb.png", UriKind.RelativeOrAbsolute));

                double latitude = 100;
                double longitude = 100;
                try
                {
                    latitude = double.Parse(ev.Venue_Latitude);
                    longitude = double.Parse(ev.Venue_Longitude);
                    this.map.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(String.Format("http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/{0},{1}/16?mapSize=600,200&pp={0},{1};34&mapVersion=v1&key=AhGs2lwlgYWijQdXshst6ZGVLD1Gm3oVUpiO8CEajIV2sAYerOB1UgP7TV1D4mNl", latitude, longitude), UriKind.RelativeOrAbsolute));
                }
                catch (Exception)
                {
                    this.textBlock1.Visibility = Visibility.Collapsed;
                    this.map.Visibility = Visibility.Collapsed;
                }

                String venue_address = ev.GetAddress();
                if (!String.IsNullOrEmpty(venue_address))
                {
                    venue_address = venue_address.Replace("\r\n", "\n");
                    var regex = new Regex(@"(\n\s*)+");
                    venue_address = regex.Replace(venue_address, "\n");
                    if (venue_address[venue_address.Length - 1] == '\n') // remove trailing /n if there
                    {
                        venue_address = venue_address.Substring(0, venue_address.Length - 1);
                    }
                    address.Text = venue_address;
                }
                else
                {
                    addresslabel.Visibility = Visibility.Collapsed;
                    address.Visibility = Visibility.Collapsed;
                }
                this.Host.Text = ev.Host;
            }
            base.OnNavigatedTo(e);
        }


        public void SetData(string data)
        {
            pageDataObject = data;

            // Set the Application class member variable to the data so that the
            // Application class can store it when the application is deactivated or closed.
            (Application.Current as You.In.App).AppDataObject = pageDataObject;
        }


        private void showProgressbar(bool show)
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

        private void GetData()
        {
            showProgressbar(true);

            BackgroundWorker getAttending = new BackgroundWorker();
            getAttending.DoWork += new DoWorkEventHandler(getAttending_DoWork);
            getAttending.RunWorkerAsync();

            BackgroundWorker getDeclined = new BackgroundWorker();
            getDeclined.DoWork += new DoWorkEventHandler(getDeclined_DoWork);
            getDeclined.RunWorkerAsync();

            if (!_is_suggestion_event)
            {
                BackgroundWorker getMaybe = new BackgroundWorker();
                getMaybe.DoWork += new DoWorkEventHandler(getMaybe_DoWork);
                getMaybe.RunWorkerAsync();

                BackgroundWorker getNoResponse = new BackgroundWorker();
                getNoResponse.DoWork += new DoWorkEventHandler(getNoResponse_DoWork);
                getNoResponse.RunWorkerAsync();
            }
        }

        void getNoResponse_DoWork(object sender, DoWorkEventArgs e)
        {
            FQLCall fql3 = new FQLCall(ev.ID);
            fql3.Status = FQLCall.NOT_REPLIED;
            fql3.Finished += delegate(object sender1, EventArgs e1)
            {
                _noreplyList = ev.GetAllInvitees(RSVP.not_replied);
                //_noreplyList = FacebookData.GetSortedUsersList(_noreplyList);
                _noreply_loaded = true;
                AddToAssociatedFriends(_noreplyList, RSVP.not_replied);
            };
            
            if (!ev.LoadedGuests)
            {
                fql3.GetInviteeDetail(AppSettings.TokenSetting, ev.ID, RSVP.not_replied);
            }
            //else
            //{
            //    _noreply_loaded = true;
            //    AddToAssociatedFriends(_noreplyList, RSVP.not_replied);                
            //}
        }        

        void getMaybe_DoWork(object sender, DoWorkEventArgs e)
        {
            FQLCall fql2 = new FQLCall(ev.ID);
            fql2.Status = FQLCall.UNSURE;
            fql2.Finished += delegate(object sender1, EventArgs e1)
            {
                _tentativeList = ev.GetAllInvitees(RSVP.maybe);
                //_tentativeList = FacebookData.GetSortedUsersList(_tentativeList);
                _tentative_loaded = true;
                AddToAssociatedFriends(_tentativeList, RSVP.maybe);               
            };

            if (!ev.LoadedGuests)
            {
                fql2.GetInviteeDetail(AppSettings.TokenSetting, ev.ID, RSVP.unsure);
            }
        }

        void getDeclined_DoWork(object sender, DoWorkEventArgs e)
        {
            FQLCall fql1 = new FQLCall(ev.ID);
            fql1.Status = FQLCall.DECLINED;

            fql1.Finished += delegate(object sender1, EventArgs e1)
            {
                _declinedList = ev.GetAllInvitees(RSVP.declined);
               // _declinedList = FacebookData.GetSortedUsersList(_declinedList);
                _declined_loaded = true;
                AddToAssociatedFriends(_declinedList, RSVP.declined);                
                try
                {
                    setCountText(notattendingcount, ev.GetDeclinedCount().ToString());
                }
                catch (NullReferenceException)
                {
                    setCountText(notattendingcount, "0");
                }           
            };
            
            if (!ev.LoadedGuests)
            {
                fql1.GetInviteeDetail(AppSettings.TokenSetting, ev.ID, RSVP.declined);
            }
            //else
            //{
            //    _declined_loaded = true;
            //    AddToAssociatedFriends(_declinedList, RSVP.declined);
            //    try
            //    {
            //        setCountText(this.notattendingcount, ev.GetDeclinedCount().ToString());
            //    }
            //    catch (NullReferenceException)
            //    {
            //        setCountText(notattendingcount, "0");
            //    }
            //}
        }

        void AddToAssociatedFriends(List<User> list, RSVP rsvp)
        {
            if (list != null)
            {
                foreach (User u in list)
                {
                    try
                    {
                        User temp = FacebookData.friends[u.Uid];
                        if (!_myFriends.ContainsKey(u.Uid))
                        {
                            _myFriends.Add(u.Uid, rsvp);
                        }
                        else
                        {
                            _myFriends[u.Uid] = rsvp;
                        }
                    }
                    catch (KeyNotFoundException)
                    {

                    }
                }
            }
            if (_attending_loaded && _declined_loaded && _tentative_loaded && _noreply_loaded)
            {
                
                _friends_loaded = true;
                PopulateListBox(listBox5, _myFriends, true);  
            }
        }        

        void getAttending_DoWork(object sender, DoWorkEventArgs e)
        {
            FQLCall fql = new FQLCall(ev.ID);
            fql.Status = FQLCall.ATTENDING;
            if (!ev.LoadedGuests)
            {
                fql.Finished += delegate(object sender1, EventArgs e1)
                {
                    _attendingList = ev.GetAllInvitees(RSVP.attending);
                    //_attendingList=FacebookData.GetSortedUsersList(_attendingList);
                    _attending_loaded = true;
                    AddToAssociatedFriends(_attendingList,RSVP.attending);                    
                    try
                    {
                        setCountText(this.attendingcount, ev.GetAcceptedCount().ToString());
                    }
                    catch (NullReferenceException)
                    {
                        setCountText(this.attendingcount, "0");
                    }
                };
            }
            if (!ev.LoadedGuests)
            {
                fql.GetInviteeDetail(AppSettings.TokenSetting, ev.ID, RSVP.attending);
            }
            //else
            //{
            //    _attending_loaded = true;
            //    AddToAssociatedFriends(_attendingList, RSVP.attending);                
            //    try
            //    {
            //        setCountText(this.attendingcount, ev.GetAcceptedCount().ToString());
            //    }
            //    catch (NullReferenceException)
            //    {
            //        setCountText(this.attendingcount, "0");
            //    }
            //}
        }
        

        void setCountText(TextBlock box, String count)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                box.Text = count;
            });
        }

        void fql_FinishedAttending(object sender, EventArgs e)
        {
        }

        //private void PopulateListBox(ListBox box, List<User> users, bool append)
        //{
        //    this.Dispatcher.BeginInvoke(() =>
        //    {
        //        if (!append) box.Items.Clear();
        //        if (users!=null && users.Count > 0)
        //        {
        //            foreach (User u in users)
        //            {                        
        //                box.Items.Add(new ItemViewModel() { EventTitle = u.Name, PicSource = u.Pic });
        //            }
        //        }
        //        else
        //        {
        //            box.Items.Add(new ItemViewModel() { EventTitle = "No Guests" });
        //        }
        //    });
        //    showProgressbar(false);
        //}

        private void PopulateListBox(ListBox box, Dictionary<long,RSVP> friends, bool append)
        {
            try
            {
                List<ItemViewModel> attendingItems = new List<ItemViewModel>();
                List<ItemViewModel> declinedItems = new List<ItemViewModel>();
                List<ItemViewModel> maybeItems = new List<ItemViewModel>();
                List<ItemViewModel> noreplyItems = new List<ItemViewModel>();
                if (friends != null && friends.Keys.Count > 0)
                {
                    foreach (long id in friends.Keys)
                    {
                        User u = FacebookData.friends[id];
                        if (friends[id].Equals(RSVP.attending))
                            attendingItems.Add(new ItemViewModel() { EventTitle = u.Name, PicSource = u.Pic, EventConfirmed = new Uri("/Images/accept.png", UriKind.Relative).ToString() });//friends[id].ToString() });                        
                        else if (friends[id].Equals(RSVP.declined))
                            declinedItems.Add(new ItemViewModel() { EventTitle = u.Name, PicSource = u.Pic, EventConfirmed = new Uri("/Images/declined.png", UriKind.Relative).ToString() });//friends[id].ToString() });                        
                        else if (friends[id].Equals(RSVP.maybe))
                            maybeItems.Add(new ItemViewModel() { EventTitle = u.Name, PicSource = u.Pic, EventConfirmed = new Uri("/Images/maybe.png", UriKind.Relative).ToString() });//friends[id].ToString() });                        
                        else if (friends[id].Equals(RSVP.not_replied))
                            noreplyItems.Add(new ItemViewModel() { EventTitle = u.Name, PicSource = u.Pic, EventConfirmed = new Uri("/Images/noreply.png", UriKind.Relative).ToString() });//friends[id].ToString() });                        
                    }

                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
         {
             box.Items.Clear();
             box.Items.Add(new ItemViewModel() { EventTitle = "No Guests" });
         });
                    return;
                }
                this.Dispatcher.BeginInvoke(() =>
                {
                    box.Items.Clear();
                    foreach (ItemViewModel i in attendingItems)
                    {
                        box.Items.Add(i);
                    }
                    foreach (ItemViewModel i in maybeItems)
                    {
                        box.Items.Add(i);
                    }
                    foreach (ItemViewModel i in declinedItems)
                    {
                        box.Items.Add(i);
                    }
                    foreach (ItemViewModel i in noreplyItems)
                    {
                        box.Items.Add(i);
                    }
                });
            }
            finally
            {
                showProgressbar(false);
            }
        }

        void Event_Pivots_BeginLayoutChanged(object sender, OrientationChangedEventArgs e)
        {
            Border border = new Border();
        }   
        

        private void map_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            ////send the user to the maps application for directions.
            var task = new WebBrowserTask();
            task.URL = String.Format("maps:{0}%2C{1}", ev.Venue_Latitude, ev.Venue_Longitude);
            task.Show();                        
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<long, RSVP> temp = ev.GetAssociatedFriends();
            if (!_friends_loaded && _is_suggestion_event)
            {
                _friends_loaded = true;
                PopulateListBox(listBox5, temp, true);                
            }
        }


        private void CloseImage()
        {
            this.popup.IsOpen = false;
        }

        private void EventPic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenImage();
        }

        private void OpenImage()
        {
            this.popup.IsOpen = true;
            popupImage.Source = EventPic.Source;
        }

        private void popupImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseImage();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (this.popup.IsOpen)
            {
                CloseImage();
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
            
        }
    }
}