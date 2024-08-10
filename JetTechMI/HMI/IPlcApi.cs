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

namespace JetTechMI.HMI;

/// <summary>
/// An interface for interacting with a PLC device
/// </summary>
public interface IPlcApi {
    /// <summary>
    /// Gets whether or not the PLC is currently connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Called every so often to ensure the PLC is still connected. If not, attempt reconnection
    /// </summary>
    /// <returns>True if already connected or the reconnection was successful</returns>
    bool CheckConnection();

    /// <summary>
    /// Reads multiple addresses from this PLC device and places the resulting data in the result data object
    /// </summary>
    /// <param name="request"></param>
    /// <param name="result"></param>
    void ReadBatchedData(PlcBatchRequestData request, PlcBatchResultData result);
    
    PlcOperation<bool> ReadBool(string address);
    PlcOperation<bool[]> ReadBoolArray(string address, ushort length);
    PlcOperation<byte> ReadByte(string address);
    PlcOperation<byte[]> ReadByteArray(string address, ushort length);
    PlcOperation<short> ReadInt16(string address);
    PlcOperation<short[]> ReadInt16Array(string address, ushort length);
    PlcOperation<ushort> ReadUInt16(string address);
    PlcOperation<ushort[]> ReadUInt16Array(string address, ushort length);
    PlcOperation<int> ReadInt32(string address);
    PlcOperation<int[]> ReadInt32Array(string address, ushort length);
    PlcOperation<uint> ReadUInt32(string address);
    PlcOperation<uint[]> ReadUInt32Array(string address, ushort length);
    PlcOperation<float> ReadFloat(string address);
    PlcOperation<float[]> ReadFloatArray(string address, ushort length);
    PlcOperation<long> ReadInt64(string address);
    PlcOperation<long[]> ReadInt64Array(string address, ushort length);
    PlcOperation<ulong> ReadUInt64(string address);
    PlcOperation<ulong[]> ReadUInt64Array(string address, ushort length);
    PlcOperation<double> ReadDouble(string address);
    PlcOperation<double[]> ReadDoubleArray(string address, ushort length);
    PlcOperation<string> ReadString(string address, ushort length);
    
    
    PlcOperation WriteBool(string address, bool value);
    PlcOperation WriteByte(string address, byte value);
    PlcOperation WriteByteArray(string address, byte[] value);
    PlcOperation WriteInt16(string address, short value);
    PlcOperation WriteInt16Array(string address, short[] values);
    PlcOperation WriteUInt16(string address, ushort value);
    PlcOperation WriteUInt16Array(string address, ushort[] values);
    PlcOperation WriteInt32(string address, int value);
    PlcOperation WriteInt32Array(string address, int[] values);
    PlcOperation WriteUInt32(string address, uint value);
    PlcOperation WriteUInt32Array(string address, uint[] values);
    PlcOperation WriteFloat(string address, float value);
    PlcOperation WriteFloatArray(string address, float[] values);
    PlcOperation WriteInt64(string address, long value);
    PlcOperation WriteInt64Array(string address, long[] values);
    PlcOperation WriteUInt64(string address, ulong value);
    PlcOperation WriteUInt64Array(string address, ulong[] values);
    PlcOperation WriteDouble(string address, double value);
    PlcOperation WriteDoubleArray(string address, double[] values);
    PlcOperation WriteString(string address, string value);
    PlcOperation WriteString(string address, string value, int length);
}