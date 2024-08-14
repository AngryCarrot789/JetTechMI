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

using System.Diagnostics.CodeAnalysis;
using HslCommunication.Core.Types;
using JetTechMI.Utils;

namespace JetTechMI.HMI;

/// <summary>
/// Contains batch-read data from a PLC
/// </summary>
public class BatchResultList {
    public DictionaryList<BatchResultData> Results { get; }

    public BatchResultList() {
        this.Results = new DictionaryList<BatchResultData>();
    }
    
    public void Clear() {
        foreach (BatchResultData? data in this.Results.List) {
            data?.Clear();
        }
    }
    
    public bool TryGetResultDataForDevice(int device, [NotNullWhen(true)] out BatchResultData? data) {
        return this.Results.TryGet(device, out data);
    }
}

public abstract class BatchResultData {
    public readonly ushort Device;

    protected BatchResultData(ushort deviceId) {
        this.Device = deviceId;
    }

    public abstract void Clear();

    public abstract LightOperationResult<bool> ReadBool(DeviceAddress address); 
    public abstract LightOperationResult<byte> ReadByte(DeviceAddress address); 
    public abstract LightOperationResult<short> ReadInt16(DeviceAddress address); 
    public abstract LightOperationResult<int> ReadInt32(DeviceAddress address); 
    public abstract LightOperationResult<long> ReadInt64(DeviceAddress address); 
    public abstract LightOperationResult<float> ReadFloat(DeviceAddress address); 
    public abstract LightOperationResult<double> ReadDouble(DeviceAddress address); 
}