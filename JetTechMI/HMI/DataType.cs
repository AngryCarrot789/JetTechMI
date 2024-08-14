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