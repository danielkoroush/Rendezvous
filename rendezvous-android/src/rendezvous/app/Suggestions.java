package rendezvous.app;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLEncoder;
import java.util.ArrayList;
import java.util.Collections;
import org.json.JSONArray;

public class Suggestions 
{
	
	public static ArrayList<Event> SuggestionsEvents;
	public static void GetSuggestionsEvent(String id) {
		try {
			long epoch = System.currentTimeMillis() / 1000;
			String param = URLEncoder.encode(String.format("SELECT eid,name,host,start_time,end_time,location,venue,pic_big,description FROM event WHERE eid in (select eid from event_member where uid=%s) AND end_time> %d",id,epoch),"UTF-8");
			String request = 
			 "https://api.facebook.com/method/fql.query?query="+param+"&access_token="+rendezvous.mFacebook.getAccessToken()+"&format=json";			
			URL url = new URL(request);						
			// Read all the text returned by the server			
			URLConnection conn = url.openConnection();
			BufferedReader in = new BufferedReader(new InputStreamReader(conn.getInputStream()));
			String line;
			StringBuffer sb = new StringBuffer();
			while ((line=in.readLine())!=null){
				sb.append(line);
			}
			in.close();
			ParseSuggestionsJson(sb.toString());
		} catch (Throwable e) {

		}
	}
	
	public static void ParseSuggestionsJson(String object) {
		try {
			SuggestionsEvents = new ArrayList<Event>();
			JSONArray jarray = new JSONArray(object);
			
			for (int i = 0; i < jarray.length(); i++) {
				Event ev = new Event(jarray.getJSONObject(i));
				SuggestionsEvents.add(ev);
			}
			
			Collections.sort(SuggestionsEvents);
		} catch (Throwable e) {
			e.printStackTrace();
		}
		rendezvous.hideProgressBar(false);
	}

}
