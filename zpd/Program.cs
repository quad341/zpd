using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;


namespace zpd
{

    class Program
    {
        static void Main()
        {
            ZuneMediaPlayerManager.EnsureInstance();
            TolkenAuthenticator.Init(AuthTolkenTimeout.FiveSeconds);
            Console.WriteLine("Auth tolken is: {0}", TolkenAuthenticator.AuthString);
            StartService();
        }
        
        static void StartService()
        {
            var localV4Address = GetLocalV4Address();

            var localAddress = "http://" + localV4Address + ":8000/zpd";
            var baseAddress = new Uri(localAddress);
            var host = new ServiceHost(typeof(ZPDService), baseAddress);
            try
            {
                //host.AddServiceEndpoint(typeof(IZPDService), new WSHttpBinding(), "Service");
                var smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
                host.Description.Behaviors.Add(smb);
                host.Open();
                Console.WriteLine("Service is ready at {0}. Press <ENTER> to terminate", localAddress);
                Console.ReadLine();
                host.Close();
                ZuneMediaPlayerManager.ClosePlayer();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                host.Abort();
            }

            Console.WriteLine("Press <ENTER> to close");
            Console.ReadLine();
        }

        private static string GetLocalV4Address()
        {
            var localIpEntry = Dns.GetHostEntry(String.Empty);
            // We will try to get non-localhost if possible
            var localV4Address = "127.0.0.1";
            foreach (var ip in localIpEntry.AddressList)
            {
                if (ProtocolFamily.InterNetwork.ToString() == ip.AddressFamily.ToString() && localV4Address != ip.ToString())
                {
                    localV4Address = ip.ToString();
                    break;
                }
            }
            return localV4Address;
        }
    }
}
