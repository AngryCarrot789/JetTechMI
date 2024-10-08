﻿// 
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
using System.Threading.Tasks;
using Avalonia.Controls;

namespace JetTechMI.HMI;

public abstract class BaseControlData<T> : IJtControlData<T> where T : Control {
    public bool IsConnected { get; private set; }
    
    Control IJtControlData.Control => this.Control;

    public T Control { get; }

    /// <summary>
    /// Helper property for quickly accessing the jet tech context
    /// </summary>
    public JetTechContext Context => JetTechContext.Instance;

    protected BaseControlData(T control) {
        this.Control = control ?? throw new ArgumentNullException(nameof(control), "Control cannot be null");
    }

    public abstract Task UpdateAsync(BatchResultList batches);

    public void OnConnectToManager() {
        if (this.IsConnected)
            throw new InvalidOperationException("Already connected");
        this.IsConnected = true;
        this.OnConnectedCore();
    }

    public void OnDisconnectFromManager() {
        if (!this.IsConnected)
            throw new InvalidOperationException("Not connected");
        this.OnDisconnectedCore();
        this.IsConnected = false;
    }

    protected abstract void OnConnectedCore();
    
    protected abstract void OnDisconnectedCore();

    public virtual void SubmitBatchData(BatchRequestList data) {
    }
    
    public bool TryReadBool(BatchResultList batches, DeviceAddress? address, out bool value) {
        if (address != null && batches.TryGetResultDataForDevice(address.Device, out BatchResultData? data)) {
            return data.ReadBool(address).TryGetValue(out value);
        }

        return value = false;
    }

    public bool TryReadByte(BatchResultList batches, DeviceAddress? address, out byte value) {
        if (address != null && batches.TryGetResultDataForDevice(address.Device, out BatchResultData? data)) {
            return data.ReadByte(address).TryGetValue(out value);
        }

        value = default;
        return false;
    }

    public bool TryReadInt16(BatchResultList batches, DeviceAddress? address, out short value) {
        if (address != null && batches.TryGetResultDataForDevice(address.Device, out BatchResultData? data)) {
            return data.ReadInt16(address).TryGetValue(out value);
        }

        value = default;
        return false;
    }

    public bool TryReadInt32(BatchResultList batches, DeviceAddress? address, out int value) {
        if (address != null && batches.TryGetResultDataForDevice(address.Device, out BatchResultData? data)) {
            return data.ReadInt32(address).TryGetValue(out value);
        }

        value = default;
        return false;
    }

    public bool TryReadInt64(BatchResultList batches, DeviceAddress? address, out long value) {
        if (address != null && batches.TryGetResultDataForDevice(address.Device, out BatchResultData? data)) {
            return data.ReadInt64(address).TryGetValue(out value);
        }

        value = default;
        return false;
    }

    public bool TryReadFloat(BatchResultList batches, DeviceAddress? address, out float value) {
        if (address != null && batches.TryGetResultDataForDevice(address.Device, out BatchResultData? data)) {
            return data.ReadFloat(address).TryGetValue(out value);
        }

        value = default;
        return false;
    }

    public bool TryReadDouble(BatchResultList batches, DeviceAddress? address, out double value) {
        if (address != null && batches.TryGetResultDataForDevice(address.Device, out BatchResultData? data)) {
            return data.ReadDouble(address).TryGetValue(out value);
        }

        value = default;
        return false;
    }
}