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

namespace Paco.Application
{
    public class Groupon
    {
        static readonly String CLIENT_ID = "e5dcea00842bce098ef3818fbfc79f4d35cc24c6";
        static readonly string REFERAL_ID = "&referral_id=uu34410658";
        static readonly String DEALS_REQUEST = "http://api.groupon.com/v2/deals.json?client_id={0}&lat={1}&lng={2}" + REFERAL_ID;
        static readonly String DEAL_DETAILS_REQUEST = "http://api.groupon.com/v2/deals/{0}.json?client_id={1}";

        public static List<Deal> Deals = new List<Deal>();

        public delegate void DealsFinshedEventHandler(object sender, EventArgs e);
        public event DealsFinshedEventHandler Finished;

        public delegate void DetailsFinshedEventHandler(object sender, EventArgs e);
        public event DetailsFinshedEventHandler DetailsFinished;

        protected virtual void OnDealsFinished(EventArgs e)
        {
            if (Finished != null)
                Finished(this, e);
        }

        protected virtual void OnDetailsFinished(EventArgs e)
        {
            if (DetailsFinished != null)
                DetailsFinished(this, e);
        }

        public void getDeals(String lat, String lng)
        {
            Uri uri = new Uri(String.Format(DEALS_REQUEST, CLIENT_ID, lat, lng));
            Deals = new List<Deal>();
            HttpWebRequest request =
                    WebRequest.Create(uri) as HttpWebRequest;
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
                    Newtonsoft.Json.Linq.JObject o;
                    o = JObject.Parse(json);
                    JArray deals = (JArray)o["deals"];
                    foreach (JToken deal in deals)
                    {
                        bool isSoldOut = (bool)deal["isSoldOut"];
                        if (!isSoldOut)
                        {
                            Deals.Add(new Deal(deal));
                        }
                    }
                    Deals.Sort();
                }
            }
            catch (Exception) { }
            OnDealsFinished(EventArgs.Empty);
        }


        public void getDealDetails(String deal)
        {
            Uri uri = new Uri(String.Format(DEAL_DETAILS_REQUEST, deal, CLIENT_ID));
            HttpWebRequest request =
                    WebRequest.Create(uri) as HttpWebRequest;
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(DetailsResponseCallback), request);
        }


        private void DetailsResponseCallback(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            {
                string json = streamReader1.ReadToEnd();
                Newtonsoft.Json.Linq.JObject o;
                o = JObject.Parse(json);
            }
            OnDealsFinished(EventArgs.Empty);
        }
    }
}
