package rendezvous.app;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.ArrayList;

import android.app.ProgressDialog;
import android.app.TabActivity;
import android.os.AsyncTask;
import android.os.Bundle;
import android.util.Base64;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.BaseAdapter;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.TabHost;
import android.widget.TextView;
import android.widget.Toast;
import android.view.MenuInflater;
import android.content.Context;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.widget.ListView;
import android.widget.AdapterView.OnItemClickListener;
import com.facebook.android.*;
import com.facebook.android.Facebook.*;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.content.pm.Signature;

public class rendezvous extends TabActivity {

	public static final String ACCESS_TOKEN = "AcessToken";
	public static EfficientAdapter mUpcomingAdapter;
	public static ArrayAdapter mSuggestionsAdapter;
	public static Context context;
	public static ProgressBar pg;
	public static ListView lv;
	static ProgressDialog mDialog1;
	AsyncTask<OperationsEnum, Void, OperationsEnum> _upcoming;
	AsyncTask<OperationsEnum, Void, OperationsEnum> _userinfo;
	AsyncTask<OperationsEnum, Void, OperationsEnum> _friends;
	static boolean Loaded_Information = false;
	public static Facebook mFacebook = new Facebook("137117509680418");
	String[] permissions = new String[] { "user_events", "friends_events",
			"create_event", "rsvp_event", "offline_access", "publish_stream" };

	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		mDialog1 = new ProgressDialog(this);
		Intent i = getIntent();
		boolean cancel = i.getBooleanExtra("cancel", false);
		boolean refresh = i.getBooleanExtra("refresh", false);

		requestWindowFeature(Window.FEATURE_CUSTOM_TITLE);
		context = this;
		TabHost tabHost = getTabHost();

		getWindow().setFeatureInt(Window.FEATURE_CUSTOM_TITLE,
				R.layout.title_bar2);

		LayoutInflater.from(this).inflate(R.layout.main,
				tabHost.getTabContentView(), true);
		tabHost.addTab(tabHost.newTabSpec("Upcoming").setIndicator("Upcoming")
				.setContent(R.id.ListView01));

		Intent intent; // Reusable Intent for each tab
		// Create an Intent to launch an Activity for the tab (to be reused)
		intent = new Intent().setClass(this, suggestions_activity.class);

		LayoutInflater.from(this).inflate(R.layout.main,
				tabHost.getTabContentView(), true);
		tabHost.addTab(tabHost.newTabSpec("tab3")
				.setIndicator("Friends' Events").setContent(intent));

		pg = (ProgressBar) findViewById(R.id.leadProgressBar);
		lv = (ListView) this.findViewById(R.id.ListView01);

		lv.setOnItemClickListener(new OnItemClickListener() {
			public void onItemClick(AdapterView<?> arg0, View arg1, int arg2,
					long arg3) {
				if (!mUpcomingAdapter.emptyEvent) {
				Intent intent = new Intent();
				intent.putExtra("id", (int) arg3);			
					intent.setClass(getApplicationContext(),
							event_details.class);
					startActivity(intent);
				}
			}
		});
              
		if (mUpcomingAdapter == null) {
			mUpcomingAdapter = new EfficientAdapter(this);
		} else {
			rendezvous.lv.setAdapter(rendezvous.mUpcomingAdapter);
		}

		if (mSuggestionsAdapter == null) {
			mSuggestionsAdapter = new ArrayAdapter<String>(this,
					android.R.layout.simple_spinner_item);
		}

		if (refresh) {
			_upcoming = new BackgroundWorker()
					.execute(OperationsEnum.GetUpcoming);
		}

		SharedPreferences settings = getSharedPreferences(ACCESS_TOKEN, 0);
		String token = settings.getString("token", "");

		mDialog1.setTitle("Processing");
		mDialog1.setMessage("Please wait while loading...");
		mDialog1.setIndeterminate(true);
		mDialog1.setCancelable(true);

