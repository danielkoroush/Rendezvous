package rendezvous.app;

import java.util.List;

import org.json.JSONException;
import org.json.JSONObject;

public class Place {
	static List<Place> PlacesFound;
	String name="";
	JSONObject location;
	String category="";
	
	public Place(JSONObject json){
		try {
			this.name=json.getString("name");
			this.location=json.getJSONObject("location");
			this.category=json.getString("category");
		} catch (JSONException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	public String getStreet(){
		try {
			return location.getString("street");
		} catch (JSONException e) {
			// TODO Auto-generated catch block
			return category;
		}
	}
	
	public String getCity(){
		try {
			return location.getString("city");
		} catch (JSONException e) {
			// TODO Auto-generated catch block
			return "";
		}
	}
	
	public String getState(){
		try {
			return location.getString("state");
		} catch (JSONException e) {
			// TODO Auto-generated catch block
			return "";
		}
	}

}
