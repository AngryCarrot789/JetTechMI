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
using System.Collections.Generic;
using System.Linq;
using HslCommunication.Core.Address;
using HslCommunication.Core.Types;
using HslCommunication.Devices.Melsec;
using JetTechMI.HMI;
using JetTechMI.Utils;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualBasic.CompilerServices;

namespace JetTechMI.Hsl;

public class HslMelsecPlc : BaseLogicController {
    public override bool IsConnected => this.plc.IsOpen();
    public override bool IsDoubleSupported => true;
    public override bool IsInt64Supported => true;

    private readonly MelsecFxSerial plc;

    public HslMelsecPlc(MelsecFxSerial plc, ushort id) : base(id) {
        this.plc = plc;
    }

    public override LightOperationResult CheckConnection() {
        // THIS IS ASS CHEEKS!
        if (!this.IsConnected) {
            try {
                this.plc.Open();
            }
            catch (Exception e) {
                return new LightOperationResult(e.Message);
            }
        }
        
        return LightOperationResult.CreateSuccessResult();
    }

    public override BatchRequestInfo CreateRequestData() => new MelsecBatchRequestInfo(this.Id);
    public override BatchResultData CreateResultData() => new MelsecBatchResultData(this.Id);

    private class MelsecBatchRequestInfo : BatchRequestInfo {
        public Dictionary<MelsecMcDataType, IntegerRangeList>?[] AllRequests;
        
        public MelsecBatchRequestInfo(int device) : base(device) {
            this.AllRequests = new Dictionary<MelsecMcDataType, IntegerRangeList>?[5];
        }

        public override void Clear() {
            
        }

        public override void Request(string address, DataSize dataSize) {
            LightOperationResult result = McAddressData.ExtractInfo(address, out MelsecMcDataType type, out int addressStart);
            if (!result.IsSuccess)
                return;
            
            Dictionary<MelsecMcDataType, IntegerRangeList> dictionary = this.AllRequests[(int) dataSize] ??= new Dictionary<MelsecMcDataType, IntegerRangeList>();
            if (!dictionary.TryGetValue(type, out IntegerRangeList? list))
                dictionary[type] = list = new IntegerRangeList();

            list.Add(addressStart);
        }
    }
    
    private class MelsecBatchResultData : BatchResultData {
        public Dictionary<string, bool>? Bits;
        public Dictionary<string, byte>? Bytes;
        public Dictionary<string, short>? Words;
        public Dictionary<string, int>? DWords;
        public Dictionary<string, long>? QWords;
        
        public MelsecBatchResultData(ushort id) : base(id) {
        }

        public void Paste<T>(DataSize size, T[] values) {
            
        }

        public override void Clear() {
            this.Bits?.Clear();
            this.Bytes?.Clear();
            this.Words?.Clear();
            this.DWords?.Clear();
            this.QWords?.Clear();
        }

        private static bool TryParseAddress(DeviceAddress address, out ushort index, DataSize dataSize) {
            if (!DeviceAddress.TryGetPlcData(address, out MelsecAddressInfo? addressInfo)) {
                if (!MelsecAddressInfo.TryParse(address, out addressInfo)) {
                    index = default;
                    return false;
                }

                DeviceAddress.SetPlcData(address, addressInfo);
            }

            LightOperationResult<ushort> info = MelsecAddressInfo.GetActualStartAddress(addressInfo.dataType, addressInfo.startAddress, dataSize);
            if (!info.IsSuccess) {
                index = 0;
                return false;
            }

            index = info.Content;
            return true;
        }
        
        private static LightOperationResult<T> Read<T>(DeviceAddress address, Dictionary<string, T>? dictionary, DataSize dataSize) {
            if (dictionary == null) {
                return new LightOperationResult<T>();
            }
            
            if (!DeviceAddress.TryGetPlcData(address, out MelsecAddressInfo? addressInfo)) {
                if (!MelsecAddressInfo.TryParse(address, out addressInfo))
                    return new LightOperationResult<T>("Invalid address");
                DeviceAddress.SetPlcData(address, addressInfo);
            }
            
            if (dictionary.TryGetValue(addressInfo.UniformAddress, out T? state)) {
                return LightOperationResult.CreateSuccessResult(state);
            }

            return new LightOperationResult<T>();
        }
        
        public override LightOperationResult<bool> ReadBool(DeviceAddress address) => Read(address, this.Bits, DataSize.Bit);

        public override LightOperationResult<byte> ReadByte(DeviceAddress address) => Read(address, this.Bytes, DataSize.Byte);

