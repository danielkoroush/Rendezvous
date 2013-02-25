package rendezvous.app;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.ByteArrayOutputStream;
import java.io.DataOutputStream;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLEncoder;

import android.content.ContentResolver;
import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.drawable.Drawable;
import android.os.AsyncTask;
import android.os.Bundle;
import android.util.Xml.Encoding;
import android.widget.ImageView;

public class UploadEventImage extends AsyncTask<String, Void, Void> {

	static final String request = "https://api.facebook.com/method/events.edit";
	static String err="";
	
	@Override
	protected void onPostExecute(Void v) {
		Create.alertbox.setMessage(err);
		Create.alertbox.setNeutralButton("OK", null);
		Create.alertbox.show();
	}
	
	
	@Override
	protected Void doInBackground(String... params) {
		UploadImage(params[0]);
		return null;
	}

	static void UploadImage(String eid) {

		byte[] data = null;
		try {

			  StringBuilder headerBuilder = new StringBuilder(1024);
			  String boundary = java.util.UUID.randomUUID().toString();
			  
			  headerBuilder.append("--");
			  //headerBuilder.append(boundary);
			  headerBuilder.append("\r\n");
			  headerBuilder.append("Content-Disposition: form-data; name=\"");
			  headerBuilder.append("access_token");
			  headerBuilder.append("\"\r\n\r\n");
			  headerBuilder.append(rendezvous.mFacebook.getAccessToken());
			  headerBuilder.append("\r\n");
			  
			  headerBuilder.append("--"); 
			  //headerBuilder.append(boundary);
			  headerBuilder.append("\r\n");
			  headerBuilder.append("Content-Disposition: form-data; name=\"");
			  headerBuilder.append("eid");
			  headerBuilder.append("\"\r\n\r\n");
			  headerBuilder.append(eid);
			  headerBuilder.append("\r\n");
			 
			  headerBuilder.append("--");
			  //headerBuilder.append(boundary);
			  headerBuilder.append("\r\n");
			  headerBuilder.append("Content-Disposition: form-data; name=\"");
			  headerBuilder.append("event_info");
			  headerBuilder.append("\"\r\n\r\n");
			  headerBuilder.append("{}");
			  headerBuilder.append("\r\n");
			  
			  
			  headerBuilder.append("--"); 
			  //headerBuilder.append(boundary);
			  headerBuilder.append("\r\n");
			  headerBuilder.append("Content-Disposition: form-data; filename=\"");
			  headerBuilder.append("uin.jpg");
			  headerBuilder.append("\"\r\n");
			  headerBuilder.append("Content-Type: ");
			  headerBuilder.append("image/jpg"); 
			  headerBuilder.append("\r\n\r\n");
			 
			
			  
			byte[] headerBytes = URLEncoder.encode(headerBuilder.toString()).getBytes();
			String footer = "\r\n--";// + boundary;
			byte[] footerBytes =URLEncoder.encode(footer).getBytes();
			
			URL iurl = new URL(request);
			HttpURLConnection uc = (HttpURLConnection) iurl.openConnection();
			uc.setDoOutput(true);
			uc.setDoInput(true);
			uc.setRequestMethod("POST");
			uc.setRequestProperty
			    ("Content-Type", "multipart/form-data");

			//DataOutputStream dos = new DataOutputStream( uc.getOutputStream() );

			
			
			Bitmap bi = BitmapFactory.decodeResource(
					rendezvous.context.getResources(), R.drawable.new_event);
			ByteArrayOutputStream baos = new ByteArrayOutputStream();
			bi.compress(Bitmap.CompressFormat.PNG, 100, baos);
			data = baos.toByteArray();
		
			
			  DataOutputStream wr = new DataOutputStream(uc.getOutputStream());
            
            //write parameters
            wr.writeUTF(URLEncoder
					.encode(headerBuilder.toString(), "UTF-8"));
           wr.write(data);
            wr.writeUTF(URLEncoder
					.encode(footer, "UTF-8"));
            wr.flush();
            
            
            
            // Get the response
            
            StringBuffer answer = new StringBuffer();
            BufferedReader reader = new BufferedReader(new InputStreamReader(uc.getInputStream()));
            String line;
            while ((line = reader.readLine()) != null) {
                answer.append(line);
            }
           
            reader.close();
			wr.close();
			//dos.write(headerBuilder.toString());
			//dos.write(data);
			//dos.write(footer);
			
			
		     			err=answer.toString();
			//dos.flush();
			//dos.close();

			
			/*
			Bitmap bi = BitmapFactory.decodeResource(
					rendezvous.context.getResources(), R.drawable.new_event);
			ByteArrayOutputStream baos = new ByteArrayOutputStream();
			bi.compress(Bitmap.CompressFormat.JPEG, 100, baos);
			data = baos.toByteArray();
			
			
			String x = "{\"name\":\"name\",\"location\":\"location\"}";
			Bundle params = new Bundle(); 
			
			params.putString("method", "events.edit"); 
			params.putByteArray("picture", data);
			params.putString("eid",eid);
			//params.putString("event_info", x);
			//params.putByteArray("photo", data);
			
			
			rendezvous.mFacebook.request(params);
			//params.putByteArray("picture", data);
			*/
		} catch (Exception e) {
				err=e.getMessage();
				for (StackTraceElement t:e.getStackTrace()){
					err+=" "+t.getLineNumber()+"  "+t.getMethodName();
				}		
		}

	}

}
