using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace zpd
{
    public static class TolkenAuthenticator
    {
        public static string AuthString { get; private set; }
        public static AuthTolkenTimeout AcceptedAuthTolkenTimeouts { get; private set; }

        public static bool IsValid(AuthPacket authPacket)
        {
            return AuthTolkenTimeout.Unset == authPacket.Timeout
                       ? IsValid(authPacket.AuthTolken, authPacket.Offset)
                       : IsValid(authPacket.Timeout, authPacket.AuthTolken, authPacket.Offset);
        }

        public static bool IsValid(string authTolken, int adjustedAmount)
        {
            Debug.Assert(AuthTolkenTimeout.Any != AcceptedAuthTolkenTimeouts);
            var valid = false;
            if (AuthTolkenTimeout.Any != AcceptedAuthTolkenTimeouts)
            {
                valid = IsValid(AcceptedAuthTolkenTimeouts, authTolken, adjustedAmount);
            }

            return valid;
        }

        public static bool IsValid(AuthTolkenTimeout timeout, string authTolken, int adjustedAmount)
        {
            var valid = false;
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
                        numAdjustSeconds -= numAdjustSeconds > adjustedAmount ? 5 : 0;
                        break;
                    case AuthTolkenTimeout.TenSecionds:
                        numAdjustSeconds = 10 + (10 - (now.Second%10));
                        numAdjustSeconds -= numAdjustSeconds > adjustedAmount ? 10 : 0;
                        break;
                    case AuthTolkenTimeout.ThirtySeconds:
                        numAdjustSeconds = 30 + (30 - (now.Second%30));
                        numAdjustSeconds -= numAdjustSeconds > adjustedAmount ? 30 : 0;
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

                var computedAuthTolken =
                    Sha1HashOfString(adjustedTimeToSecond.ToString("yyyy-MM-dd:HH:mm:ss") + AuthString);

                valid = authTolken == computedAuthTolken;

            }
            return valid;
        }

        public static void Init(AuthTolkenTimeout authTolkenTimeout)
        {
            Debug.Assert(null == AuthString);
            AcceptedAuthTolkenTimeouts = authTolkenTimeout;
            AuthString = GetRandomString();
        }

        private static string GetRandomString()
        {
            return Path.GetRandomFileName().Replace(".", "");
        }

        // Taken from http://dotnetpulse.blogspot.com/2007/12/sha1-hash-calculation-in-c.html
        private static string Sha1HashOfString(string input)
        {
            var buffer = Encoding.ASCII.GetBytes(input);
            var crypto = new SHA1CryptoServiceProvider();

            return BitConverter.ToString(crypto.ComputeHash(buffer)).Replace("-", "");
        }

    }
}