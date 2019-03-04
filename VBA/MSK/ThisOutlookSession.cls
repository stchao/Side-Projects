VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
END
Attribute VB_Name = "ThisOutlookSession"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = True
Private cancel As String
Private outlookItems As Outlook.Items
Private numAppt As Integer
Private teleNum As Integer

Public Function setCancelString(value As String)
    cancel = value
End Function

Public Function getNumAppt() As Integer
    getNumAppt = numAppt
End Function

'creates email and fills it with the information grabbed from the text boxes
Public Sub createTemplates()
    'variables
    Dim myUserForm As UserForm1
    Dim objMail As MailItem
    Dim pt As String, phone As String, time As String, dr As String, result As String, _
        service As String, device As String, ptSex As String, ccEmail As String, _
        drFirst As String, signature As String
    
    'creating userform
    Set myUserForm = New UserForm1
    myUserForm.Show
    
    'creating mail item
    Set objMail = Application.CreateItem(olMailItem)
                
    'if the user didn't click the cancel button
    If (StrComp(cancel, "Cancel", 1) <> 0) Then
    
        'if the user clicked intro
        If (StrComp(myUserForm.EmailComboBox.value, "Intro", 1) = 0) Then

            'get the necessary information from the form
            service = myUserForm.serviceTextBox.value
            dr = myUserForm.drTextBox.value
            time = myUserForm.timeTextBox.value
            pt = myUserForm.ptTextBox.value
            phone = myUserForm.phoneTextBox.value
            
            With objMail
                .Display
            End With
            
        'saves the signature as a string
            signature = Replace(objMail.HTMLBody, "<p class=MsoNormal><o:p>&nbsp;</o:p></p>", "", 1)
                        
            'modify the mail item
            With objMail
                 'subject intro template
                 .Subject = ""
                 'body intro template
                 .HTMLBody = "template" & signature
            End With
        
        'if the user clicked success
        ElseIf (StrComp(myUserForm.EmailComboBox.value, "Success", 1) = 0) Then
        
            'get the necessary information from the form
            service = myUserForm.serviceTextBox.value
            dr = myUserForm.drTextBox.value
            drFirst = getFirstName(dr)
            time = myUserForm.timeTextBox.value
            pt = myUserForm.ptTextBox.value
            ptSex = myUserForm.ptComboBox.value
            device = myUserForm.deviceTextBox.value
            ccEmail = myUserForm.ccTextBox.value
            
            With objMail
                .Display
            End With
            
            signature = Replace(objMail.HTMLBody, "<p class=MsoNormal><o:p>&nbsp;</o:p></p>", "", 1)
            
            'modify the mail item
            With objMail
                 .cc = ccEmail
                 'subject success template
                 .Subject = ""
                 'body success template
                 .HTMLBody = "template" & signature
                 .Display
            End With
        
        'if the user clicked fail
        ElseIf (StrComp(myUserForm.EmailComboBox.value, "Fail", 1) = 0) Then
            
            'get the necessary information from the form
            service = myUserForm.serviceTextBox.value
            dr = myUserForm.drTextBox.value
            drFirst = getFirstName(dr)
            time = myUserForm.timeTextBox.value
            pt = myUserForm.ptTextBox.value
            ptSex = myUserForm.ptComboBox.value
            ccEmail = myUserForm.ccTextBox.value
            phone = myUserForm.phoneTextBox.value
            result = myUserForm.resultTextBox.value
            
            With objMail
                .Display
            End With
            
            signature = Replace(objMail.HTMLBody, "<p class=MsoNormal><o:p>&nbsp;</o:p></p>", "", 1)
            
            'modify the mail item
            With objMail
                 .cc = ccEmail
                 'subject failed template
                 .Subject = ""
                 'body failed template
                 .HTMLBody = "template" & signature
                 .Display
            End With
            
        End If
    
    End If
    
    'cleanup
    Set outlookItems = Nothing
    Set myUserForm = Nothing
    Set objMail = Nothing
    Set oCalendar = Nothing
    Set oItems = Nothing
End Sub

'function to get the first name
Private Function getFirstName(name As String)
    'variables
    Dim spaceNum As Integer
    
    'if the string contains the string "Dr." just return the string
    If (InStr(name, "Dr.") > 0) Then
        getFirstName = name
        
    'otherwise just get the first name before the space dividing
    Else
        spaceNum = InStr(name, " ")
        getFirstName = Mid(name, 1, spaceNum)
    End If
End Function

'function to get the pronoun based on value
Private Function getPronoun(value As String, noun As Integer)

    'if noun = 0/subject pronoun
    If (noun = 0) Then
    
        'if the string = M, return "he"
        If (StrComp(value, "M", 1) = 0) Then
            getPronoun = "he"
            
        'otherwise return "she"
        Else
            getPronoun = "she"
        End If
        
    
    ElseIf (noun = 1) Then
        'if the string = M, return "he"
        If (StrComp(value, "M", 1) = 0) Then
            getPronoun = "him"
            
        'otherwise return "she"
        Else
            getPronoun = "her"
        End If
    
    'otherwise
    Else
       
        'if the string = M, return "him"
        If (StrComp(value, "M", 1) = 0) Then
            getPronoun = "his"
            
        'otherwise return "her"
        Else
            getPronoun = "her"
        End If
        
    End If
    
