using System;
using System.Diagnostics.CodeAnalysis;
using HslCommunication.Core.Types;
using HslCommunication.Devices.Melsec;
using JetTechMI.HMI;
using Microsoft.CodeAnalysis.Operations;

namespace JetTechMI.Hsl;

public class MelsecAddressInfo {
    public readonly MelsecMcDataType dataType;
    public readonly ushort startAddress;
    public readonly string UniformAddress;

    public MelsecAddressInfo(MelsecMcDataType dataType, ushort startAddress) {
        this.dataType = dataType;
        this.startAddress = startAddress;
        this.UniformAddress = dataType.AsciiCodeOrChar + startAddress.ToString();
    }

    public static LightOperationResult<ushort> GetActualStartAddress(MelsecMcDataType type, ushort startAddress, DataSize requestedSize) {
        if (requestedSize == DataSize.Bit) {
            LightOperationResult<ushort, ushort, ushort> result = MelsecFxSerial.FxCalculateBoolStartAddress(type, startAddress);
            if (!result.IsSuccess)
                return new LightOperationResult<ushort>(result.ErrorCode, result.Message);
            
            return LightOperationResult.CreateSuccessResult((ushort) (result.Content1 + result.Content3));
        }
        else {
            return MelsecFxSerial.FxCalculateWordStartAddress(type, startAddress);
        }
    }

    public static bool TryParse(DeviceAddress address, [NotNullWhen(true)] out MelsecAddressInfo? info) {
        LightOperationResult<MelsecMcDataType, ushort> result = MelsecFxSerial.FxAnalysisAddress(address.Address);
        if (!result.IsSuccess) {
            info = default;
            return false;
        }

        info = new MelsecAddressInfo(result.Content1, result.Content2);
        return true;
    }
}