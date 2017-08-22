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

    Public Function geometry(state, path, runtime)
        'Primary illustrator handshake util. Passes state of rhino changes and reads from staging file.

        'states:
        '1: create new path
        '2: delete path
        '3: transform existing

        Dim illustratorRef
        illustratorRef = CreateObject("Illustrator.Application")

        If state = 1 Then
            Dim scriptPath = path + "G10.jsx"
            'illustratorRef.DoJavaScriptFile(scriptPath, {path, runtime})
        ElseIf state = 2 Then
            Dim scriptPath = path + "G20.jsx"
            'illustratorRef.DoJavaScriptFile(scriptPath, {path, runtime})
        ElseIf state = 3 Then
            Dim scriptPath = path + "G30.jsx"
            'illustratorRef.DoJavaScriptFile(scriptPath, {path, runtime})
        End If

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

    Public Function docBounds(x, y, conv, path)
        'Set document bounds in illustrator.

        Dim illustratorRef
        illustratorRef = CreateObject("Illustrator.Application")

        Dim scriptPath = path + "D10.jsx"
        illustratorRef.DoJavaScriptFile(scriptPath, {x, y, conv})

    End Function

    Public Function alert(message)
        'Throw up an alert window.

        Dim illustratorRef
        illustratorRef = CreateObject("Illustrator.Application")

        illustratorRef.DoJavaScript("alert(arguments[0])", {message})
    End Function

End Class
