using System;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace KawaFling
{
    class OAuth
    {
        static Token isNeed;

        public static string access_token;
        public static string access_token_secret;

        static string oauth_token;
        static string oauth_token_secret;
        public static string oauth_verifier;

        static string Callback = "oob";
        static string ConsumerKey = "xUV8UwzpJk4aMyTFzJXXXv4eQ";
        static string SignatureMethod = "HMAC-SHA1";
        static string version = "1.0";

        public static string Consumer_secret {private get; set; }

        public static void GetAccessToken()
        {
            isNeed = Token.oauth;

            string Timestamp = GetTimestamp();
            string Nonce = GetNonce(Timestamp);
            string SignatureBaseString = GetSignatureBaseString(Timestamp, Nonce,
                new Uri(@"https://api.twitter.com/oauth/access_token"), "POST");
            string SHA1 = GetSha1Hash(Consumer_secret, oauth_token_secret, SignatureBaseString);

            HttpWebRequest httpWebRequest = 
                (HttpWebRequest)WebRequest.Create(String.IsNullOrEmpty(oauth_verifier) ? @"https://api.twitter.com/oauth/access_token" : @"https://api.twitter.com/oauth/access_token?oauth_verifier=" + oauth_verifier);

            httpWebRequest.Method = "GET";
            httpWebRequest.ContentLength = 0;
            httpWebRequest.UseDefaultCredentials = true;
            


            string Header =
                "OAuth realm=\"Twitter API\"," +
                "oauth_consumer_key=" + '"' + ConsumerKey + '"' + "," +
                "oauth_nonce=" + '"' + Nonce + '"' + "," +
                "oauth_signature_method=" + '"' + SignatureMethod + '"' + "," +
                "oauth_timestamp=" + '"' + Timestamp + '"' + "," +
                "oauth_token=" + '"' + oauth_token + '"' + "," +
                "oauth_version=" + '"' + version + '"' + "," +
                "oauth_signature=" + '"' + Uri.EscapeDataString(SHA1) + '"';

            httpWebRequest.Headers.Add("Authorization", Header);
            Console.WriteLine(httpWebRequest.Headers);
            Console.WriteLine(httpWebRequest.RequestUri);
            var webResponse = httpWebRequest.GetResponse();

            Stream responseStream = webResponse.GetResponseStream();
            string responseBody = String.Empty;
            if (responseStream != null) responseBody = new StreamReader(responseStream).ReadToEnd();

            access_token = Regex.Match(responseBody, @"oauth_token=([^&]+)").Groups[1].Value;
            access_token_secret = Regex.Match(responseBody, @"oauth_token_secret=([^&]+)").Groups[1].Value;
            Console.Write("\n{0}\n\n{1}", access_token, access_token_secret);

            isNeed = 0;
        }

        public static void GetRequestToken()
        {
            isNeed = Token.callback;

            string Timestamp = GetTimestamp();
            string Nonce = GetNonce(Timestamp);
            string SignatureBaseString = GetSignatureBaseString(Timestamp, Nonce,
                new Uri(@"https://api.twitter.com/oauth/request_token"), "POST");
            string SHA1 = GetSha1Hash(Consumer_secret, String.Empty, SignatureBaseString);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(@"https://api.twitter.com/oauth/request_token?oauth_callback=oob");
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = 0;
            httpWebRequest.UseDefaultCredentials = true;



            string Header =
                "OAuth realm=\"Twitter API\"," +
                "oauth_consumer_key=" + '"' + ConsumerKey + '"' + "," +
                "oauth_nonce=" + '"' + Nonce + '"' + "," +
                "oauth_version=" + '"' + version + '"' + "," +
                "oauth_signature_method=" + '"' + SignatureMethod + '"' + "," +
                "oauth_timestamp=" + '"' + Timestamp + '"' + "," +
                "oauth_signature = " + '"' + Uri.EscapeDataString(SHA1) + '"';
            

            httpWebRequest.Headers.Add("Authorization", Header);
            Console.WriteLine(httpWebRequest.Headers);
            Console.WriteLine(httpWebRequest.RequestUri);
            var webResponse = httpWebRequest.GetResponse();

            Stream responseStream = webResponse.GetResponseStream();
            string responseBody = String.Empty;
            if (responseStream != null) responseBody = new StreamReader(responseStream).ReadToEnd();
            Console.WriteLine("request token 성공!\n\n");

            oauth_token = ParseQuerystringParameter("oauth_token", responseBody);
            oauth_token_secret = ParseQuerystringParameter("oauth_token_secret", responseBody);
            oauth_verifier = ParseQuerystringParameter("oauth_verifier", responseBody);

            Console.WriteLine("{0}\n{1}\n{2}\n", oauth_token, oauth_token_secret, oauth_verifier);

            isNeed = 0;
        }

        static private string GetTimestamp() {
            //return "1234";
            return ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }
        static private string GetNonce(string Timestamp) {
            //return "abcdef";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Timestamp + Timestamp + Timestamp));
        }

        private static string ParseQuerystringParameter(string parameterName, string text)
        {
            Match expressionMatch = Regex.Match(text, string.Format(@"{0}=(?<value>[^&]+)", parameterName));

            if (!expressionMatch.Success)
            {
                return string.Empty;
            }

            return expressionMatch.Groups["value"].Value;
        }

        public static string GetSha1Hash(string key, string key2, string message)
        {
            string Sha1Result = String.Empty;

            string keys = string.Format(
                CultureInfo.InvariantCulture,
                "{0}&{1}", Uri.EscapeDataString(key), Uri.EscapeDataString(key2));

            using (HMACSHA1 SHA1 = new HMACSHA1(Encoding.UTF8.GetBytes(keys)))
            {
                var Hashed = SHA1.ComputeHash(Encoding.UTF8.GetBytes(message));
                Sha1Result = Convert.ToBase64String(Hashed);
            }
            Console.WriteLine(Sha1Result);
            Console.WriteLine();
            return Sha1Result;
        }

        private static string GetSignatureBaseString(string timestamp, string nonce, Uri uri, string method)
        {
            //1.Convert the HTTP Method to uppercase and set the output string equal to this value.
            string Signature_Base_String = method;
            Signature_Base_String = Signature_Base_String.ToUpper();

            //2.Append the ‘&’ character to the output string.
            Signature_Base_String = Signature_Base_String + "&";

            //3.Percent encode the URL and append it to the output string.
            string PercentEncodedURL = Uri.EscapeDataString(uri.ToString());

            Signature_Base_String = Signature_Base_String + PercentEncodedURL;

            //4.Append the ‘&’ character to the output string.
            Signature_Base_String = Signature_Base_String + "&";

            //5.append parameter string to the output string.
            if (isNeed.HasFlag(Token.callback))
                Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("oauth_callback=" + Callback);
            Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_consumer_key=" + ConsumerKey);
            Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_nonce=" + nonce);
            Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_signature_method=" + SignatureMethod);
            Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_timestamp=" + timestamp);
            if(isNeed.HasFlag(Token.oauth))
                Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_token=" + oauth_token);
            Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_version=" + version);
            Console.WriteLine(Signature_Base_String);
            return Signature_Base_String;
        }
    }
}
