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
using System.IO;
using System.Text;
using Paco.Application;
using System.Text.RegularExpressions;

namespace You.In
{
    public partial class Create : PhoneApplicationPage
    {
        public static string _place_name = String.Empty;
        public static string _invitees = String.Empty;
        private bool imageloaded = false;
        private WebClient m_webClient = new WebClient();
        // URL for sample image
        private Uri m_uri = new Uri("http://rend.ezvo.us/images/fb_applicationicon.png");
        // Local file name
        private string m_fileid = "uin.jpg";
        //private double EpochTime;
        DateTime dateTime;
        
        GraphAPI graph = new GraphAPI();
        public static string _title = String.Empty;
        public static string _descirption = String.Empty;
        public static bool _public = false;

        static DateTime _startDate = DateTime.Today;
        static DateTime _startTime = DateTime.Now;
        public static bool _endTimeSelected = false;
        public static DateTime _endDate = DateTime.Today.AddDays(1);
        public static DateTime _endTime = DateTime.Now.AddHours(1);
        ApplicationBarIconButton save;
        public Create()
        {
            InitializeComponent(); ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            save = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative));
            save.Text = "create";
            save.Click += new EventHandler(save_Click);

            ApplicationBarIconButton cancelled = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancelled.Text = "cancel";
            cancelled.Click += new EventHandler(cancelled_Click);

