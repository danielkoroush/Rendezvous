package rendezvous.app;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLEncoder;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;
import org.json.JSONArray;
import org.json.JSONObject;
import com.facebook.android.Util;

public class UserInfo {
	static String fql_host = "https://api.facebook.com/method/fql.query?query=";
	static String friends_request = "SELECT name,uid,pic_big FROM user WHERE uid in (SELECT uid2 FROM friend WHERE uid1 = me())";
	public static Map<Long, User> Friends = new HashMap<Long, User>();
	public static ArrayList<User> FriendsList = new ArrayList<User>();
	public static boolean LoadedFriends = false;

	// Getting User's friends Information
	public static void GetFriends() {
		try {

			String request = String.format("%s%s&access_token=%s&format=json",
					fql_host, URLEncoder.encode(friends_request, "UTF-8"),
					rendezvous.mFacebook.getAccessToken());
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

			JSONArray jarray = new JSONArray(sb.toString());
			for (int i = 0; i < jarray.length(); i++) {
				User f = new User(jarray.getJSONObject(i));
				Friends.put(f.id, f);
				FriendsList.add(f);
			}
			Collections.sort(FriendsList);
			for (User f : FriendsList) {
				rendezvous.mSuggestionsAdapter.add(f);
			}
		} catch (Throwable e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}
	}

	public static User UserInformation;

	// Current User Information
	public static void GetUserInfo() {
		try {
			JSONObject json = Util
					.parseJson(rendezvous.mFacebook.request("me"));
			UserInformation = new User(json, true);
		} catch (Throwable e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
}
