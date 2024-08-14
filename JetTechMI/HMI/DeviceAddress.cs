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
using System.Diagnostics.CodeAnalysis;
using JetTechMI.Utils;

namespace JetTechMI.HMI;

/// <summary>
/// Represents an address on a specific PLC device.
/// Format is #ID.ADDRESS. E.g.: <code>#0.M2</code>
/// <para>
/// The device identifier can be ignored to default to the device with an ID of 0 (main device)
/// </para>
/// </summary>
public class DeviceAddress : IEquatable<DeviceAddress> {
    /// <summary>
    /// Gets the device ID
    /// </summary>
    public readonly int Device;
    
    /// <summary>
    /// Gets the actual address part. The meaning of this differs between PLCs
    /// </summary>
    public readonly string Address;

    // A PLC-specific object used to storing temporary data about this
    // address, e.g. parsed data from Address for performance reasons
    private object? plcData;

    public DeviceAddress(int device, string address) {
        Validate.NotNullOrWhiteSpaces(address);
        
        this.Device = device;
        this.Address = address;
    }

    // 0.Y5
    public static DeviceAddress Parse(string? value) {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null, empty or consist of only whitespaces");

        int id = 0, i = 0, idx;
        if (value[0] == '#') {
            if ((idx = value.IndexOf('.')) == -1)
                throw new Exception("Missing a dot ('.') after the device identifier");
            
            if (!int.TryParse(value.AsSpan(1, idx - 1), out id))
                throw new Exception("Invalid device ID: " + value.Substring(1, idx));

            i = idx + 1;
        }

        return new DeviceAddress(id, value.Substring(i));
    }

    public bool Equals(DeviceAddress? other) {
        return ReferenceEquals(this, other) || (other != null && other.Device == this.Device && other.Address == this.Address);
    }

    public override bool Equals(object? obj) => obj is DeviceAddress address && this.Equals(address);

    public override int GetHashCode() => HashCode.Combine(this.Device, this.Address);

    public static bool TryGetPlcData<T>(DeviceAddress address, [NotNullWhen(true)] out T? data) where T : class => (data = address.plcData as T) != null;

    public static void SetPlcData(DeviceAddress address, object? value) {
        // Just in case...
        if (address.plcData is IDisposable disposable)
            disposable.Dispose();
        
        address.plcData = value;
    }
}