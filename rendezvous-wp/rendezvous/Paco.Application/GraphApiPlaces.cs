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
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Paco.Application
{
    public class GraphApiPlaces
    {
        public static string PlacesRequest = "https://graph.facebook.com/search?{0}&type=place&center={1},{2}&access_token={3}";
        public static string MoreRequest = "&limit={0}&offset={1}";
        public static Dictionary<string, Place> FoundPlaces = new Dictionary<string, Place>();

        public static ManualResetEvent allDone = new ManualResetEvent(false);        
        
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler Changed;

        public delegate void CitySearchEventHandler(object sender, EventArgs e);
        public event CitySearchEventHandler CitySearch;

        private int limit= 30;
        private int offset = 0;
        private Uri uri;
        String requested=String.Empty;
        bool _custom_city = false;

        string _query;
        string _latitude;
        string _longitue;
        string _accessToken;

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        protected virtual void OnCitySearch(EventArgs e)
        {
            if (CitySearch != null)
                CitySearch(this, e);
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

        public void GetPlaces(String query, String latitude, String longitude, String accessToken, bool newcity)
        {
            this._query = query;
            this._latitude = latitude;
            this._longitue = longitude;
            this._accessToken = accessToken;

            _custom_city = newcity;
            if (!String.IsNullOrEmpty(query))
            {
                query = "q=" + query;
            }
            GraphApiPlaces.FoundPlaces = new Dictionary<string, Place>();
            requested = String.Format(PlacesRequest, query, latitude, longitude, accessToken);
            uri = new Uri(requested +String.Format(MoreRequest,limit,offset));
            HttpWebRequest request =
                    WebRequest.Create(uri) as HttpWebRequest;
                IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
        }

        public void GetMoreResult()
        {
            if (!String.IsNullOrEmpty(requested))
            {
                offset += 30;
                limit += 30;
                uri = new Uri(requested + String.Format(MoreRequest, limit, offset));
                HttpWebRequest request =
                        WebRequest.Create(uri) as HttpWebRequest;
                IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
            }
        }
        private void ResponseCallback(IAsyncResult ar)
        {            
             HttpWebRequest request =(HttpWebRequest)ar.AsyncState;
             try
             {
                 HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                 using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                 {
                     string json = streamReader1.ReadToEnd();
                     Newtonsoft.Json.Linq.JObject o;
                     o = JObject.Parse(json);
                     JArray places = (JArray)o["data"];
                     for (int i = 0; i < places.Count; i++)
                     {
                         Place place = new Place(places[i]);

                             if (!FoundPlaces.ContainsKey(place.Name))
                             {
                                 FoundPlaces.Add(place.Name, place);
                             }

                     }
                 }
                 if (_custom_city)
                 {
                     OnCitySearch(EventArgs.Empty);
                 }
                 else
                 {
                     OnChanged(EventArgs.Empty);
                 }
             }
             catch (WebException)
             {
                 GetPlaces(_query, _latitude, _longitue, _accessToken, _custom_city);
             }
             catch (Exception e)
             {
                 string x = e.Message;
             }
        }        
    }
}
