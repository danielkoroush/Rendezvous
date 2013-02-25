package rendezvous.app;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.URL;
import java.net.URLConnection;

import android.os.AsyncTask;

public class SendInvites extends AsyncTask<Void, Void, Void> {

	final static String invite = "https://api.facebook.com/method/events.invite?access_token=";

	@Override
	protected Void doInBackground(Void... params) {
		sendInvites();
		return null;
	}
	
	@Override
	protected void onPostExecute(Void v) {
		Create.writeOnWall();
	}

	static void sendInvites() {
		String request = invite + rendezvous.mFacebook.getAccessToken();
		request += "&eid=" + Create.id + "&uids=" + Create.guestsId;
		try {
			URL url = new URL(request);
			URLConnection conn = url.openConnection();
			BufferedReader in = new BufferedReader(new InputStreamReader(
					conn.getInputStream()));
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

}
