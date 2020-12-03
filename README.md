# hhnl.ProcessIsolation
A .net library to start isolated processes.

Eventhough this is a .net standard project, only windows is currently supported.
Processes are run inside an appcontainer which restricts network, file and windows access.

Example:
``` csharp
IProcessIsolator isolator = new AppContainerIsolator()
isolator.StartIsolatedProcess("MyIsolatedProcess", "notepad.exe", makeApplicationDirectoryReadable: false);
```
This should open notepad. If you try to open a file with "File => Open" you should get an error "Access denied". 
See https://docs.microsoft.com/en-us/windows/win32/secauthz/appcontainer-isolation for more information.

Allow network access:
``` csharp
using hhnl.ProcessIsolation.Windows;

// Allows internet and local network access
isolator.StartIsolatedProcess("MyIsolatedProcess", "myapp.exe", networkPermissions: NetworkPermissions.Internet | NetworkPermissions.LocalNetwork);
```

Allow file access:
``` csharp
using System;
using hhnl.ProcessIsolation.Windows;

// Allows read and write access to the desktop
var desktopPath = Environment.ExpandEnvironmentVariables("%userprofile%\\Desktop");
var desktopFileAccess = new FileAccess(desktopPath, FileAccess.Right.Read | FileAccess.Right.Write);

isolator.StartIsolatedProcess("MyIsolatedProcess", "myapp.exe", fileAccess: new[] { desktopFileAccess });
```
