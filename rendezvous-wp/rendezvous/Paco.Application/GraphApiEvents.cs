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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Paco.Application
{
    public class GraphApiEvents
    {
        /*

        public static Dictionary<long, User> Invitees = new Dictionary<long, User>();
        
        //private static String EventRequest = "https://api.facebook.com/method/fql.multiquery?format=json&queries={{\"rsvps\":\"SELECT eid,rsvp_status,uid FROM event_member WHERE eid in (SELECT eid FROM event_member WHERE uid={0})\",\"events\":\"SELECT eid,name,host,start_time,end_time,location,venue,pic_big,description FROM event WHERE eid in (SELECT eid FROM event_member WHERE uid={0})\", \"friends\":\"SELECT name,uid,pic_square FROM user WHERE uid in (SELECT uid FROM event_member WHERE eid in (SELECT eid FROM event_member WHERE uid={0}))\"}}&access_token={1}";
        private static String EventRequest = "https://api.facebook.com/method/fql.multiquery?format=json&queries={{\"rsvps\":\"SELECT eid,rsvp_status,uid FROM event_member WHERE eid in (SELECT eid FROM event WHERE eid in (SELECT eid FROM event_member WHERE uid=632062845) AND start_time>1291006017) AND uid in (SELECT uid2 FROM friend WHERE uid1 = 632062845)\", \"events\":\"SELECT eid,name,host,start_time,end_time,location,venue,pic_big,description FROM event WHERE eid in (SELECT eid FROM event_member WHERE uid={0}) AND start_time>{2}\", \"friends\":\"SELECT name,uid,pic_square FROM user WHERE uid in (SELECT uid2 FROM friend WHERE uid1 = {0})\"}}&access_token={1}";

        //https://api.facebook.com/method/fql.multiquery?format=json&queries={"rsvps":"SELECT eid,rsvp_status,uid FROM event_member WHERE eid in (SELECT eid FROM event WHERE eid in (SELECT eid FROM event_member WHERE uid=6320628450 AND start_time>1291006017)"}&access_token=156960027671102|cd771853cce1f4d555d75e47-632062845|qBRSshIH3-HOS2LD5NU8EmUFK8M
      //  {"rsvps":"SELECT eid,rsvp_status,uid FROM event_member WHERE eid in (SELECT eid FROM event WHERE eid in "}
        public delegate void FBEventHandler(object sender, EventArgs e);
        public event FBEventHandler Finished;

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnFBEvent(EventArgs e)
        {
            if (Finished != null)
                Finished(this, e);
        }

        public void GetEvents(string uid, String accessToken)
        {
           // Uri uri = new Uri(String.Format(EventRequest,uid,accessToken));
            Uri uri = new Uri(String.Format(EventRequest, uid, accessToken,String.Format("{0:##}",Utility.ConvertUTtcToEpoch(DateTime.Now.ToUniversalTime())) ));
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
                Newtonsoft.Json.Linq.JArray o = JArray.Parse(json);
                JArray EventJson=null;
                JArray FriendsJson=null;
                JArray RSVPSJson=null;
                foreach (JToken j in o)
                {
                    JObject j1 = JObject.Parse(j.ToString());
                    if (j1["name"].ToString().Equals("\"friends\"", StringComparison.InvariantCultureIgnoreCase))
                    {
                        FriendsJson = (JArray)j1["fql_result_set"];
                          //  ParseUsers((JArray)j1["fql_result_set"]);                                                   
                    }
                    else if (j1["name"].ToString().Equals("\"events\"", StringComparison.InvariantCultureIgnoreCase))
                    {
                        EventJson = (JArray)j1["fql_result_set"];
                        //ParseEvents((JArray)j1["fql_result_set"]);
                    }
                    else if (j1["name"].ToString().Equals("\"rsvps\"", StringComparison.InvariantCultureIgnoreCase))
                    {
                        RSVPSJson = (JArray)j1["fql_result_set"];
                      //  ParseRSVPS((JArray)j1["fql_result_set"]);
                    }
                }
                ParseEvents(EventJson);
                ParseUsers(FriendsJson);
                ParseRSVPS(RSVPSJson);

            }
            OnFBEvent(EventArgs.Empty);
        }

        private void ParseUsers(JArray json)
        {
            foreach (JToken jtoken in json)
            {
                long uid = (long)jtoken["uid"];
                if (!Invitees.ContainsKey(uid))
                {
                    Invitees.Add(uid, new User(jtoken));
                }
            }
        }

        private void ParseEvents(JArray json)
        {
            foreach (JToken jtoken in json)
            {
                long eid = System.Int64.Parse(((object)jtoken["eid"]).ToString());
                myEvents.Add(eid,new Event(jtoken));
            }
        }

        private void ParseRSVPS(JArray json)
        {
            foreach (JToken jtoken in json)
            {
                long eid = System.Int64.Parse(((String)jtoken["eid"]));
                long uid = System.Int64.Parse(((String)jtoken["uid"]));
                Event e = myEvents[eid];                
                e.AddInvitees(Invitees[uid],(string)jtoken["rsvp_status"]);
            }
        }
         */
    }
}
