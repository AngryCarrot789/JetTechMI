using System;
using System.Collections.Generic;
using HslCommunication.Core.Types;
using HslCommunication.Devices.Melsec;
using JetTechMI.HMI;
using JetTechMI.Utils;

namespace JetTechMI.Hsl;

public class HslMelsecPlc : BasePlcAPI {
    public override bool IsConnected => this.plc.IsOpen();

    private readonly MelsecFxSerial plc;

    public HslMelsecPlc(MelsecFxSerial plc) {
        this.plc = plc;
    }
    
    public override bool CheckConnection() {
        // THIS IS ASS CHEEKS!
        if (!this.IsConnected) {
            try {
                this.plc.Open();
            }
            catch (Exception e) {
                return false;
            }
        }
        
        return true;
    }

    private static readonly IntegerRangeList EmptyIntRangeList = new IntegerRangeList();

    private static void ApplyOperation<T>(OperateResult<T> result, int index, ref Dictionary<int, T>? dictionary) {
        if (result.IsSuccess)
            (dictionary ??= new Dictionary<int, T>())[index] = result.Content;
    }
    
    private static void ApplyOperations<T>(MelsecFxSerial serial, string prefix, IntegerRangeList? list, ref Dictionary<int, T>? dictionary, Func<MelsecFxSerial, string, ushort, OperateResult<T[]>> operate) {
        if (list == null || list.IsEmpty) {
            return;
        }

        foreach (IntRange range in list.Items) {
            int count = range.B - range.A + 1;
            if (count < 0 || count > ushort.MaxValue)
                throw new InvalidOperationException("Out of range array length");
            
            OperateResult<T[]> result = operate(serial, prefix + range.A, (ushort) count);
            if (result.IsSuccess && result.Content != null) {
                T[] array = result.Content;
                if (array.Length != count)
                    throw new InvalidOperationException("WTF");
                
                if (dictionary == null)
                    dictionary = new Dictionary<int, T>();
                
                for (int i = range.A; i <= range.B; i++)
                    dictionary[i] = array[i - range.A];
            }
        }
    }
    
    public override void ReadBatchedData(PlcBatchRequestData request, PlcBatchResultData result) {
        ApplyOperations(this.plc, "C", request.ListForC16, ref result.ListForC16, (p, a, c) => p.ReadUInt16(a, c));
        ApplyOperations(this.plc, "C", request.ListForC32, ref result.ListForC32, (p, a, c) => p.ReadInt32(a, c));
        ApplyOperations(this.plc, "M", request.ListForM, ref result.ListForM, (p, a, c) => p.ReadBool(a, c));
        ApplyOperations(this.plc, "S", request.ListForS, ref result.ListForS, (p, a, c) => p.ReadBool(a, c));
        ApplyOperations(this.plc, "T", request.ListForT, ref result.ListForT, (p, a, c) => p.ReadUInt16(a, c));
        ApplyOperations(this.plc, "X", request.ListForX, ref result.ListForX, (p, a, c) => p.ReadBool(a, c));
        ApplyOperations(this.plc, "Y", request.ListForY, ref result.ListForY, (p, a, c) => p.ReadBool(a, c));

        // request.ListForM = new IntegerRangeList(); 
        // request.ListForM.AddRange(40, 1999);
        // ApplyOperations(this.plc, "M", request.ListForM, ref result.ListForX, (p, a, c) => p.ReadBool(a, c));
    }

