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
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Phone.Tasks;

namespace You.In
{
    public partial class Checkin_Page : PhoneApplicationPage
    {
        ApplicationBarIconButton send;
        ApplicationBarIconButton cancel;
        ApplicationBarIconButton add_friends;
        private const String checkinMsgTxt = "Message(optional)...";
        private const String picture = "&picture=http://rend.ezvo.us/images/fb_newsfeed_icon.png";
        static bool futureCheckin = false;
        GraphAPI api;
        Groupon groupon;

        static String chknMsg = String.Empty;
        static DateTime selectedDate = DateTime.Now;
        static DateTime selectedTime = DateTime.Now;
        double EpochTime;
        static String checkin_msg;
        public Checkin_Page()
        {            
            InitializeComponent();

            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            send = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative));         
            send.Text = "publish";
            send.Click += new EventHandler(send_Click);

            cancel = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancel.Text = "cancel";
            cancel.Click += new EventHandler(cancel_Click);

            add_friends = new ApplicationBarIconButton(new Uri("/Images/appbar.friends.rest2.png", UriKind.Relative));
            add_friends.Text = "tag friends";
            add_friends.Click += new EventHandler(add_friends_Click);

            ApplicationBar.Buttons.Add(send);
            ApplicationBar.Buttons.Add(cancel);
            ApplicationBar.Buttons.Add(add_friends);
            if (String.IsNullOrEmpty(chknMsg))
                checkinMsg.Text = checkinMsgTxt;
            else
                checkinMsg.Text = chknMsg;
            if (FacebookData.choosenPlace == null)
            {
                this.Dispatcher.BeginInvoke(() =>
{
    NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
});
            }
            else
            {
                placeName.Text = FacebookData.choosenPlace.Name;
            }
            placeImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(FacebookData.choosenPlace.GetMapImage(), UriKind.RelativeOrAbsolute));
            placeAddress.Text = FacebookData.choosenPlace.GetAddress();

        }

        void add_friends_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FriendsChooser_Page.xaml?checkin=1", UriKind.Relative));
        }

        void cancel_Click(object sender, EventArgs e)
        {
            FacebookData.choosenFriends = new List<long>();
            NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
        }

        void send_Click(object sender, EventArgs e)
        {
            send.IsEnabled = false;
            if (FacebookData.choosenPlace == null)
            {
                MessageBox.Show("Please select a place first...");
                return;
            }
            else if (String.IsNullOrEmpty(FacebookData.choosenPlace.GetLatitude()) || String.IsNullOrEmpty(FacebookData.choosenPlace.GetLongitude()))
            {
                MessageBox.Show("Sorry, you can't checkin to this place because it doesn't contain any geo coordinates.");
                return;
            }
            String parameters = String.Empty;
            if (!checkinMsg.Text.Equals(checkinMsgTxt))
            {
                checkin_msg = checkinMsg.Text;
            }
            else
            {
                checkin_msg = String.Empty;
            }
            HttpWebRequest request = WebRequest.Create(new Uri(String.Format("https://graph.facebook.com/me/checkins?access_token={0}", AppSettings.TokenSetting))) as HttpWebRequest;
            request.Method = "POST";
            IAsyncResult asyncResult = request.BeginGetRequestStream(new AsyncCallback(CheckinRequestCallback), request);            
        }

        /// <summary>
        /// Post on User Wall
        /// </summary>
        /// <param name="ar"></param>
        private void CheckinRequestCallback(IAsyncResult ar)
        {
            string parameters;
            string friend = String.Empty;
            string uids = String.Empty;

            if (FacebookData.choosenFriends != null)
            {
                for (int i = 0; i < FacebookData.choosenFriends.Count; i++)
                {
                    friend += FacebookData.friends[FacebookData.choosenFriends[i]].Name+ ",";
                    uids += FacebookData.friends[FacebookData.choosenFriends[i]].Uid + ",";
                }
                if (!String.IsNullOrEmpty(friend))
                {
                    friend = " With " + friend.Remove(friend.Length - 1) + "!";
                    uids = uids.Remove(uids.Length - 1);
                }
            }

            if (futureCheckin)
            {                
                String msg = String.Format("Heading to {0} @ {1}!", FacebookData.choosenPlace.Name, Utility.ConvertEpochToUtc(EpochTime)) + friend;
                String link = String.Format("http://www.facebook.com/pages/{0}/{1}", FacebookData.choosenPlace.Name.Replace(" ", "-"), FacebookData.choosenPlace.ID);                
              //  String place = String.Format("&coordinates={{\"latitude\":\"{0}\", \"longitude\": \"{1}\"}}&place={2}", FacebookData.choosenPlace.GetLatitude(), FacebookData.choosenPlace.GetLongitude(), FacebookData.choosenPlace.ID);
                
                api = new GraphAPI();
                api.EventCreated += new GraphAPI.FBEventHandler(api_EventCreated);
                //api.CreateEvent(AppSettings.TokenSetting, msg, EpochTime, 0,String.Format("{0}{1}{2}" ,msg,Environment.NewLine,link), EventType.OPEN, FacebookData.choosenPlace);
            

            }
            else
            {
                String place = String.Format("&coordinates={{\"latitude\":\"{0}\", \"longitude\": \"{1}\"}}&place={2}", FacebookData.choosenPlace.GetLatitude(), FacebookData.choosenPlace.GetLongitude(), FacebookData.choosenPlace.ID);
                String msg=String.Empty;
                if (checkin_msg != String.Empty)
                {
                    msg = String.Format("message={0}", checkin_msg.Replace("&", "%26"));
                }
      
                String link = String.Format("&link=http://www.facebook.com/pages/{0}/{1}/", (FacebookData.choosenPlace.Name.Replace(" ", "-")).Replace("&", String.Empty), FacebookData.choosenPlace.ID);                
                String tags = "&tags=" + uids;
                parameters = msg + link + picture + place + ((String.IsNullOrEmpty(uids)) ? String.Empty : tags);
                HttpWebRequest request = ar.AsyncState as HttpWebRequest;
                System.IO.Stream requestStream = request.EndGetRequestStream(ar);
                StreamWriter streamWriter = new StreamWriter(requestStream);
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(parameters);
                requestStream.Write(bytes, 0, bytes.Length);
                streamWriter.Flush();
                streamWriter.Close();
                request.BeginGetResponse(new AsyncCallback(CheckinResponseCallback), request);
            }

        }

        void api_EventCreated(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                //MessageBox.Show("Future Checkin Published");
                NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
            });
        }

        private void CheckinResponseCallback(IAsyncResult ar)
        {
            send.IsEnabled = true;
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    String msg = streamReader1.ReadToEnd();
                    this.Dispatcher.BeginInvoke(() =>
                     {
                         Regex regex = new Regex("\"id\":(\")?[0-9]+(\")?");
                         Match m = regex.Match(msg);
                         if (m.Success)
                         {
                             MessageBox.Show("Checkin published :)");
                         }
                         else
                         {
                             MessageBox.Show("Sorry, we couldn't publish the checkin at this time :(");
                         }

                     });
                }
            }
            catch (WebException e)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Sorry, we couldn't publish the checkin at this time :(");
                });
                // Utility.GetWebException(ex);
            }
            finally
            {
                FacebookData.choosenFriends = new List<long>();
                this.Dispatcher.BeginInvoke(() =>
                   {
                      NavigationService.Navigate(new Uri("/Events_Page.xaml?refresh=1", UriKind.Relative));
                 });
            }           
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (FacebookData.choosenFriends == null || FacebookData.choosenFriends.Count == 0)
            {
                tags.Visibility = Visibility.Collapsed;
            }
            else
            {
                tags.Text = FacebookData.choosenFriends.Count + " friends tagged.";
                tags.Visibility = Visibility.Visible;
            }
        }

        private void textBox1_LostFocus(object sender, RoutedEventArgs e)
        {
            chknMsg = checkinMsg.Text;
        }

        private void checkinMsg_GotFocus(object sender, RoutedEventArgs e)
        {
            if (checkinMsg.Text.Equals(checkinMsgTxt))
                ((TextBox)sender).Text = "";
            ((TextBox)sender).FontStyle = FontStyles.Normal;
            ((TextBox)sender).Foreground = new SolidColorBrush(Colors.Black);
        }

        private void checkinMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                chknMsg = checkinMsg.Text;
            }
        }

        private void checkinMsg_TextChanged(object sender, TextChangedEventArgs e)
        {
            chknMsg = checkinMsg.Text;
        }
    }
}