using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VosSoft.ZuneLcd.Api;
using System.Threading;
using System.ServiceModel;



namespace zpd
{
    [ServiceContract(Namespace = "http://zpd")]
    public interface IZPDClient
    {
        [OperationContract]
        void Play();
        [OperationContract]
        void Pause();
        [OperationContract]
        void Close();
    }

    class Program
    {
        static ZuneApi zune;
        static void Main(string[] args)
        {
            zune = new ZuneApi();
            var zuneThread = new Thread(ZuneThread);
            zuneThread.Start();
            do
            {
                Thread.Sleep(1000);
            }
            while (!zune.IsReady);

            zune.Play();
            var c = Console.ReadKey();
            zune.Close();
            zuneThread.Join();
        }

        static void ZuneThread()
        {
            zune.Launch();
        }
    }
}
