VERSION 5.00
Begin {C62A69F0-16DC-11CE-9E98-00AA00574A4F} UserForm1 
   Caption         =   "Templates"
   ClientHeight    =   3012
   ClientLeft      =   108
   ClientTop       =   456
   ClientWidth     =   5868
   OleObjectBlob   =   "UserForm1.frx":0000
   StartUpPosition =   1  'CenterOwner
End
Attribute VB_Name = "UserForm1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private startAppt As Integer
Private currentAppt As Integer
Private numAppt As Integer

Private Sub autoBtn_Click()
    'variables
    Dim arrayString() As String
        
    'gets the required strings and fill the textboxes
    arrayString = ThisOutlookSession.autofillStd(currentAppt)
    serviceTextBox.value = arrayString(1)
    drTextBox.value = arrayString(2)
    timeTextBox.value = arrayString(3)
    ptTextBox.value = arrayString(4)
    phoneTextBox.value = arrayString(5)
    
    If currentAppt = numAppt Then
        currentAppt = startAppt
    Else
        currentAppt = currentAppt + 1
    End If
End Sub

Private Sub EmailComboBox_Change()
    'Enable and disable based on selected option
    If (StrComp(EmailComboBox.value, "Intro", 1) = 0) Then
        Me.Height = 141
        enableStandard
        enableIntro
    ElseIf (StrComp(EmailComboBox.value, "Success", 1) = 0) Then
        Me.Height = 160
        enableStandard
        enableSuccess
    ElseIf (StrComp(EmailComboBox.value, "Fail", 1) = 0) Then
        Me.Height = 183
        enableStandard
        enableFail
    Else
        Me.Height = 56
        disableAll
    End If
End Sub

Private Sub OkBtn_Click()
    ThisOutlookSession.setCancelString ("not")
    Me.Hide
End Sub

Private Sub CancelBtn_Click()
    ThisOutlookSession.setCancelString ("Cancel")
    Me.Hide
End Sub

Private Sub UserForm_Initialize()
    EmailComboBox.List = Array("Intro", "Success", "Fail")
    ptComboBox.List = Array("M", "F")
    startAppt = ThisOutlookSession.getFirstAppt
    currentAppt = startAppt
    numAppt = ThisOutlookSession.getNumAppt
End Sub

Private Function enableStandard()
    ptLbl.Visible = True
    ptTextBox.Visible = True
    timeLbl.Visible = True
    timeTextBox.Visible = True
    serviceLbl.Visible = True
    serviceTextBox.Visible = True
    drLbl.Visible = True
    drTextBox.Visible = True
    phoneLbl.Visible = True
    phoneTextBox.Visible = True
    autoBtn.Visible = True
    ptLbl.Enabled = True
    ptTextBox.Enabled = True
    timeLbl.Enabled = True
    timeTextBox.Enabled = True
    serviceLbl.Enabled = True
    serviceTextBox.Enabled = True
    drLbl.Enabled = True
    drTextBox.Enabled = True
    phoneLbl.Enabled = True
    phoneTextBox.Enabled = True
    autoBtn.Enabled = True
End Function

Private Function enableIntro()
    ptComboBox.Visible = False
    ptComboBox.Enabled = False
    deviceLbl.Visible = False
    deviceTextBox.Visible = False
    ccLbl.Visible = False
    ccTextBox.Visible = False
    resultLbl.Visible = False
    resultTextBox.Visible = False
    deviceLbl.Enabled = False
    deviceTextBox.Enabled = False
    ccLbl.Enabled = False
    ccTextBox.Enabled = False
    resultLbl.Enabled = False
    resultTextBox.Enabled = False
End Function

Private Function enableSuccess()
    ptComboBox.Visible = True
    deviceLbl.Visible = True
    deviceTextBox.Visible = True
    ccLbl.Visible = True
    ccTextBox.Visible = True
    resultLbl.Visible = False
    resultTextBox.Visible = False
    ptComboBox.Enabled = True
    deviceLbl.Enabled = True
    deviceTextBox.Enabled = True
    ccLbl.Enabled = True
    ccTextBox.Enabled = True
    resultLbl.Enabled = False
    resultTextBox.Enabled = False
End Function

Private Function enableFail()
    ptComboBox.Visible = True
    deviceLbl.Visible = False
    deviceTextBox.Visible = False
    ccLbl.Visible = True
    ccTextBox.Visible = True
    resultLbl.Visible = True
    resultTextBox.Visible = True
    ptComboBox.Enabled = True
    deviceLbl.Enabled = False
    deviceTextBox.Enabled = False
    ccLbl.Enabled = True
    ccTextBox.Enabled = True
    resultLbl.Enabled = True
    resultTextBox.Enabled = True
End Function

Private Function disableAll()
    ptLbl.Visible = False
    ptTextBox.Visible = False
    ptComboBox.Visible = False
    timeLbl.Visible = False
    timeTextBox.Visible = False
    serviceLbl.Visible = False
    serviceTextBox.Visible = False
    drLbl.Visible = False
    drTextBox.Visible = False
    phoneLbl.Visible = False
    phoneTextBox.Visible = False
    deviceLbl.Visible = False
    deviceTextBox.Visible = False
    ccLbl.Visible = False
    ccTextBox.Visible = False
    resultLbl.Visible = False
    resultTextBox.Visible = False
    autoBtn.Visible = False
    ptLbl.Enabled = False
    ptTextBox.Enabled = False
    ptComboBox.Enabled = False
    timeLbl.Enabled = False
    timeTextBox.Enabled = False
    serviceLbl.Enabled = False
    serviceTextBox.Enabled = False
    drLbl.Enabled = False
    drTextBox.Enabled = False
    phoneLbl.Enabled = False
    phoneTextBox.Enabled = False
    deviceLbl.Enabled = False
    deviceTextBox.Enabled = False
    ccLbl.Enabled = False
    ccTextBox.Enabled = False
    resultLbl.Enabled = False
    resultTextBox.Enabled = False
    autoBtn.Enabled = False
End Function

