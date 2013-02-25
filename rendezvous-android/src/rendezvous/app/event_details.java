package rendezvous.app;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import rendezvous.app.Event.*;

import android.app.TabActivity;
import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.Window;
import android.widget.AdapterView;
import android.widget.ListView;
import android.widget.ProgressBar;
import android.widget.TabHost;
import android.widget.AdapterView.OnItemClickListener;

public class event_details extends TabActivity {
	Event event;
	public static ImageAdapter mFriendsAdapter;
	public static ProgressBar pg;
	GetEventGuests attending_worker;
	GetEventGuests declined_worker;
	GetEventGuests unsure_worker;
	GetEventGuests noreply_worker;
	static String msg = "";
	public static int rsvp_threads = 0;
	public static Map<rsvp_status, List<Long>> Guests = new HashMap<rsvp_status, List<Long>>();
	
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		requestWindowFeature(Window.FEATURE_CUSTOM_TITLE);

		TabHost tabHost = getTabHost();
		rsvp_threads = 0;
		Intent i = getIntent();
		int index = i.getIntExtra("id", 0);
		boolean suggestion = i.getBooleanExtra("Suggestion", false);

		if (!suggestion) {
			event = Upcoming._upcomingEvents.get(index);
		} else {
			event = Suggestions.SuggestionsEvents.get(index);
		}

		Intent intent; // Reusable Intent for each tab
		// Create an Intent to launch an Activity for the tab (to be reused)
		intent = new Intent().setClass(this, event_details_activity.class);
		intent.putExtra("id", index);
		intent.putExtra("Suggestion", suggestion);
		LayoutInflater.from(this).inflate(R.layout.main,
				tabHost.getTabContentView(), true);
		tabHost.addTab(tabHost.newTabSpec("tab1").setIndicator("Detail")
				.setContent(intent));

		tabHost.addTab(tabHost.newTabSpec("tab3").setIndicator("Friends' RSVP")
				.setContent(R.id.ListView02));

		getWindow().setFeatureInt(Window.FEATURE_CUSTOM_TITLE,
				R.layout.title_bar2);

		ListView lv = (ListView) this.findViewById(R.id.ListView02);
		pg = (ProgressBar) findViewById(R.id.leadProgressBar);
		mFriendsAdapter = new ImageAdapter(this);
;

		lv.setAdapter(mFriendsAdapter);
		if (!event.LoadedGuests) {
			Guests = new HashMap<rsvp_status, List<Long>>();
			attending_worker = new GetEventGuests(event);
			attending_worker.execute(rsvp_status.attending);

			declined_worker = new GetEventGuests(event);
			declined_worker.execute(rsvp_status.declined);

			unsure_worker = new GetEventGuests(event);
			unsure_worker.execute(rsvp_status.unsure);

			noreply_worker = new GetEventGuests(event);
			noreply_worker.execute(rsvp_status.not_replied);

		} else {
			event_details_activity.accept_count.setText(event.attending_count
					+ "");
			event_details_activity.declined_count.setText(event.declined_count
					+ "");
			event_details_activity.maybe_count.setText(event.unsure_count + "");
		}
	}

	public static void stopProgressBar(boolean stop) {
		if (!stop) {
			pg.setVisibility(ProgressBar.VISIBLE);
		} else {
			pg.setVisibility(ProgressBar.INVISIBLE);
		}
	}
}
