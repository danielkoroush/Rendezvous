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
using Paco.Application.ServiceReference;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections.Generic;

namespace Paco.Application
{
    public static class Tracing
    {
        private static object lockObject = new object();
        public static TracingLevel TraceLevel = TracingLevel.Instrumentation;
        private static Guid SessionGuid = Guid.NewGuid();
        public static Version Version = new Version(1, 0, 0, 0);
        private static EndpointAddress EndpointAddress = new EndpointAddress("http://paco.myvnc.com/Paco/Service.svc");
        public static List<Trace> TraceList = new List<Trace>();
        private static ServiceClient serviceClient;
        private static ServiceClient ServiceClient            
        {
            get
            {
                if (serviceClient == null)
                {
                    lock (lockObject)
                    {
                        if (serviceClient == null)
                        {
                            serviceClient = new ServiceClient(new BasicHttpBinding(), EndpointAddress);
                        }
                    }
                }

                return serviceClient;
            }

        }

        public static void TraceException(Exception ex, string tracePointName)
        {
            Trace trace = new Trace();
            trace.TraceDetails = ex.ToString();
            trace.TraceMessage = ex.Message;
            trace.TracePointName = tracePointName;

            trace.SentDateTime = DateTime.Now;
            trace.SessionGuid = SessionGuid;
            trace.SourceType = (int)TraceSourceType.Client;
            trace.SourceVersion = Version.ToString();
            trace.UserId = FacebookData.CurrentUser == null ? "" : FacebookData.CurrentUser.Uid.ToString();


            Trace(trace);
 
        }

        public static void Trace(Trace trace)
        {
            ServiceClient.TraceCompleted +=new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_TraceCompleted);
            lock (lockObject)
            {
                TraceList.Add(trace);
            }
            
            ServiceClient.TraceAsync(trace, trace);
        }


        static void  ServiceClient_TraceCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
 	        if (!e.Cancelled)
            {
                lock(lockObject)
                {
                    TraceList.Remove((Trace)e.UserState);
                }
            }
        }
    }

    public enum TracingLevel
    {
        Trace,
        Instrumentation,
        Exception
    }

    public enum TraceSourceType
    {
        Client = 0,
    }
}
