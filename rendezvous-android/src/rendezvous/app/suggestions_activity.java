package rendezvous.app;

import rendezvous.app.rendezvous.EfficientAdapter;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.AdapterView.OnItemSelectedListener;

public class suggestions_activity extends Activity {

	public static Context mContext;
	public static Spinner mSpinner;
	static ListView mSuggestionsLV;
	public static EfficientAdapter mSuggestionsLVAdapter;
	AsyncTask<OperationsEnum, Void, OperationsEnum> _suggestion;
	public static String id;

	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		//rendezvous.pg = (ProgressBar) findViewById(R.id.leadProgressBar);
		setContentView(R.layout.suggestions);
		mSpinner = (Spinner) findViewById(R.id.FriendsSpinner);
		mSuggestionsLV = (ListView) findViewById(R.id.SuggestionsList);
		mContext = this;
		mSuggestionsLVAdapter = new EfficientAdapter(this);
		mSuggestionsLV.setAdapter(mSuggestionsLVAdapter);
		mSpinner.setAdapter(rendezvous.mSuggestionsAdapter);
		mSpinner.setOnItemSelectedListener(new MyOnItemSelectedListener());
		mSuggestionsLV.setOnItemClickListener(new OnItemClickListener() {
			public void onItemClick(AdapterView<?> arg0, View arg1, int arg2,
					long arg3) {
				if (!suggestions_activity.mSuggestionsLVAdapter.emptyEvent) {
					Intent intent = new Intent();
					intent.putExtra("id", (int) arg3);
					intent.putExtra("Suggestion", true);
					intent.setClass(getApplicationContext(),
							event_details.class);
					startActivity(intent);
				}
			}
		});
	}

	public class MyOnItemSelectedListener implements OnItemSelectedListener {

		@Override
		public void onItemSelected(AdapterView<?> parent, View arg1, int pos,
				long id) {
			rendezvous.hideProgressBar(true);
			User u = (User) parent.getItemAtPosition(pos);
			suggestions_activity.id = u.id + "";
			_suggestion = new BackgroundWorker()
					.execute(OperationsEnum.GetSuggestions);
		}

		@Override
		public void onNothingSelected(AdapterView<?> arg0) {
			// TODO Auto-generated method stub
		}
	}
}