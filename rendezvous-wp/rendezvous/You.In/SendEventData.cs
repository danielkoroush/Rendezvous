using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using Paco.Application;
using System.Text;

namespace You.In
{
    public class SendEventData
    {

        private readonly String _facebook_uri = "https://api.facebook.com/method/events.create?access_token={0}";
        private string _eid=String.Empty;
        private string m_fileid = "uin.jpg";
        public void UpdateEventImage(string eid)
        {
            _eid=eid;
            Uri editUri = new Uri("https://api.facebook.com/method/events.edit");
            HttpWebRequest request3 = WebRequest.Create(editUri) as HttpWebRequest;
            request3.Method = "POST";
            request3.BeginGetRequestStream(new AsyncCallback(EventEditRequestCallback), request3);
        }

             private void EventEditRequestCallback(IAsyncResult ar)
        {
            HttpWebRequest request = ar.AsyncState as HttpWebRequest;
            System.IO.Stream requestStream = request.EndGetRequestStream(ar);
            StreamWriter streamWriter = new StreamWriter(requestStream);           
            System.Text.StringBuilder headerBuilder = new System.Text.StringBuilder(1024);
            
            string boundary = new Guid().ToString("D");
           
            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");
            headerBuilder.Append("Content-Disposition: form-data; name=\"");
            headerBuilder.Append("access_token");
            headerBuilder.Append("\"\r\n\r\n");
            headerBuilder.Append(AppSettings.TokenSetting);
            headerBuilder.Append("\r\n");

            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");
            headerBuilder.Append("Content-Disposition: form-data; name=\"");
            headerBuilder.Append("eid");
            headerBuilder.Append("\"\r\n\r\n");
            headerBuilder.Append(_eid);
            headerBuilder.Append("\r\n");

            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");

            headerBuilder.Append("Content-Disposition: form-data; name=\"");
            headerBuilder.Append("event_info");
            headerBuilder.Append("\"\r\n\r\n");
            headerBuilder.Append("{}");          
            headerBuilder.Append("\r\n");
            
            headerBuilder.Append("--");
            headerBuilder.Append(boundary);
            headerBuilder.Append("\r\n");
            headerBuilder.Append("Content-Disposition: form-data; filename=\"");
            headerBuilder.Append("uin.jpg");
            headerBuilder.Append("\"\r\n");
            headerBuilder.Append("Content-Type: ");
            headerBuilder.Append("image/jpg");
            headerBuilder.Append("\r\n\r\n");

            System.IO.IsolatedStorage.IsolatedStorageFileStream stream;
            using (System.IO.IsolatedStorage.IsolatedStorageFile isoFile = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                stream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(m_fileid, FileMode.Open, isoFile);
            }

            string header = headerBuilder.ToString();
            string footer = "\r\n--" + boundary;

            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            byte[] footerBytes = Encoding.UTF8.GetBytes(footer);

            request.ContentType = "multipart/form-data; boundary=" + boundary;

            requestStream.Write(headerBytes, 0, headerBytes.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = 0;

            while ((bytesRead = stream.Read(buffer, 0, 1024)) != 0)
            {
                requestStream.Write(buffer, 0, bytesRead);
            }
            requestStream.Write(footerBytes, 0, footerBytes.Length);
            streamWriter.Flush();
            streamWriter.Close();
            stream.Close();
            stream.Dispose();
            request.BeginGetResponse(new AsyncCallback(EventEditResponseCallback), request);
        }

        private void EventEditResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                {
                    String x = streamReader1.ReadToEnd();
                }
            }
            catch (Exception) { }
        }
        }

    
}
