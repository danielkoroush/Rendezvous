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
using Microsoft.Phone.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paco.Application;
using System.Text;

namespace You.In
{
    public enum PublishType
    {
        Event = 0,
        FutureCheckin = 1,
        Checkin = 2        
    };
    public partial class PivotPage1 : PhoneApplicationPage
    {
        //key is uid, value is full name of user
        public Dictionary<string, string> friends = new Dictionary<string, string>();
        //PhoneNumberChooserTask phoneNumberChooserTask;
        private static String eid = String.Empty;
        private DateTime dateTime;
        private User currentUser;
        private double EpochTime;
        private Place choosePlace;
        private bool imageloaded = false;
        private PublishType eventType;
        private WebClient m_webClient = new WebClient();
        // URL for sample image
        private Uri m_uri = new Uri("http://img402.imageshack.us/img402/9158/eventb.jpg");
        // Local file name
        private string m_fileid = "uin.jpg";

        public PivotPage1()
        {
            InitializeComponent();
            //phoneNumberChooserTask = new PhoneNumberChooserTask();
            //phoneNumberChooserTask.Completed += new EventHandler<PhoneNumberResult>(phoneNumberChooserTask_Completed);
        }

        void phoneNumberChooserTask_Completed(object sender, PhoneNumberResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                textBox1.Text = e.PhoneNumber;
            }
        }