            ApplicationBar.Buttons.Add(save);
            ApplicationBar.Buttons.Add(cancelled);
            datePicker1.Value = _startDate;
            timePicker1.Value = _startTime;
            datePicker1.ValueChanged +=new EventHandler<DateTimeValueChangedEventArgs>(datePicker1_ValueChanged);
            timePicker1.ValueChanged +=new EventHandler<DateTimeValueChangedEventArgs>(timePicker1_ValueChanged);
            #region titles
            if (!String.IsNullOrEmpty(_title))
            {
                event_name.Text = _title;
                event_name.Visibility = Visibility.Visible;
                name_image.Visibility = Visibility.Collapsed;
                nameGrid.ColumnDefinitions[0].Width = new GridLength(12);
            }
            else
            {
                event_name.Visibility = Visibility.Collapsed;
                name_image.Visibility = Visibility.Visible;
                nameGrid.ColumnDefinitions[0].Width = new GridLength(72);
            }
            #endregion
            #region place
            if (!String.IsNullOrEmpty(_place_name))
            {
                PlaceBox.Text = _place_name;
            }
            #endregion
            #region people
            if (!String.IsNullOrEmpty(_invitees))
            {
                event_people.Text = _invitees + " people selected";
                event_people.Visibility = Visibility.Visible;
                people_image.Visibility = Visibility.Collapsed;
                peopleGrid.ColumnDefinitions[0].Width = new GridLength(12);
            }
            else
            {
                event_people.Visibility = Visibility.Collapsed;
                people_image.Visibility = Visibility.Visible;
                peopleGrid.ColumnDefinitions[0].Width = new GridLength(72);
            }
            #endregion
            #region description
            if (!String.IsNullOrEmpty(_descirption) || _endTimeSelected || _public)
            {
                event_details.Text = (!String.IsNullOrEmpty(_descirption)) ? _descirption + '\n' + '\n' : "";
                event_details.Text += (_public) ? "Public Event!\n" : "Private Event!\n";
                event_details.Text += (_endTimeSelected) ? "Ending: " + _endDate.ToString("ddd M/dd") + " " +_endTime.ToString("hh:mm tt") : "";
                event_details.Visibility = Visibility.Visible;
                details_image.Visibility = Visibility.Collapsed;
                detailsGrid.ColumnDefinitions[0].Width = new GridLength(12);
            }
            else
            {
                event_details.Visibility = Visibility.Collapsed;
                name_image.Visibility = Visibility.Visible;
                detailsGrid.ColumnDefinitions[0].Width = new GridLength(72);
            }
            #endregion
        }

        void cancelled_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
        }

        void save_Click(object sender, EventArgs e)
        {
            if (FacebookData.choosenPlace != null)
            {
                FacebookData.AddRecentPlace(FacebookData.choosenPlace);
                save.IsEnabled = false;
                String date = datePicker1.ValueString;
                String time = timePicker1.ValueString;                
                dateTime = DateTime.Parse(String.Format("{0} {1}", date, time));
                DateTime dateTime2= new DateTime();
                if (_endTimeSelected)
                {
                    dateTime2 = DateTime.Parse(String.Format("{0} {1}", _endDate.ToShortDateString(), _endTime.ToShortTimeString()));
                }
                //EpochTime = double.Parse(Utility.ConvertUTtcToEpoch(dateTime.ToUniversalTime()));
                graph.EventCreated += new GraphAPI.FBEventHandler(graph_EventCreated);
                graph.CreateEvent(AppSettings.TokenSetting, _title, dateTime, _endTimeSelected ? double.Parse(Utility.ConvertUTtcToEpoch(dateTime2.ToUniversalTime())) : 0,
                    _descirption, _public ? EventType.OPEN : EventType.CLOSED, FacebookData.choosenPlace);
            }
            else
            {
                MessageBox.Show("Please select a place.");
            }
        }

        void graph_EventCreated(object sender, EventArgs e)
        {
            save.IsEnabled = true;
            FacebookData.choosenFriends = new List<long>();
            FacebookData.choosenPlace = null;
            _place_name = String.Empty;
            _invitees = String.Empty;
            _title = String.Empty;
            _endTimeSelected = false;
            _public = false;
            this.Dispatcher.BeginInvoke(() =>
            {
                if (!String.IsNullOrEmpty(graph.eid))
                {
                    MessageBox.Show("Event created.");
                }
                else
                {
                    MessageBox.Show("Sorry could not create an event at this time. Please try again later.");
                }
                NavigationService.Navigate(new Uri("/Events_Page.xaml?refresh=1", UriKind.Relative));
            });
        }
        private void Canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            //SaveInfo();
            NavigationService.Navigate(new Uri("/new_event_name.xaml", UriKind.Relative));
        }

        private void SaveInfo()
        {
            _startDate = (DateTime)datePicker1.Value;
            _startTime = (DateTime)timePicker1.Value;
        }

        private void People_MouseEnter(object sender, MouseEventArgs e)
        {
            //SaveInfo();
            //NavigationService.Navigate(new Uri("/ChooseInvitees.xaml", UriKind.Relative));
            NavigationService.Navigate(new Uri("/FriendsChooser_Page.xaml", UriKind.Relative));
        }

        private void Details_MouseEnter(object sender, MouseEventArgs e)
        {
            NavigationService.Navigate(new Uri("/new_event_details.xaml", UriKind.Relative));
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = false;
            ((TextBox)sender).Text = "";
            ((TextBox)sender).FontStyle = FontStyles.Normal;
            ((TextBox)sender).Foreground = new SolidColorBrush(Colors.Black);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = true;
            if (String.IsNullOrEmpty(((TextBox)sender).Text))
            {
                if (((TextBox)sender).Name.Equals("EventTitle"))
                    ((TextBox)sender).Text = "event title...";
                else if (((TextBox)sender).Name.Equals("EventDescription"))
                    ((TextBox)sender).Text = "add event details [optional]...";
                ((TextBox)sender).FontStyle = FontStyles.Italic;
                ((TextBox)sender).Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
        private void ShowFeedBack(String msg)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(msg);
            });
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("pname"))
            {
                _place_name = (this.NavigationContext.QueryString["pname"]);                
            }
            if (this.NavigationContext.QueryString.ContainsKey("invitees"))
            {
                _invitees = (this.NavigationContext.QueryString["invitees"]);
            }
            base.OnNavigatedTo(e);
        }


        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!imageloaded)
            {
                //this is the image that gets used when you create a new event
                m_webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
                m_webClient.OpenReadAsync(m_uri);
            }
            if (!String.IsNullOrEmpty(_place_name))
            {
                PlaceBox.Text = _place_name;
                PlaceBox.Background = new SolidColorBrush(Colors.Black);
                PlaceBox.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// loading the event image and saving it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                // Read complete
                int count;
                byte[] buffer = new byte[1024];
                // Create (or replace) file and write image to it
                System.IO.Stream stream = e.Result;
                using (System.IO.IsolatedStorage.IsolatedStorageFile isf = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (System.IO.IsolatedStorage.IsolatedStorageFileStream isfs = new System.IO.IsolatedStorage.IsolatedStorageFileStream(m_fileid, FileMode.Create, isf))
                    {
                        count = 0;
                        while (0 < (count = stream.Read(buffer, 0, buffer.Length)))
                        {
                            isfs.Write(buffer, 0, count);
                        }
                        imageloaded = true;
                        stream.Close();
                        isfs.Close();
                    }
                }
            }
            catch (Exception)
            {
                imageloaded = false;
            }
        }

        private void datePicker1_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            SaveInfo();
        }

        private void Places_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AppSettings.GeoLocation)
            {
                NavigationService.Navigate(new Uri("/ChoosePlacePivot.xaml", UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/CustomPlace.xaml", UriKind.Relative));   
            }
        }

        private void PhoneApplicationPage_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (!imageloaded)
            {
                //this is the image that gets used when you create a new event
                m_webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
                m_webClient.OpenReadAsync(m_uri);
            }
        }

        private void timePicker1_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            SaveInfo();
        }

    }
}