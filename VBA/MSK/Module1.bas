Attribute VB_Name = "Module1"
Option Explicit

Private successfulEmails() As String
Private failedEmails() As String
Private removedEmails() As String
Private emailIndex() As Integer
Private emailList() As String
Private cancel As String

Public Function setCancelString(value As String)
    cancel = value
End Function

Sub getFromOutlook()
    'Variables
    Dim OutlookNamespace As NameSpace
    Dim Folder As MAPIFolder
    Dim OutlookMail As Variant
    Dim strBody As String, emailStr As String
    Dim emailNum As Integer, emailNumBeg As Integer, emailNumEnd As Integer, fromNumBeg As Integer
    Dim emailCount As Integer
    
    Dim i As Integer
    
    'Setting outlook and the folder
    Set OutlookNamespace = Application.GetNamespace("MAPI")
    Set Folder = "folder"
                
    'initialize i
    i = 1
    
    emailCount = Folder.Items.Count
    
    'setting the upper bound for emailList and emailIndex arrays
    ReDim emailList(emailCount)
    ReDim emailIndex(emailCount)
    
    'Goes through each item in the folder
    For Each OutlookMail In Folder.Items
    
        DoEvents
    
        progress ((i / emailCount) * 100), " (retrieving emails)"
        
        OutlookMail.UnRead = False
        
        'If the item is a report item
        If (StrComp(TypeName(OutlookMail), "ReportItem", 1) = 0) Then
            'convert the string using unicode
            strBody = StrConv(OutlookMail.Body, vbUnicode)
            
            'get the index of the beginning of where the email starts, email ends, and date start
            emailNumBeg = InStr(strBody, "<a href=")
            
            'if it is able to find the a href (which contains the email in reportitems)
            If (emailNumBeg <> 0) Then
                
                'find the position of where the email ends and date emails and get the email based on where the email begins and ends
                emailNumEnd = InStr(emailNumBeg, strBody, ">")
                
            'otherwise it might be too spaced due to string conversion, try getting the numbers from the body before conversion
            Else
                strBody = OutlookMail.Body
                fromNumBeg = InStr(strBody, "From: ")
                emailNumBeg = InStr(fromNumBeg, strBody, "To: ")
                emailNumEnd = InStr(emailNumBeg, strBody, "Date:")
            End If
            
            'get the email based on where the email string begins and ends
            emailStr = Mid(strBody, emailNumBeg, emailNumEnd - emailNumBeg)
            
        'Or if the item is a mail item
        ElseIf (StrComp(TypeName(OutlookMail), "MailItem", 1) = 0) Then
        
            strBody = OutlookMail.Body
            
            'get the index of the beginning where the email starts, email ends, and date starts
            fromNumBeg = InStr(strBody, "From: ")
            
            'if it is able to find the from (which contains the email in mailitems)
            If (fromNumBeg <> 0) Then
            
                'find the position of where the email begins and ends, and get the email based on those positions
                emailNumBeg = InStr(fromNumBeg, strBody, "To: ")
                emailNumEnd = InStr(emailNumBeg, strBody, "Date:")
                emailStr = Mid(strBody, emailNumBeg, emailNumEnd - emailNumBeg)
                
            'otherwise set it as "%"
            Else
                emailStr = "%"
            End If
        End If
        
        'if the string doesn't contain a percent
        If (InStr(emailStr, "%") = 0) Then
            Debug.Print i, Mid(strBody, emailNumBeg + 16, emailNumEnd - (emailNumBeg + 17))
                    
            'removing the strings ("<a href=", "mailto:"), punctuations, carriage returns (Chr13), new line (Chr10), and spaces
            'emailStr = Trim(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Mid(strBody, emailNumBeg, emailNumEnd - emailNumBeg), "<a href=", ""), "To: ", ""), "mailto:", ""), """", ""), "<", ""), ">", ""), Chr(13), ""), Chr(10), ""))
            emailStr = Mid(strBody, emailNumBeg + 16, emailNumEnd - (emailNumBeg + 17))
        
            'add email to successful email array
            emailList(i) = emailStr
            
            'increment
            i = i + 1
            
        'otherwise
        Else
            Debug.Print i
        
            'add the index of the email that failed
            emailList(i) = "N/A"
            
            'increment
            i = i + 1
        End If
        
        'save the location of the email
        emailIndex(i - 1) = i - 1
        
    'Go to the next item
    Next OutlookMail
    
    'cleanup
    Set Folder = Nothing
    Set OutlookNamespace = Nothing
End Sub

