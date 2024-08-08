using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Threading;
using JetTechMI.Utils;

namespace JetTechMI.HMI;

/// <summary>
/// Contains information about the context of the JetTechMI library instance, such as active connections
/// </summary>
public class JetTechContext {
    public static JetTechContext Instance { get; } = new JetTechContext();

    private readonly DictionaryList<IPlcApi> connections;
    private readonly DispatcherTimer reconnectTimer;
    
    public JetTechContext() {
        this.connections = new DictionaryList<IPlcApi>();
        this.reconnectTimer = new DispatcherTimer(DispatcherPriority.Background) {
            Interval = TimeSpan.FromSeconds(2)
        };
        
        this.reconnectTimer.Tick += this.OnTickReconnectDevices;
        this.reconnectTimer.Start();
    }

    private void OnTickReconnectDevices(object? sender, EventArgs e) {
        foreach (IPlcApi? plc in this.connections.List) {
            plc?.CheckConnection();
        }
    }

    public void RegisterConnection(int id, IPlcApi api) {
        if (api == null)
            throw new ArgumentNullException(nameof(api));
        
        this.connections.Set(id, api);
        api.CheckConnection();
    }
    
    public bool UnregisterConnection(int id) {
        return this.connections.Unset(id);
    }

    /// <summary>
    /// Tries to get a connection from the Id and returns true when it is connected
    /// </summary>
    /// <param name="id">The PLC connection ID</param>
    /// <param name="plcApi">The PLC device</param>
    /// <returns></returns>
    public bool TryGetPLC(int id, [NotNullWhen(true)] out IPlcApi? plcApi) {
        return this.connections.TryGet(id, out plcApi) && plcApi.IsConnected;
    }
    
    public bool TryGetPLC(DeviceAddress address, [NotNullWhen(true)] out IPlcApi? plcApi) {
        return this.TryGetPLC(address.Device, out plcApi);
    }

    public void SubmitBatchRequestData(PlcBatchRequest requests, PlcBatchResults results) {
        foreach (PlcBatchRequestData? info in requests.Requests.List) {
            if (info != null && this.TryGetPLC(info.Device, out IPlcApi? api)) {
                api.ReadBatchedData(info, results.GetResultDataForDevice(info.Device));
            }
        }
    }
}

public static class JtContextExtensions {
    public static bool TryReadBool(this JetTechContext context, DeviceAddress address, out bool value) {
        if (address.IsValid && context.TryGetPLC(address.Device, out IPlcApi? plc)) {
            PlcOperation<bool> operation = plc.ReadBool(address.FullAddress);
            if (operation.IsSuccessful) {
                value = operation.Result;
                return true;
            }
        }

        return value = false;
    }

    public static bool TryReadBool(this JetTechContext context, PlcBatchResults batches, DeviceAddress address, out bool value) {
        if (batches.TryGetResultDataForDevice(address.Device, out PlcBatchResultData? data)) {
            Dictionary<int, bool>? dictionary = null;
            switch (address.AddressPrefix) {
                case 'M': dictionary = data.ListForM; break;
                case 'S': dictionary = data.ListForS; break;
                case 'X': dictionary = data.ListForX; break;
                case 'Y': dictionary = data.ListForY; break;
            }

            if (dictionary != null && dictionary.TryGetValue(address.AddressSlot, out value))
                return true;
        }
        
        return TryReadBool(context, address, out value);
    }
}