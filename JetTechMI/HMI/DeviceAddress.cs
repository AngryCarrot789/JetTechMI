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

namespace JetTechMI.HMI;

public readonly struct DeviceAddress : IEquatable<DeviceAddress> {
    public readonly int Device;
    public readonly char AddressPrefix;
    public readonly int AddressSlot;
    
    public readonly string FullAddress;

    public bool IsValid => this.Device >= 0 && this.AddressPrefix != '\0' && this.AddressSlot >= 0;
    
    public DeviceAddress(int Device, char addressPrefix, int addressSlot) {
        this.Device = Device;
        this.AddressPrefix = addressPrefix;
        this.AddressSlot = addressSlot;
        this.FullAddress = addressPrefix.ToString() + addressSlot;
    }

    // 0.Y5
    public static DeviceAddress Parse(string? value) {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null, empty or consist of only whitespaces");
        
        if (value.Length < 2)
            throw new ArgumentException("Not enough characters in the string for a valid address: " + value);
        
        int index = value.IndexOf('#');
        if (index == -1) {
            (char addrCh2, int addrInd2) = ParseAddressPart(value, 0);
            return new DeviceAddress(0, addrCh2, addrInd2);
        }

        if (index == 0)
            throw new ArgumentException("Device number separator is at the start of the string: " + value);
        
        if (index == (value.Length - 1))
            throw new ArgumentException("Device number separator is at the end of the string: " + value);
        
        if (value.Length < 4)
            throw new ArgumentException("Not enough characters in the string for a valid address");
        
        if (!int.TryParse(value.AsSpan(0, index), out int device))
            throw new ArgumentException("Invalid integer for device ID: " + value.Substring(0, index) + " in string " + value);
        
        (char addrCh, int addrInd) = ParseAddressPart(value, index + 1);
        return new DeviceAddress(device, addrCh, addrInd);
    }

    private static (char, int) ParseAddressPart(string value, int startIndex) {
        switch (value[startIndex]) {
            case 'm':
            case 'M': return ('M', int.Parse(value.AsSpan(startIndex + 1)));
            case 's':
            case 'S': return ('S', int.Parse(value.AsSpan(startIndex + 1)));
            case 't':
            case 'T': return ('T', int.Parse(value.AsSpan(startIndex + 1)));
            case 'c':
            case 'C': return ('C', int.Parse(value.AsSpan(startIndex + 1)));
            case 'd':
            case 'D': return ('D', int.Parse(value.AsSpan(startIndex + 1)));
            case 'x':
            case 'X': return ('X', int.Parse(value.AsSpan(startIndex + 1)));
            case 'y':
            case 'Y': return ('Y', int.Parse(value.AsSpan(startIndex + 1)));
            case 'r':
            case 'R': return ('R', int.Parse(value.AsSpan(startIndex + 1)));
            default: throw new ArgumentException("Unknown address target: " + value);
        }
    }
    
    public static bool TryParse(string? value, out DeviceAddress address) {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 2) {
            address = default;
            return false;
        }

        int index = value.IndexOf('#');
        if (index == -1) {
            try {
                (char addrCh, int addrInd) = ParseAddressPart(value, 0);
                address = new DeviceAddress(0, addrCh, addrInd);
                return true;
            }
            catch {
                address = default;
                return false;
            }
        }

        if (value.Length > 3 && index > 0 && index < (value.Length - 1) && int.TryParse(value.AsSpan(0, index), out int device)) {
            string actualAddress = value.Substring(index + 1);
            if (actualAddress.Length > 1) {
                try {
                    (char addrCh, int addrInd) = ParseAddressPart(value, index + 1);
                    address = new DeviceAddress(device, addrCh, addrInd);
                    return true;
                }
                catch {
                    address = default;
                    return false;
                }
            }
        }

        address = new DeviceAddress(-1, '\0', -1);
        return false;
    }

    public void Deconstruct(out int Device, out string Address) {
        Device = this.Device;
        Address = this.FullAddress;
    }

    public bool Equals(DeviceAddress other) {
        return this.Device == other.Device && this.FullAddress == other.FullAddress;
    }

    public override bool Equals(object? obj) {
        return obj is DeviceAddress other && this.Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(this.Device, this.FullAddress);
    }

    public static bool operator ==(DeviceAddress left, DeviceAddress right) => left.Equals(right);

    public static bool operator !=(DeviceAddress left, DeviceAddress right) => !(left == right);
}