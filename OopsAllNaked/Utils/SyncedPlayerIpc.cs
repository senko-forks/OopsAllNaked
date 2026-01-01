using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using System;
using System.Collections.Generic;

namespace OopsAllNaked.Utils;

internal class SyncedPlayerIpc
{
    private readonly HashSet<nint> handledAddresses = new(capacity: 1000);

    private readonly List<ICallGateSubscriber<List<nint>>> getHandledGameAddresses = [];
    private readonly string[] functions = [
        "KittenSync.GetHandledAddresses",
        "LaciSynchroni.GetHandledAddresses",
        "LightlessSync.GetHandledAddresses",
        "LoporritSync.GetHandledAddresses",
        "MareSynchronos.GetHandledAddresses",
        "Snowcloak.GetHandledAddresses",
    ];

    private DateTime lastUpdate = DateTime.MinValue;

    public SyncedPlayerIpc(IDalamudPluginInterface pluginInterface)
    {
        foreach (var fn in functions)
            getHandledGameAddresses.Add(pluginInterface.GetIpcSubscriber<List<nint>>(fn));
    }

    public void Update()
    {
        if (lastUpdate == Service.framework.LastUpdate)
            return;

        lastUpdate = Service.framework.LastUpdate;
        handledAddresses.Clear();

        foreach (var ipc in getHandledGameAddresses)
        {
            if (ipc.HasFunction)
            {
                try
                {
                    foreach (nint addr in ipc.InvokeFunc())
                        handledAddresses.Add(addr);
                }
                catch { }
            }
        }
    }

    public bool IsHandledAddress(nint address)
    {
        return handledAddresses.Contains(address);
    }

    public void Clear()
    {
        handledAddresses.Clear();
    }
}
