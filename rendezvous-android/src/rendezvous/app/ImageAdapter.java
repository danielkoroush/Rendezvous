package rendezvous.app;

import java.util.ArrayList;
import java.util.List;

import rendezvous.app.Event.rsvp_status;
import rendezvous.app.rendezvous.EfficientAdapter.ViewHolder;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.TextView;

public class ImageAdapter extends BaseAdapter {

	private ArrayList<User> URLS = new ArrayList<User>();
	private ArrayList<rsvp_status> Status = new ArrayList<rsvp_status>();
	private LayoutInflater mInflater;

	private final ImageDownloader imageDownloader = new ImageDownloader();

	public ImageAdapter(Context context) {
		mInflater = LayoutInflater.from(context);
	}

	public int getCount() {
		return URLS.size();
	}

	public User getItem(int position) {
		return URLS.get(position);
	}

	public long getItemId(int position) {
		if (noFriend){
			return -1;
		}
		return URLS.get(position).hashCode();
	}

	public View getView(final int position, View convertView, ViewGroup parent) {

		// A ViewHolder keeps references to children views to avoid
		// unneccessary calls
		// to findViewById() on each row.
		final ViewHolder holder;

		// When convertView is not null, we can reuse it directly, there is
		// no need
		// to reinflate it. We only inflate a new View when the convertView
		// supplied
		// by ListView is null.
		if (convertView == null) {
			convertView = mInflater.inflate(R.layout.friend_item, null);

			// Creates a ViewHolder and store references to the two children
			// views
			// we want to bind data to.
			holder = new ViewHolder();
			holder.Name = (TextView) convertView.findViewById(R.id.friend_name);
			holder.Pic = (ImageView) convertView.findViewById(R.id.friend_pic);
			holder.RSVP = (TextView) convertView.findViewById(R.id.friend_rsvp);

			convertView.setTag(holder);
		} else {
			// Get the ViewHolder back to get fast access to the TextView
			// and the ImageView.
			holder = (ViewHolder) convertView.getTag();
		}
		if (!noFriend) {
			holder.Name.setText(URLS.get(position).Name);
			rsvp_status rsvp = Status.get(position);
			if (rsvp.equals(rsvp_status.attending)) {
				holder.RSVP.setText("RSVP: Attending");
			} else if (rsvp.equals(rsvp_status.declined)) {
				holder.RSVP.setText("RSVP: Not Attending");
			} else if (rsvp.equals(rsvp_status.unsure)) {
				holder.RSVP.setText("RSVP: Maybe Attending");
			} else {
				holder.RSVP.setText("RSVP: Awaiting Reply");
			}
			imageDownloader.download(URLS.get(position).Pic, holder.Pic);
		}else{
			holder.Name.setText("No friends found.");
			holder.RSVP.setText("");
		}
		return convertView;
	}

	static class ViewHolder {
		TextView Name;
		ImageView Pic;
		TextView RSVP;
	}

	public ImageDownloader getImageDownloader() {
		return imageDownloader;
	}

	boolean noFriend = false;

	public void AddNoFriend() {
		noFriend = true;
		URLS.add(null);
		Status.add(null);
		notifyDataSetChanged();
	}

	public void AddFriend(User u, rsvp_status s) {
		noFriend = false;
		Status.add(s);
		URLS.add(u);
		notifyDataSetChanged();
	}
}