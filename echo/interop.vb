Public Class interop

    Public Function locate(state As Int16, test_arg As String)
        'Debug interop util. Pings program based on state to ensure handshake is active.

        'states:
        '0: illustrator

        Dim illustratorRef
        illustratorRef = CreateObject("Illustrator.Application")

        If state = 0 Then
            illustratorRef.DoJavaScript("alert( 'Argument passed in state ' + arguments[0] + ': ' + arguments[1])", {state, test_arg})
        ElseIf state = 1 Then
            illustratorRef.DoJavaScript("alert( 'Argument passed in state 1: ' + arguments[0])", {test_arg})
        End If

    End Function

    Public Function illustrator(state As Int16, guid As Guid)
        'Primary illustrator handshake util. Passes rhino operations and arguments to relevant extendscript process.

        'states:
        '0: create new path
        '1: delete path

        Dim illustratorRef
        illustratorRef = CreateObject("Illustrator.Application")

        illustratorRef.DoJavaScriptFile("E:\git\gravy\illustrator_boat\Inbound\ai__get lines.jsx", {guid})

    End Function

    Public Function selection(state, guid)
        'General selection sync util. Updates selection across all synchronized programs.

        'states:
        '0: increase selection
        '1: decrease selection
        '2: remove all selection

        Dim illustratorRef
        illustratorRef = CreateObject("Illustrator.Application")

        illustratorRef.DoJavaScriptFile("E:\git\gravy\conductor\ai__sync selection.jsx", {guid})

    End Function

End Class
