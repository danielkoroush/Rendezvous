package rendezvous.app;

import java.io.BufferedInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.net.URLConnection;
import java.text.ParseException;
import java.util.Date;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.HttpStatus;
import org.apache.http.client.methods.HttpGet;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.http.AndroidHttpClient;
import android.util.Log;

public class Utility {
    public static String ConvertEpochToUtc(long epoch)
    {
    	String date = new java.text.SimpleDateFormat("MM/dd/yyyy hh:mm  aaa").format(new java.util.Date (epoch*1000));
        return date;
    }

    public static Long getCurrentEpoch(){
    	return System.currentTimeMillis()/1000;
    }
    
    public static String convertStringToEpoch(String s){
    	long epoch;
		try {
			epoch = (new java.text.SimpleDateFormat ("MM-dd-yyyy hh:mm a").parse(s).getTime())/1000;
		} catch (ParseException e) {
			epoch = 00000;
		}
    	return epoch+"";
    }
    
	public static Bitmap getImageBitmap(String url) {
		Bitmap bm = null;
		try {
			URL aURL = new URL(url);
			URLConnection conn = aURL.openConnection();
			conn.connect();
			InputStream is = conn.getInputStream();
			BufferedInputStream bis = new BufferedInputStream(is);
			bm = BitmapFactory.decodeStream(bis);
			bis.close();
			is.close();
		} catch (IOException e) {
			bm=BitmapFactory.decodeResource(rendezvous.context.getResources(),
					R.drawable.event_thumb);
		}
		return bm;
	}
	
	public static Bitmap downloadBitmap(String url) {
	    final AndroidHttpClient client = AndroidHttpClient.newInstance("Android");
	    final HttpGet getRequest = new HttpGet(url);

	    try {
	        HttpResponse response = client.execute(getRequest);
	        final int statusCode = response.getStatusLine().getStatusCode();
	        if (statusCode != HttpStatus.SC_OK) { 
	            Log.w("ImageDownloader", "Error " + statusCode + " while retrieving bitmap from " + url); 
	            return null;
	        }
	        
	        final HttpEntity entity = response.getEntity();
	        if (entity != null) {
	            InputStream inputStream = null;
	            try {
	                inputStream = entity.getContent(); 
	                final Bitmap bitmap = BitmapFactory.decodeStream(inputStream);
	                return bitmap;
	            } finally {
	                if (inputStream != null) {
	                    inputStream.close();  
	                }
	                entity.consumeContent();
	            }
	        }
	    } catch (Exception e) {
	        // Could provide a more explicit error message for IOException or IllegalStateException
	        getRequest.abort();
	        Log.w("ImageDownloader", "Error while retrieving bitmap from " + url);
	    } finally {
	        if (client != null) {
	            client.close();
	        }
	    }
	    return null;
	}
}
