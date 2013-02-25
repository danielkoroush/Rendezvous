package rendezvous.app;

import rendezvous.app.suggestions_activity.MyOnItemSelectedListener;
import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.AdapterView.OnItemSelectedListener;

public class event_details_activity extends Activity {

	static TextView accept_count;
	static TextView declined_count;
	static TextView maybe_count;
	String[] rsvp_items = new String[] { "Attending", "Maybe Attending",
			"Not Attending", "No Response" };
	Event e;
	static boolean firstime = true;

	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		setContentView(R.layout.event_details2);

		Intent i = getIntent();
		int index = i.getIntExtra("id", 0);
		boolean suggestion = i.getBooleanExtra("Suggestion", false);
		firstime = true;
		if (!suggestion) {
			e = Upcoming._upcomingEvents.get(index);
		} else {
			e = Suggestions.SuggestionsEvents.get(index);
		}
		Spinner spinner = (Spinner) findViewById(R.id.EventSpinner);
		ArrayAdapter<String> adapter = new ArrayAdapter<String>(this,
				android.R.layout.simple_spinner_item, rsvp_items);
		adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
		spinner.setAdapter(adapter);

		spinner.setSelection(3);
		if (e.RSVP.equals(Event.rsvp_status.attending)) {
			spinner.setSelection(0);
		} else if (e.RSVP.equals(Event.rsvp_status.declined)) {
			spinner.setSelection(2);
		} else if (e.RSVP.equals(Event.rsvp_status.unsure)) {
			spinner.setSelection(1);
		}
		
		ImageView image = (ImageView) this.findViewById(R.id.EventIcon);
		image.setImageBitmap(e.Pic);

		TextView start = (TextView) this.findViewById(R.id.EventStart);
		start.setText("Start: " + Utility.ConvertEpochToUtc(e.Start));

		TextView end = (TextView) this.findViewById(R.id.EventEnd);
		end.setText("End: " + Utility.ConvertEpochToUtc(e.End));

		TextView title = (TextView) this.findViewById(R.id.EventTitle);
		title.setText(e.Name);

		TextView address = (TextView) this.findViewById(R.id.EventAddress);
		address.setText(e.GetVenueAddress());

		TextView description = (TextView) this
				.findViewById(R.id.EventDescription);
		description.setText(e.Description);

		TextView host = (TextView) this.findViewById(R.id.EventHost);
		host.setText(e.Host);

		TextView rsvp = (TextView) this.findViewById(R.id.EventRsvp);
		rsvp.setText("");

		accept_count = (TextView) this.findViewById(R.id.EventAttendingCount);
		declined_count = (TextView) this.findViewById(R.id.EventDeclinedCount);
		maybe_count = (TextView) this.findViewById(R.id.EventMaybeCount);
		spinner.setOnItemSelectedListener(new MyOnItemSelectedListener());
	}

	public class MyOnItemSelectedListener implements OnItemSelectedListener {

		@Override
		public void onItemSelected(AdapterView<?> parent, View arg1, int pos,
				long id) {
			if (firstime) {
				firstime = false;
			} else {
				String json_result;
				Bundle b = new Bundle();
				b.putString("access_token",
						rendezvous.mFacebook.getAccessToken());
				try {
					if (pos == 0) {
						json_result = rendezvous.mFacebook.request(e.id
								+ "/attending", b, "POST");
						showRSVPResult(json_result, "attending");
					} else if (pos == 2) {

						json_result = rendezvous.mFacebook.request(e.id
								+ "/declined", b, "POST");
						showRSVPResult(json_result, "declined");
					} else if (pos == 1) {

						json_result = rendezvous.mFacebook.request(e.id
								+ "/maybe", b, "POST");
						showRSVPResult(json_result, "maybe");
					}

				} catch (Exception x) {
				}
			}
		}

		@Override
		public void onNothingSelected(AdapterView<?> arg0) {
			// TODO Auto-generated method stub
		}
	}

	public void showRSVPResult(String result, String rsvp) {
		Toast toast = Toast.makeText(this, "new event", 10);
		toast = Toast.makeText(this, GetEventGuests.mymsg, 3000);
		if (result.equals("true")) {
			toast = Toast.makeText(this,
					String.format("Status changed to %s.", rsvp), 1000);
			if (rsvp.equals("attending")){
				e.RSVP=Event.rsvp_status.attending;
			}else if (rsvp.equals("declined")){
				e.RSVP=Event.rsvp_status.declined;
			}else{
				e.RSVP=Event.rsvp_status.unsure;
			}
		} else {
			toast = Toast.makeText(this,
					"Sorry we could not change your status at this moment.",
					1000);
		}
		toast.show();
	}
}