        public override LightOperationResult<short> ReadInt16(DeviceAddress address) => Read(address, this.Words, DataSize.Word);

        public override LightOperationResult<int> ReadInt32(DeviceAddress address) => Read(address, this.DWords, DataSize.DWord);

        public override LightOperationResult<long> ReadInt64(DeviceAddress address) => Read(address, this.QWords, DataSize.QWord);

        public override LightOperationResult<float> ReadFloat(DeviceAddress address) => Read(address, this.DWords, DataSize.DWord).Select(Int32ToFloat);

        public override LightOperationResult<double> ReadDouble(DeviceAddress address) => Read(address, this.QWords, DataSize.QWord).Select(Int64ToDouble);

        private static unsafe float Int32ToFloat(int value) => *(float*) &value;
        private static unsafe double Int64ToDouble(long value) => *(double*) &value;

        private static void DoAppend<T>(MelsecMcDataType dataType, ushort startAddress, OperateResult<T[]> responseData, ref Dictionary<string, T>? dictionary) {
            if (!responseData.IsSuccess)
                return;

            if (dictionary == null)
                dictionary = new Dictionary<string, T>();

            // There could be an issue here... We need to create a list of address sections rather than assume we
            // can just increment without problem, especially with counters where the memory location changes after counter 199

            foreach (T value in responseData.Content) {
                dictionary[dataType.AsciiCodeOrChar + startAddress++] = value;
            }
        }
        
