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
using Microsoft.Phone;
using System.IO;
using System.Text;
using Paco.Application;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace You.In
{
    public partial class Create3 : PhoneApplicationPage
    {
        public static string Place = String.Empty;
        public static String Title = String.Empty;
        public static String Description = String.Empty;

        public static string _invitees = String.Empty;
        private bool imageloaded = false;
        private WebClient m_webClient = new WebClient();
        // URL for sample image
        private Uri m_uri = new Uri("http://img692.imageshack.us/img692/1086/rendezvous.png");
            //("http://rend.ezvo.us/images/fb_applicationicon.png");
        // Local file name
        private string m_fileid = "uin.jpg";
        //private double EpochTime;

        GraphAPI graph = new GraphAPI();

        public static DateTime startTime = DateTime.Now;
        public static DateTime endTime = startTime.AddMinutes(180);
        public static DateTime startDate = DateTime.Now;
        public static DateTime endDate = endTime;
        static bool _isPublic = false;
        Deal deal;

        ApplicationBarIconButton save;
        ApplicationBarIconButton cancelled;
        public Create3()
        {
            InitializeComponent();
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            save = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative));
            save.Text = "create";
            save.Click += new EventHandler(save_Click);
            save.IsEnabled = true;

            cancelled = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancelled.Text = "cancel";
            cancelled.Click += new EventHandler(cancelled_Click);

            ApplicationBar.Buttons.Add(save);
            ApplicationBar.Buttons.Add(cancelled);

            #region titles
            if (FacebookData.choosenFriends != null && FacebookData.choosenFriends.Count > 0)
            {
                AddListBoxItems();
            }
            #endregion
        }

        void cancelled_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("deal"))
            {
                int id = int.Parse(this.NavigationContext.QueryString["deal"]);
                deal = Groupon.Deals[id];
                Title = deal.Title;
                endTime = deal.EndTime;
                endDate = deal.EndTime;
                Place = Deal_Pivot.CompanyName;
                Description = Deal_Pivot.Description.ToString();
            }

            start_dateBox.Text = startDate.Date.ToShortDateString();
            if (startDate.CompareTo(endDate) == 1)
            {
                endDate = DateTime.Parse(startDate.Date.ToLongDateString());
            }
            end_dateBox.Text = endDate.Date.ToShortDateString();
            timePicker1.Value = startTime;
            end_timePicker.Value = endTime;
            isPublic.IsChecked = _isPublic;
            if (!String.IsNullOrEmpty(Place)) venue.Text = Place;
            if (!String.IsNullOrEmpty(Title)) titleBox.Text = Title;
            if (!String.IsNullOrEmpty(Description)) descBox.Text = Description;
        }

        void save_Click(object sender, EventArgs e)
        {
            save.IsEnabled = false;
            if (FacebookData.choosenPlace != null || !String.IsNullOrEmpty(Title))
            {
                DateTime temp1 = DateTime.Parse(String.Format("{0} {1}", startDate.ToShortDateString(), startTime.TimeOfDay.ToString()));
                double EpochStartTime = double.Parse(Utility.ConvertUTtcToEpoch(temp1.ToUniversalTime()));

                DateTime temp2 = DateTime.Parse(String.Format("{0} {1}", endDate.ToShortDateString(),endTime.TimeOfDay.ToString()));
 
                double EpochEndTime = double.Parse(Utility.ConvertUTtcToEpoch(temp2.ToUniversalTime()));

                graph.EventCreated += new GraphAPI.FBEventHandler(graph_EventCreated);
                graph.CreateEvent(AppSettings.TokenSetting, Title, temp1, EpochEndTime, Description, _isPublic ? EventType.OPEN : EventType.CLOSED, FacebookData.choosenPlace);
            }
            else
            {
                MessageBox.Show("Please enter a name for the event or select a venue.");
                save.IsEnabled = true;
            }
        }

        void graph_EventCreated(object sender, EventArgs e)
        {
            try
            {
                FacebookData.choosenFriends = new List<long>();
                FacebookData.choosenPlace = null;
                Place = String.Empty;
                Title = String.Empty;
                Description = string.Empty;
                startTime = DateTime.Now;
                endTime = DateTime.Now.AddHours(3);
                startDate = DateTime.Now;
                endDate = endTime;

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
            finally
            {
                save.IsEnabled = true;
            }
        }

        private void ShowFeedBack(String msg)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(msg);
            });
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

        private void PhoneApplicationPage_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (!imageloaded)
            {
                //this is the image that gets used when you create a new event
                m_webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
                m_webClient.OpenReadAsync(m_uri);
            }
        }

        private void AddListBoxItems()
        {
            for (int i = 0; i < FacebookData.choosenFriends.Count; i++)
            {
                User u = FacebookData.friends[FacebookData.choosenFriends[i]];
                AddInvitee(u);
            }
        }

        private void AddInvitee(User u)
        {
            if (u != null)
            {
                StackPanel b = new StackPanel();
                b.Name = u.Uid.ToString();
                Canvas c = new Canvas()
                {
                    Height = 70,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    Width = 453,
                    Margin = new Thickness(-5, 5, 0, 0),
                };
                c.Name = u.Uid.ToString();
                Image i = new Image()
                {
                    Width = 65,
                    Height = 65,

                };
                Uri uri = new Uri(u.Pic); // Resource 
                BitmapImage imgSource = new BitmapImage(uri);
                i.Source = imgSource;

                Image i2 = new Image()
                {
                    Margin = new Thickness(410, 30, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Width = 35,
                    Height = 35,

                };

                BitmapImage imgSource2 = new BitmapImage(new Uri("/Images/remove.png", UriKind.Relative));
                i2.Source = imgSource2;

                TextBlock tb = new TextBlock()
                {
                    TextWrapping = TextWrapping.NoWrap,
                    Width = 250,
                    Text = u.Name,
                    Margin = new Thickness(75, -10, 0, 10),
                    Style = (Style)Application.Current.Resources["PhoneTextLargeStyle"]
                };

                c.Children.Add(i);
                c.Children.Add(tb);
                c.Children.Add(i2);

                b.Children.Add(c);

                GestureListener listener = GestureService.GetGestureListener(i2);
                listener.Tap += new EventHandler<Microsoft.Phone.Controls.GestureEventArgs>(WrapPanelSample_Tap);
                invitees.Children.Add(b);
            }
        }

        private void txtbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AutoCompleteBox acb = (AutoCompleteBox)sender;
            User u = (User)acb.SelectedItem;
            if (FacebookData.choosenFriends == null)
            {
                FacebookData.choosenFriends = new List<long>();
            }
            if (u != null && !FacebookData.choosenFriends.Contains(u.Uid))
            {
                FacebookData.choosenFriends.Add(u.Uid);
                AddInvitee(u);
            }
            txtbox.Text = String.Empty;
            b_MouseEnter(null, null);
        }

        void b_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                this.Focus();
            });
        }



        void WrapPanelSample_Tap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            Image i = (Image)sender;
            StackPanel s = (StackPanel)((Canvas)i.Parent).Parent;
            invitees.Children.Remove(s);
            FacebookData.choosenFriends.Remove(long.Parse(s.Name));
        }

        private void start_dateBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Uri("/DatePicker.xaml?date=" + startDate.Date.ToLongDateString(), UriKind.Relative));
        }

        private void end_dateBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Uri("/DatePicker2.xaml?date=" + endDate.Date.ToLongDateString(), UriKind.Relative));
        }

        private void timePicker1_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            startTime = (DateTime)timePicker1.Value;
        }

        private void isPublic_Checked(object sender, RoutedEventArgs e)
        {
            _isPublic = true;
        }

        private void isPublic_Unchecked(object sender, RoutedEventArgs e)
        {
            _isPublic = false;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ChoosePlacePivot.xaml", UriKind.Relative));
        }

        private void venue_TextChanged(object sender, TextChangedEventArgs e)
        {
            Place p = new Place(venue.Text, 0, 0, String.Empty);
            Place = venue.Text;
            FacebookData.choosenPlace = p;
        }


        private void venue_GotFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = false;
        }

        private void venue_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = true;
        }

        private void titleBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Title = titleBox.Text;
        }

        private void titleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = true;
        }

        private void titleBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = false;
        }

        private void titleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Title = titleBox.Text;
        }

        private void descBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Description = descBox.Text;
        }

        private void end_timePicker_ValueChanged_1(object sender, DateTimeValueChangedEventArgs e)
        {
            endTime = (DateTime)end_timePicker.Value;
        }
    }
}