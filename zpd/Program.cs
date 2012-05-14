using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VosSoft.ZuneLcd.Api;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;



namespace zpd
{
    [ServiceContract(Namespace = "http://zpd")]
    public interface IZPDService
    {
        [OperationContract]
        void Play();
        [OperationContract]
        void Pause();
        [OperationContract]
        void Close();
    }

    class ZPDService : IZPDService
    {
        ZuneApi zune;
        Thread zuneThread;

        private void EnsureZune()
        {
            if (zune == null)
            {
                Init();
                Thread.Sleep(10000);
            }
        }

        private void Init()
        {
            zune = new ZuneApi();
            zuneThread = new Thread(ZuneThread);
            zuneThread.Start();
        }

        public void Play()
        {
            EnsureZune();
            zune.Play();
        }

        public void Pause()
        {
            EnsureZune();
            zune.Pause();
        }

        public void Close()
        {
            EnsureZune();
            zune.Close();
            zuneThread.Join();
        }

        private void ZuneThread()
        {
            zune.Launch();
        }
    }

    class Program
    {
        static ZuneApi zune;
        static void Main(string[] args)
        {
            //StartService();
            zune = new ZuneApi();
            //var zuneThread = new Thread(ZuneThread);
            //zuneThread.Start();
            //Thread.Sleep(10000);
            zune.ReIndexMusic();
            Console.WriteLine(zune.Search("nine"));
            Console.ReadLine();
        }
        static void ZuneThread()
        {
            zune.Launch();
        }

        static void StartService()
        {
            Uri baseAddress = new Uri("http://localhost:8000/zpd");
            var host = new ServiceHost(typeof(ZPDService), baseAddress);
            try
            {
                host.AddServiceEndpoint(typeof(IZPDService), new WSHttpBinding(), "Service");
                var smb = new ServiceMetadataBehavior() { HttpGetEnabled = true };
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
