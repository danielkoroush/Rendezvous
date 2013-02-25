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
using Paco.Application;
using System.Text;

namespace Paco.Application
{
    public class FQLCall
    {
        public static readonly String ATTENDING = "attending";
        public static readonly String DECLINED = "declined";
        public static readonly String UNSURE = "unsure";
        public static readonly String NOT_REPLIED = "not_replied";

        private const String FriendsRequestFormat = "https://api.facebook.com/method/fql.multiquery?format=json&queries={{\"friends\":\"SELECT name,uid,pic_big FROM user WHERE uid in (SELECT uid2 FROM friend WHERE uid1 = me())\"}}&access_token={0}";
        private const String InviteeRequestFormat = "https://api.facebook.com/method/fql.query?query=SELECT name,uid,pic_big FROM user WHERE uid in (SELECT uid FROM event_member WHERE eid={2} and rsvp_status=\"{1}\")&access_token={0}&format=json";
        private const String EventSummayRequstFormat = "https://api.facebook.com/method/fql.multiquery?queries={{\"events\":\"SELECT eid,name,host,start_time,end_time,location,venue,pic_big,description FROM event WHERE eid in (SELECT eid FROM event_member WHERE uid=me()) AND end_time>{0}\",\"rsvps\":\"SELECT eid,rsvp_status FROM event_member WHERE uid=me() and eid IN (SELECT eid FROM event WHERE eid in (SELECT eid FROM event_member WHERE uid=me()) AND end_time>{0})\"}}&access_token={1}&format=json";

        //private const String CheckinRequestFormat = "https://api.facebook.com/method/fql.multiquery?queries={{\"checkins\":\"SELECT checkin_id,author_uid,timestamp,page_id,tagged_uids,message FROM checkin WHERE timestamp>{1} AND (author_uid=me() OR author_uid in (SELECT uid2 FROM friend WHERE uid1=me()))\",\"places\":\"SELECT name,page_id,latitude,longitude FROM place WHERE page_id in (SELECT page_id FROM checkin WHERE timestamp>{1} AND (author_uid=me() OR author_uid in (SELECT uid2 FROM friend WHERE uid1=me())))\"}}&access_token={0}&format=json";
        private const String SuggestionRequestFormat = "https://api.facebook.com/method/fql.query?query=SELECT eid,name,host,start_time,end_time,location,venue,pic_big,description  from  event  WHERE  privacy=\"OPEN\"  AND  start_time>{1}  AND  eid  IN  (SELECT  eid  FROM  event_member  WHERE  uid  IN  (SELECT  uid2  FROM  friend  WHERE  uid1=me()))&access_token={0}&format=json";
        private const String UserInfoRequestFormat = "https://api.facebook.com/method/fql.query?query=SELECT uid, name, pic_big from  user  WHERE uid=me()&access_token={0}&format=json";
        private const String TagsRequestFormat = "https://api.facebook.com/method/fql.query?query=SELECT name,uid,pic_big FROM user WHERE uid in ({1})&access_token={0}&format=json";
        private const String SuggestionFriendsRequestFormat = "https://api.facebook.com/method/fql.query?query=SELECT eid,uid,rsvp_status FROM  event_member  WHERE  eid  IN  ({1}) AND uid IN (SELECT uid2 FROM friend WHERE uid1=me())&access_token={0}&format=json";

        private long eid;
        private String status;

        private string _access_token;
        private string _start_time;
        private string _end_time;

        StringBuilder eids_str = new StringBuilder();
        public static int _friends_processed = 0;
        bool _finished_suggestions = false;
        static List<long> _friends = new List<long>();
        public FQLCall()
        {
        }

        public FQLCall(long eid)
        {
            this.eid = eid;
        }

        public String Status
        {
            set
            {
                status = value;
            }
        }

        public delegate void FacebookErrorHandler(object sender, EventArgs e);
        public static event FacebookErrorHandler FacebookError;

        public delegate void FBEventHandler(object sender, EventArgs e);
        public event FBEventHandler Finished;

