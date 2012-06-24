using System;
using System.Collections;
using System.Linq;
using System.Web;

namespace ZpdWebClient.Models
{
    public static class RequestManager
    {
        private static Hashtable _requests;

        public static Hashtable Requests { get { return _requests ?? (_requests = new Hashtable()); } }

        public static DateTime? GetLastRequestedTime(int mediaId)
        {
            DateTime? lastRequestedTime = null;
            if (Requests.ContainsKey(mediaId))
            {
                lastRequestedTime = _requests[mediaId] as DateTime?;
            }

            return lastRequestedTime;
        }

        public static void UpdateLastRequestedTime(int mediaId)
        {
            if (Requests.ContainsKey(mediaId))
            {
                Requests.Remove(mediaId);
            }

            Requests.Add(mediaId, new DateTime?(DateTime.Now));
        }
    }
}