		if (token.length() > 0) {
			mFacebook.setAccessToken(token);
		} else {
			FacebookAuth();
		}
		if (!Loaded_Information) {
			mDialog1.show();
			get_information();
		}
	}

	private void get_information() {
		Loaded_Information = true;
		BackgroundWorker.done = 0;
		suggestions_activity.mSpinner = (Spinner) findViewById(R.id.FriendsSpinner);
		_upcoming = new BackgroundWorker().execute(OperationsEnum.GetUpcoming);
		_friends = new BackgroundWorker().execute(OperationsEnum.GetFriends);
		_userinfo = new BackgroundWorker().execute(OperationsEnum.GetUserInfo);
	}
 
	private void FacebookAuth() {
		if ((mFacebook.getAccessToken() == null)) {
			mFacebook.authorize(this, permissions,mFacebook.FORCE_DIALOG_AUTH, new DialogListener() {
				public void onComplete(Bundle values) {
					try {
						SharedPreferences settings = getSharedPreferences(
								ACCESS_TOKEN, 0);
						SharedPreferences.Editor editor = settings.edit();
						editor.putString("token", mFacebook.getAccessToken());
						editor.commit();
						mDialog1.show();
						get_information();

					} catch (Throwable e) {
						Toast.makeText(getApplicationContext(),
								"ERROR" + e.getCause() + "", Toast.LENGTH_LONG)
								.show();
					}
				}
 
				public void onFacebookError(FacebookError error) {
					//FacebookAuth();
					Toast.makeText(getApplicationContext(), error.getMessage(),
							Toast.LENGTH_LONG).show();
				}

				public void onError(DialogError e) {
					FacebookAuth();
					Toast.makeText(getApplicationContext(), "Login failed please try again.",
							Toast.LENGTH_LONG).show();
				}

				public void onCancel() {
					FacebookAuth();
					//Toast.makeText(getApplicationContext(), "FB Cancel",
					//		Toast.LENGTH_LONG).show();
				}
			});
		}
	}

	public static void hideProgressBar(boolean visible) {
		if (visible) {
			pg.setVisibility(ProgressBar.VISIBLE);
		} else {
			pg.setVisibility(ProgressBar.INVISIBLE);
		}
	}

	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		super.onActivityResult(requestCode, resultCode, data);
		mFacebook.authorizeCallback(requestCode, resultCode, data);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Hold on to this
		MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.menu, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case R.id.new_event:
			Intent intent = new Intent();
			intent.setClass(getApplicationContext(), Create.class);
			startActivity(intent);
			return true;
		case R.id.refresh:
			_upcoming = new BackgroundWorker()
					.execute(OperationsEnum.GetUpcoming);
			return true;
		default:
			return super.onOptionsItemSelected(item);
		}
	}

	static class EfficientAdapter extends BaseAdapter {
		private LayoutInflater mInflater;
		private Bitmap mIcon1;

		private ArrayList<Event> mEvents = new ArrayList<Event>();

		public EfficientAdapter(Context context) {
			// Cache the LayoutInflate to avoid asking for a new one each time.
			mInflater = LayoutInflater.from(context);

			// Icons bound to the rows.
			mIcon1 = BitmapFactory.decodeResource(context.getResources(),
					R.drawable.event_thumb);
		}

		public int getCount() {
			return mEvents.size();
		}

		public void clearContent() {
			mEvents = new ArrayList<Event>();
		}

		public Object getItem(int position) {
			return position;
		}

		public long getItemId(int position) {
			return position;
		}

		public View getView(final int position, View convertView,
				ViewGroup parent) {
			final ViewHolder holder;

			if (convertView == null) {
				convertView = mInflater.inflate(R.layout.test, null);

				// Creates a ViewHolder and store references to the two children
				// views
				// we want to bind data to.
				holder = new ViewHolder();
				holder.text = (TextView) convertView.findViewById(R.id.title);
				holder.icon = (ImageView) convertView.findViewById(R.id.icon);
				holder.start = (TextView) convertView.findViewById(R.id.start);
				holder.venue = (TextView) convertView.findViewById(R.id.venue);

				convertView.setTag(holder);
			} else {
				// Get the ViewHolder back to get fast access to the TextView
				// and the ImageView.
				holder = (ViewHolder) convertView.getTag();
			}

			// Bind the data efficiently with the holder.
			holder.text.setText(mEvents.get(position).Name);
			holder.icon.setImageBitmap(mEvents.get(position).Pic);
			if (mEvents.get(position).id != -1) {
				holder.start.setText(Utility.ConvertEpochToUtc((mEvents
						.get(position).Start)));
			} else {
				holder.start.setText("");
			}
			holder.venue.setText(mEvents.get(position).Location);

			return convertView;
		}

		static class ViewHolder {
			TextView text;
			ImageView icon;
			TextView venue;
			TextView start;
		}

		boolean emptyEvent = false;

		public void addEmptyEvent() {
			emptyEvent = true;
			mEvents.add(new Event("No events found.", "-1"));
			notifyDataSetChanged();
		}

		public void noUpcomingEvent() {
			emptyEvent = true;
			mEvents.add(new Event("You have no upcoming event.", "-1"));
			notifyDataSetChanged();
		}

		public void addEvent(Event e) {
			emptyEvent = false;
			mEvents.add(e);
			notifyDataSetChanged();
		}
	}

}