        public delegate void CheckinEventHandler(object sender, EventArgs e);
        public event CheckinEventHandler CheckinFinished;

        public delegate void FriendsEventHandler(object sender, EventArgs e);
        public event FriendsEventHandler FriendsFinished;

        public delegate void SuggestionsEventHandler(object sender, EventArgs e);
        public event SuggestionsEventHandler SuggestionsFinished;

        public delegate void UpcomingEventsEventHandler(object sender, EventArgs e);
        public event UpcomingEventsEventHandler UpcomingEventsFinished;

        public delegate void UpcomingEventsErrorHandler(object sender, EventArgs e);
        public event UpcomingEventsErrorHandler UpcomingEventsError;

        public delegate void UserInfoEventHandler(object sender, EventArgs e);
        public event UserInfoEventHandler UserInfoFinished;

        public delegate void TaggedUsersInfoEventHandler(object sender, EventArgs e);
        public event TaggedUsersInfoEventHandler TaggedUsersInfoFinished;

        protected virtual void OnFacebookError(EventArgs e)
        {
            if (FacebookError != null)
                FacebookError(null, e);
        }

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnFriendsEvent(EventArgs e)
        {
            if (FriendsFinished != null)
                FriendsFinished(this, e);
        }

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnSuggestionsEvent(EventArgs e)
        {
            if (SuggestionsFinished != null)
                SuggestionsFinished(this, e);
        }

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnUpcomingEventsEvent(EventArgs e)
        {
            if (UpcomingEventsFinished != null)
                UpcomingEventsFinished(this, e);
        }

        protected virtual void OnUpcomingEventsError(EventArgs e)
        {
            if (UpcomingEventsError != null)
                UpcomingEventsError(this, e);
        }

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnFBEvent(EventArgs e)
        {
            if (Finished != null)
                Finished(this, e);
        }

        //protected virtual void OnCheckinEvent(EventArgs e)
        //{
        //    if (CheckinFinished != null)
        //        CheckinFinished(this, e);
        //}

        protected virtual void OnUserInfoEvent(EventArgs e)
        {
            if (UserInfoFinished != null)
                UserInfoFinished(this, e);
        }

        protected virtual void OnTaggedUsersInfoEvent(EventArgs e)
        {
            if (TaggedUsersInfoFinished != null)
                TaggedUsersInfoFinished(this, e);
        }

        public void GetTagUsers(String accessToken, String uids)
        {
            Uri uri = new Uri(String.Format(TagsRequestFormat, accessToken, uids));
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(TagResponseCallback), request);
        }