End Function

'function to reformat a phone number to XXX - XXX - XXXX
Private Function reformatPhoneNum(phoneNum As String) As String
    reformatPhoneNum = Mid(phoneNum, 1, 3) & " – " & Mid(phoneNum, 4, 3) & " – " & Mid(phoneNum, 7, 4)
End Function

'function to get the first telemedicine appointment in the items
Public Function getFirstAppt() As Integer
    Dim myStart As Date, myEnd As Date
    Dim oCalendar As Outlook.Folder
    Dim oItems As Outlook.Items, oResItems As Outlook.Items
    Dim oAppt As Outlook.AppointmentItem
    Dim strRestriction As String
    Dim num As Integer
    
    'creating calendar
    Set oCalendar = Application.Session.GetDefaultFolder(olFolderCalendar)
    Set oItems = oCalendar.Items
    
    'setting restrictions
    myStart = Date
    myEnd = DateAdd("d", 1, myStart)
    strRestriction = "[Start] >= '" & Format$(myStart, "mm/dd/yyyy hh:mm AMPM") _
    & "' AND [End] <= '" & Format(myEnd, "mm/dd/yyyy hh:mm AMPM") & "'"
    
    'sorting the items by date
    oItems.Sort "[Start]"
     
    'restrict the Items collection
    Set oResItems = oItems.Restrict(strRestriction)
    
    'save a copy of the restricted items collection
    Set outlookItems = oResItems
    
    'save the number of restricted items
    numAppt = oResItems.Count
    
    teleNum = 0
    num = 1
    'go through each item until you hit the first appointment that says telemedicine
    For Each oAppt In oResItems
        If (InStr(oAppt.Subject, "Telemedicine") <> 0) Then
            getFirstAppt = num
            Exit For
        End If
        
        If ((num = oResItems.Count) And teleNum = 0) Then
            getFirstAppt = 0
        Else
            num = num + 1
        End If
    Next
    
    'cleanup
    Set oCalendar = Nothing
    Set oItems = Nothing
End Function

'function to get necessary information from the appointments
Public Function autofillStd(currentApptNum As Integer) As String()
    Dim currentAppt As AppointmentItem
    Dim serviceNum As Integer, drNum As Integer
    Dim arrStr(1 To 5) As String
    Dim ptStr As String

    Set currentAppt = outlookItems.Item(currentApptNum)
    
    serviceNum = IIf(InStr(currentAppt.Subject, Chr(150)) = 0, InStr(currentAppt.Subject, "-"), InStr(currentAppt.Subject, Chr(150)))
    serviceNum = IIf(serviceNum = 0, InStr(currentAppt.Subject, "/"), serviceNum)
    
    drNum = IIf(InStr(serviceNum, currentAppt.Subject, "/") = 0, InStr(serviceNum, currentAppt.Subject, Chr(150)), InStr(serviceNum, currentAppt.Subject, "/"))
    drNum = IIf(drNum = 0, InStr(serviceNum, Replace(currentAppt.Subject, "-", "", , 1), "-"), drNum)
    drNum = IIf(drNum = serviceNum, InStr(serviceNum + 1, currentAppt.Subject, "/"), drNum)
    
    'service
    arrStr(1) = Trim(Replace(Replace(Replace(Mid(currentAppt.Subject, serviceNum, drNum - serviceNum), Chr(150), ""), "-", ""), "/", ""))
    
    'dr
    arrStr(2) = Trim(Replace(Replace(Replace(Mid(currentAppt.Subject, drNum), "/", ""), Chr(150), ""), "-", ""))
    
    'time
    arrStr(3) = Format(currentAppt.Start, "hh:mm AM/PM")
    
    serviceNum = InStr(currentAppt.Location, Chr(150))
    serviceNum = IIf(serviceNum = 0, InStr(Replace(currentAppt.Location, "/", Chr(150), , 1), Chr(150)), serviceNum)
    
    'pt
    ptStr = Trim(Replace(Replace(Left(currentAppt.Location, serviceNum), Chr(150), ""), "/", ""))
    'ptStr = IIf(InStr(ptStr, ",") > 0, reverseFirstandLast(ptStr), ptStr)
    If (InStr(ptStr, ",") > 0) Then
        ptStr = reverseFirstandLast(ptStr)
    End If
    
    arrStr(4) = ptStr
    
    'phone
    arrStr(5) = Replace(Replace(Replace(Right(currentAppt.Location, Len(currentAppt.Location) - serviceNum), Chr(150), ""), "-", ""), " ", "")
    
    autofillStd = arrStr
    
    'cleanup
    Set currentAppt = Nothing
End Function

'Function to reverse the first and last name
Private Function reverseFirstandLast(name As String) As String
    Dim commaLoc As String
    Dim firstName As String
    Dim lastName As String
    
    commaLoc = InStr(name, ",")
    firstName = Replace(Mid(name, commaLoc), ",", "")
    lastName = Replace(Mid(name, 1, commaLoc), ",", "")
    
    reverseFirstandLast = UCase(Left(firstName, 1)) & LCase(Mid(firstName, 2)) & " " & UCase(Left(lastName, 1)) & LCase(Mid(lastName, 2))
End Function


