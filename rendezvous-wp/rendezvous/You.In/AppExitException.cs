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

namespace You.In
{
    /// <summary>
    /// Exception thrown to exit the app.
    /// </summary>
    public class AppExitException : Exception
    {
        public AppExitException()
            : base("Exiting app.")
        {
        }
    }
}
