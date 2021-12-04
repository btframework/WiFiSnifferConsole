using System;

using wclCommon;

namespace WiFiSnifferConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting sniffer");

            SnifferThread WiFiSniffer = new SnifferThread();
            if (WiFiSniffer.Run() != wclErrors.WCL_E_SUCCESS)
                Console.WriteLine("Start sniffer failed");

            Console.WriteLine("Press ENTER to terminate.");
            Console.ReadLine();

            if (WiFiSniffer.Running)
                WiFiSniffer.Terminate();

            WiFiSniffer = null;
        }
    }
}
