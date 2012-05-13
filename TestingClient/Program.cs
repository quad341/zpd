using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace TestingClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ZPDServiceClient();
            client.Play();
            Console.WriteLine("Should be playing; waits 10 seconds to ensure player is open though");
            Console.WriteLine("Press <enter> to close player and client");
            Console.ReadLine();
            client.Close();
        }
    }
}
