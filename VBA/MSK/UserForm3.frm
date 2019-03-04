VERSION 5.00
Begin {C62A69F0-16DC-11CE-9E98-00AA00574A4F} UserForm3 
   Caption         =   "Emails"
   ClientHeight    =   1635
   ClientLeft      =   168
   ClientTop       =   492
   ClientWidth     =   4344
   OleObjectBlob   =   "UserForm3.frx":0000
   StartUpPosition =   1  'CenterOwner
End
Attribute VB_Name = "UserForm3"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False

Private Sub checkAndDeleteBtn_Click()
    openIEFillandDelete
    UserForm3.exportBtn.Visible = True
    UserForm3.exportBtn.Enabled = True
    UserForm3.checkAndDeleteBtn.Enabled = False
End Sub

Private Sub exportBtn_Click()
    exportEmails
    UserForm3.exportBtn.Enabled = False
End Sub

Private Sub retrieveBtn_Click()
    getFromOutlook
    UserForm3.retrieveBtn.Enabled = False
    UserForm3.checkAndDeleteBtn.Visible = True
    UserForm3.checkAndDeleteBtn.Enabled = True
End Sub

Public Sub UserForm_Activate()
    UserForm3.Width = UserForm3.barLbl.Width + 30
End Sub
