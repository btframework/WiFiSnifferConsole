program WiFiSnifferConsole;

{$APPTYPE CONSOLE}

uses
  wclErrors,
  Sniffer in 'Sniffer.pas';

var
  WiFiSniffer: TSnifferThread;

begin
  WriteLn('Starting sniffer');

  WiFiSniffer := TSnifferThread.Create;
  if WiFiSniffer.Run <> WCL_E_SUCCESS then
    WriteLn('Start sniffer failed');

  WriteLn('Press ENTER to terminate.');
  ReadLn;

  if WiFiSniffer.Running then
    WiFiSniffer.Terminate;

  WiFiSniffer.Free;
end.
