// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of JetTechMI.
// 
// JetTechMI is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// JetTechMI is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with JetTechMI. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Threading;
using HslCommunication.Core.Types;
using JetTechMI.Utils;

namespace JetTechMI.HMI;

/// <summary>
/// Contains information about the context of the JetTechMI library instance, such as active connections
/// </summary>
public class JetTechContext {
    public static JetTechContext Instance { get; } = new JetTechContext();

    private readonly DictionaryList<ILogicController> connections;
    private readonly DispatcherTimer reconnectTimer;

    public BatchRequestList BatchRequestList { get; }

    public BatchResultList BatchResultList { get; }

    public JetTechContext() {
        this.connections = new DictionaryList<ILogicController>();
        this.reconnectTimer = new DispatcherTimer(DispatcherPriority.Background) {
            Interval = TimeSpan.FromSeconds(2)
        };
        
        this.reconnectTimer.Tick += this.OnTickReconnectDevices;
        this.reconnectTimer.Start();

        this.BatchRequestList = new BatchRequestList(new DictionaryList<BatchRequestInfo>());
        this.BatchResultList = new BatchResultList();
    }

    private void OnTickReconnectDevices(object? sender, EventArgs e) {
        foreach (ILogicController? plc in this.connections.List) {
            LightOperationResult result = plc!.CheckConnection();
            if (!result.IsSuccess)
                Debug.WriteLine("Failed to reconnect PLC #" + plc.Id + ": " + result.Message);
        }
    }

    /// <summary>
    /// Registers the PLC. <see cref="ILogicController.Id"/> will be accessed
    /// </summary>
    /// <param name="plc"></param>
    /// <exception cref="ArgumentNullException">PLC is null</exception>
    public void RegisterConnection(ILogicController plc) {
        if (plc == null)
            throw new ArgumentNullException(nameof(plc));

        if (this.connections.IsSet(plc.Id))
            throw new InvalidOperationException("PLC already registered with ID " + plc.Id);
        
        this.connections.Set(plc.Id, plc);
        this.BatchRequestList.Requests.Set(plc.Id, plc.CreateRequestData());
        this.BatchResultList.Results.Set(plc.Id, plc.CreateResultData());
        plc.CheckConnection();
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
    public bool TryGetPLC(int id, [NotNullWhen(true)] out ILogicController? plcApi) {
        return this.connections.TryGet(id, out plcApi) && plcApi.IsConnected;
    }
    
    public bool TryGetPLC(DeviceAddress address, [NotNullWhen(true)] out ILogicController? plcApi) {
        return this.TryGetPLC(address.Device, out plcApi);
    }

    public void SubmitBatchRequestData(BatchRequestList requestList, BatchResultList resultList) {
        foreach (BatchRequestInfo? info in requestList.Requests.List) {
            if (info != null && this.TryGetPLC(info.Device, out ILogicController? api) && resultList.TryGetResultDataForDevice(info.Device, out BatchResultData? results)) {
                api.ReadBatchedData(info, results);
            }
        }
    }
}