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