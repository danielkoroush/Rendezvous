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
    public partial class Create2 : PhoneApplicationPage
    {
        private static  string _place_name = String.Empty;
        private static string _invitees = String.Empty;
        private bool imageloaded = false;
        private WebClient m_webClient = new WebClient();
        // URL for sample image
        private Uri m_uri = new Uri("http://img525.imageshack.us/img525/3014/applicationicon.png");
        // Local file name
        private string m_fileid = "uin.jpg";
        private double EpochTime;
        DateTime dateTime;
        ApplicationBarIconButton newEvent;
        GraphAPI graph = new GraphAPI();
        static readonly string _title_msg= "event title...";
        static readonly string _description_msg = "add event details (optional)...";
        static string _title=_title_msg;
        static string _descirption=_description_msg;

        static DateTime _startDate = DateTime.Today.AddDays(1);
        static DateTime _startTime = DateTime.Now;

        public Create2()
        {
            InitializeComponent();
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1;

            //FacebookData.choosenFriends = new List<long>();
            ApplicationBarIconButton cancelled = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative));
            cancelled.Text = "cancel";
            cancelled.Click += new EventHandler(cancelled_Click);

            newEvent = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative));
            newEvent.Text = "save";
            newEvent.Click += new EventHandler(newEvent_Click);

            ApplicationBar.Buttons.Add(newEvent);
            ApplicationBar.Buttons.Add(cancelled);
            datePicker1.Value = _startDate;
            timePicker1.Value = _startTime;
        }

        void cancelled_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        void newEvent_Click(object sender, EventArgs e)
        {
            String date = datePicker1.ValueString;
            String time = timePicker1.ValueString;
            dateTime = DateTime.Parse(String.Format("{0} {1}", date, time));
            EpochTime = double.Parse(Utility.ConvertUTtcToEpoch(dateTime.ToUniversalTime()));
            newEvent.IsEnabled = false;
            graph.EventCreated += new GraphAPI.FBEventHandler(graph_EventCreated);
            //graph.CreateEvent(AppSettings.TokenSetting, EventTitle.Text, EpochTime, 0, EventDescription.Text, Public.IsChecked.Value?EventType.OPEN:EventType.CLOSED, FacebookData.choosenPlace);
            _title = _title_msg;
            _descirption = _description_msg;
            FacebookData.choosenFriends = new List<long>();
            FacebookData.choosenPlace = null;
        }

        void graph_EventCreated(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(graph.eid);
                NavigationService.Navigate(new Uri("/Events_Page.xaml", UriKind.Relative));
            });
        }

        /// <summary>
        /// Post the Event Data for Facebook REST server
        /// </summary>
        /// <param name="ar"></param>
        private void RequestCallback(IAsyncResult ar)
        {
            HttpWebRequest request = ar.AsyncState as HttpWebRequest;
            System.IO.Stream requestStream = request.EndGetRequestStream(ar);
            StreamWriter streamWriter = new StreamWriter(requestStream);
            EpochTime = double.Parse(Utility.ConvertUTtcToEpoch(dateTime));
            System.Text.StringBuilder headerBuilder = new System.Text.StringBuilder(1024);
            string boundary = new Guid().ToString("D");
            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");
            headerBuilder.Append("Content-Disposition: form-data; name=\"");
            headerBuilder.Append("access_token");
            headerBuilder.Append("\"\r\n\r\n");
            headerBuilder.Append(AppSettings.TokenSetting);
            headerBuilder.Append("\r\n");

            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");
            headerBuilder.Append("Content-Disposition: form-data; name=\"");
            headerBuilder.Append("event_info");
            headerBuilder.Append("\"\r\n\r\n");
            
            headerBuilder.Append("{\"name\":\""+_title+"\",\"start_time\":\"" + EpochTime + "\",\"privacy_type\":\"SECRET\"," + FacebookData.choosenPlace.WebRequestMethod() + ",\"category\":\"1\",\"subcategory\":\"1\",\"host\":\"" + FacebookData.CurrentUser.Name + "\",\"tagline\":\"Created by Jamm!\"}");
            headerBuilder.Append("\r\n");

            System.IO.IsolatedStorage.IsolatedStorageFileStream stream;
            using (System.IO.IsolatedStorage.IsolatedStorageFile isoFile = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                stream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(m_fileid, FileMode.Open, isoFile);
            }
            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");
            headerBuilder.Append("Content-Disposition: form-data; filename=\"");
            headerBuilder.Append("uin.jpg");
            headerBuilder.Append("\"\r\n");
            headerBuilder.Append("Content-Type: ");
            headerBuilder.Append("image/jpg");
            headerBuilder.Append("\r\n\r\n");

            string header = headerBuilder.ToString();
            string footer = "\r\n--" + boundary;

            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            byte[] footerBytes = Encoding.UTF8.GetBytes(footer);

            request.ContentType = "multipart/form-data; boundary=" + boundary;

            requestStream.Write(headerBytes, 0, headerBytes.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = 0;

            while ((bytesRead = stream.Read(buffer, 0, 1024)) != 0)
            {
                requestStream.Write(buffer, 0, bytesRead);
            }
            requestStream.Write(footerBytes, 0, footerBytes.Length);
            streamWriter.Flush();
            streamWriter.Close();
            stream.Close();
            stream.Dispose();
            request.BeginGetResponse(new AsyncCallback(ResponseCallback2), request);
        }


        /// <summary>
        /// Get the response back from Facebook after creating the event including the eid and inviting friends after
        /// </summary>
        /// <param name="ar"></param>
        private void ResponseCallback2(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string line = streamReader1.ReadToEnd();
                    Regex regex = new Regex(">(?<eid>[0-9]+)<");
                    Match m = regex.Match(line);

                    if (m.Success)
                    {
                        String eid = m.Groups["eid"].Value;
                        ShowFeedBack(eid);
                        string uids = String.Empty;
                        //this.Dispatcher.BeginInvoke(() =>
                        //{
                        //    for (int i = 0; i < listBox1.SelectedItems.Count; i++)
                        //    {
                        //        uids += friends[listBox1.SelectedItems[i].ToString()] + ",";
                        //    }
                        //    response.Close();
                        //    response.Dispose();
                        //    if (!string.IsNullOrEmpty(uids))
                        //    {
                        //        uids = uids.Remove(uids.Length - 1);
                        //        Uri myUri = new Uri(String.Format("https://api.facebook.com/method/events.invite?access_token={0}&uids={2}&eid={1}", AppSettings.TokenSetting, eid, uids));
                        //        HttpWebRequest request2 = WebRequest.Create(myUri) as HttpWebRequest;
                        //        IAsyncResult asyncResult = request2.BeginGetResponse(new AsyncCallback(ResponseCallback3), request2);
                        //    }
                        //});
                    }
                    else
                    {
                        MessageBox.Show(line);
                    }
                }
            }
            catch (WebException ex)
            {
               ShowFeedBack(ex.Message);
                Utility.GetWebException(ex);
            }
          
            this.Dispatcher.BeginInvoke(() =>
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                });
            
        }

        private void ShowFeedBack(String msg)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(msg);
            });
        }

        private void endTime_Click(object sender, RoutedEventArgs e)
        {
            this.Focus();
            endTime.Visibility = Visibility.Collapsed;
            EventEnd.Visibility = Visibility.Visible;            
        }

        private void SaveInfo()
        {
            _startDate = (DateTime) datePicker1.Value;
            _startDate = (DateTime) timePicker1.Value; 
            _title = EventTitle.Text;
            _descirption = EventDescription.Text;
        }

        private void Places_GotFocus(object sender, RoutedEventArgs e)
        {
            SaveInfo();
            NavigationService.Navigate(new Uri("/ChoosePlace.xaml", UriKind.Relative));
        }

        private void Invitees_GotFocus(object sender, RoutedEventArgs e)
        {
            SaveInfo();
            //NavigationService.Navigate(new Uri("/ChooseInvitees.xaml", UriKind.Relative));
            NavigationService.Navigate(new Uri("/FriendsChooser_Page.xaml", UriKind.Relative));
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
            if (!String.IsNullOrEmpty(_invitees))
            {
                InviteesBox.Text = String.Format("{0} Friends Selected.",_invitees);
                InviteesBox.Background = new SolidColorBrush(Colors.Black);
                InviteesBox.Foreground = new SolidColorBrush(Colors.Red);
            }
            EventTitle.Text = _title;
            EventDescription.Text = _descirption;
        }

        /// <summary>
        /// loading the event image and saving it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
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

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
               
            }
        }

        private void datePicker1_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            SaveInfo();
        }
    }

}