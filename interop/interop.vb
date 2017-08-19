Public Class interop

    Public Function locate(state As Int32, test_arg As String)
        'Debug interop util. Pings program based on state to ensure handshake is active.

        'states:
        '0: illustrator

        Dim illustratorRef
        illustratorRef = CreateObject("Illustrator.Application")

        illustratorRef.DoJavaScript("alert( 'Argument passed with state ' + arguments[0] + ': ' + arguments[1])", {state, test_arg})

    End Function

    Public Function init(state, path, runtime)
        'Initialize parts of handshake between rhino and illustrator.

        'states:
        '0: create docbox

        Dim illustratorRef
        illustratorRef = CreateObject("Illustrator.Application")

        If state = 0 Then
            Dim scriptPath = path + "D01.jsx"
            illustratorRef.DoJavaScriptFile(scriptPath, {path, runtime})
        End If

    End Function

    Public Function illustrator(state, stagingFile)
        'Primary illustrator handshake util. Passes state of rhino changes and reads from staging file.

        'states:
        '0: create new path
        '1: delete path

        Dim illustratorRef
        illustratorRef = CreateObject("Illustrator.Application")

        'illustratorRef.DoJavaScriptFile("TRANSFORMATION FILEPATH", {state, stagingFile})

    End Function

    Public Function selection(state, path, runtime)
        'General selection sync util. Updates selection across all synchronized programs.
        'path arg is string of path up to ..\\JSX\\

        'states:
        '1: increase selection - read S10
        '2: decrease selection - read S20
        '3: reset selection - ez pz

        Dim illustratorRef
        illustratorRef = CreateObject("Illustrator.Application")

        'illustratorRef.DoJavaScript("alert ('Idle!')")

        'TODO: Both Illustrator selection extendscript files MUST write "true" to S01 on completion.
        If state = 1 Then
            Dim scriptPath = path + "S10.jsx"
            illustratorRef.DoJavaScriptFile(scriptPath, {path, runtime})
        ElseIf state = 2 Then
            Dim scriptPath = path + "S20.jsx"
            illustratorRef.DoJavaScriptFile(scriptPath, {path, runtime})
        ElseIf state = 3 Then
            illustratorRef.DoJavaScript("app.selection = ''")
        End If

        'illustratorRef.DoJavaScriptFile("E:\git\gravy\conductor\ai__sync selection.jsx", {Guid})

    End Function

End Class
