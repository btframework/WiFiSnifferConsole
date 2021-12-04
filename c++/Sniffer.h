#pragma once

#include "..\..\..\..\WCL7\CPP\Source\Common\wclThread.h"
#include "..\..\..\..\WCL7\CPP\Source\WiFi\wclWiFi.h"

using namespace wclCommon;
using namespace wclWiFi;

class CSnifferThread : public CwclThread
{
	DISABLE_COPY(CSnifferThread);
	
private:
	CwclWiFiClient*		FClient;
	HANDLE				FInitEvent;
	int					FInitResult;
	CwclWiFiSniffer*	FSniffer;
	
	void ClientAfterOpen(void* Sender);
	void ClientBeforeClose(void* Sender);
	
	void SnifferAfterOpen(void* Sender);
	void SnifferBeforeClose(void* Sender);
	void SnifferRawFrameReceived(void* Sender, const void* const Buffer,
		const unsigned long Size);
	
protected:
	virtual bool OnInitialize() override;
	virtual void OnTerminate() override;
	
public:
	CSnifferThread();
	virtual ~CSnifferThread();
};
