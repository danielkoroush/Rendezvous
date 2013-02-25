package rendezvous.app;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;

public class Custom_Place extends Activity {
	static AlertDialog.Builder alertbox;
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.custom_place);
		
		Button save = (Button)findViewById(R.id.venueOk);
		save.setOnClickListener(new View.OnClickListener() {	
			@Override
			public void onClick(View v) {
				EditText name1= (EditText)findViewById(R.id.venueTitle);
				EditText address2= (EditText)findViewById(R.id.venueAddress);
				EditText city2= (EditText)findViewById(R.id.venueCity);
				String name = name1.getText().toString().trim();
				if (name.length()>0){
					Intent intent = new Intent();
					intent.putExtra("Custom",true);
					intent.putExtra("name", name);
					intent.putExtra("address", address2.getText().toString().trim());
					intent.putExtra("city", city2.getText().toString().trim());
					intent.setClass(getApplicationContext(),Create.class);
					startActivity(intent);
				}else{
					alertbox.setMessage("Please enter a venue name.");
					alertbox.setNeutralButton("OK", null);
					alertbox.show();
				}
			}
		});
	}

}
