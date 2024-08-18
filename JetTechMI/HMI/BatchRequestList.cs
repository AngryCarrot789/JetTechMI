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

using HslCommunication.Core.Types;
using JetTechMI.Utils;

namespace JetTechMI.HMI;

/// <summary>
/// Contains information about the requested addresses to be read for every PLC currently registered
/// </summary>
public class BatchRequestList {
    /// <summary>
    /// Gets a dictionary that maps a PLC id to the request data for that specific PLC
    /// </summary>
    public DictionaryList<BatchRequestInfo> Requests { get; }

    public BatchRequestList(DictionaryList<BatchRequestInfo> list) {
        this.Requests = list;
    }

    public void Prepare() {
        foreach (BatchRequestInfo? device in this.Requests.List)
            device?.Clear();
    }

    public void Submit(JetTechContext context, BatchResultList batchesData) => context.SubmitBatchRequestData(this, batchesData);

    /// <summary>
    /// Tries to find the associated batch request info for the address' device and then
    /// requests the address for that PLC device via <see cref="BatchRequestInfo.Request"/>
    /// </summary>
    /// <param name="address">The address of the data</param>
    /// <param name="dataSize">The size of the data</param>
    public void TryRequest(DeviceAddress? address, DataSize dataSize) {
        if (address != null && this.Requests.TryGet(address.Device, out BatchRequestInfo? data)) {
            data.Request(address.Address, dataSize);
        }
    }
}

/// <summary>
/// A class which stores all of the variable requests in relation to a specific PLC device
/// </summary>
public abstract class BatchRequestInfo {
    /// <summary>
    /// The device ID associated with this request info
    /// </summary>
    public int Device { get; }

    protected BatchRequestInfo(int device) {
        this.Device = device;
    }

    /// <summary>
    /// Clears all of the requests. This is called automatically before the batch tick
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Adds the address to the request list with the given data size. Care must be taken to ensure the data size
    /// matches the underlying data on the PLC, otherwise you may get back invalid data. For example, if you request
    /// "M0", make sure the data size is <see cref="DataSize.Bit"/> (which is the case for most PLCs). Or if you
    /// request "D2", make sure the data size is <see cref="DataSize.Word"/> (since in most PLCs, data registers are
    /// 16 bits, although you can actually combine 2 registers into one, in that case, supply <see cref="DataSize.DWord"/>)
    /// </summary>
    /// <param name="address">The address of the data</param>
    /// <param name="dataSize">The size of the data</param>
    /// <returns>The result of the request. May contain an error such as invalid address format</returns>
    public abstract LightOperationResult Request(string address, DataSize dataSize);
}