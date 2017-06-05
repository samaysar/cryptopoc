using System;
using Microsoft.Owin.Hosting;

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
        }
    }
}
