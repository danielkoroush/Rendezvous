using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace You.In
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        private string _eventTitle;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string EventTitle
        {
            get
            {
                return _eventTitle;
            }
            set
            {
                if (value != _eventTitle)
                {
                    _eventTitle = value;
                    NotifyPropertyChanged("EventTitle");
                }
            }
        }

        private string _eventAddress;
        public string EventAddress
        {
            get { return _eventAddress; }
            set
            {
                if (value != _eventAddress)
                {
                    _eventAddress = value;
                    NotifyPropertyChanged("EventAddress");
                }
            }
        }


        private string _eventConfirmed;
        public string EventConfirmed
        {
            get {
                return _eventConfirmed; }
            set
            {
                if (value != _eventConfirmed)
                {
                    _eventConfirmed = value;
                    NotifyPropertyChanged("EventConfirmed");
                }
            }
        }


        private string _eventDate;
        public string EventDate
        {
            get {
                if (_eventDate == null)
                    return "date tbd...";
                return _eventDate; }
            set
            {
                if (value != _eventDate)
                {
                    _eventDate = value;
                    NotifyPropertyChanged("EventTime");
                }
            }
        }

        private string _eventTime; 
        public string EventTime
        {
            get
            {
                if (_eventTime == null)
                    return "";
                return _eventTime;
            }
            set
            {
                if (value != _eventTime)
                {
                    _eventTime = value;
                    NotifyPropertyChanged("EventTime");
                }
            }
        }
        private string _picSource;
        public string PicSource
        {
            get
            {
                if (String.IsNullOrEmpty(_picSource))
                    return "Images/event_thumb.png";
                return _picSource;
            }
            set
            {
                if (value != _picSource)
                {
                    _picSource= value;
                    NotifyPropertyChanged("PicSource");
                }
            }
        }

        private long _eid;
        public long Eid
        {
            get
            {
                return _eid;
            }
            set
            {
                _eid = value;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}