        public void Append(DataSize dataSize, MelsecMcDataType type, ushort address, OperateResult<bool[]> data) => DoAppend(type, address, data, ref this.Bits);
        public void Append(DataSize dataSize, MelsecMcDataType type, ushort address, OperateResult<byte[]> data) => DoAppend(type, address, data, ref this.Bytes);
        public void Append(DataSize dataSize, MelsecMcDataType type, ushort address, OperateResult<short[]> data) => DoAppend(type, address, data, ref this.Words);
        public void Append(DataSize dataSize, MelsecMcDataType type, ushort address, OperateResult<int[]> data) => DoAppend(type, address, data, ref this.DWords);
        public void Append(DataSize dataSize, MelsecMcDataType type, ushort address, OperateResult<long[]> data) => DoAppend(type, address, data, ref this.QWords);
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
            if (result.IsSuccess) {
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
    
    private double[] avgs;
    private int nextAvgIndx;
    
    public override void ReadBatchedData(BatchRequestInfo requests, BatchResultData results) {
        MelsecBatchRequestInfo requestInfo = (MelsecBatchRequestInfo) requests;
        MelsecBatchResultData resultData = (MelsecBatchResultData) results;

        // DateTime start = DateTime.Now;
        for (int i = 0; i < requestInfo.AllRequests.Length; i++) {
            Dictionary<MelsecMcDataType, IntegerRangeList>? dictionary = requestInfo.AllRequests[i];
            if (dictionary == null)
                continue;
            
            DataSize dataSize = (DataSize) i;
            foreach (KeyValuePair<MelsecMcDataType,IntegerRangeList> pair in dictionary) {
                foreach (IntRange addressRange in pair.Value.Items) {
                    int count = addressRange.B - addressRange.A + 1;
                    if (count < 0 || count > ushort.MaxValue)
                        throw new InvalidOperationException("Out of range array length");

                    string actualAddress = pair.Key.AsciiCodeOrChar + addressRange.A;

                    switch (dataSize) {
                        case DataSize.Bit:
                            resultData.Append(dataSize, pair.Key, (ushort) addressRange.A, this.plc.ReadBool(actualAddress, (ushort) count));
                            break;
                        case DataSize.Byte:
                            resultData.Append(dataSize, pair.Key, (ushort) addressRange.A, this.plc.Read(actualAddress, (ushort) count));
                            break;
                        case DataSize.Word:
                            resultData.Append(dataSize, pair.Key, (ushort) addressRange.A, this.plc.ReadInt16(actualAddress, (ushort) count));
                            break;
                        case DataSize.DWord:
                            resultData.Append(dataSize, pair.Key, (ushort) addressRange.A, this.plc.ReadInt32(actualAddress, (ushort) count));
                            break;
                        case DataSize.QWord:
                            resultData.Append(dataSize, pair.Key, (ushort) addressRange.A, this.plc.ReadInt64(actualAddress, (ushort) count));
                            break;
                    }
                }   
            }
        }

        // Test to see what was causing performance issues
        // double millis = (DateTime.Now - start).TotalMilliseconds;
        // double[] array = this.avgs ??= new double[10];
        // if (this.nextAvgIndx == 10)
        //     this.nextAvgIndx = 0;
        // array[this.nextAvgIndx++] = millis;
        // double totalTime = array.Average();
    }

    public override LightOperationResult<bool>     ReadBool(       string address               ) => WrapOperation(this.plc.ReadBool(address));
    public override LightOperationResult<bool[]>   ReadBoolArray(  string address, ushort length) => WrapOperation(this.plc.ReadBool(address, length));
    public override LightOperationResult<byte[]>   ReadByteArray(  string address, ushort length) => WrapOperation(this.plc.Read(address, length));
    public override LightOperationResult<short>    ReadInt16(      string address               ) => WrapOperation(this.plc.ReadInt16(address));
    public override LightOperationResult<short[]>  ReadInt16Array( string address, ushort length) => WrapOperation(this.plc.ReadInt16(address, length));
    public override LightOperationResult<int>      ReadInt32(      string address               ) => WrapOperation(this.plc.ReadInt32(address));
    public override LightOperationResult<int[]>    ReadInt32Array( string address, ushort length) => WrapOperation(this.plc.ReadInt32(address, length));
    public override LightOperationResult<float>    ReadFloat(      string address               ) => WrapOperation(this.plc.ReadFloat(address));
    public override LightOperationResult<float[]>  ReadFloatArray( string address, ushort length) => WrapOperation(this.plc.ReadFloat(address, length));
    public override LightOperationResult<long>     ReadInt64(      string address               ) => WrapOperation(this.plc.ReadInt64(address));
    public override LightOperationResult<long[]>   ReadInt64Array( string address, ushort length) => WrapOperation(this.plc.ReadInt64(address, length));
    public override LightOperationResult<double>   ReadDouble(     string address               ) => WrapOperation(this.plc.ReadDouble(address));
    public override LightOperationResult<double[]> ReadDoubleArray(string address, ushort length) => WrapOperation(this.plc.ReadDouble(address, length));
    public override LightOperationResult<string>   ReadString(     string address, ushort length) => WrapOperation(this.plc.ReadString(address, length));
    
    public override LightOperationResult WriteBool       (string address, bool value)      => WrapOperation(this.plc.Write(address, value));
    public override LightOperationResult WriteBoolArray(string address, bool[] values) => WrapOperation(this.plc.Write(address, values));
    public override LightOperationResult WriteByteArray  (string address, byte[] value)    => WrapOperation(this.plc.Write(address, value));
    public override LightOperationResult WriteInt16      (string address, short value)     => WrapOperation(this.plc.Write(address, value));
    public override LightOperationResult WriteInt16Array (string address, short[] values)  => WrapOperation(this.plc.Write(address, values));
    public override LightOperationResult WriteInt32      (string address, int value)       => WrapOperation(this.plc.Write(address, value));
    public override LightOperationResult WriteInt32Array (string address, int[] values)    => WrapOperation(this.plc.Write(address, values));
    public override LightOperationResult WriteFloat      (string address, float value)     => WrapOperation(this.plc.Write(address, value));
    public override LightOperationResult WriteFloatArray (string address, float[] values)  => WrapOperation(this.plc.Write(address, values));
    public override LightOperationResult WriteInt64      (string address, long value)      => WrapOperation(this.plc.Write(address, value));
    public override LightOperationResult WriteInt64Array (string address, long[] values)   => WrapOperation(this.plc.Write(address, values));
    public override LightOperationResult WriteDouble     (string address, double value)    => WrapOperation(this.plc.Write(address, value));
    public override LightOperationResult WriteDoubleArray(string address, double[] values) => WrapOperation(this.plc.Write(address, values));
    public override LightOperationResult WriteString     (string address, string value)    => WrapOperation(this.plc.Write(address, value));
    public override LightOperationResult WriteString     (string address, string value, int length) => WrapOperation(this.plc.Write(address, value, length));
    
    private static LightOperationResult WrapOperation(OperateResult result) {
        return result.IsSuccess ? LightOperationResult.CreateSuccessResult() : new LightOperationResult(result.ErrorCode, result.Message);
    }
    
    private static LightOperationResult<T> WrapOperation<T>(OperateResult<T> result) {
        return result.IsSuccess ? LightOperationResult.CreateSuccessResult(result.Content) : new LightOperationResult<T>(result.ErrorCode, result.Message);
    }
}