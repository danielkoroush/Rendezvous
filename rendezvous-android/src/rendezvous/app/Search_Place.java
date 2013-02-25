package rendezvous.app;

import java.util.ArrayList;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.Intent;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.BaseAdapter;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.Toast;

public class Search_Place extends Activity {

	ListView mPlacesLV;
	static PlaceAdapter mPlacesAdapter;
	static AlertDialog.Builder alertbox;
	static String json_result;
	static double Latitude;
	static double Longitude;
	static boolean GPS_DISABLED = false;
	ImageView searchButton;
	EditText searchBox;
	static ProgressBar pg;
	static Context context;
	private static final long MINIMUM_DISTANCE_CHANGE_FOR_UPDATES = 1; 
	private static final long MINIMUM_TIME_BETWEEN_UPDATES = 1000; 
																
	protected LocationManager locationManager;

	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.search_place);
		
		pg = (ProgressBar) findViewById(R.id.leadProgressBar);
		context = this;
		mPlacesLV = (ListView) findViewById(R.id.PlacesLV);
		mPlacesAdapter = new PlaceAdapter(this);
		mPlacesLV.setAdapter(mPlacesAdapter);
		mPlacesLV.setOnItemClickListener(new OnItemClickListener() {
			public void onItemClick(AdapterView<?> arg0, View arg1, int arg2,
					long arg3) {
				Intent intent = new Intent();
				intent.putExtra("id", (int) arg3);
				intent.putExtra("Custom", false);
				intent.setClass(getApplicationContext(), Create.class);
				startActivity(intent);
			}
		});

		searchBox = (EditText) findViewById(R.id.searchPlaceBox);
		searchButton = (ImageView) findViewById(R.id.search_place_button);

		searchButton.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				if (!GPS_DISABLED) {
					mPlacesAdapter.clearContent();
					getPlaces(searchBox.getText().toString());
				} else {
					Toast.makeText(
							Search_Place.this,
							"Sorry we could not find places near you. GPS is turned off",
							Toast.LENGTH_LONG).show();
				}
			}
		});
		locationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
		locationManager.requestLocationUpdates(
				LocationManager.NETWORK_PROVIDER, MINIMUM_TIME_BETWEEN_UPDATES,
				MINIMUM_DISTANCE_CHANGE_FOR_UPDATES, new MLocationListener());
	}

	private static void getPlaces(String q) {
//		pg.setVisibility(ProgressBar.VISIBLE);
		Bundle args = new Bundle();
		if (q.length() > 0) {
			args.putString("q", q);
		}
		args.putString("type", "place");
		args.putString("center", Latitude + "," + Longitude);
		args.putString("distance", "19999");
		try {
			json_result = rendezvous.mFacebook.request("/search", args);
			parsePlaces(json_result);
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	private static void parsePlaces(String json) {
		Place.PlacesFound = new ArrayList<Place>();
		try {
			JSONObject j = new JSONObject(json);
			JSONArray places = j.getJSONArray("data");
			for (int i = 0; i < places.length(); i++) {
				Place p = new Place(places.getJSONObject(i));
				Place.PlacesFound.add(p);
			}
			for (Place p : Place.PlacesFound) {
				mPlacesAdapter.addPlace(p);
			}

		} catch (JSONException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}finally{
			pg.setVisibility(ProgressBar.INVISIBLE);
		}
	}

	static class PlaceAdapter extends BaseAdapter {
		private LayoutInflater mInflater;

		private ArrayList<Place> mPlaces = new ArrayList<Place>();

		public PlaceAdapter(Context context) {
			// Cache the LayoutInflate to avoid asking for a new one each time.
			mInflater = LayoutInflater.from(context);
		}

		/**
		 * The number of items in the list is determined by the number of
		 * speeches in our array.
		 * 
		 * @see android.widget.ListAdapter#getCount()
		 */
		public int getCount() {
			return mPlaces.size();
		}

		public void clearContent() {
			mPlaces = new ArrayList<Place>();
		}

		/**
		 * Since the data comes from an array, just returning the index is
		 * sufficent to get at the data. If we were using a more complex data
		 * structure, we would return whatever object represents one row in the
		 * list.
		 * 
		 * @see android.widget.ListAdapter#getItem(int)
		 */
		public Object getItem(int position) {
			return position;
		}

		/**
		 * Use the array index as a unique id.
		 * 
		 * @see android.widget.ListAdapter#getItemId(int)
		 */
		public long getItemId(int position) {
			return position;
		}

		/**
		 * Make a view to hold each row.
		 * 
		 * @see android.widget.ListAdapter#getView(int, android.view.View,
		 *      android.view.ViewGroup)
		 */
		public View getView(final int position, View convertView,
				ViewGroup parent) {
			// A ViewHolder keeps references to children views to avoid
			// unneccessary calls
			// to findViewById() on each row.
			final ViewHolder holder;

			// When convertView is not null, we can reuse it directly, there is
			// no need
			// to reinflate it. We only inflate a new View when the convertView
			// supplied
			// by ListView is null.
			if (convertView == null) {
				convertView = mInflater.inflate(R.layout.place_row, null);

				// Creates a ViewHolder and store references to the two children
				// views
				// we want to bind data to.
				holder = new ViewHolder();
				holder.text = (TextView) convertView
						.findViewById(R.id.place_Name);
				holder.start = (TextView) convertView
						.findViewById(R.id.place_address);
				convertView.setTag(holder);
			} else {
				// Get the ViewHolder back to get fast access to the TextView
				// and the ImageView.
				holder = (ViewHolder) convertView.getTag();
			}

			// Bind the data efficiently with the holder.
			holder.text.setText(mPlaces.get(position).name);
			holder.start.setText((mPlaces.get(position).getStreet()));

			return convertView;
		}

		static class ViewHolder {
			TextView text;
			TextView start;
		}

		public void addPlace(Place p) {
			mPlaces.add(p);
			notifyDataSetChanged();
		}
	}

	public class MLocationListener implements LocationListener {
		@Override
		public void onLocationChanged(Location location) {

			String message = String.format(
					"New Location \n Longitude: %1$s \n Latitude: %2$s",
					location.getLongitude(), location.getLatitude());

			Latitude = location.getLatitude();
			Longitude = location.getLongitude();
			GPS_DISABLED = false;
			getPlaces("");
		}

		@Override
		public void onProviderDisabled(String provider) {
			Toast.makeText(
					Search_Place.this,
					"Sorry we could not find places near you. GPS is turned off",
					Toast.LENGTH_LONG).show();
			Latitude = 37.76;
			Longitude = -122.427;
			GPS_DISABLED = true;
		}

		@Override
		public void onProviderEnabled(String provider) {
			Toast.makeText(Search_Place.this,
					"Provider enabled by the user. GPS turned on",
					Toast.LENGTH_LONG).show();
			GPS_DISABLED = false;
		}

		@Override
		public void onStatusChanged(String provider, int status, Bundle extras) {

		}

	}
}
