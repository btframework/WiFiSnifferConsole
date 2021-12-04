#include "stdafx.h"
#include "Sniffer.h"

#include <iostream>

using namespace std;

void WriteLn(const char* str)
{
	cout << str << endl;
}

void WriteLn(const char* str, const char* str2)
{
	cout << str << str2 << endl;
}

void WriteLn(const char* str, int Res)
{
	cout << str << hex << Res << dec << endl;
}

void WriteLn(const char* str, size_t size, const char* str2)
{
	cout << str << size << str2 << endl;
}

CSnifferThread::CSnifferThread()
	: CwclThread()
{
	WriteLn("Changing synchronization method");
	CwclMessageBroadcaster::SetSyncMethod(skApc);
	
	WriteLn("Preparing components");
	
	FClient = new CwclWiFiClient();
	__hook(&CwclWiFiClient::AfterOpen, FClient, &CSnifferThread::ClientAfterOpen);
	__hook(&CwclWiFiClient::BeforeClose, FClient, &CSnifferThread::ClientBeforeClose);
	
	FSniffer = new CwclWiFiSniffer();
	__hook(&CwclWiFiSniffer::AfterOpen, FSniffer, &CSnifferThread::SnifferAfterOpen);
	__hook(&CwclWiFiSniffer::BeforeClose, FSniffer, &CSnifferThread::SnifferBeforeClose);
	__hook(&CwclWiFiSniffer::OnRawFrameReceived, FSniffer, &CSnifferThread::SnifferRawFrameReceived);
	
	WriteLn("Components are ready");
}

CSnifferThread::~CSnifferThread()
{
	WriteLn("Releasing components");
	
	delete FSniffer;
	delete FClient;
	
	WriteLn("Components released");
}

bool CSnifferThread::OnInitialize()
{
	WriteLn("Create initialization completion event");
	FInitEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
	if (FInitEvent == NULL)
		WriteLn("Create initialization completion event failed");
	else
	{
		FInitResult = WCL_E_SUCCESS;
		
		WriteLn("Opening WiFi client");
		int Res = FClient->Open();
		if (Res != WCL_E_SUCCESS)
		{
			FInitResult = Res;
			WriteLn("WiFi client open failed: 0x", FInitResult);
		}
		else
		{
			WriteLn("Wait for WiFi sniffer initialization");
			Res = CwclMessageBroadcaster::Wait(FInitEvent);
			if (Res != WCL_E_SUCCESS)
			{
				WriteLn("Wait failed: 0x", Res);
				FInitResult = Res;
			}
			
			WriteLn("Initialization completed with result: 0x", FInitResult);
			
			if (FInitResult != WCL_E_SUCCESS)
			{
				WriteLn("Closing WiFi client");
				FClient->Close();
			}
		}
		
		CloseHandle(FInitEvent);
	}
	
	return (FInitResult == WCL_E_SUCCESS);
}

void CSnifferThread::OnTerminate()
{
	WriteLn("Stopping WiFi Sniffer");
	if (FSniffer->GetActive())
		FSniffer->Close();
	
	WriteLn("Closing WiFi Client");
	if (FClient->GetActive())
		FClient->Close();
}

void CSnifferThread::ClientAfterOpen(void* Sender)
{
	WriteLn("WiFi client opened");
	WriteLn("Get WiFi interfaces");
	
	wclWiFiInterfaces Ifaces;
	FInitResult = FClient->EnumInterfaces(Ifaces);
	if (FInitResult != WCL_E_SUCCESS)
		WriteLn("Enum interfaces failed: 0x", FInitResult);
	else
	{
		if (Ifaces.size() == 0)
		{
			WriteLn("No WiFi intyerfaces found");
			FInitResult = WCL_E_WIFI_NOT_AVAILABLE;
		}
		else
		{
			WriteLn("Found ", Ifaces.size(), " interfaces");
			
			WriteLn("Start WiFi sniffer");
			FInitResult = FSniffer->Open(Ifaces[0].Id);
			if (FInitResult != WCL_E_SUCCESS)
				WriteLn("Start sniffer failed: 0x", FInitResult);
		}
	}
	
	SetEvent(FInitEvent);
}

void CSnifferThread::ClientBeforeClose(void* Sender)
{
	WriteLn("WiFi client is closing");
	
	if (FSniffer->GetActive())
	{
		WriteLn("Closing WiFi sniffer");
		int Res = FSniffer->Close();
		if (Res != WCL_E_SUCCESS)
			WriteLn("Close sniffer failed: 0x", Res);
	}
}

void CSnifferThread::SnifferAfterOpen(void* Sender)
{
	WriteLn("Sniffer started");
	WriteLn("Here is the good place to set Phy and Channel if needed");
}

void CSnifferThread::SnifferBeforeClose(void* Sender)
{
	WriteLn("Sniffer is closing");
	WriteLn("Do cleanup here if needed");
}

void CSnifferThread::SnifferRawFrameReceived(void* Sender, const void* const Buffer,
	const unsigned long Size)
{
	WriteLn("Raw frame received");
}