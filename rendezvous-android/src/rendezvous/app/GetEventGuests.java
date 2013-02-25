package rendezvous.app;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.lang.ref.WeakReference;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLEncoder;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import rendezvous.app.Event.*;
import android.os.AsyncTask;

public class GetEventGuests extends AsyncTask<rsvp_status, Void, Void> {
	private final WeakReference<Event> eventReference;
	static String mymsg = "";

	public GetEventGuests(Event e) {
		eventReference = new WeakReference<Event>(e);
	}

	@Override
	protected void onPostExecute(Void v) {
		event_details.rsvp_threads++;
		try {
			if (event_details.rsvp_threads == 4) {
				event_details_activity.accept_count.setText(eventReference
						.get().attending_count + "");
				event_details_activity.declined_count.setText(eventReference
						.get().declined_count + "");
				event_details_activity.maybe_count
						.setText(eventReference.get().unsure_count + "");
				AddFriends();
				event_details.stopProgressBar(true);
			}
		} catch (Throwable e) {
			event_details.msg = e.getMessage();
		}
	}
 
	private void AddFriends() {
		List<rsvp_status> rsvp_order = new ArrayList<rsvp_status>();
		rsvp_order.add(rsvp_status.attending);
		rsvp_order.add(rsvp_status.declined);
		rsvp_order.add(rsvp_status.unsure);
		rsvp_order.add(rsvp_status.not_replied);
		boolean addedfriend=false;
		for (rsvp_status rsvp : rsvp_order) {
			List<Long> list = event_details.Guests.get(rsvp);
			if (list != null) {
				List<User> guests = new ArrayList<User>();
				for (Long id : list) {
					guests.add(UserInfo.Friends.get(id));
				}
				Collections.sort(guests);
				for (int i = 0; i < guests.size(); i++) {
					addedfriend=true;
					event_details.mFriendsAdapter
							.AddFriend(guests.get(i), rsvp);
				}
			}
		}
		if (!addedfriend){
			event_details.mFriendsAdapter.AddNoFriend();
		}
	}

	@Override
	protected Void doInBackground(rsvp_status... arg0) {
		// TODO Auto-generated method stub
		GetGuests(arg0[0], eventReference.get());
		return null;
	}

	public void GetGuests(rsvp_status rsvp, Event e) {
		try {

			String query = URLEncoder
					.encode(String
							.format("SELECT uid FROM event_member WHERE eid=%d and rsvp_status=\"%s\"",
									e.id, rsvp.toString()), "UTF-8");
			String request = "https://api.facebook.com/method/fql.query?query="
					+ query + "&access_token="
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
			ParseGuests(e, sb.toString(), rsvp);
		} catch (Throwable e1) {
			event_details.msg = "   ERROR";
		}
	}

	public void ParseGuests(Event e, String json, rsvp_status rsvp) {
		if (rsvp.equals(rsvp_status.attending)) {
			e.attending_count = 0;
		} else if (rsvp.equals(rsvp_status.declined)) {
			e.declined_count = 0;
		} else if (rsvp.equals(rsvp_status.unsure)) {
			e.unsure_count = 0;
		}
		try {
			JSONArray jarray = new JSONArray(json);
			for (int i = 0; i < jarray.length(); i++) {
				JSONObject jo = jarray.getJSONObject(i);
				Long id = Long.parseLong(jo.getString("uid"));
				if (UserInfo.Friends.containsKey(id)) {
					if (event_details.Guests.containsKey(rsvp)) {
						event_details.Guests.get(rsvp).add(id);
					} else {
						List<Long> list = new ArrayList<Long>();
						list.add(id);
						event_details.Guests.put(rsvp, list);
					}
				}
				UpdateGuestCount(rsvp, e);
			}
		} catch (JSONException e1) {
			event_details.msg = e1.getMessage();
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}
	}

	private void UpdateGuestCount(rsvp_status rsvp, Event e) {
		if (rsvp.equals(rsvp_status.attending)) {
			e.attending_count++;
		} else if (rsvp.equals(rsvp_status.declined)) {
			e.declined_count++;
		} else if (rsvp.equals(rsvp_status.unsure)) {
			e.unsure_count++;
		}
	}
}
