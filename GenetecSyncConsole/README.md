
## Deployment

1. Run
   ```powershell
   dotnet publish --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfContained=true -p:IncludeAllContentForSelfContained=true
   ```
2. Visit ```\bin\Release\net8.0\win-x64\publish\``` and replace to destination. 

## Steps to Create a Scheduled Task
1. Open as an Administrator a powershell console (runs every 2m)
    ```powershell
    $taskAction = New-ScheduledTaskAction -Execute "$PATH_TO_CONSOLE_APP\GenetecSyncConsole.exe"
    $taskAction = New-ScheduledTaskAction -Execute "C:\Alusa\GenetecSyncConsole.exe"
    $taskTrigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 2)
    Register-ScheduledTask -TaskName "RunGenetecSyncConsoleEvery5min" -Action $taskAction -Trigger $taskTrigger -User "SYSTEM" -RunLevel Highest
2. Validate task status
    ```powershell
    Get-ScheduledTaskInfo -TaskName "RunGenetecSyncConsoleEvery5min"
    ```
3. Check logs 
   - Open the $PATH_TO_CONSOLE_APP folder and check the log files
4. Remove the task if required
    ```powershell
    Unregister-ScheduledTask -TaskName "RunGenetecSyncConsoleEvery5Min" -Confirm:$false
    ```
