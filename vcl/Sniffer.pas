unit Sniffer;

interface

uses
  wclMessaging, wclThread, wclWiFi, wclErrors;

type
  TSnifferThread = class(TwclThread)
  private
    FClient: TwclWiFiClient;
    FInitEvent: THandle;
    FInitResult: Integer;
    FSniffer: TwclWiFiSniffer;

    procedure ClientAfterOpen(Sender: TObject);
    procedure ClientBeforeClose(Sender: TObject);

    procedure SnifferAfterOpen(Sender: TObject);
    procedure SnifferBeforeClose(Sender: TObject);
    procedure SnifferRawFrameReceived(Sender: TObject; const Buffer: Pointer;
      const Size: Cardinal);

  protected
    function OnInitialize: Boolean; override;
    procedure OnTerminate; override;

  public
    constructor Create; override;
    destructor Destroy; override;
  end;

implementation

uses
  wclWiFiErrors, SysUtils, Windows;

{ TSnifferThread }

constructor TSnifferThread.Create;
begin
  inherited Create;

  WriteLn('Changin synchronization method');
  TwclMessageBroadcaster.SetSyncMethod(skApc);

  WriteLn('Preparing components');

  FClient := TwclWiFiClient.Create(nil);
  FClient.AfterOpen := ClientAfterOpen;
  FClient.BeforeClose := ClientBeforeClose;

  FSniffer := TwclWiFiSniffer.Create(nil);
  FSniffer.AfterOpen := SnifferAfterOpen;
  FSniffer.BeforeClose := SnifferBeforeClose;
  FSniffer.OnRawFrameReceived := SnifferRawFrameReceived;

  WriteLn('Components are ready');
end;

destructor TSnifferThread.Destroy;
begin
  WriteLn('Releasing components');

  FSniffer.Free;
  FClient.Free;

  WriteLn('Components released');

  inherited;
end;

function TSnifferThread.OnInitialize: Boolean;
var
  Res: Integer;
begin
  WriteLn('Create initialization completion event');
  FInitEvent := CreateEvent(nil, True, False, nil);
  if FInitEvent = 0 then
    WriteLn('Create initialization completion event failed')

  else begin
    FInitResult := WCL_E_SUCCESS;

    WriteLn('Opening WiFi client');
    Res := FClient.Open;
    if Res <> WCL_E_SUCCESS then begin
      FInitResult := Res;
      WriteLn('WiFi client open failed: 0x' + IntToHex(FInitResult, 8))

    end else begin
      WriteLn('Wait for WiFi sniffer initialization');
      Res := TwclMessageBroadcaster.Wait(FInitEvent);
      if Res <> WCL_E_SUCCESS then begin
        WriteLn('Wait failed: 0x' + IntToHex(Res, 8));
        FInitResult := Res;
      end;

      WriteLn('Initialization completed with result: 0x' +
        IntToHex(FInitResult, 8));

      if FInitResult <> WCL_E_SUCCESS then begin
        WriteLn('Closing WiFi client');
        FClient.Close;
      end;
    end;

    CloseHandle(FInitEvent);
  end;

  Result := (FInitResult = WCL_E_SUCCESS);
end;

procedure TSnifferThread.OnTerminate;
begin
  WriteLn('Stopping WiFi Sniffer');
  if FSniffer.Active then
    FSniffer.Close;

  WriteLn('Closing WiFi Client');
  if FClient.Active then
    FClient.Close;
end;

procedure TSnifferThread.ClientAfterOpen(Sender: TObject);
var
  Ifaces: TwclWiFiInterfaces;
  Id: TGUID;
begin
  WriteLn('WiFi client opened');
  WriteLn('Get WiFi interfaces');

  FInitResult := FClient.EnumInterfaces(Ifaces);
  if FInitResult <> WCL_E_SUCCESS then
    WriteLn('Enum interfaces failed: 0x' + IntToHex(FInitResult, 8))

  else begin
    if Length(Ifaces) = 0 then begin
      WriteLn('No WiFi intyerfaces found');
      FInitResult := WCL_E_WIFI_NOT_AVAILABLE;

    end else begin
      WriteLn('Found ' + IntToStr(Length(Ifaces)) + ' interfaces');
      Id := Ifaces[0].Id;
      WriteLn('Use ' + GuidToString(Id));

      WriteLn('Start WiFi sniffer');
      FInitResult := FSniffer.Open(Id);
      if FInitResult <> WCL_E_SUCCESS then
        WriteLn('Start sniffer failed: 0x' + IntToHex(FInitResult, 8));
    end;
  end;

  SetEvent(FInitEvent);
end;

procedure TSnifferThread.ClientBeforeClose(Sender: TObject);
var
  Res: Integer;
begin
  WriteLn('WiFi client is closing');

  if FSniffer.Active then begin
    WriteLn('Closing WiFi sniffer');
    Res := FSniffer.Close;
    if Res <> WCL_E_SUCCESS then
      WriteLn('Close sniffer failed: 0x' + IntToHex(Res, 8));
  end;
end;

procedure TSnifferThread.SnifferAfterOpen(Sender: TObject);
begin
  WriteLn('Sniffer started');
  WriteLn('Here is the good place to set Phy and Channel if needed');
end;

procedure TSnifferThread.SnifferBeforeClose(Sender: TObject);
begin
  WriteLn('Sniffer is closing');
  WriteLn('Do cleanup here if needed');
end;

procedure TSnifferThread.SnifferRawFrameReceived(Sender: TObject;
  const Buffer: Pointer; const Size: Cardinal);
begin
  WriteLn('Raw frame received');
end;

end.
