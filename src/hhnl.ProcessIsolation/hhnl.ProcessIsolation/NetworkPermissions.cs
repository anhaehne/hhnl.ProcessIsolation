using System;

namespace hhnl.ProcessIsolation
{
    [Flags]
    public enum NetworkPermissions
    {
        None = 0,
        LocalNetwork = 1,
        Internet = 2
    }
}