package rendezvous.app;

import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;

import org.json.JSONObject;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.DatePickerDialog;
import android.app.Dialog;
import android.app.ProgressDialog;
import android.app.TimePickerDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.View;
import android.view.Window;
import android.widget.*;
import android.widget.AdapterView.OnItemClickListener;

public class Create extends Activity {

	static ProgressDialog mDialog1;
	static AlertDialog.Builder alertbox;
	private int mYear;
	private int mMonth;
	private int mDay;
	private int mHour;
	private int mMinute;
	private int mYear2;
	private int mMonth2;
	private int mDay2;
	private int mHour2;
	private int mMinute2;
	private int mAM_PM, mAM_PM2;

	public static String title;
	public static String description;
	public static boolean isPublic = false;
	public static long epoch_start;
	public static long epoch_end;
	static String address = "";
	static String city = "";
	static String guest_names = "";

	static String json_result;
	public static String id;
	static int place_index = -1;

	static final int TIME_DIALOG_ID = 0;
	static final int DATE_DIALOG_ID = 1;
	static final int TIME_DIALOG_ID2 = 3;
	static final int DATE_DIALOG_ID2 = 4;
	int choice = 0;

	static MultiAutoCompleteTextView guestsBox;
	static HashMap<String, User> guests;
	public static StringBuffer guestsId;
	static EditText venue;

	static Button startTime;
	static Button startDate;
	static Button endTime;
	static Button endDate;
	RadioButton publicEvent;
	RadioButton privateEvent;
	Button ok;
	Button cancel;

