using UnityEngine;
using Fusion;

public static class ExtensionMethods 
{
    public static bool IsNetworkObjectOfMasterClient(this NetworkRunner runner, NetworkObject Object)
    {
        return (runner.IsSharedModeMasterClient && Object.HasStateAuthority) || GameManager.isSinglePlayerMode;
    }

    public static bool IsMasterClient(this NetworkRunner runner)
    {
        return runner.IsSharedModeMasterClient || GameManager.isSinglePlayerMode;
    }
}
