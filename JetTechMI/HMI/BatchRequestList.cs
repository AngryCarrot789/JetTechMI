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

    public void TryRequest(DeviceAddress? address, DataSize dataSize) {
        if (address != null && this.Requests.TryGet(address.Device, out BatchRequestInfo? data)) {
            data.Request(address.Address, dataSize);
        }
    }
}

/// <summary>
/// A struct which contains batch request information in relation to a specific PLC device
/// </summary>
public abstract class BatchRequestInfo {
    public readonly int Device;

    public BatchRequestInfo(int device) {
        this.Device = device;
    }

    public abstract void Clear();
    
    public abstract void Request(string address, DataSize dataSize);
}