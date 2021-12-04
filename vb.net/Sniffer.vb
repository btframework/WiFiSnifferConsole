Public Class SnifferThread : Inherits wclThread
    Private WithEvents FClient As wclWiFiClient
    Private FInitEvent As ManualResetEvent
    Private FInitResult As Int32
    Private WithEvents FSniffer As wclWiFiSniffer

    Private Sub ClientAfterOpen(ByVal Sender As Object, ByVal e As EventArgs) Handles FClient.AfterOpen
        Console.WriteLine("WiFi client opened")
        Console.WriteLine("Get WiFi interfaces")

        Dim Ifaces() As wclWiFiInterfaceData = Nothing
        FInitResult = FClient.EnumInterfaces(Ifaces)
        If FInitResult! = wclErrors.WCL_E_SUCCESS Then
            Console.WriteLine("Enum interfaces failed: 0x" + FInitResult.ToString("X8"))
        Else
            If Ifaces Is Nothing Or Ifaces.Length = 0 Then
                Console.WriteLine("No WiFi intyerfaces found")
                FInitResult = wclWiFiErrors.WCL_E_WIFI_NOT_AVAILABLE
            Else
                Console.WriteLine("Found " + Ifaces.Length.ToString() + " interfaces")
                Dim Id As Guid = Ifaces(0).Id
                Console.WriteLine("Use " + Id.ToString())

                Console.WriteLine("Start WiFi sniffer")
                FInitResult = FSniffer.Open(Id)
                If FInitResult! = wclErrors.WCL_E_SUCCESS Then
                    Console.WriteLine("Start sniffer failed: 0x" + FInitResult.ToString("X8"))
                End If
            End If
        End If

        FInitEvent.Set()
    End Sub

    Private Sub ClientBeforeClose(ByVal Sender As Object, ByVal e As EventArgs) Handles FClient.BeforeClose
        Console.WriteLine("WiFi client is closing")

        If FSniffer.Active Then
            Console.WriteLine("Closing WiFi sniffer")
            Dim Res As Int32 = FSniffer.Close()
            If Res <> wclErrors.WCL_E_SUCCESS Then
                Console.WriteLine("Close sniffer failed: 0x" + Res.ToString("X8"))
            End If
        End If
    End Sub

    Private Sub SnifferAfterOpen(ByVal Sender As Object, ByVal e As EventArgs) Handles FSniffer.AfterOpen
        Console.WriteLine("Sniffer started")
        Console.WriteLine("Here is the good place to set Phy and Channel if needed")
    End Sub

    Private Sub SnifferBeforeClose(ByVal Sender As Object, ByVal e As EventArgs) Handles FSniffer.BeforeClose
        Console.WriteLine("Sniffer is closing")
        Console.WriteLine("Do cleanup here if needed")
    End Sub

    Private Sub SnifferRawFrameReceived(ByVal Sender As Object, ByVal Buffer() As Byte) Handles FSniffer.OnRawFrameReceived
        Console.WriteLine("Raw frame received")
    End Sub

    Protected Overrides Function OnInitialize() As Boolean
        Console.WriteLine("Create initialization completion event")
        Try
            FInitEvent = New ManualResetEvent(False)
        Catch
            FInitEvent = Nothing
        End Try

        If FInitEvent Is Nothing Then
            Console.WriteLine("Create initialization completion event failed")
        Else
            FInitResult = wclErrors.WCL_E_SUCCESS

            Console.WriteLine("Opening WiFi client")
            Dim Res As Int32 = FClient.Open()
            If Res <> wclErrors.WCL_E_SUCCESS Then
                FInitResult = Res
                Console.WriteLine("WiFi client open failed: 0x" + FInitResult.ToString("X8"))
            Else
                Console.WriteLine("Wait for WiFi sniffer initialization")
                Res = wclMessageBroadcaster.Wait(FInitEvent)
                If Res <> wclErrors.WCL_E_SUCCESS Then
                    Console.WriteLine("Wait failed: 0x" + Res.ToString("X8"))
                    FInitResult = Res
                End If

                Console.WriteLine("Initialization completed with result: 0x" + FInitResult.ToString("X8"))

                If FInitResult! = wclErrors.WCL_E_SUCCESS Then
                    Console.WriteLine("Closing WiFi client")
                    FClient.Close()
                End If
            End If

            FInitEvent.Close()
        End If

        Return (FInitResult = wclErrors.WCL_E_SUCCESS)
    End Function

    Protected Overrides Sub OnTerminate()
        Console.WriteLine("Stopping WiFi Sniffer")
        If FSniffer.Active Then
            FSniffer.Close()
        End If

        Console.WriteLine("Closing WiFi Client")
        If FClient.Active Then
            FClient.Close()
        End If
    End Sub

    Public Sub New()
        MyBase.New()

        Console.WriteLine("Changing synchronization method")
        wclMessageBroadcaster.SetSyncMethod(wclMessageSynchronizationKind.skApc)

        Console.WriteLine("Preparing components")

        FClient = New wclWiFiClient()
        FSniffer = New wclWiFiSniffer()

        Console.WriteLine("Components are ready")
    End Sub

    Protected Overrides Sub Finalize()
        Console.WriteLine("Releasing components")

        FSniffer = Nothing
        FClient = Nothing

        Console.WriteLine("Components released")
    End Sub
End Class