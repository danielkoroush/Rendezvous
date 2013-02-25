package rendezvous.app;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLEncoder;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Vector;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import rendezvous.app.rendezvous.EfficientAdapter;

public class Upcoming {

	public static Map<Long, Integer> UpcomingEvents = new HashMap<Long, Integer>();
	public static List<Event> _upcomingEvents = new Vector<Event>();

	public static void ParseUpcomingJson(String object) {
		try {
			JSONArray jarray = new JSONArray(object);
			JSONArray events;
			JSONArray rsvps = new JSONArray();
			_upcomingEvents = new Vector<Event>();
			UpcomingEvents = new HashMap<Long, Integer>();
			rendezvous.mUpcomingAdapter = new EfficientAdapter(
					rendezvous.context);
			for (int i = 0; i < jarray.length(); i++) {
				JSONObject temp = jarray.getJSONObject(i);
				if (temp.get("name").equals("events")) {
					events = temp.getJSONArray("fql_result_set");
					for (int j = 0; j < events.length(); j++) {
						Event ev = new Event(events.getJSONObject(j));
						_upcomingEvents.add(ev);
					}
				} else {
					rsvps = temp.getJSONArray("fql_result_set");
				}
			}
			Collections.sort(_upcomingEvents);
			if (_upcomingEvents.size() > 0) {
				for (int i = 0; i < _upcomingEvents.size(); i++) {
					UpcomingEvents.put(_upcomingEvents.get(i).id, i);
					rendezvous.mUpcomingAdapter
							.addEvent(_upcomingEvents.get(i));
				}
			} else {
				rendezvous.mUpcomingAdapter.noUpcomingEvent();
			}

			for (int j = 0; j < rsvps.length(); j++) {
				JSONObject jobject = rsvps.getJSONObject(j);
				addRSVPtoEvent(jobject);
			}
		} catch (Throwable e) {
			e.printStackTrace();
		}
		rendezvous.hideProgressBar(false);
	}

	private static void addRSVPtoEvent(JSONObject jobject) {
		try {
			long eid = Long.parseLong((jobject.getString("eid")));
			String rsvp = jobject.getString("rsvp_status");
			if (rsvp.equals("unsure")) {
				_upcomingEvents.get(UpcomingEvents.get(eid)).RSVP = Event.rsvp_status.unsure;
			} else if (rsvp.equals("attending")) {
				_upcomingEvents.get(UpcomingEvents.get(eid)).RSVP = Event.rsvp_status.attending;
			} else if (rsvp.equals("declined")) {
				_upcomingEvents.get(UpcomingEvents.get(eid)).RSVP = Event.rsvp_status.declined;
			} else {
				_upcomingEvents.get(UpcomingEvents.get(eid)).RSVP = Event.rsvp_status.not_replied;
			}
		} catch (Exception e) {
		}
	}

	public static void GetUpcomingEvents() {
		try {
			long epoch = System.currentTimeMillis() / 1000;
			String param = URLEncoder
					.encode(String
							.format("{\"events\":\"SELECT eid,name,host,start_time,end_time,location,venue,pic_big,description FROM event WHERE eid in (SELECT eid FROM event_member WHERE uid=me()) AND end_time> %d \",\"rsvps\":\"SELECT eid,rsvp_status FROM event_member WHERE uid=me() and eid IN (SELECT eid FROM event WHERE eid in (SELECT eid FROM event_member WHERE uid=me()) AND end_time> %d )\"}",
									epoch, epoch), "UTF-8");
			String request = "https://api.facebook.com/method/fql.multiquery?queries="
					+ param
					+ "&access_token="
					+ rendezvous.mFacebook.getAccessToken() + "&format=json";
			URL url = new URL(request);
			// Read all the text returned by the server
			URLConnection conn = url.openConnection();
			BufferedReader in = new BufferedReader(new InputStreamReader(
					conn.getInputStream()));
			String line;
			StringBuffer sb = new StringBuffer();
			while ((line = in.readLine()) != null) {
				sb.append(line);
			}
			in.close();
			Upcoming.ParseUpcomingJson(sb.toString());
		} catch (Throwable e) {

		}
	}

}
