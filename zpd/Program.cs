using System;
using System.ServiceModel;
using System.ServiceModel.Description;


namespace zpd
{

    class Program
    {
        static void Main()
        {
            StartService();
            //_zune = new ZuneApi();
            //var zuneThread = new Thread(ZuneThread);
            //zuneThread.Start();
            //Thread.Sleep(10000);
            //_zune.ReIndexMusic();
            //Console.WriteLine(_zune.Search("nine"));
            Console.ReadLine();
        }
        
        static void StartService()
        {
            var baseAddress = new Uri("http://localhost:8000/zpd");
            var host = new ServiceHost(typeof(ZPDService), baseAddress);
            try
            {
                host.AddServiceEndpoint(typeof(IZPDService), new WSHttpBinding(), "Service");
                var smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
                host.Description.Behaviors.Add(smb);
                host.Open();
                Console.WriteLine("Service is ready. Press <ENTER> to terminate");
                Console.ReadLine();
                host.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                host.Abort();
            }

            Console.WriteLine("Press <ENTER> to close");
            Console.ReadLine();
        }
    }
}
