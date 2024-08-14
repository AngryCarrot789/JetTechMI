using HslCommunication.Core.Types;

namespace JetTechMI.HMI;

/// <summary>
/// An interface that provides basic 
/// </summary>
public interface IWriteableAddressMemory {
    LightOperationResult WriteBool(string address, bool value);
    LightOperationResult WriteBoolArray(string address, bool[] values);
    LightOperationResult WriteByte(string address, byte value);
    LightOperationResult WriteByteArray(string address, byte[] values);
    LightOperationResult WriteInt16(string address, short value);
    LightOperationResult WriteInt16Array(string address, short[] values);
    LightOperationResult WriteInt32(string address, int value);
    LightOperationResult WriteInt32Array(string address, int[] values);
    LightOperationResult WriteInt64(string address, long value);
    LightOperationResult WriteInt64Array(string address, long[] values);
    LightOperationResult WriteFloat(string address, float value);
    LightOperationResult WriteFloatArray(string address, float[] values);
    LightOperationResult WriteDouble(string address, double value);
    LightOperationResult WriteDoubleArray(string address, double[] values);
    LightOperationResult WriteString(string address, string value, int length);
}
