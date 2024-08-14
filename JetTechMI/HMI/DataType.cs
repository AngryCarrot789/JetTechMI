using System;

namespace JetTechMI.HMI;

public enum DataType {
    Bool,
    Byte,
    Short,
    Int,
    Long,
    Float,
    Double
}

public static class DataTypeExtension {
    public static DataSize GetDataSize(this DataType type) {
        switch (type) {
            case DataType.Bool: return DataSize.Bit;
            case DataType.Byte: return DataSize.Byte;
            case DataType.Short: return DataSize.Word;
            case DataType.Int: return DataSize.DWord;
            case DataType.Long: return DataSize.QWord;
            case DataType.Float: return DataSize.DWord;
            case DataType.Double: return DataSize.QWord;
            default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}