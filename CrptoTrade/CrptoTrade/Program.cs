using System;
using Microsoft.Owin.Hosting;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace CrptoTrade
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (WebApp.Start<ApiBootStrap>("http://localhost:9001"))
            //{
            //    Console.WriteLine("Server started on port 9001... press ENTER to shut down");
            //    Console.ReadLine();
            //}

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
//            };
//            websocket.MessageReceived+= (sender, eventArgs) =>
//            {
//                Console.Out.WriteLine("Message" + eventArgs.Message);
//            };
//            websocket.Open();
//            Console.ReadLine();
        }
    }
}
