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

namespace JetTechMI.HMI;

/// <summary>
/// An interface that provides basic 
/// </summary>
public interface IReadableAddressMemory {
    LightOperationResult<bool> ReadBool(string address);
    LightOperationResult<bool[]> ReadBoolArray(string address, ushort length);
    LightOperationResult<byte> ReadByte(string address);
    LightOperationResult<byte[]> ReadByteArray(string address, ushort length);
    LightOperationResult<short> ReadInt16(string address);
    LightOperationResult<short[]> ReadInt16Array(string address, ushort length);
    LightOperationResult<int> ReadInt32(string address);
    LightOperationResult<int[]> ReadInt32Array(string address, ushort length);
    LightOperationResult<long> ReadInt64(string address);
    LightOperationResult<long[]> ReadInt64Array(string address, ushort length);
    LightOperationResult<float> ReadFloat(string address);
    LightOperationResult<float[]> ReadFloatArray(string address, ushort length);
    LightOperationResult<double> ReadDouble(string address);
    LightOperationResult<double[]> ReadDoubleArray(string address, ushort length);
    LightOperationResult<string> ReadString(string address, ushort length);
}