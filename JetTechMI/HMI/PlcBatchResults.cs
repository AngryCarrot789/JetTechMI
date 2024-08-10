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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetTechMI.Utils;

namespace JetTechMI.HMI;

/// <summary>
/// Contains batch-read data from a PLC
/// </summary>
public class PlcBatchResults {
    public DictionaryList<PlcBatchResultData> List { get; }

    public PlcBatchResults() {
        this.List = new DictionaryList<PlcBatchResultData>();
    }
    
    public void Clear() {
        this.List.List.Clear();
    }

    public PlcBatchResultData GetResultDataForDevice(int device) {
        if (!this.List.TryGet(device, out PlcBatchResultData? data))
            this.List.Set(device, data = new PlcBatchResultData());
        return data;
    }
    
    public bool TryGetResultDataForDevice(int device, [NotNullWhen(true)] out PlcBatchResultData? data) {
        return this.List.TryGet(device, out data);
    }

    public bool TryGetValue<T>(DeviceAddress address, Func<PlcBatchResultData, Dictionary<int, T>> dictProvider, [NotNullWhen(true)] out T? value) {
        if (address.IsValid && this.List.TryGet(address.Device, out PlcBatchResultData? data)) {
            Dictionary<int, T> dict = dictProvider(data);
            return dict.TryGetValue(address.AddressSlot, out value);
        }

        value = default;
        return false;
    }
}

public class PlcBatchResultData {
    public Dictionary<int, bool>? ListForM;
    public Dictionary<int, bool>? ListForS;
    public Dictionary<int, bool>? ListForX;
    public Dictionary<int, bool>? ListForY;
    
    public Dictionary<int, short>? ListForD;
    public Dictionary<int, ushort>? ListForT;
    public Dictionary<int, ushort>? ListForC16;
    public Dictionary<int, int>? ListForC32;

    public PlcBatchResultData() {
    }
}