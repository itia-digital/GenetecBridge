
## Steps to Create a Scheduled Task
1. Open Task Scheduler (taskschd.msc from the Run dialog or Start Menu). 
2. Create a Basic Task:
   - Click "Create Basic Task". 
   - Give it a name and description. 
   - Set a Trigger:
     - Choose how often the task runs (e.g., Daily, Weekly, At startup). 
     - Configure the schedule. 
   - Set an Action:
     - Choose "Start a Program".
     - Browse to your executable (.exe), batch script (.bat), or PowerShell script (.ps1). 
3. Finish & Enable the Task:
   - Check "Open Properties for this task when I click Finish".
   - In the General tab, select "Run whether user is logged on or not".
   - If necessary, check "Run with highest privileges".
   - Click OK and enter credentials.