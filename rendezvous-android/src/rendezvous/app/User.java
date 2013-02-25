package rendezvous.app;
import org.json.JSONException;
import org.json.JSONObject;

public class User implements Comparable{
	public String Name;
	public long id;
	public String Pic;
	
	public User(JSONObject json){
		try {
			this.Name=json.getString("name");
			this.id = json.getLong("uid");
			this.Pic = json.getString("pic_big");
		} catch (JSONException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	public User(JSONObject json, boolean current){
		try {
			this.Name=json.getString("name");
			this.id = json.getLong("id");
		} catch (JSONException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	public int compareTo(Object o) {
		return this.Name.compareTo(((User) o).Name);		
	}
	
	@Override
	public String toString(){
		return this.Name;
	}
}
