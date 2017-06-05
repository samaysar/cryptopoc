using System;
using System.IO;
using System.Text;
using Microsoft.Owin.Hosting;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace CrptoTrade
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<ApiBootStrap>("http://localhost:9001"))
            {
                Console.WriteLine("Server started on port 9001... press ENTER to shut down");
                Console.ReadLine();
            }
            //            var file = new FileStream(@"C:\Users\Ours\Downloads\response.json", FileMode.Create,
            //                FileAccess.ReadWrite, FileShare.None, 8 * 1024, FileOptions.Asynchronous);
            //            var writer = new StreamWriter(file, Encoding.UTF8, 8 * 1024, true)
            //            {
            //                AutoFlush = true
            //            };
            //            var websocket = new WebSocket("wss://ws-feed.gdax.com");
            //            websocket.Opened += (sender, eventArgs) =>
            //            {
            //                Console.Out.WriteLine("OPENED ---------");
            //                websocket.Send(@"{
            //    ""type"": ""subscribe"",
            //    ""product_ids"": [
            //        ""BTC-USD"",
            //        ""ETH-USD"",
            //        ""ETH-BTC""
            //    ]
            //}");
            //            };
            //            websocket.Error += (sender, eventArgs) =>
            //            {
            //                Console.Out.WriteLine("ERROR:" + eventArgs.Exception);
            //            };
            //            websocket.Closed += (sender, eventArgs) =>
            //            {
            //                Console.Out.WriteLine("---------------- CLOSED");
            //                using (file)
            //                {
            //                    using (writer)
            //                    {
            //                        Console.ReadLine();
            //                    }
            //                }
            //            };
            //            websocket.MessageReceived += (sender, eventArgs) =>
            //            {
            //                writer?.WriteLine(eventArgs.Message);
            //            };
            //            websocket.Open();
            //            Console.ReadLine();
            //            websocket.Close();
        }
    }
}