    public override PlcOperation<bool>     ReadBool(       string address               ) => WrapOperation(this.plc.ReadBool(address));
    public override PlcOperation<bool[]>   ReadBoolArray(  string address, ushort length) => WrapOperation(this.plc.ReadBool(address, length));
    public override PlcOperation<byte[]>   ReadByteArray(  string address, ushort length) => WrapOperation(this.plc.Read(address, length));
    public override PlcOperation<short>    ReadInt16(      string address               ) => WrapOperation(this.plc.ReadInt16(address));
    public override PlcOperation<short[]>  ReadInt16Array( string address, ushort length) => WrapOperation(this.plc.ReadInt16(address, length));
    public override PlcOperation<ushort>   ReadUInt16(     string address               ) => WrapOperation(this.plc.ReadUInt16(address));
    public override PlcOperation<ushort[]> ReadUInt16Array(string address, ushort length) => WrapOperation(this.plc.ReadUInt16(address, length));
    public override PlcOperation<int>      ReadInt32(      string address               ) => WrapOperation(this.plc.ReadInt32(address));
    public override PlcOperation<int[]>    ReadInt32Array( string address, ushort length) => WrapOperation(this.plc.ReadInt32(address, length));
    public override PlcOperation<uint>     ReadUInt32(     string address               ) => WrapOperation(this.plc.ReadUInt32(address));
    public override PlcOperation<uint[]>   ReadUInt32Array(string address, ushort length) => WrapOperation(this.plc.ReadUInt32(address, length));
    public override PlcOperation<float>    ReadFloat(      string address               ) => WrapOperation(this.plc.ReadFloat(address));
    public override PlcOperation<float[]>  ReadFloatArray( string address, ushort length) => WrapOperation(this.plc.ReadFloat(address, length));
    public override PlcOperation<long>     ReadInt64(      string address               ) => WrapOperation(this.plc.ReadInt64(address));
    public override PlcOperation<long[]>   ReadInt64Array( string address, ushort length) => WrapOperation(this.plc.ReadInt64(address, length));
    public override PlcOperation<ulong>    ReadUInt64(     string address               ) => WrapOperation(this.plc.ReadUInt64(address));
    public override PlcOperation<ulong[]>  ReadUInt64Array(string address, ushort length) => WrapOperation(this.plc.ReadUInt64(address, length));
    public override PlcOperation<double>   ReadDouble(     string address               ) => WrapOperation(this.plc.ReadDouble(address));
    public override PlcOperation<double[]> ReadDoubleArray(string address, ushort length) => WrapOperation(this.plc.ReadDouble(address, length));
    public override PlcOperation<string>   ReadString(     string address, ushort length) => WrapOperation(this.plc.ReadString(address, length));
    
    public override PlcOperation WriteBool       (string address, bool value)      => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteByteArray  (string address, byte[] value)    => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteInt16      (string address, short value)     => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteInt16Array (string address, short[] values)  => WrapOperation(this.plc.Write(address, values));
    public override PlcOperation WriteUInt16     (string address, ushort value)    => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteUInt16Array(string address, ushort[] values) => WrapOperation(this.plc.Write(address, values));
    public override PlcOperation WriteInt32      (string address, int value)       => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteInt32Array (string address, int[] values)    => WrapOperation(this.plc.Write(address, values));
    public override PlcOperation WriteUInt32     (string address, uint value)      => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteUInt32Array(string address, uint[] values)   => WrapOperation(this.plc.Write(address, values));
    public override PlcOperation WriteFloat      (string address, float value)     => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteFloatArray (string address, float[] values)  => WrapOperation(this.plc.Write(address, values));
    public override PlcOperation WriteInt64      (string address, long value)      => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteInt64Array (string address, long[] values)   => WrapOperation(this.plc.Write(address, values));
    public override PlcOperation WriteUInt64     (string address, ulong value)     => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteUInt64Array(string address, ulong[] values)  => WrapOperation(this.plc.Write(address, values));
    public override PlcOperation WriteDouble     (string address, double value)    => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteDoubleArray(string address, double[] values) => WrapOperation(this.plc.Write(address, values));
    public override PlcOperation WriteString     (string address, string value)    => WrapOperation(this.plc.Write(address, value));
    public override PlcOperation WriteString     (string address, string value, int length) => WrapOperation(this.plc.Write(address, value, length));
    
    private static PlcOperation WrapOperation(OperateResult result) {
        return result.IsSuccess ? PlcOperation.Success : new PlcOperation(false, result.ErrorCode, result.Message);
    }
    
    private static PlcOperation<T> WrapOperation<T>(OperateResult<T> result) {
        return result.IsSuccess ? new PlcOperation<T>(true, result.Content, 0, null) : new PlcOperation<T>(false, default!, result.ErrorCode, result.Message);
    }
}