Sub openIEFillandDelete()

    'Variables
    Dim myUserForm As UserForm4
    'Dim ie As SHDocVw.InternetExplorer
    Dim ie As Object
    Dim OutlookNamespace As NameSpace
    Dim Folder As MAPIFolder
    Dim length As Integer, emailLength As Integer, emailLengthComp As Integer, emailTableOffset As Integer, emailTableLength As Integer
    Dim j As Integer, k As Integer, l As Integer, currentEmailNumber As Integer, emailNumOffset As Integer, emailNum As Integer
    Dim login As Object, pass As Object, loginBtn As Object
    Dim email As Object, checkEmail As Object, emailTable As Object, emailResult As Object, deleteUser As Object
    Dim emailStr As String, dateStr As String
    Dim NowTick As Long, EndTick As Long
    
    'Creating and showing userform
    Set myUserForm = New UserForm4
    myUserForm.Show
    
    'If the cancel button is clicked
    If (cancel <> "Cancel") Then
        
        'Making the ie window visible and going to the url
        'Set ie = New SHDocVw.InternetExplorer
        Set ie = CreateObject("InternetExplorer.Application")
        ie.Visible = True
        ie.Navigate "email website"
        
        'Waiting for the page to load
        Do While ie.Busy = True Or ie.ReadyState <> 4
            DoEvents
        Loop
        
        'If not already logged on
        If IsObject(ie.Document.getElementById("username")) Then
        
            'getting the username and pw from the user, filling it in, and clicking login
            With ie.Document
                .getElementById("username").value = myUserForm.TextBox1
                .getElementById("password").value = myUserForm.TextBox2
                .getElementsByClassName("btn btn-primary")(0).Click
            End With

        End If
            
        'Waiting for the page to load
        Do While ie.Busy = True Or ie.ReadyState <> 4
            DoEvents
        Loop
        
        'get the length of the emailList array
        length = UBound(emailList)
        
        'set the upper bounds for the successfulEmails and failedEmails array
        ReDim successfulEmails(length)
        ReDim failedEmails(length)
        
        'get the textbox, addEmail "button", the result table, and the result
        Set email = "email textbox"
        Set checkEmail = "add email button"
        Set emailTable = "table of emails"
        Set emailResult = "result"
        
        'Setting outlook and the folder
        Set OutlookNamespace = Application.GetNamespace("MAPI")
        Set Folder = "email folder"
        
        k = 1
        currentEmailNumber = length
        emailNumOffset = 0
        
        'Go through the emails
        For j = length To 1 Step -1
            DoEvents
            
            progress ((k / length) * 100), " (checking and deleting emails)"
                        
            'if the email doesn't equal to "N/A"
            If (StrComp(emailList(j), "N/A", 1) <> 0) Then
                
                'get the textbox
                Set email = "email textbox"
            
                'fill the textbox with an email grabbed from the array, get the number of emails in the table, and click the addEmail "button"
                email.value = emailList(j)
                
                emailLength = emailTable.length
                checkEmail.Click
                
                EndTick = VBA.Timer + 3
                
                'wait until either you get a message or the table size increases
                Do
                    Set emailTable = "table of emails"
                    Set emailResult = "result"
                    NowTick = VBA.Timer
                    DoEvents
                Loop Until ((StrComp(emailResult.innerHTML, "Email does not match current user account(s)", 1) = 0) Or (emailTable.length > emailLength) Or _
                            (NowTick >= EndTick))
                
                'if there is a message, it means the email was not found
                If (StrComp(emailResult.innerHTML, "Email does not match current user account(s)", 1) = 0) Then
                    failedEmails(j) = emailList(j)
                    
                    'if the current number of emails in the folder doesn't match the number of emails when generating the list,
                    'set the offset to the difference
                    If (Folder.Items.Count <> (currentEmailNumber + emailNumOffset)) Then
                        emailNumOffset = Folder.Items.Count - currentEmailNumber
                    End If
                    
                    'get the position of the email that needs to be deleted and add the offset to it
                    emailNum = emailIndex(j) + emailNumOffset
                    
                    'delete that email
                    Folder.Items(emailNum).Delete
                    
                    'decrement the number of emails
                    currentEmailNumber = currentEmailNumber - 1
                    
                'otherwise, it means the email was added successfully
                Else
                    successfulEmails(j) = emailList(j)
                End If
                
                'set the result and textbox to blank
                ie.Document.getElementById("email textbox").value = ""
                ie.Document.getElementById("result").innerHTML = ""
             
            'Otherwise
            Else
                'if the current number of emails in the folder doesn't match the number of emails when generating the list,
                'set the offset to the difference
                If (Folder.Items.Count <> (currentEmailNumber + emailNumOffset)) Then
                    emailNumOffset = Folder.Items.Count - currentEmailNumber
                End If
                
                'get the position of the email that needs to be deleted and add the offset to it
                emailNum = emailIndex(j) + emailNumOffset
                
                'delete that email
                Folder.Items(emailNum).Delete
                
                'decrement the number of emails
                currentEmailNumber = currentEmailNumber - 1
            End If
            
            k = k + 1
        'next email
        Next j
        
        emailTableOffset = 0
        emailTableLength = "email table"
        
        ReDim removedEmails(emailTableLength)
        
        'go through the emails in the email table
        For l = 1 To emailTableLength - 1
            DoEvents
        
            progress (l / (emailTableLength - 1)) * 100, " (removing emails)"
            
            'get the email table
            Set emailTable = "email table"
            
            'get the initial table size
            emailLengthComp = emailTable.Rows.length
            
            'get the date and email string
            emailStr = emailTable.Rows(l - emailTableOffset).cells(0).innerHTML
            dateStr = emailTable.Rows(l - emailTableOffset).cells(2).innerHTML
            
            'if the date string is blank, set the date the same as setting
            dateStr = IIf(StrComp(dateStr, "", 1) = 0, DateAdd("ww", -52, Date), dateStr)
            
            'if the date was within the last year, remove the email and add the email to the removeEmails array
            If (CDate(dateStr) > DateAdd("ww", -52, Date)) Then
                removedEmails(l) = emailStr
                Set deleteUser = ie.Document.getElementById("EmailTable").Rows(l - emailTableOffset).cells(4).querySelector("a")
                deleteUser.Click
                emailTableOffset = emailTableOffset + 1
                
                EndTick = VBA.Timer + 3
                
                'wait until either the table size decreases or there are no more emails
                Do
                    DoEvents
                    NowTick = VBA.Timer
                    Set emailTable = ie.Document.getElementById("EmailTable").Rows
                Loop Until ((emailLengthComp > emailTable.length) Or (emailTable.length = 1) Or (NowTick >= EndTick))
            End If
                    
        Next l
    End If
    
    'cleanup
    Set myUserForm = Nothing
    Set ie = Nothing
    Set login = Nothing
    Set pass = Nothing
    Set loginBtn = Nothing
    Set email = Nothing
    Set checkEmail = Nothing
    Set emailTable = Nothing
    Set emailResult = Nothing
    Set deleteUser = Nothing
    
