Imports System.IO
Imports System.Runtime.CompilerServices
Public Class Form1
    Public ValorsGcodes() As String
    Public ValorsForbiden() As String
    Public ValorsSpecials() As String
    Public CharactersToJump() As String
    Public path As String = ""
    Public dot As String = "."
    Public GCODE As String = ""
    Public SPACE As String = ""

    Sub CrearArrays()
        ReDim ValorsGcodes(0)
        ReDim ValorsForbiden(0)
        ReDim ValorsSpecials(0)
        ReDim CharactersToJump(0)

        Dim dimensio As Integer = 0
        Dim valorProvisional As String = ""
        For Each valor In TextBox2.Text
            If valor = " " Then
                dimensio += 1
                ReDim Preserve ValorsForbiden(dimensio - 1)
                ValorsForbiden(dimensio - 1) = valorProvisional
                valorProvisional = ""
            Else
                valorProvisional = valorProvisional & valor
            End If
        Next
        If TextBox1.Text.Length > 1 And TextBox1.Text <> " " Then
            dimensio += 1
            ReDim Preserve ValorsForbiden(dimensio - 1)
            ValorsForbiden(dimensio - 1) = valorProvisional
        End If
        dimensio = 0
        valorProvisional = ""
        For Each valor In TextBox1.Text
            If valor = "," Then
                dimensio += 1
                ReDim Preserve ValorsGcodes(dimensio - 1)
                ValorsGcodes(dimensio - 1) = valorProvisional
                valorProvisional = ""
            Else
                valorProvisional = valorProvisional & valor
            End If
        Next

        If TextBox2.Text.Length > 1 And TextBox2.Text <> " " Then
            dimensio += 1
            ReDim Preserve ValorsGcodes(dimensio - 1)
            ValorsGcodes(dimensio - 1) = valorProvisional
        End If
        dimensio = 0
        valorProvisional = ""
        For Each valor In TextBox3.Text
            If valor = "," Then
                dimensio += 1
                ReDim Preserve ValorsSpecials(dimensio - 1)
                ValorsSpecials(dimensio - 1) = valorProvisional
                valorProvisional = ""
            Else
                valorProvisional = valorProvisional & valor
            End If
        Next
        If TextBox3.Text.Length > 1 And TextBox3.Text <> " " Then
            dimensio += 1
            ReDim Preserve ValorsSpecials(dimensio - 1)
            ValorsSpecials(dimensio - 1) = valorProvisional
        End If
        dimensio = 0
        valorProvisional = ""
        For Each valor In TextBox4.Text
            If valor = "," Then
                dimensio += 1
                ReDim Preserve CharactersToJump(dimensio - 1)
                CharactersToJump(dimensio - 1) = valorProvisional
                valorProvisional = ""
            Else
                valorProvisional = valorProvisional & valor
            End If
        Next
        If TextBox4.Text.Length > 0 And TextBox4.Text <> " " Then
            dimensio += 1
            ReDim Preserve CharactersToJump(dimensio - 1)
            CharactersToJump(dimensio - 1) = valorProvisional
        End If

    End Sub
    Sub convertFileToGcode()
        path = Button1.Text
        If Not File.Exists(path) Then
            MsgBox("File doesn't exist. Please check the file and try it again.", MsgBoxStyle.OkOnly, "Error to load file")
            Exit Sub
        End If
        SetSpace()
        CrearArrays()
        If ComboBox1.Text.IndexOf(".") > -1 Then dot = "." Else dot = ","
        Dim name As String = SetNameFileFromFolder(path)

        Dim reader As StreamReader = My.Computer.FileSystem.OpenTextFileReader(path)
        Dim writer As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(name, 1)
        Dim lastOption As String = ""
        Dim a As String
        Dim LastGcode As String = ""
        Dim rsl As String = ""
        Do
            a = reader.ReadLine
            rsl = LineConverter(a)
            If rsl <> "" Then writer.WriteLine(rsl)
        Loop Until a Is Nothing
        writer.Close()
        reader.Close()
        MsgBox("Completed, " & vbCrLf & name, MsgBoxStyle.OkOnly, "The file is exported properly")
        SaveDefaultValues()
    End Sub
    Sub SaveDefaultValues()
        My.Settings.OriginPath = Button1.Text
        My.Settings.GCODES = TextBox1.Text
        My.Settings.CommentId = TextBox2.Text
        My.Settings.Special = TextBox3.Text
        My.Settings.Forbidden = TextBox4.Text
        My.Settings.DotValue = ComboBox1.Text
        My.Settings.Spaces = CheckBox1.Checked
        My.Settings.NLines = CheckBox2.Checked

    End Sub
    Function SetNameFileFromFolder(path As String)

        Dim name As String = IO.Path.GetFileNameWithoutExtension(path)
        Dim ext As String = IO.Path.GetExtension(path)
        path = IO.Path.GetDirectoryName(path)
        For i = 1 To 10000000
            If File.Exists(path & "\" & name & "_" & i & ext) = False Then
                path = path & "\" & name & "_" & i & ext
                Exit For
            End If
        Next

        Return path
    End Function
    Sub SetSpace()
        If CheckBox1.Checked = True Then
            SPACE = " "
        Else
            SPACE = ""
        End If

    End Sub
    Public Function LineConverter(StrToProcess As String)
        If IsForbiden(StrToProcess) = 1 Then Return ""
        StrToProcess = DeleteNValues(StrToProcess)
        StrToProcess = SkipValues(StrToProcess)
        GetGCODE(StrToProcess)
        StrToProcess = AddGcode(StrToProcess)
        StrToProcess = SetLineSpaces(StrToProcess)
        Return StrToProcess
    End Function
    Function SetLineSpaces(StrToProcess As String)

        Dim lletra As String = ""
        Dim results As String = ""
        For Each e In StrToProcess
            If System.Text.RegularExpressions.Regex.IsMatch(e, "^[A-Za-z]+$") = True Then
                If lletra <> "" Then
                    lletra = SPACE & e
                Else
                    lletra = e
                End If
            End If
            If e = " " Then
                lletra = SPACE
            Else
                lletra = e
            End If
            results = results & lletra
        Next
        Return results
    End Function
    Function AddGcode(StrToProcess As String)
        If IsNothing(StrToProcess) Then Return ""
        Dim linia As String = ""
        Dim NValue As Integer = 0
        For Each e In StrToProcess
            For Each element In ValorsSpecials
                If e = element And NValue = 0 Then
                    linia = linia & GCODE & SPACE
                    NValue = 1
                Else
                    For Each valor In ValorsGcodes
                        If e = valor Then NValue = 1
                    Next
                End If
            Next
            linia = linia & e
        Next
        If linia <> "" Or linia <> " " Then StrToProcess = linia
        Return StrToProcess
    End Function
    Sub GetGCODE(StrToProcess As String)
        If IsNothing(StrToProcess) Then StrToProcess = ""
        Dim linia As String = ""
        Dim NValue As Integer = 0
        For Each element In ValorsGcodes
            For Each e In StrToProcess
                If NValue = 0 And e = element Then
                    NValue = 1
                    linia = e
                ElseIf NValue = 1 Then
                    If IsNumeric(e) Then
                        If e <> " " Then linia = linia & e
                    Else
                        NValue = 0
                    End If
                End If
            Next
        Next
        If linia <> "" Or linia <> " " And linia.Length > 0 Then GCODE = linia
    End Sub
    Function IsForbiden(StrToProcess As String)
        If IsNothing(StrToProcess) Then StrToProcess = ""
        For Each e In StrToProcess
            If existValue(e, ValorsForbiden) Then Return 1
        Next
        Return 0
    End Function
    Function DeleteNValues(StrToProcess As String)
        If IsNothing(StrToProcess) Then StrToProcess = ""
        Dim linia As String = ""
        Dim Nstart As Integer = 0
        Dim NValue As Integer = 0
        If CheckBox2.Checked = True Then
            For Each e In StrToProcess
                If Nstart = 0 And e = "N" Then
                    NValue = 1
                ElseIf NValue = 1 Then
                    If IsNumeric(e) = False Then
                        NValue = -1
                        If e <> " " Then linia = linia & e
                    End If
                Else
                    linia = linia & e
                End If
                Nstart = 1

            Next
            StrToProcess = linia
        End If
        Return StrToProcess
    End Function
    Function SkipValues(StrToProcess As String)
        If IsNothing(StrToProcess) Then Return ""
        If CharactersToJump.Count < 1 Then Return StrToProcess
        Dim linia As String = ""
        Dim Nstart As Integer = 0
        Dim NValue As Integer = 0
        For Each e In StrToProcess
            For Each lletra In CharactersToJump
                If IsNothing(lletra) Then Exit For
                If NValue = 0 And e = lletra Then
                    NValue = 1
                ElseIf NValue = 1 Then
                    If IsNumeric(e) = False Then
                        NValue = -1
                        If e <> " " Then linia = linia & e
                    End If
                Else
                    linia = linia & e
                End If
            Next
        Next
        StrToProcess = linia
        Return StrToProcess
    End Function

    Function existValue(checkString As String, arr_() As String)
        For Each value In arr_
            If value = checkString Then
                Return 1
            End If
        Next
        Return 0
    End Function
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        convertFileToGcode()
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim msgooption As OpenFileDialog = New OpenFileDialog
        If msgooption.ShowDialog() = DialogResult.OK Then
            path = msgooption.FileName
            Button1.Text = path
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.Items.Add("Dot (.)")
        ComboBox1.Items.Add("Comma (,)")
        ComboBox1.SelectedIndex = 0
        Button1.Text = My.Settings.OriginPath
        TextBox1.Text = My.Settings.GCODES
        TextBox2.Text = My.Settings.CommentId
        TextBox3.Text = My.Settings.Special
        TextBox4.Text = My.Settings.Forbidden
        ComboBox1.Text = My.Settings.DotValue
        If My.Settings.NLines = True Then CheckBox2.Checked = True
        If My.Settings.Spaces = True Then CheckBox1.Checked = True

    End Sub
End Class
