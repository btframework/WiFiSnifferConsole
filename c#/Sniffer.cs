using System;
using System.Threading;

using wclCommon;
using wclWiFi;

namespace WiFiSnifferConsole
{
    public class SnifferThread : wclThread
    {
        private wclWiFiClient FClient;
        private ManualResetEvent FInitEvent;
        private Int32 FInitResult;
        private wclWiFiSniffer FSniffer;

        private void ClientAfterOpen(Object Sender, EventArgs e)
        {
            Console.WriteLine("WiFi client opened");
            Console.WriteLine("Get WiFi interfaces");

            wclWiFiInterfaceData[] Ifaces;
            FInitResult = FClient.EnumInterfaces(out Ifaces);
            if (FInitResult != wclErrors.WCL_E_SUCCESS)
                Console.WriteLine("Enum interfaces failed: 0x" + FInitResult.ToString("X8"));
            else
            {
                if (Ifaces == null || Ifaces.Length == 0)
                {
                    Console.WriteLine("No WiFi intyerfaces found");
                    FInitResult = wclWiFiErrors.WCL_E_WIFI_NOT_AVAILABLE;
                }
                else
                {
                    Console.WriteLine("Found " + Ifaces.Length.ToString() + " interfaces");
                    Guid Id = Ifaces[0].Id;
                    Console.WriteLine("Use " + Id.ToString());

                    Console.WriteLine("Start WiFi sniffer");
                    FInitResult = FSniffer.Open(Id);
                    if (FInitResult != wclErrors.WCL_E_SUCCESS)
                        Console.WriteLine("Start sniffer failed: 0x" + FInitResult.ToString("X8"));
                }
            }
            
            FInitEvent.Set();
        }

        private void ClientBeforeClose(Object Sender, EventArgs e)
        {
            Console.WriteLine("WiFi client is closing");

            if (FSniffer.Active)
            {
                Console.WriteLine("Closing WiFi sniffer");
                Int32 Res = FSniffer.Close();
                if (Res != wclErrors.WCL_E_SUCCESS)
                    Console.WriteLine("Close sniffer failed: 0x" + Res.ToString("X8"));
            }
        }

        private void SnifferAfterOpen(Object Sender, EventArgs e)
        {
            Console.WriteLine("Sniffer started");
            Console.WriteLine("Here is the good place to set Phy and Channel if needed");
        }

        private void SnifferBeforeClose(Object Sender, EventArgs e)
        {
            Console.WriteLine("Sniffer is closing");
            Console.WriteLine("Do cleanup here if needed");
        }

        private void SnifferRawFrameReceived(Object Sender, Byte[] Buffer)
        {
            Console.WriteLine("Raw frame received");
        }

        protected override Boolean OnInitialize()
        {
            Console.WriteLine("Create initialization completion event");
            try { FInitEvent = new ManualResetEvent(false); } catch { FInitEvent = null; }
            if (FInitEvent == null)
                Console.WriteLine("Create initialization completion event failed");
            else
            {
                FInitResult = wclErrors.WCL_E_SUCCESS;

                Console.WriteLine("Opening WiFi client");
                Int32 Res = FClient.Open();
                if (Res != wclErrors.WCL_E_SUCCESS)
                {
                    FInitResult = Res;
                    Console.WriteLine("WiFi client open failed: 0x" + FInitResult.ToString("X8"));
                }
                else
                {
                    Console.WriteLine("Wait for WiFi sniffer initialization");
                    Res = wclMessageBroadcaster.Wait(FInitEvent);
                    if (Res != wclErrors.WCL_E_SUCCESS)
                    {
                        Console.WriteLine("Wait failed: 0x" + Res.ToString("X8"));
                        FInitResult = Res;
                    }

                    Console.WriteLine("Initialization completed with result: 0x" + FInitResult.ToString("X8"));

                    if (FInitResult != wclErrors.WCL_E_SUCCESS)
                    {
                        Console.WriteLine("Closing WiFi client");
                        FClient.Close();
                    }
                }

                FInitEvent.Close();
            }

            return (FInitResult == wclErrors.WCL_E_SUCCESS);
        }

        protected override void OnTerminate()
        {
            Console.WriteLine("Stopping WiFi Sniffer");
            if (FSniffer.Active)
                FSniffer.Close();

            Console.WriteLine("Closing WiFi Client");
            if (FClient.Active)
                FClient.Close();
        }

        public SnifferThread()
            : base()
        {
            Console.WriteLine("Changing synchronization method");
            wclMessageBroadcaster.SetSyncMethod(wclMessageSynchronizationKind.skApc);

            Console.WriteLine("Preparing components");

            FClient = new wclWiFiClient();
            FClient.AfterOpen += ClientAfterOpen;
            FClient.BeforeClose += ClientBeforeClose;

            FSniffer = new wclWiFiSniffer();
            FSniffer.AfterOpen += SnifferAfterOpen;
            FSniffer.BeforeClose += SnifferBeforeClose;
            FSniffer.OnRawFrameReceived += SnifferRawFrameReceived;

            Console.WriteLine("Components are ready");
        }

        ~SnifferThread()
        {
            Console.WriteLine("Releasing components");

            FSniffer = null;
            FClient = null;

            Console.WriteLine("Components released");
        }
    };
}