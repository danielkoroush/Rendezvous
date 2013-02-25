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
using System.Device.Location;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paco.Application
{
    public class GeoLocationSearch
    {
        string _request = "http://maps.googleapis.com/maps/api/geocode/json?address={0}&sensor=false";
        GeoCoordinate _geoCoordinate;
        public delegate void GeoLocationHandler(object sender, EventArgs e);
        public event GeoLocationHandler GeoLocationFound;

        public GeoLocationSearch(String location)
        {
            _request = String.Format(_request, location);
        }
        // Invoke the Changed event; called whenever list changes
        protected virtual void OnGeoLocationEvent(EventArgs e)
        {
            if (GeoLocationFound != null)
                GeoLocationFound(this, e);
        }

        public void GetGeoCoordinate()
        {
            _geoCoordinate = new GeoCoordinate();
            Uri uri = new Uri(_request);
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
        }

        private void ResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string json = streamReader1.ReadToEnd();
                    JObject o = JObject.Parse(json);
                    string g = o["status"].ToString();
                    JArray result = JArray.Parse(o["results"].ToString());
                    JObject something = JObject.Parse(result[0].ToString());
                    JObject geometry = JObject.Parse(something["geometry"].ToString());
                    JObject location = JObject.Parse(geometry["location"].ToString());
                    double lat = (double)location["lat"];
                    double lng = (double)location["lng"];
                    _geoCoordinate = new GeoCoordinate(lat, lng);
                  //  String geometry=result["geometry"].ToString();
                   // JObject geometry = JObject.Parse(result["geometry"].ToString());
                }
                
            }
            catch (Exception e) {string x = e.Message; }
            OnGeoLocationEvent(EventArgs.Empty);
        }

        public GeoCoordinate City
        {
            get
            {
                return _geoCoordinate;
            }
        }
    }
}
