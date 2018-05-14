using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.IO;
namespace KawaFling
{
    class Program
    {

        public static void Main(string[] args)
        {
            using (StreamReader read = new StreamReader("../../../config.txt", Encoding.UTF8))
            {
                TwitterRequest.Consumer_secret = read.ReadLine();
            }
          
        }
        
    }
}
