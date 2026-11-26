## Part 0:

### 1. The file key location and the line in .gitignore that keeps it out of git.
- It in the Al-BoomehAPI project next to Program.cs
- The line is in Sensitive configuration section 

### 2. The output from git status :
``
λ git status
On branch day-08-notifications
Changes not staged for commit:
  (use "git add <file>..." to update what will be committed)
  (use "git restore <file>..." to discard changes in working directory)
        modified:   .gitignore

no changes added to commit (use "git add" and/or "git commit -a")
``
### 3. To delete the key of a service account:
- Open the servie account in the project settings then open the link `MeManage service account permissions` then select the file key and click on the delete button

## Part 1:

### 1. The register method:
```csharp
public async Task Register(DevicesDto devicesDto)
{
    if (!_user.UserId.HasValue)
    {
        throw new InvalidOperationException("Current user does not have a valid UserId.");
    }

    var device =await _context.PushDevices
        .Where(d=>d.InstallationId == devicesDto.InstallationId)
        .FirstOrDefaultAsync();
    if (device == null)
    {
        var newDevice = new PushDevice()
        {
            InstallationId = devicesDto.InstallationId,
            UserId = _user.UserId.Value.ToString(),
            Platform=(int)devicesDto.Platform,
            IsActive=true
        };
        await _context.PushDevices.AddAsync(newDevice);
    }
    //the lines below that move an FID to a new user 
    else
    {
        device.LastSeenAtUtc = DateTime.UtcNow;
        device.UserId= _user.UserId.Value.ToString();
        device.IsActive = true;
    }
    int rowEffected=await _context.SaveChangesAsync();
    if (rowEffected > 0)
    {
        _logger.LogInformation("Device register successfully with FID {FID}",
            devicesDto.InstallationId);
         await Task.CompletedTask;
        return;
    }

    _logger.LogWarning("Device register failed with FID {FID}",
        devicesDto.InstallationId);
    throw new Exception("Failed to register the device");
} 
```
### 2. The rows from SSMS:

- after step 8 :
1	8f9c6f46-120d-4903-b804-f0474062c4a1	test-fid-1	0	NULL	1	2026-11-26 06:20:07.3203089	NULL	0	NULL
- after step 10 : 1	046C0675-0260-4F37-9FD8-000AFB12BBB0	test-fid-1	0	2026-11-26 06:26:40.0480258	1	2026-11-26 06:20:07.3203089	2026-11-26 06:26:40.0480908	0	NULL

### 3. The status code that unregister returned in step 11:
- is 403 Forbidden

