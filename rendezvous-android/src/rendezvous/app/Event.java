package rendezvous.app;
import java.util.*;

import org.json.JSONException;
import org.json.JSONObject;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.widget.ImageView;
public class Event implements Comparable
{
	public static enum rsvp_status { attending, unsure, declined,not_replied }
	public String Name;
	public long Start;
	public long End;
	public Bitmap Pic;
	public long id;
	public String Host;
	public String Location="";
	public JSONObject Venue;
	public String Description;
	public int attending_count=0;
	public int declined_count=0;
	public int unsure_count=0;
	public rsvp_status RSVP=rsvp_status.not_replied;
	public boolean LoadedGuests=false;
	public ArrayList<Long> attending_friends =  new ArrayList<Long>();
	public Event(JSONObject json) {
		try {
			this.Name= json.getString("name");
			this.id=Long.parseLong(json.getString("eid"));	
			this.Start=json.getLong("start_time");
			this.End=json.getLong("end_time");
			this.Pic = Utility.getImageBitmap(json.getString("pic_big"));
			this.Location= json.getString("location");
			this.Description=json.getString("description");
			this.Venue=json.getJSONObject("venue");
			this.Host=json.getString("host");
		} catch (JSONException e) {
			// TODO Auto-generated catch block
		///	e.printStackTrace();
		}
	}
	
	public Event(String name, String id){
		this.Name=name;
		this.id = Long.parseLong(id);
	}

	public String GetVenueAddress(){
		StringBuffer address = new StringBuffer();
		address.append(Location+"\n");
		try {
			address.append(Venue.getString("street")+"\n");
		} catch (JSONException e) {
			e.printStackTrace();
		}
		try {
			address.append(Venue.getString("city")+" ");
		} catch (JSONException e) {
			e.printStackTrace();
		}
		try {
			address.append(Venue.getString("state"));
		} catch (JSONException e) {
			e.printStackTrace();
		}		
		return address.toString();
	}
	
	public int compareTo(Object o) {
		if (this.Start == ((Event) o).Start)
            return 0;
        else if ((this.Start) > ((Event) o).Start)
            return 1;
        else
            return -1;
	}	
}
