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
    //TODO: many methods should support authentication in some way
    [ServiceContract(Namespace = "http://zpd")]
    public interface IZPDService
    {
        [OperationContract]
        void Play();
        [OperationContract]
        void PlaySongIndex(int index);
        [OperationContract]
        void Pause();
        [OperationContract]
        void Stop();
        [OperationContract]
        void NextTrack();
        [OperationContract]
        void PreviousTrack();
        [OperationContract]
        void ToggleFastForward();
        [OperationContract]
        void ToggleRewind();
        [OperationContract]
        void ToggleShuffle();
        [OperationContract]
        void ToggleRepeat();
        [OperationContract]
        void ReIndexLibrary();
        //TODO:Search needs to return sane types

        [OperationContract]
        void QueueTrack(int mediaId, int mediaTypeId);
        [OperationContract]
        void QueueTrackAtIndex(int mediaId, int mediaTypeId, int index);
        [OperationContract]
        void RemoveTrackAtIndex(int index);
        //TODO:Get current track should return sane type; should get current info for current track and time
        //TODO:Get current queue
        [OperationContract]
        void ClosePlayer();
    }

    class ZPDService : IZPDService
    {
        //TODO: this shouldn't be directly calling the ZuneApi; should make intermediate for MediaPlayerManager to take care of threading, etc.
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
