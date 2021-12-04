Module Module1

    Sub Main()
        Console.WriteLine("Starting sniffer")

        Dim WiFiSniffer As SnifferThread = New SnifferThread()
        If WiFiSniffer.Run() <> wclErrors.WCL_E_SUCCESS Then
            Console.WriteLine("Start sniffer failed")
        End If
        Console.WriteLine("Press ENTER to terminate.")
        Console.ReadLine()

        If WiFiSniffer.Running Then
            WiFiSniffer.Terminate()
        End If

        WiFiSniffer = Nothing
    End Sub

End Module
