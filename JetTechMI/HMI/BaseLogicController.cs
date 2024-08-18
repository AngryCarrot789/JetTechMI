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
/// The base class for a PLC API
/// </summary>
public abstract class BaseLogicController : ILogicController {
    public abstract bool IsConnected { get; }
    public abstract bool IsDoubleSupported { get; }
    public abstract bool IsInt64Supported { get; }
    public ushort Id { get; }

    protected BaseLogicController(ushort id) {
        this.Id = id;
    }

    public abstract LightOperationResult CheckConnection();
    public abstract BatchRequestInfo CreateRequestData();
    public abstract BatchResultData CreateResultData();

    public abstract void ReadBatchedData(BatchRequestInfo requests, BatchResultData results);

    public virtual LightOperationResult<bool> ReadBool(string address) => this.ReadBoolArray(address, 1).Select(x => x[0]);
    public abstract LightOperationResult<bool[]> ReadBoolArray(string address, ushort length);
    public virtual LightOperationResult<byte> ReadByte(string address) => this.ReadByteArray(address, 1).Select(x => x[0]);
    public abstract LightOperationResult<byte[]> ReadByteArray(string address, ushort length);
    public virtual LightOperationResult<short> ReadInt16(string address) => this.ReadInt16Array(address, 1).Select(x => x[0]);
    public abstract LightOperationResult<short[]> ReadInt16Array(string address, ushort length);
    public virtual LightOperationResult<int> ReadInt32(string address) => this.ReadInt32Array(address, 1).Select(x => x[0]);
    public abstract LightOperationResult<int[]> ReadInt32Array(string address, ushort length);
    public virtual LightOperationResult<long> ReadInt64(string address) => this.ReadInt64Array(address, 1).Select(x => x[0]);
    public abstract LightOperationResult<long[]> ReadInt64Array(string address, ushort length);
    public virtual LightOperationResult<float> ReadFloat(string address) => this.ReadFloatArray(address, 1).Select(x => x[0]);
    public abstract LightOperationResult<float[]> ReadFloatArray(string address, ushort length);
    public virtual LightOperationResult<double> ReadDouble(string address) => this.ReadDoubleArray(address, 1).Select(x => x[0]);
    public abstract LightOperationResult<double[]> ReadDoubleArray(string address, ushort length);
    public abstract LightOperationResult<string> ReadString(string address, ushort length);

    public abstract LightOperationResult WriteBool(string address, bool value);
    public virtual LightOperationResult WriteByte(string address, byte value) => this.WriteByteArray(address, new byte[] { value });
    public abstract LightOperationResult WriteByteArray(string address, byte[] value);
    public virtual LightOperationResult WriteInt16(string address, short value) => this.WriteInt16Array(address, new short[] { value });
    public abstract LightOperationResult WriteInt16Array(string address, short[] values);
    public virtual LightOperationResult WriteInt32(string address, int value) => this.WriteInt32Array(address, new int[] { value });
    public abstract LightOperationResult WriteInt32Array(string address, int[] values);
    public virtual LightOperationResult WriteInt64(string address, long value) => this.WriteInt64Array(address, new long[] { value });
    public abstract LightOperationResult WriteInt64Array(string address, long[] values);
    public virtual LightOperationResult WriteFloat(string address, float value) => this.WriteFloatArray(address, new float[] { value });
    public abstract LightOperationResult WriteFloatArray(string address, float[] values);
    public virtual LightOperationResult WriteDouble(string address, double value) => this.WriteDoubleArray(address, new double[] { value });
    public abstract LightOperationResult WriteDoubleArray(string address, double[] values);
    public abstract LightOperationResult WriteString(string address, string value);
    public abstract LightOperationResult WriteString(string address, string value, int length);
}