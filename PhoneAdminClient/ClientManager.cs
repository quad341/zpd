using System.ServiceModel;
using PhoneAdminClient.ZpdService;

namespace PhoneAdminClient
{
    public static class ClientManager
    {
        /// <summary>
        /// Number of requests we think should be possible to have open before we think we are not connected
        /// </summary>
        public const int MAX_OPEN_REQUESTS = 3;

        public static bool CanOpenMoreRequests
        {
            get
            {
                return MAX_OPEN_REQUESTS >= RequestCount;
            }
        }
        public static ZPDServiceClient Client { get; private set; }
        public static int RequestCount { get; set; }
        public static int ClientGeneration { get; private set; }

        public static ZPDServiceClient InitAndGetClient()
        {
            if (null != Client)
            {
                Client.Abort();
                Client = null;
            }

            Client = new ZPDServiceClient(new BasicHttpBinding(),
                                          new EndpointAddress("http://" + SettingsManager.Instance.Host + ":" +
                                                              SettingsManager.Instance.Port +
                                                              "/zpd"));

            ClientGeneration++;

            return Client;
        }
    }
}