End Sub

Sub exportEmails()
    
    'Variables
    Dim excelDoc As Excel.Application
    Dim excelWB As Workbook
    Dim excelWS As Worksheet
    Dim i As Integer, j As Integer, k As Integer, l As Integer, m As Integer, n As Integer
    
    'creating excel docs
    Set excelDoc = New Excel.Application
    Set excelWB = excelDoc.Workbooks.Add
    excelDoc.Visible = True
    Set excelWS = excelWB.Sheets(1)
    
    k = 2
    l = 2
    n = 2
    
    'loop through the successful array and print in "A" column in the excel document
    excelWS.Range("A1").value = "Successful Emails:"
    For i = LBound(successfulEmails) To UBound(successfulEmails)
    
        'if the email at i is not blank, print it in the column and increment k
        If (StrComp(successfulEmails(i), "", 1) <> 0) Then
            excelWS.Range("A" + CStr(k)).value = successfulEmails(i)
            k = k + 1
        End If
    Next i
    
    'loop through the failed array and print in "B" column in the excel document
    excelWS.Range("B1").value = "Failed Emails:"
    For j = LBound(failedEmails) To UBound(failedEmails)
    
        'if the email at j is not blank, print it in the column and increment l
        If (StrComp(failedEmails(j), "", 1) <> 0) Then
            excelWS.Range("B" + CStr(l)).value = failedEmails(j)
            l = l + 1
        End If
    Next j
    
    'loop through the removed array and print in "C" column in the excel document
    excelWS.Range("C1").value = "Removed Emails:"
    For m = LBound(removedEmails) To UBound(removedEmails)
    
        'if the email at m is not blank, print it in the column and increment n
        If (StrComp(removedEmails(m), "", 1) <> 0) Then
            excelWS.Range("C" + CStr(n)).value = removedEmails(m)
            n = n + 1
        End If
    Next m
    
    progress 100, " (generated excel)"
    
    'autofit all rows
    excelWS.Columns.AutoFit
    
    'cleanup
    Set excelDoc = Nothing
    Set excelWB = Nothing
    Set excelWS = Nothing
End Sub


Sub progress(pctCompl As Double, status As String)
    UserForm3.progLbl.Caption = Round(pctCompl, 2) & "% Completed" & status
    UserForm3.barLbl.Width = Round(pctCompl, 2) * 2
End Sub

Sub processEmails()
    UserForm3.Show
End Sub