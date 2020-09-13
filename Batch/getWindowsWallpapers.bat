@echo off
setlocal EnableDelayedExpansion
Rem sets the location of the user's documents and user's windows pictures
set userDirectory=%USERPROFILE%\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets
set userDocuments=%USERPROFILE%\Documents
set "count=0"

Rem if the Images folder doesn't exist in the user's document folder, create it
If Not Exist "%userDocuments%\Images" mkdir %userDocuments%\Images

Rem go through all the files and if the files are greater than 350KB, copy them to the Images folder
Rem try 300KB
for /r %userDirectory% %%F in (*.) do @if %%~zF geq 300000 copy "%%F" %userDocuments%\Images

Rem go through all the files in the Images folder and add the .jpg extension
for /r %userDocuments%\Images %%G in (*.) do (
 set image=%%G
 call:loop)
echo "The program has completed"
goto:eof

Rem increment and if the image name exists, loop until the name isn't taken
:loop
set /a "count+=1"
If Exist "%USERPROFILE%\Documents\Images\image %count%.jpg" goto:loop
rename "%image%" image" "%count%.jpg



 
