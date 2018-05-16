using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KawaFling
{
    class Program
    {

        public static void Main(string[] args)
        {
            // OAuth 인증 절차
            try
            {
                StreamReader read = new StreamReader("../../../config.txt", Encoding.UTF8);
                TwitterRequest.Consumer_secret = read.ReadLine();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("파일을 찾을 수 없습니다.");
            }

            if (!File.Exists("../../../AccessToken.json"))
            {
                TwitterRequest.GetRequestToken();
                TwitterRequest.oauth_verifier = Console.ReadLine();
                TwitterRequest.GetAccessToken();

                File.WriteAllText("../../../AccessToken.json",
                    new JObject(new JProperty("access_token", TwitterRequest.access_token),
                                new JProperty("access_token_secret", TwitterRequest.access_token_secret)).ToString());
            }
            else
            {
                JObject @object = JObject.Parse(File.ReadAllText("../../../AccessToken.json"));
                TwitterRequest.SetTokenFromFile(@object["access_token"].ToString(), @object["access_token_secret"].ToString());
            }

            TwitterRequest.PostTweets("플링 사랑해!");
        }

    }
}
