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
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paco.Application
{
    public class GraphApiUser
    {
        public static string meRequest = "https://graph.facebook.com/me/?access_token={0}";
        public static string friendRequest = "https://graph.facebook.com/{1}/?access_token={0}";
        public static User userInfo;
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler Changed;

        // Invoke the Changed event; called whenever got the user info
        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        // Abort the request if the timer fires.
        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                {
                    request.Abort();
                }
            }
        }

        public void GetUserInfo(bool me,string uid, string accessToken)
        {
            Uri uri;
            if (me)
            {
                uri = new Uri(String.Format(meRequest, accessToken));
            }
            else
            {
                uri = new Uri(String.Format(friendRequest,uid, accessToken)); 
            }
            HttpWebRequest request =
                    WebRequest.Create(uri) as HttpWebRequest;
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
        }


        private void ResponseCallback(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            {
                string json = streamReader1.ReadToEnd();                
                Newtonsoft.Json.Linq.JObject o;
                o = JObject.Parse(json);
                userInfo = new User(o);
            }
            OnChanged(EventArgs.Empty);
        } 


    }
}
