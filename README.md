# hhnl.ProcessIsolation
A .net library to start isolated processes.

![Nuget](https://img.shields.io/nuget/v/hhnl.ProcessIsolation?label=hhnl.ProcessIsolation)

Eventhough this is a .net standard project, only windows is currently supported.
Processes are run inside an appcontainer which restricts network, file and windows access.

## Example:
``` csharp
IProcessIsolator isolator = new AppContainerIsolator()
isolator.StartIsolatedProcess("MyIsolatedProcess", "c:\\windows\\notepad.exe", makeApplicationDirectoryReadable: false);
```
This should open notepad. If you try to open a file with "File => Open" you should get an error "Access denied". 
See https://docs.microsoft.com/en-us/windows/win32/secauthz/appcontainer-isolation for more information.

### Remarks:
By default the application will be granted read access to the folder the executable is in.

If you want to prevent this behaviour because the application does not need acces or the current user has no permission to that folder, you can suppress the behaviour by setting 'makeApplicationDirectoryReadable: false'.

It is included in this example because changing the permissions of 'c:\windows' requires admin privileges by default.

## Allow network access:
``` csharp
using hhnl.ProcessIsolation.Windows;

// Allows internet and local network access
isolator.StartIsolatedProcess("MyIsolatedProcess", "myapp.exe", networkPermissions: NetworkPermissions.Internet | NetworkPermissions.LocalNetwork);
```

## Allow file access:
``` csharp
using System;
using hhnl.ProcessIsolation.Windows;

// Allows read and write access to the desktop
var desktopPath = Environment.ExpandEnvironmentVariables("%userprofile%\\Desktop");
var desktopFileAccess = new FileAccess(desktopPath, FileAccess.Right.Read | FileAccess.Right.Write);

isolator.StartIsolatedProcess("MyIsolatedProcess", "myapp.exe", fileAccess: new[] { desktopFileAccess });
```
## Attach child process:
By default the create process will be attached to the current process. 
This will cause the new process to be closed once the current process is close.
To prevent this behaviour you can set `attachToCurrentProcess = false`