	static Bundle saveInfo = new Bundle();
	AsyncTask<OperationsEnum, Void, OperationsEnum> _postEvent;
	static AsyncTask<Void, Void, Void> _sendInvites;
	static AsyncTask<String, Void, Void> _imageUpload;
	static Context context;
	static boolean customPlace = false;

	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_CUSTOM_TITLE);
		setContentView(R.layout.create);
		getWindow().setFeatureInt(Window.FEATURE_CUSTOM_TITLE,
				R.layout.title_bar2);

		context = this;
		// resetValues();

		venue = ((EditText) findViewById(R.id.eventLocation));
		Intent i = getIntent();
		customPlace = i.getBooleanExtra("Custom", false);

		ProgressBar pg = (ProgressBar) findViewById(R.id.leadProgressBar);
		pg.setVisibility(ProgressBar.INVISIBLE);

		guestsBox = (MultiAutoCompleteTextView) findViewById(R.id.invitee);
		guestsBox.setAdapter(rendezvous.mSuggestionsAdapter);
		guestsBox.setTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
		guestsBox.setOnItemClickListener(new MyOnItemClickListener());
		guests = new HashMap<String, User>();

		mDialog1 = new ProgressDialog(this);
		alertbox = new AlertDialog.Builder(this);

		startTime = (Button) findViewById(R.id.start_time);
		startTime.setOnClickListener(new View.OnClickListener() {
			public void onClick(View v) {
				showDialog(TIME_DIALOG_ID);
			}
		});

		startDate = (Button) findViewById(R.id.start_date);
		startDate.setOnClickListener(new View.OnClickListener() {

			public void onClick(View v) {
				showDialog(DATE_DIALOG_ID);
			}
		});

		startTime = (Button) findViewById(R.id.start_time);
		startTime.setOnClickListener(new View.OnClickListener() {
			public void onClick(View v) {
				showDialog(TIME_DIALOG_ID);
			}
		});

		endTime = (Button) findViewById(R.id.end_time);
		endTime.setOnClickListener(new View.OnClickListener() {
			public void onClick(View v) {
				showDialog(TIME_DIALOG_ID2);
			}
		});

		endDate = (Button) findViewById(R.id.end_date);
		endDate.setOnClickListener(new View.OnClickListener() {
			public void onClick(View v) {
				showDialog(DATE_DIALOG_ID2);
			}
		});

		publicEvent = (RadioButton) findViewById(R.id.option2);
		publicEvent.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				isPublic = true;
			}
		});

		privateEvent = (RadioButton) findViewById(R.id.option1);
		privateEvent.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				isPublic = false;
			}
		});

		final Calendar c = Calendar.getInstance();
		Calendar c2 = Calendar.getInstance();
		c2.add(Calendar.HOUR_OF_DAY, 3);

		mYear2 = c2.get(Calendar.YEAR);
		mMonth2 = c2.get(Calendar.MONTH);
		mDay2 = c2.get(Calendar.DAY_OF_MONTH);
		mHour2 = c2.get(Calendar.HOUR);
		if (mHour2 == 0)
			mHour2 = 12;
		mMinute2 = c2.get(Calendar.MINUTE);
		mAM_PM2 = c2.get(Calendar.AM_PM);

		mYear = c.get(Calendar.YEAR);
		mMonth = c.get(Calendar.MONTH);
		mDay = c.get(Calendar.DAY_OF_MONTH);
		mHour = c.get(Calendar.HOUR);
		if (mHour == 0)
			mHour = 12;
		mMinute = c.get(Calendar.MINUTE);
		mAM_PM = c.get(Calendar.AM_PM);

		updateDisplay();
		updateTimeDisplay();
		updateDisplay2();
		updateTimeDisplay2();

		ok = (Button) findViewById(R.id.create);
		cancel = (Button) findViewById(R.id.cancelCreate);

		ok.setOnClickListener(new View.OnClickListener() {
			public void onClick(View v) {
				title = ((EditText) findViewById(R.id.eventTitle)).getText()
						.toString();
				description = ((EditText) findViewById(R.id.eventDesc))
						.getText().toString();

				mDialog1.setTitle("Processing");
				mDialog1.setMessage("Please wait while creating the event.");
				mDialog1.setIndeterminate(true);
				mDialog1.show();
				_postEvent = new BackgroundWorker()
						.execute(OperationsEnum.CreateEvent);
			}
		});

		cancel.setOnClickListener(new View.OnClickListener() {
			public void onClick(View v) {
				Intent intent = new Intent();
				intent.putExtra("cancel", true);
				intent.setClass(getApplicationContext(), rendezvous.class);
				startActivity(intent);
			}
		});

		ImageView search = (ImageView) findViewById(R.id.search_image);
		search.setOnClickListener(new View.OnClickListener() {
			public void onClick(View v) {
				Intent intent = new Intent();
				intent.setClass(getApplicationContext(), FindPlace.class);
				startActivity(intent);
			}
		});

		place_index = i.getIntExtra("id", -1);
		if (place_index != -1) {
			venue.setText(Place.PlacesFound.get(place_index).name);
		} else if (customPlace) {
			venue.setText(i.getStringExtra("name"));
			address = i.getStringExtra("address");
			city = i.getStringExtra("city");
		}

		// Updating the form with the saved information

		title = saveInfo.getString("title");
		if (title != null && title.length() > 0) {
			EditText titleBox = (EditText) findViewById(R.id.eventTitle);
			titleBox.setText(title);
		}

		description = saveInfo.getString("description");
		if (description != null && description.length() > 0) {
			EditText tempBox = (EditText) findViewById(R.id.eventDesc);
			tempBox.setText(description);
		}

		isPublic = saveInfo.getBoolean("isPublic");
		if (isPublic) {
			RadioButton temp = (RadioButton) findViewById(R.id.option2);
			temp.setChecked(true);
		} else {
			RadioButton temp = (RadioButton) findViewById(R.id.option1);
			temp.setChecked(true);
		}

		guest_names = saveInfo.getString("guests");
		if (guest_names != null && guest_names.length() > 0) {
			MultiAutoCompleteTextView temp = (MultiAutoCompleteTextView) findViewById(R.id.invitee);
			temp.setText(guest_names);
		}
	}

	@Override
	public void onSaveInstanceState(Bundle savedInstanceState) {
		// Save UI state changes to the savedInstanceState.
		// This bundle will be passed to onCreate if the process is
		// killed and restarted.
		super.onSaveInstanceState(savedInstanceState);
		EditText tempBox = (EditText) findViewById(R.id.eventTitle);
		saveInfo.putString("title", tempBox.getText().toString());

		tempBox = (EditText) findViewById(R.id.eventDesc);
		saveInfo.putString("description", tempBox.getText().toString());

		tempBox = (EditText) findViewById(R.id.eventDesc);
		saveInfo.putString("description", tempBox.getText().toString());

		RadioButton temp = (RadioButton) findViewById(R.id.option2);
		if (temp.isChecked()) {
			saveInfo.putBoolean("isPublic", true);
		} else {
			saveInfo.putBoolean("isPublic", false);
		}
		MultiAutoCompleteTextView temp2 = (MultiAutoCompleteTextView) findViewById(R.id.invitee);
		saveInfo.putString("guests", temp2.getText().toString());
	}

	@Override
	public void onRestoreInstanceState(Bundle savedInstanceState) {
		super.onRestoreInstanceState(savedInstanceState);
		title = saveInfo.getString("title");
	}

	public class MyOnItemClickListener implements OnItemClickListener {
		@Override
		public void onItemClick(AdapterView<?> parent, View arg1, int pos,
				long arg3) {
			User u = (User) parent.getItemAtPosition(pos);
			guests.put(u.Name.trim(), u);
		}
	}

	@Override
	protected Dialog onCreateDialog(int id) {
		switch (id) {
		case TIME_DIALOG_ID:
			choice = 1;
			return new TimePickerDialog(this, mTimeSetListener, mHour, mMinute,
					false);
		case DATE_DIALOG_ID:
			choice = 2;
			return new DatePickerDialog(this, mDateSetListener, mYear, mMonth,
					mDay);
		case DATE_DIALOG_ID2:
			choice = 3;
			return new DatePickerDialog(this, mDateSetListener, mYear2,
					mMonth2, mDay2);
		case TIME_DIALOG_ID2:
			choice = 4;
			return new TimePickerDialog(this, mTimeSetListener, mHour2,
					mMinute2, false);
		}
		return null;
	}

	@Override
	protected void onPrepareDialog(int id, Dialog dialog) {
		switch (id) {
		case TIME_DIALOG_ID:
			if (mAM_PM == 1) {
				((TimePickerDialog) dialog).updateTime(mHour + 12, mMinute);
			} else {
				((TimePickerDialog) dialog).updateTime(mHour, mMinute);
			}
			break;
		case DATE_DIALOG_ID:
			((DatePickerDialog) dialog).updateDate(mYear, mMonth, mDay);
			break;
		case TIME_DIALOG_ID2:
			if (mAM_PM2 == 1) {
				((TimePickerDialog) dialog).updateTime(mHour2 + 12, mMinute2);
			} else {
				((TimePickerDialog) dialog).updateTime(mHour2, mMinute2);
			}
			break;
		case DATE_DIALOG_ID2:
			((DatePickerDialog) dialog).updateDate(mYear2, mMonth2, mDay2);
			break;
		}
	}

	private void updateDisplay() {
		startDate.setText(new StringBuilder().append(mMonth + 1).append("-")
				.append(mDay).append("-").append(mYear).append(" "));
	}

	private void updateTimeDisplay() {
		String am = "AM";
		if (mAM_PM == 1) {
			am = "PM";
		}
		startTime.setText(new StringBuilder().append(pad(mHour)).append(":")
				.append(pad(mMinute)).append(" " + am));
	}

	private void updateDisplay2() {
		endDate.setText(new StringBuilder().append(mMonth2 + 1).append("-")
				.append(mDay2).append("-").append(mYear2).append(" "));
	}

	private void updateTimeDisplay2() {
		String am = "AM";
		if (mAM_PM2 == 1) {
			am = "PM";
		}
		endTime.setText(new StringBuilder().append(pad(mHour2)).append(":")
				.append(pad(mMinute2)).append(" " + am));
	}

	private DatePickerDialog.OnDateSetListener mDateSetListener = new DatePickerDialog.OnDateSetListener() {
		public void onDateSet(DatePicker view, int year, int monthOfYear,
				int dayOfMonth) {
			if (choice == 3) {
				mYear2 = year;
				mMonth2 = monthOfYear;
				mDay2 = dayOfMonth;
				updateDisplay2();
			} else {
				mYear = year;
				mMonth = monthOfYear;
				mDay = dayOfMonth;
				updateDisplay();
			}
		}
	};

	private TimePickerDialog.OnTimeSetListener mTimeSetListener = new TimePickerDialog.OnTimeSetListener() {
		public void onTimeSet(TimePicker view, int hourOfDay, int minute) {
			if (choice == 4) {
				if (hourOfDay > 12) {
					mHour2 = hourOfDay - 12;
					mAM_PM2 = 1;
				} else {
					mHour2 = hourOfDay;
					mAM_PM2 = 0;
				}
				mMinute2 = minute;
				updateTimeDisplay2();
			} else {
				if (hourOfDay > 12) {
					mHour = hourOfDay - 12;
					mAM_PM = 1;
				} else {
					mHour = hourOfDay;
					mAM_PM = 0;
				}
				mMinute = minute;
				updateTimeDisplay();
			}
		}
	};

	private static String pad(int c) {
		if (c >= 10)
			return String.valueOf(c);
		else
			return "0" + String.valueOf(c);
	}

	private static void resetValues() {
		title = "";
		description = "";
		isPublic = false;
		epoch_start = 0;
		epoch_end = 0;
		address = "";
		customPlace = false;
		place_name = "";
		saveInfo = new Bundle();
	}

	public static void cancelDialog() {
		mDialog1.cancel();
		try {
			JSONObject j = new JSONObject(json_result);
			id = j.getString("id");
			saveInfo = new Bundle();
			alertbox.setMessage("Event created!");
			alertbox.setPositiveButton("Ok",
					new DialogInterface.OnClickListener() {
						public void onClick(DialogInterface dialog, int id) {
							Intent intent = new Intent();
							intent.putExtra("refresh", true);
							intent.setClass(context, rendezvous.class);
							context.startActivity(intent);
						}
					});
			alertbox.show();
			if (guestsId.length() > 0) {
				_sendInvites = new SendInvites().execute();
			} else {
				writeOnWall();
			}
			// resetValues();
		} catch (Exception e) {
			alertbox.setMessage("Sorry, could not create the event. Please try again.");
			alertbox.setNeutralButton("OK", null);
			alertbox.show();
		}
	}
 
	public static void writeOnWall() {
		Bundle args = new Bundle();
		String temp = startTime.getText() + " "
				+ startDate.getText().toString().replace("-", "/");
		if (place_name.length() > 0) {
			args.putString("message", "Lets rendezvous @ " + place_name + ", "
					+ temp + "!!");
		} else {
			args.putString("message", "Lets rendezvous  " + Create.title + ", "
					+ temp + "!!");
		}
		try {
			rendezvous.mFacebook.request("/" + Create.id + "/feed", args,
					"POST");
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	private static void getGuests() {
		String[] guestsList = guestsBox.getText().toString().split(",");
		guestsId = new StringBuffer();

		for (String guest : guestsList) {
			String key = guest.trim();
			if (guests.containsKey(key)) {
				guestsId.append(guests.get(key).id + ",");
			}
		}
	}

	static String place_name = "";

	public static void PostEvent() {
		getGuests();
		Bundle args = new Bundle();
		args.putString("name", Create.title);
		if (isPublic) {
			args.putString("privacy_type", "OPEN");
		} else {
			args.putString("privacy_type", "SECRET");
		}
		args.putString("description", Create.description);

		if (place_index != -1) {
			place_name = Place.PlacesFound.get(place_index).name;
			args.putString("location", Place.PlacesFound.get(place_index).name);
			args.putString("street", Place.PlacesFound.get(place_index)
					.getStreet());
			String city = Place.PlacesFound.get(place_index).getCity();
			if (city.length() > 0) {
				args.putString("city", city);
			}
			String state = Place.PlacesFound.get(place_index).getState();
			if (state.length() > 0) {
				args.putString("state", Place.PlacesFound.get(place_index)
						.getState());
			}
		} else if (customPlace) {
			place_name = Create.venue.getText().toString().trim();
			args.putString("location", Create.venue.getText().toString().trim());
			if (city != null && city.length() > 0) {
				if (!city.contains(",")) {
					args.putString("city", city);
					//args.putString("state", "");
				} else {
					args.putString("city", city.split(",")[0]);
					args.putString("state", city.split(",")[1]);
				}
			}
			args.putString("street", address);
		} else if (Create.venue.getText().toString().trim().length() > 0) {
			place_name = Create.venue.getText().toString().trim();
			args.putString("location", Create.venue.getText().toString().trim());
		}

		String temp1 = startDate.getText() + " " + startTime.getText();
		temp1 = Utility.convertStringToEpoch(temp1);
		args.putString("start_time", temp1);

		String temp2 = endDate.getText() + " " + endTime.getText();
		args.putString("end_time", Utility.convertStringToEpoch(temp2));

		try {
			json_result = rendezvous.mFacebook.request("me/events", args,
					"POST");
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

	}

}
