using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace Paco.Application
{
    [DataContract(Name = "EventType", Namespace = "Paco.Application")]
    public enum EventType
    {
        [EnumMember()]
        OPEN,
        [EnumMember()]
        CLOSED
    }

    [DataContract(Name = "RSVP", Namespace = "Paco.Application")]
    public enum RSVP
    {        
        [EnumMember()]
        attending,
        [EnumMember()]
        declined,
        [EnumMember()]
        maybe,
        [EnumMember()]
        not_replied,
        [EnumMember()]
        unsure
    }
    public class GraphAPI
    {
        private readonly string Create_Event ="https://graph.facebook.com/me/events"; 
        private readonly string RSVP_Event = "https://graph.facebook.com/{0}/{1}?access_token={2}";
        public delegate void FBEventHandler(object sender, EventArgs e);
        public delegate void RSVPEventHandler(object sender, EventArgs e);
        public event FBEventHandler EventCreated;
        public event RSVPEventHandler RSVPSent;
        private string parameters;
        public String eid;
        String token;
        private string m_fileid = "uin.jpg";
        public string rsvp_reply;
        public bool rsvp_posted;
        public bool event_created;
        string _name=String.Empty;
        DateTime _start;
        public void CreateEvent(string token, String name, DateTime start, double end_time, String description, EventType type, Place place)
        {     
            double start_time = double.Parse(Utility.ConvertUTtcToEpoch(start.ToUniversalTime())); 
            _start = start;
            event_created=false;
            this.token = token;
            if (place!=null)_name = place.Name;
            if (String.IsNullOrEmpty(name))
            {
                if (!String.IsNullOrEmpty(place.Name))
                {
                    name = place.Name;
                    _name = name;
                }
                else
                {
                    name = "rendezvous event";
                }
            }
           
            
            String uri_format = String.Format("access_token={0}&name={1}&start_time={2}&privacy_type={3}&host={4}", token, name, start_time, (type.Equals(EventType.CLOSED))?"SECRET":type.ToString(), FacebookData.CurrentUser.Name);      
            if (end_time != 0)
            {
                uri_format+="&end_time="+end_time;
            }
            if (!String.IsNullOrEmpty(description))
            {
                uri_format += "&description=" +HttpUtility.HtmlEncode(description);
            }
            try
            {
                uri_format += "&latitude=" + place.GetLatitude();
                uri_format += "&longitude=" + place.GetLongitude();
            }                
            catch (NullReferenceException) { //Ignore the latitude and longitude if they are null
            }
            if (place != null)
            {
                if (!String.IsNullOrEmpty(place.GetCity())) uri_format += "&city=" + place.GetCity();
                if (!String.IsNullOrEmpty(place.GetState())) uri_format += "&state=" + place.GetState();
                if (!String.IsNullOrEmpty(place.GetStreet())) uri_format += "&street=" + place.GetStreet();
                uri_format += "&location=" + place.Name;
            }
            parameters = uri_format;
            Uri uri = new Uri(Create_Event+"?access_token="+token);
            HttpWebRequest request =WebRequest.Create(uri) as HttpWebRequest;
            request.Method = "POST";
            request.BeginGetRequestStream(new AsyncCallback(RequestCallback), request);
        }


        public void RSVPEvent(string token, long eid, RSVP rsvp)
        {
            rsvp_reply = rsvp.ToString();
            rsvp_posted = false;
            Uri uri = new Uri(String.Format(RSVP_Event, eid,rsvp,token));
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = "POST";
            parameters = String.Format("access_token={0}", token);
            request.BeginGetRequestStream(new AsyncCallback(RSVPRequestCallback), request);
        }

        private void RequestCallback(IAsyncResult ar)
        {
            HttpWebRequest request = ar.AsyncState as HttpWebRequest;
            System.IO.Stream requestStream = request.EndGetRequestStream(ar);
            StreamWriter streamWriter = new StreamWriter(requestStream);            
            System.Text.StringBuilder headerBuilder = new System.Text.StringBuilder(1024);
            headerBuilder.Append(parameters);           

            string header = headerBuilder.ToString();
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            request.ContentType = "form-data";
            requestStream.Write(headerBytes, 0, headerBytes.Length);
            byte[] buffer = new byte[1024];
            streamWriter.Flush();
            streamWriter.Close();
            request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
        }

        private void RSVPRequestCallback(IAsyncResult ar)
        {
            HttpWebRequest request = ar.AsyncState as HttpWebRequest;
            System.IO.Stream requestStream = request.EndGetRequestStream(ar);
            StreamWriter streamWriter = new StreamWriter(requestStream);
            System.Text.StringBuilder headerBuilder = new System.Text.StringBuilder(1024);
            headerBuilder.Append(parameters);

            string header = headerBuilder.ToString();
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            request.ContentType = "form-data";
            requestStream.Write(headerBytes, 0, headerBytes.Length);
            byte[] buffer = new byte[1024];
            streamWriter.Flush();
            streamWriter.Close();
            request.BeginGetResponse(new AsyncCallback(RSVPResponseCallback), request);
        }

        private void ResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string line = streamReader1.ReadToEnd();
                    Regex regex = new Regex("(?<eid>[0-9]+)");
                    Match m = regex.Match(line);
                    if (m.Success)
                    {
                        event_created = true;
                        eid = m.Groups["eid"].Value;
                        string uids = String.Empty;
                        if (FacebookData.choosenFriends != null && FacebookData.choosenFriends.Count > 0)
                        {
                            //Saving the recent contacts on the phone
                            AppSettings.SaveRecentFriends(FacebookData.choosenFriends);
                            if (FacebookData.choosenPlace!=null) AppSettings.SaveRecentPlaces(FacebookData.RecentPlaces);                            
                            for (int i = 0; i < FacebookData.choosenFriends.Count; i++)
                            {
                                uids += FacebookData.choosenFriends[i] + ",";
                            }
                            response.Close();
                            response.Dispose();
                            if (!string.IsNullOrEmpty(uids))
                            {
                                uids = uids.Remove(uids.Length - 1);
                                Uri myUri = new Uri(String.Format("https://api.facebook.com/method/events.invite?access_token={0}&uids={2}&eid={1}", token, eid, uids));
                                HttpWebRequest request2 = WebRequest.Create(myUri) as HttpWebRequest;
                                IAsyncResult asyncResult = request2.BeginGetResponse(new AsyncCallback(InviteResponseCallback), request2);
                            }
                        }
                        OnFBEvent(EventArgs.Empty);
                        Uri editUri = new Uri("https://api.facebook.com/method/events.edit");
                        HttpWebRequest request3 = WebRequest.Create(editUri) as HttpWebRequest;
                        request3.Method = "POST";
                        request3.BeginGetRequestStream(new AsyncCallback(EventEditRequestCallback), request3);
                    }
                    else
                    {
                        eid = String.Empty;
                    }
                }
            }
            catch (WebException ex)
            {
                string x = ex.Message;
                eid = String.Empty;
                OnFBEvent(EventArgs.Empty);
            }              
        }

        private void RSVPResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string line = streamReader1.ReadToEnd();
                    Regex regex = new Regex("true");
                    Match m = regex.Match(line);
                    if (m.Success)
                    {
                        rsvp_posted = true;
                        rsvp_reply = String.Format("Status changed to {0} :)", rsvp_reply);
                    }
                    else
                    {
                        rsvp_reply = String.Format("Couldn't change your status to {0} at this time :(", rsvp_reply);
                    }
                }
            }
            catch (WebException ex)
            {
                rsvp_reply = String.Format("Couldn't change your status to {0} at this time :(", rsvp_reply);
            }
            OnRSVPSent(EventArgs.Empty);
        }

        private void InviteResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                { string x = streamReader1.ReadToEnd(); }

                Uri myUri = new Uri(String.Format("https://graph.facebook.com/{0}/feed?access_token={1}", eid, token));
                HttpWebRequest request2 = WebRequest.Create(myUri) as HttpWebRequest;
                request2.Method = "POST";
                IAsyncResult asyncResult = request2.BeginGetRequestStream(new AsyncCallback(WallRequestCallback), request2);
            }
            catch (WebException ex)
            {
                Utility.GetWebException(ex);
            }
        }

        private void WallRequestCallback(IAsyncResult ar)
        {
            HttpWebRequest request = ar.AsyncState as HttpWebRequest;
            System.IO.Stream requestStream = request.EndGetRequestStream(ar);
            StreamWriter streamWriter = new StreamWriter(requestStream);
            System.Text.StringBuilder headerBuilder = new System.Text.StringBuilder(1024);
            headerBuilder.Append(String.Format("message=Let's rendezvous @ {0}, {1}!",_name,_start));

            string header = headerBuilder.ToString();
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            request.ContentType = "form-data";
            requestStream.Write(headerBytes, 0, headerBytes.Length);
            byte[] buffer = new byte[1024];
            streamWriter.Flush();
            streamWriter.Close();
            request.BeginGetResponse(new AsyncCallback(WallResponseCallback), request);
        }


        private void WallResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    String x = streamReader1.ReadToEnd();
                }
            }
            catch (Exception) { }
        }

        private void EventEditRequestCallback(IAsyncResult ar)
        {
            HttpWebRequest request = ar.AsyncState as HttpWebRequest;
            System.IO.Stream requestStream = request.EndGetRequestStream(ar);
            StreamWriter streamWriter = new StreamWriter(requestStream);           
            System.Text.StringBuilder headerBuilder = new System.Text.StringBuilder(1024);
            
            string boundary = new Guid().ToString("D");
           
            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");
            headerBuilder.Append("Content-Disposition: form-data; name=\"");
            headerBuilder.Append("access_token");
            headerBuilder.Append("\"\r\n\r\n");
            headerBuilder.Append(token);
            headerBuilder.Append("\r\n");

            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");
            headerBuilder.Append("Content-Disposition: form-data; name=\"");
            headerBuilder.Append("eid");
            headerBuilder.Append("\"\r\n\r\n");
            headerBuilder.Append(eid);
            headerBuilder.Append("\r\n");

            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");

            headerBuilder.Append("Content-Disposition: form-data; name=\"");
            headerBuilder.Append("event_info");
            headerBuilder.Append("\"\r\n\r\n");
            headerBuilder.Append("{}");          
            headerBuilder.Append("\r\n");
            
            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");
            headerBuilder.Append("Content-Disposition: form-data; filename=\"");
            headerBuilder.Append("uin.jpg");
            headerBuilder.Append("\"\r\n");
            headerBuilder.Append("Content-Type: ");
            headerBuilder.Append("image/jpg");
            headerBuilder.Append("\r\n\r\n");

            System.IO.IsolatedStorage.IsolatedStorageFileStream stream;
            using (System.IO.IsolatedStorage.IsolatedStorageFile isoFile = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                stream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(m_fileid, FileMode.Open, isoFile);
                //stream = new System.IO.IsolatedStorage.IsolatedStorageFileStream("ApplicationIcon.png", FileMode.Open, isoFile);
            }

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
            request.BeginGetResponse(new AsyncCallback(EventEditResponseCallback), request);
        }

        private void EventEditResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    String x = streamReader1.ReadToEnd();
                }
            }
            catch (Exception) { }
        }

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnFBEvent(EventArgs e)
        {
            if (EventCreated != null)
                EventCreated(this, e);
        }

        protected virtual void OnRSVPSent(EventArgs e)
        {
            if (RSVPSent != null)
                RSVPSent(this, e);
        }
    }
}
