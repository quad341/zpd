using System;

namespace TestingClient
{
    class Program
    {
        static void Main()
        {
            var client = new ZPDServiceClient();
            var tracks = client.Search("nine");
            foreach (var track in tracks)
            {
                Console.WriteLine("We found {0} by {1} on {2}", track.Name, track.Artist, track.Album);
            }
            Console.WriteLine("Press <enter> to close player and client");
            Console.ReadLine();
            client.Close();
        }
    }
}