        public void GetSuggestions(String accessToken, long uid)
        {
            _access_token = accessToken;
            String query = String.Format("SELECT eid,name,host,start_time,end_time,location,venue,pic_big,description FROM event WHERE eid in (select eid from event_member where uid={0}) AND end_time> {1}", uid, Utility.GetCurrentTimeInEpoch());
            string suggestions_fql = String.Format("https://api.facebook.com/method/fql.query?query={0}&access_token={1}&format=json", query, _access_token);
            Uri uri = new Uri(suggestions_fql);
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(SuggestionsResponseCallback), request);
        }

        public void GetFriends(String accessToken)
        {
            Uri uri = new Uri(String.Format(FriendsRequestFormat, accessToken));
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(FriendsResponseCallback), request);
        }

        public void GetCheckins(String accessToken, String epochTime)
        {
            //Uri uri = new Uri(String.Format(CheckinRequestFormat, accessToken,epochTime));
            //HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            //IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(CheckinResponseCallback), request);
        }

        public void GetUpcomingEvents(String accessToken)
        {
            Uri uri = new Uri(String.Format(EventSummayRequstFormat, Utility.GetCurrentTimeInEpoch(), accessToken));
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(EventSummaryResponseCallback), request);
        }

        public void GetInviteeDetail(String accessToken, long eid, RSVP rsvp)
        {
            Uri uri = new Uri(String.Format(InviteeRequestFormat, accessToken, rsvp, eid));
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(InviteesResponseCallback), request);
        }

        public void GetUserInfo(String accessToken)
        {
            Uri uri = new Uri(String.Format(UserInfoRequestFormat, accessToken));
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(UserInfoResponseCallback), request);
        }
        #region private methods

        private void UserInfoResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string json = streamReader1.ReadToEnd();
                    Newtonsoft.Json.Linq.JArray o = JArray.Parse(json);
                    FacebookData.CurrentUser = new User(o[0]);
                }
                OnUserInfoEvent(EventArgs.Empty);
            }
            catch (Exception) { OnFacebookError(EventArgs.Empty); }
        }

        private void SuggestionsFriendsResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string json = streamReader1.ReadToEnd();
                    Newtonsoft.Json.Linq.JArray o = JArray.Parse(json);
                    foreach (JToken j in o)
                    {
                        JObject j1 = JObject.Parse(j.ToString());
                        long eid = long.Parse(j1["eid"].ToString());
                        long uid = long.Parse(j1["uid"].ToString());
                        string rsvp = (string)j1["rsvp_status"];
                        FacebookData.Open_Events[eid].AddAssociateFriend(uid, rsvp);
                    }
                }
                OnSuggestionsEvent(EventArgs.Empty);
            }
            catch (Exception e)
            {
                OnFacebookError(EventArgs.Empty);
            }
        }

        private void FriendsResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string json = streamReader1.ReadToEnd();
                    Newtonsoft.Json.Linq.JArray o = JArray.Parse(json);
                    foreach (JToken j in o)
                    {
                        JObject j1 = JObject.Parse(j.ToString());
                        if (j1["name"].ToString().Equals("\"friends\"", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ParseFriends((JArray)j1["fql_result_set"]);
                        }
                    }
                }
                OnFriendsEvent(EventArgs.Empty);
            }
            catch (Exception e) { OnFacebookError(EventArgs.Empty); }
        }

        private void ParseFriends(JArray json)
        {
            foreach (JToken jtoken in json)
            {
                long uid = (long)jtoken["uid"];
                if (!FacebookData.friends.ContainsKey(uid))
                {
                    User user = new User(jtoken);
                    FacebookData.friends.Add(uid, user);
                    _friends.Add(uid);
                    FacebookData.friendsList.Add(user);
                }
            }
            //FacebookData.LoadedFriends = true;
        }

        private void EventSummaryResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string json = streamReader1.ReadToEnd();
                    Newtonsoft.Json.Linq.JArray o = JArray.Parse(json);
                    JArray eventsJson = null;
                    JArray rsvpsJson = null;
                    foreach (JToken j in o)
                    {
                        JObject j1 = JObject.Parse(j.ToString());
                        if (j1["name"].ToString().Equals("\"events\"", StringComparison.InvariantCultureIgnoreCase))
                        {
                            eventsJson = (JArray)j1["fql_result_set"];
                        }
                        else if (j1["name"].ToString().Equals("\"rsvps\"", StringComparison.InvariantCultureIgnoreCase))
                        {
                            rsvpsJson = (JArray)j1["fql_result_set"];
                        }
                    }
                    ParseEvents(eventsJson);
                    ParseRSVPs(rsvpsJson);
                }
                OnUpcomingEventsEvent(EventArgs.Empty);
                //OnFBEvent(EventArgs.Empty);
            }
            catch (Exception e)
            {
                OnUpcomingEventsError(EventArgs.Empty);
                OnFacebookError(EventArgs.Empty);
            }
        }

        private void TagResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);

                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string json = streamReader1.ReadToEnd();
                    Newtonsoft.Json.Linq.JArray o = JArray.Parse(json);
                    ParseTagged(o);
                }
                OnTaggedUsersInfoEvent(EventArgs.Empty);
            }
            catch (Exception) { OnFacebookError(EventArgs.Empty); }
        }

        private void SuggestionsResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string json = streamReader1.ReadToEnd();
                    Newtonsoft.Json.Linq.JArray o = JArray.Parse(json);
                    List<long> eids = ParseSuggestionsEvents(o);
                    eids_str = new StringBuilder();
                    foreach (long eid in eids)
                    {
                        //eids_str.Append(eid);
                        //eids_str.Append(",");
                        //break;
                        Uri uri = new Uri(String.Format(SuggestionFriendsRequestFormat, _access_token, eid));
                        HttpWebRequest request2 = WebRequest.Create(uri) as HttpWebRequest;
                        request2.BeginGetResponse(new AsyncCallback(SuggestionsFriendsResponseCallback), request2);
                    }
                    //if (eids_str.Length > 0) eids_str = eids_str.Remove(eids_str.Length - 1, 1);
                    //Uri uri = new Uri(String.Format(SuggestionFriendsRequestFormat, _access_token, eids_str));
                    //HttpWebRequest request2 = WebRequest.Create(uri) as HttpWebRequest;
                    //request2.BeginGetResponse(new AsyncCallback(SuggestionsFriendsResponseCallback), request2);
                }
            }
            catch (WebException)
            {
                Uri uri = new Uri(String.Format(SuggestionFriendsRequestFormat, _access_token, eids_str));
                HttpWebRequest request2 = WebRequest.Create(uri) as HttpWebRequest;
                request2.BeginGetResponse(new AsyncCallback(SuggestionsFriendsResponseCallback), request2);
            }
            finally
            {
                OnSuggestionsEvent(EventArgs.Empty);
            }
        }

        private void ParseEvents(JArray json)
        {
            foreach (JToken jtoken in json)
            {
                long eid = System.Int64.Parse(((object)jtoken["eid"].ToString()).ToString());
                if (!FacebookData.User_Events.ContainsKey(eid))
                {
                    FacebookData.User_Events.Add(eid, new Event(jtoken));
                }
            }
            FacebookData.LoadedEvents = true;
        }

        private void ParseRSVPs(JArray json)
        {
            foreach (JToken jtoken in json)
            {
                String temp = jtoken["eid"].ToString();
                long eid = long.Parse(temp);
                temp = (String)jtoken["rsvp_status"];
                FacebookData.User_Events[eid].SetRSVP(temp);
            }
        }

        private List<long> ParseSuggestionsEvents(JArray json)
        {
            List<long> eids = new List<long>();
            foreach (JToken jtoken in json)
            {
                long eid = System.Int64.Parse(((object)jtoken["eid"]).ToString());
                eids.Add(eid);
                try
                {
                    FacebookData.Open_Events.Add(eid, new Event(jtoken));
                }
                catch (System.ArgumentException) { }
            }
            FacebookData.LoadedSuggestions = true;
            return eids;
        }

        private void InviteesResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    string json = streamReader1.ReadToEnd();
                    try
                    {
                        Newtonsoft.Json.Linq.JArray o = JArray.Parse(json);
                        ParseUsers(o);
                    }
                    catch (Exception)
                    {
                        //Json returned is empty
                    }
                }
            }
            catch (Exception) { }
            OnFBEvent(EventArgs.Empty);
        }

        private void ParseTagged(JArray json)
        {
            FacebookData.TaggedUsers = new List<User>();
            foreach (JToken jtoken in json)
            {
                FacebookData.TaggedUsers.Add(new User(jtoken));
            }
        }

        private void ParseUsers(JArray json)
        {
            foreach (JToken jtoken in json)
            {
                long uid = (long)jtoken["uid"];
                try
                {
                    FacebookData.User_Events[eid].AddInvitees(new User(jtoken), status);
                }
                catch (KeyNotFoundException)
                {
                    FacebookData.Open_Events[eid].AddInvitees(new User(jtoken), status);
                }
            }
            try
            {
                if (FacebookData.User_Events[eid].Invitees.Keys.Count == 4) FacebookData.User_Events[eid].LoadedGuests = true;
            }
            catch (KeyNotFoundException)
            {
                if (FacebookData.Open_Events[eid].Invitees.Keys.Count == 4) FacebookData.Open_Events[eid].LoadedGuests = true;
            }

        }

        #endregion

    }
}
