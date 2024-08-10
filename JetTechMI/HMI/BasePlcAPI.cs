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
/// The base class for a PLC API
/// </summary>
public abstract class BasePlcAPI : IPlcApi {
    public abstract bool IsConnected { get; }

    public BasePlcAPI() {
    }

    public abstract bool CheckConnection();

    public abstract void ReadBatchedData(PlcBatchRequestData request, PlcBatchResultData result);

    public virtual PlcOperation<bool> ReadBool(string address) => this.ReadBoolArray(address, 1).Select(x => x[0]);
    public abstract PlcOperation<bool[]> ReadBoolArray(string address, ushort length);
    public virtual PlcOperation<byte> ReadByte(string address) => this.ReadByteArray(address, 1).Select(x => x[0]);
    public abstract PlcOperation<byte[]> ReadByteArray(string address, ushort length);
    public virtual PlcOperation<short> ReadInt16(string address) => this.ReadInt16Array(address, 1).Select(x => x[0]);
    public abstract PlcOperation<short[]> ReadInt16Array(string address, ushort length);
    public virtual PlcOperation<ushort> ReadUInt16(string address) => this.ReadUInt16Array(address, 1).Select(x => x[0]);
    public abstract PlcOperation<ushort[]> ReadUInt16Array(string address, ushort length);
    public virtual PlcOperation<int> ReadInt32(string address) => this.ReadInt32Array(address, 1).Select(x => x[0]);
    public abstract PlcOperation<int[]> ReadInt32Array(string address, ushort length);
    public virtual PlcOperation<uint> ReadUInt32(string address) => this.ReadUInt32Array(address, 1).Select(x => x[0]);
    public abstract PlcOperation<uint[]> ReadUInt32Array(string address, ushort length);
    public virtual PlcOperation<float> ReadFloat(string address) => this.ReadFloatArray(address, 1).Select(x => x[0]);
    public abstract PlcOperation<float[]> ReadFloatArray(string address, ushort length);
    public virtual PlcOperation<long> ReadInt64(string address) => this.ReadInt64Array(address, 1).Select(x => x[0]);
    public abstract PlcOperation<long[]> ReadInt64Array(string address, ushort length);
    public virtual PlcOperation<ulong> ReadUInt64(string address) => this.ReadUInt64Array(address, 1).Select(x => x[0]);
    public abstract PlcOperation<ulong[]> ReadUInt64Array(string address, ushort length);
    public virtual PlcOperation<double> ReadDouble(string address) => this.ReadDoubleArray(address, 1).Select(x => x[0]);
    public abstract PlcOperation<double[]> ReadDoubleArray(string address, ushort length);
    public abstract PlcOperation<string> ReadString(string address, ushort length);
    public abstract PlcOperation WriteBool(string address, bool value);
    public virtual PlcOperation WriteByte(string address, byte value) => this.WriteByteArray(address, new byte[] { value });
    public abstract PlcOperation WriteByteArray(string address, byte[] value);
    public virtual PlcOperation WriteInt16(string address, short value) => this.WriteInt16Array(address, new short[] { value });
    public abstract PlcOperation WriteInt16Array(string address, short[] values);
    public virtual PlcOperation WriteUInt16(string address, ushort value) => this.WriteUInt16Array(address, new ushort[] { value });
    public abstract PlcOperation WriteUInt16Array(string address, ushort[] values);
    public virtual PlcOperation WriteInt32(string address, int value) => this.WriteInt32Array(address, new int[] { value });
    public abstract PlcOperation WriteInt32Array(string address, int[] values);
    public virtual PlcOperation WriteUInt32(string address, uint value) => this.WriteUInt32Array(address, new uint[] { value });
    public abstract PlcOperation WriteUInt32Array(string address, uint[] values);
    public virtual PlcOperation WriteFloat(string address, float value) => this.WriteFloatArray(address, new float[] { value });
    public abstract PlcOperation WriteFloatArray(string address, float[] values);
    public virtual PlcOperation WriteInt64(string address, long value) => this.WriteInt64Array(address, new long[] { value });
    public abstract PlcOperation WriteInt64Array(string address, long[] values);
    public virtual PlcOperation WriteUInt64(string address, ulong value) => this.WriteUInt64Array(address, new ulong[] { value });
    public abstract PlcOperation WriteUInt64Array(string address, ulong[] values);
    public virtual PlcOperation WriteDouble(string address, double value) => this.WriteDoubleArray(address, new double[] { value });
    public abstract PlcOperation WriteDoubleArray(string address, double[] values);
    public abstract PlcOperation WriteString(string address, string value);
    public abstract PlcOperation WriteString(string address, string value, int length);
}