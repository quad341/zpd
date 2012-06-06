using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace zpd
{
    public static class TolkenAuthenticator
    {
        private static List<int> s_connectionCounter;

        public static string AuthString { get; private set; }
        public static AuthTolkenTimeout AcceptedAuthTolkenTimeouts { get; private set; }

        public static bool IsValid(AuthPacket authPacket)
        {
            return AuthTolkenTimeout.Unset == authPacket.Timeout
                       ? IsValid(authPacket.AuthTolken, authPacket.Offset, authPacket.ClientId)
                       : IsValid(authPacket.Timeout, authPacket.AuthTolken, authPacket.Offset, authPacket.ClientId);
        }

        public static bool IsValid(string authTolken, int adjustedAmount, int clientId)
        {
            Debug.Assert(AuthTolkenTimeout.Any != AcceptedAuthTolkenTimeouts);
            var valid = false;
            if (AuthTolkenTimeout.Any != AcceptedAuthTolkenTimeouts)
            {
                valid = IsValid(AcceptedAuthTolkenTimeouts, authTolken, adjustedAmount, clientId);
            }

            return valid;
        }

        public static bool IsValid(AuthTolkenTimeout timeout, string authTolken, int adjustedAmount, int clientId)
        {
            return IsValid(timeout, authTolken, adjustedAmount, clientId, false /*adjustForTransmission*/);
        }

        public static bool IsValid(AuthTolkenTimeout timeout, string authTolken, int adjustedAmount, int clientId, bool adjustForTransmission)
        {
            var valid = false;
            // This has to be single threaded to avoid race conditions for authenticating the client packet count
            lock (typeof(TolkenAuthenticator))
            {
                if (AcceptedAuthTolkenTimeouts == AuthTolkenTimeout.NoAuth)
                {
                    valid = true;
                }
                else if (AuthTolkenTimeout.Any != timeout &&
                         (AcceptedAuthTolkenTimeouts == AuthTolkenTimeout.Any || AcceptedAuthTolkenTimeouts == timeout))
                {
                    var now = DateTime.UtcNow;
                    var numAdjustSeconds = 0;

                    // We have to round our seconds to the every nth second based on timeout
                    switch (timeout)
                    {
                        case AuthTolkenTimeout.FiveSeconds:
                            numAdjustSeconds = 5 + (5 - (now.Second%5));
                            numAdjustSeconds -= adjustForTransmission && numAdjustSeconds > adjustedAmount ? 5 : 0;
                            break;
                        case AuthTolkenTimeout.TenSecionds:
                            numAdjustSeconds = 10 + (10 - (now.Second%10));
                            numAdjustSeconds -= adjustForTransmission && numAdjustSeconds > adjustedAmount ? 10 : 0;
                            break;
                        case AuthTolkenTimeout.ThirtySeconds:
                            numAdjustSeconds = 30 + (30 - (now.Second%30));
                            numAdjustSeconds -= adjustForTransmission && numAdjustSeconds > adjustedAmount ? 30 : 0;
                            break;
                        case AuthTolkenTimeout.SixtySeconds:
                            numAdjustSeconds = 60 + (60 - (now.Second%60));
                            numAdjustSeconds -= adjustForTransmission && numAdjustSeconds > adjustedAmount ? 60 : 0;
                            break;
                        default:
                            Debug.Assert(false, "Unexpected timeout");
                            break;
                    }

                    var adjustedTime = now.AddSeconds(numAdjustSeconds);
                    // we only want granularity down to seconds
                    var adjustedTimeToSecond = new DateTime(adjustedTime.Year,
                                                            adjustedTime.Month,
                                                            adjustedTime.Day,
                                                            adjustedTime.Hour,
                                                            adjustedTime.Minute,
                                                            adjustedTime.Second);

                    var currentClientCounter = s_connectionCounter[clientId - 1];

                    var computedAuthTolken =
                        Sha1HashOfString(adjustedTimeToSecond.ToString("yyyy-MM-dd:HH:mm:ss") + AuthString +
                                         currentClientCounter);

                    valid = authTolken == computedAuthTolken;

                    if (!valid && !adjustForTransmission)
                    {
                        // lets try one last time adjusting
                        valid = IsValid(timeout, authTolken, adjustedAmount, clientId, true /*adjustForTransmission*/);
                    }
                    else if (valid)
                    {
                        IncrementClient(clientId);
                    }

                }
            }
            return valid;
        }

        public static void Init(AuthTolkenTimeout authTolkenTimeout)
        {
            Debug.Assert(null == AuthString);
            AcceptedAuthTolkenTimeouts = authTolkenTimeout;
            AuthString = GetRandomString();
            s_connectionCounter = new List<int>(0);
        }

        public static int GetNewClientId()
        {
            s_connectionCounter.Add(0); // add the initial message count
            return s_connectionCounter.Count; // the id is the 1 based index, i.e. validate _connecitonCounter[id-1]==id
        }

        private static void IncrementClient(int clientId)
        {
            s_connectionCounter[clientId - 1]++;
        }

        private static string GetRandomString()
        {
#if DEBUG
            return "d3bug";
#else
            return Path.GetRandomFileName().Replace(".", "");
#endif //DEBUG
        }

        // Taken from http://dotnetpulse.blogspot.com/2007/12/sha1-hash-calculation-in-c.html
        private static string Sha1HashOfString(string input)
        {
            var buffer = Encoding.Unicode.GetBytes(input);
            var crypto = new SHA256Managed();

            return BitConverter.ToString(crypto.ComputeHash(buffer)).Replace("-", "");
        }

    }
}