        private void AddFriends()
        {
            this.Dispatcher.BeginInvoke(() =>
       {
           foreach (string name in friends.Keys)
           {
               listBox1.Items.Add(name);
           }
       });
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

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
                        eid = m.Groups["eid"].Value;
                        string uids = String.Empty;
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            for (int i = 0; i < listBox1.SelectedItems.Count; i++)
                            {
                                uids += friends[listBox1.SelectedItems[i].ToString()] + ",";
                            }
                            response.Close();
                            response.Dispose();
                            if (!string.IsNullOrEmpty(uids))
                            {
                                uids = uids.Remove(uids.Length - 1);
                                Uri myUri = new Uri(String.Format("https://api.facebook.com/method/events.invite?access_token={0}&uids={2}&eid={1}", AppSettings.TokenSetting, eid, uids));
                                HttpWebRequest request2 = WebRequest.Create(myUri) as HttpWebRequest;
                                IAsyncResult asyncResult = request2.BeginGetResponse(new AsyncCallback(ResponseCallback3), request2);
                            }
                        });
                    }
                }
            }
            catch (WebException ex)
            {
                GetWebException(ex);
            }
        }

        /// <summary>
        /// display the list of friends
        /// </summary>
        /// <param name="ar"></param>
        private void ResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string line;
                    while ((line = streamReader1.ReadLine()) != null)
                    {
                        Regex nameRegex = new Regex("\"name\":\"(?<name>[^\"]+)\"");
                        Regex idRegex = new Regex("\"id\":\"(?<id>[0-9]+)\"");
                        MatchCollection m1 = nameRegex.Matches(line);
                        MatchCollection m2 = idRegex.Matches(line);
                        for (int i = 0; i < m1.Count; i++)
                        {
                            friends.Add(m1[i].Groups["name"].Value, m2[i].Groups["id"].Value);
                        }
                    }
                    AddFriends();
                }
            }
            catch (WebException ex)
            {
                GetWebException(ex);
            }

        }

        public static void GetWebException(WebException ex)
        {
            // To help with debugging, we grab the exception stream to get full error details
            StreamReader errorStream = null;
            try
            {
                errorStream = new StreamReader(ex.Response.GetResponseStream());
                Console.WriteLine(errorStream.ReadToEnd());
            }
            finally
            {
                if (errorStream != null) errorStream.Close();
            }            
        }

        private void ResponseCallback3(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                { string x = streamReader1.ReadToEnd(); }
            }
            catch (WebException ex)
            {
                GetWebException(ex);
            }
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// selecting and de-selecting friends
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (object o in e.AddedItems)
            {
                listBox2.Items.Add(o);
            }
            foreach (object o in e.RemovedItems)
            {
                listBox2.Items.Remove(o);
            }
        }


        private void button2_Click(object sender, RoutedEventArgs e)
        {
            String date = datePicker1.ValueString;
            String time = timePicker1.ValueString;
            dateTime = DateTime.Parse(String.Format("{0} {1}", date, time));
            EpochTime = double.Parse(Utility.ConvertUTtcToEpoch(dateTime.ToUniversalTime()));

            if (listBox3.SelectedItem != null)
            {
                choosePlace = GraphApiPlaces.FoundPlaces[listBox3.SelectedItem.ToString()];
            }
            if (radioButton2.IsChecked.Value)
            {
                eventType = PublishType.FutureCheckin;                
                HttpWebRequest request = WebRequest.Create(new Uri(String.Format("https://graph.facebook.com/me/checkins?access_token={0}",AppSettings.TokenSetting))) as HttpWebRequest;
                request.Method = "POST";
                IAsyncResult asyncResult = request.BeginGetRequestStream(new AsyncCallback(CheckinRequestCallback), request);
            }
            else if (radioButton1.IsChecked.Value)
            {                
                eventType = PublishType.Checkin;
                HttpWebRequest request = WebRequest.Create(new Uri(String.Format("https://graph.facebook.com/me/checkins?access_token={0}", AppSettings.TokenSetting))) as HttpWebRequest;
                request.Method = "POST";
                IAsyncResult asyncResult = request.BeginGetRequestStream(new AsyncCallback(CheckinRequestCallback), request);
            }
            else
            {
                eventType = PublishType.Event;
                HttpWebRequest request = WebRequest.Create(new Uri(String.Format("https://api.facebook.com/method/events.create?access_token={0}", AppSettings.TokenSetting))) as HttpWebRequest;
                request.Method = "POST";
                IAsyncResult asyncResult = request.BeginGetRequestStream(new AsyncCallback(RequestCallback), request);
            }
        }

        /// <summary>
        /// Gets user info and sets it to the User object for logged in user
        /// </summary>
        private void GetUserInfo()
        {         
            HttpWebRequest request = WebRequest.Create(new Uri(String.Format("https://graph.facebook.com/me/?access_token={0}", AppSettings.TokenSetting))) as HttpWebRequest;
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(UserInfoResponseCallback), request);                
        }

        private void UserInfoResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    Newtonsoft.Json.Linq.JObject o;
                    o = JObject.Parse(streamReader1.ReadToEnd());
                    currentUser = new User(o);
                }
            }
            catch (WebException ex)
            {
                GetWebException(ex);
            }
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
            for (int i = 0; i < listBox1.SelectedItems.Count; i++)
            {
                friend += listBox1.SelectedItems[i].ToString() + ",";
                uids+=friends[listBox1.SelectedItems[i].ToString()] + ",";
            }
            if (!String.IsNullOrEmpty(friend))
            {
                friend = " With " + friend.Remove(friend.Length - 1) + "!";
                uids = uids.Remove(uids.Length - 1);
            }
            String picture = "&picture=http://img402.imageshack.us/img402/9158/eventb.jpg";
            if (eventType==PublishType.FutureCheckin)
            {
                String date = datePicker1.ValueString;
                String time = timePicker1.ValueString;
                DateTime dateTime = DateTime.Parse(String.Format("{0} {1}", date, time));
                double EpochTime = double.Parse(Utility.ConvertUTtcToEpoch(dateTime.ToUniversalTime()));
                String msg = String.Format("message=Heading to {0} @ {1}!", FacebookData.choosenPlace.Name, Utility.ConvertEpochToUtc(EpochTime).AddHours(-8)) + friend;
                String link = String.Format("&link=http://www.facebook.com/pages/{0}/{1}", FacebookData.choosenPlace.Name.Replace(" ", "-"), FacebookData.choosenPlace.ID);
                String place = String.Format("&coordinates={{\"latitude\":\"{0}\", \"longitude\": \"{1}\"}}&place={2}", FacebookData.choosenPlace.GetLatitude(), FacebookData.choosenPlace.GetLongitude(), FacebookData.choosenPlace.ID);
                parameters = msg + link + picture + place;
            }
            else
            {
                String place = String.Format("&coordinates={{\"latitude\":\"{0}\", \"longitude\": \"{1}\"}}&place={2}", FacebookData.choosenPlace.GetLatitude(), FacebookData.choosenPlace.GetLongitude(), FacebookData.choosenPlace.ID);
                String msg = String.Format("message=is at {0}!", FacebookData.choosenPlace.Name.Replace("&", "%26")) + friend;
                String link = String.Format("&link=http://www.facebook.com/pages/{0}/{1}/", (FacebookData.choosenPlace.Name.Replace(" ", "-")).Replace("&", String.Empty), FacebookData.choosenPlace.ID);
                String tags = "&tags=" + uids;
                parameters = msg + link + picture + place + ((String.IsNullOrEmpty(uids)) ? String.Empty : tags);


            }




            HttpWebRequest request = ar.AsyncState as HttpWebRequest;
            System.IO.Stream requestStream = request.EndGetRequestStream(ar);
            StreamWriter streamWriter = new StreamWriter(requestStream);
            request.ContentType = "application/x-www-form-urlencoded";            
            
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(parameters);            
            requestStream.Write(bytes, 0, bytes.Length);                 
            streamWriter.Flush();
            streamWriter.Close();
            request.BeginGetResponse(new AsyncCallback(ResponseCallback3), request);
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
            headerBuilder.Append("{\"name\":\"Project P\",\"start_time\":\"" + EpochTime + "\",\"privacy_type\":\"SECRET\"," + choosePlace.WebRequestMethod() + ",\"category\":\"1\",\"subcategory\":\"1\",\"host\":\"" + currentUser.Name + "\",\"tagline\":\"Created by Jamm!\"}");
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


        private void button3_Click(object sender, RoutedEventArgs e)
        {

        }


        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                //get the list of friends from FB, passing in access token
                HttpWebRequest request = WebRequest.Create(new Uri(String.Format("https://graph.facebook.com/me/friends?access_token={0}", AppSettings.TokenSetting))) as HttpWebRequest;
                IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
            }
            if (!imageloaded)
            {
                //this is the image that gets used when you create a new event
                m_webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
                m_webClient.OpenReadAsync(m_uri);
            }
            if (currentUser == null)
            {
                GetUserInfo();
            }
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

        /// <summary>
        /// search for places
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            GraphApiPlaces p = new GraphApiPlaces();
            p.Changed += new GraphApiPlaces.ChangedEventHandler(p_Changed);
            //p.GetPlaces(textBox1.Text, "47.612", "-122.34", "1000", AppSettings.TokenSetting);
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
                listBox3.Items.Clear();
                foreach (string name in GraphApiPlaces.FoundPlaces.Keys)
                {
                    listBox3.Items.Add(name);
                }
            });
        }

        /// <summary>
        /// get users Events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            //GraphApiEvents ec = new GraphApiEvents();
            //ec.GetEvents(currentUser.Uid.ToString(), AppSettings.TokenSetting);
            //ec.Finished += new GraphApiEvents.FBEventHandler(ec_Finished);
        }

        void ec_Finished(object sender, EventArgs e)
        {            
            //this.Dispatcher.BeginInvoke(() =>
            //{
            //    listBox4.Items.Clear();
            //    foreach (long eid in GraphApiEvents.myEvents.Keys)
            //    {
            //        listBox4.Items.Add(GraphApiEvents.myEvents[eid].ToString());
            //    }
            //});
        }
    }
}