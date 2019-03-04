VERSION 5.00
Begin {C62A69F0-16DC-11CE-9E98-00AA00574A4F} UserForm4 
   Caption         =   "Prompt"
   ClientHeight    =   1440
   ClientLeft      =   120
   ClientTop       =   456
   ClientWidth     =   4416
   OleObjectBlob   =   "UserForm4.frx":0000
   StartUpPosition =   1  'CenterOwner
End
Attribute VB_Name = "UserForm4"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private Sub CommandButton1_Click()
    If ((TextBox1 = "") Or (TextBox2 = "")) Then
        MsgBox "Your username or password is empty"
    Else
        setCancelString ("not")
        Me.Hide
    End If
End Sub

Private Sub CommandButton2_Click()
    setCancelString ("Cancel")
    Me.Hide
End Sub

Private Sub EmptyFields()
    MsgBox "Your username or password is empty"
End Sub

Sub closeForm()
    Unload Me
End Sub
