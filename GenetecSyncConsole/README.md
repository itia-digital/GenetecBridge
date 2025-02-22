
## Steps to Create a Scheduled Task
1. Open as an Administrator a powershell console
    ```powershell
    $taskAction = New-ScheduledTaskAction -Execute "$PATH_TO_CONSOLE_APP\GenetecSyncConsole.exe"
    $taskTrigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 5)
    Register-ScheduledTask -TaskName "RunGenetecSyncConsoleEvery5min" -Action $taskAction -Trigger $taskTrigger -User "SYSTEM" -RunLevel Highest
2. Validate task status
    ```powershell
    Get-ScheduledTaskInfo -TaskName "RunGenetecSyncConsoleEvery5min"
    ```
3. Check logs 
   - Open the $PATH_TO_CONSOLE_APP folder and check the log files