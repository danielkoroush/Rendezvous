package rendezvous.app;

import android.app.TabActivity;
import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.Window;
import android.widget.TabHost;

public class FindPlace extends TabActivity {
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_CUSTOM_TITLE);
		TabHost tabHost = getTabHost();
		Intent intent, intent2;
		intent = new Intent().setClass(this, Search_Place.class);
		intent2 = new Intent().setClass(this, Custom_Place.class);
		LayoutInflater.from(this).inflate(R.layout.main,
				tabHost.getTabContentView(), true);

		tabHost.addTab(tabHost.newTabSpec("tab1").setIndicator("Nearby Places")
				.setContent(intent));

		tabHost.addTab(tabHost.newTabSpec("tab3").setIndicator("Custom Place")
				.setContent(intent2));
		getWindow().setFeatureInt(Window.FEATURE_CUSTOM_TITLE, R.layout.title_bar2);
	}
}
