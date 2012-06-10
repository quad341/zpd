using System.ServiceModel;
using ZpdWebClient.ZPDService;

namespace ZpdWebClient.Models
{
    public static class ClientManager
    {
        private static ZPDServiceClient s_client;

        public static ZPDServiceClient Client
        {
            get
            {
                return s_client ?? (s_client = new ZPDServiceClient(new BasicHttpBinding(),
                                                                    new EndpointAddress("http://localhost:8000/zpd")));
            }
        }
    }
}