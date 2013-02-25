package rendezvous.app;

import android.os.AsyncTask;

public class BackgroundWorker extends
		AsyncTask<OperationsEnum, Void, OperationsEnum> {
	public static int done=0;
	protected void onPostExecute(OperationsEnum result) {
		if (result.equals(OperationsEnum.GetFriends)) {
			// suggestions_activity.mSpinner.setAdapter(rendezvous.mSuggestionsAdapter);
			done++;
		} else if (result.equals(OperationsEnum.GetUpcoming)) {
			rendezvous.lv.setAdapter(rendezvous.mUpcomingAdapter);
			done++;
		} else if (result.equals(OperationsEnum.GetSuggestions)) {
			suggestions_activity.mSuggestionsLVAdapter.clearContent();
			if (Suggestions.SuggestionsEvents.size() > 0) {
				for (int i = 0; i < Suggestions.SuggestionsEvents.size(); i++) {
					suggestions_activity.mSuggestionsLVAdapter
							.addEvent(Suggestions.SuggestionsEvents.get(i));
				}
			} else {
				suggestions_activity.mSuggestionsLVAdapter
				.addEmptyEvent();
			}
		} else if (result.equals(OperationsEnum.CreateEvent)) {
			Create.cancelDialog();
		}
		if (done==2){
			rendezvous.mDialog1.cancel();
		}
	}

	@Override
	protected OperationsEnum doInBackground(OperationsEnum... params) {
		//rendezvous.hideProgressBar(true);
		if (params[0].equals(OperationsEnum.GetUpcoming)) {
			Upcoming.GetUpcomingEvents();
		} else if (params[0].equals(OperationsEnum.GetFriends)) {
			UserInfo.GetFriends();
			UserInfo.LoadedFriends = true;
		} else if (params[0].equals(OperationsEnum.GetUserInfo)) {
			UserInfo.GetUserInfo();
		} else if (params[0].equals(OperationsEnum.CreateEvent)) {
			Create.PostEvent();
		} else {
			Suggestions.GetSuggestionsEvent(suggestions_activity.id);
		}
		return params[0];
	}

}
