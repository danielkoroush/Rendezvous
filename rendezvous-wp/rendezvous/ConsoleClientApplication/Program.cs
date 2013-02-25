using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleClientApplication.ServiceReference1;

namespace ConsoleClientApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ServiceClient();
            client.AddUserActivity(Guid.NewGuid().ToString(), Guid.Empty, "SomeActionOnConsole", "Some Comment from Console");
        }
    }
}
