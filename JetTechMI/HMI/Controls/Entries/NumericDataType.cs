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

namespace JetTechMI.HMI.Controls.Entries;

public enum NumericDataType {
    /// <summary>
    /// A 32-bit floating point number (single)
    /// </summary>
    Float, 
    /// <summary>
    /// A 64-bit floating point number (double precision)
    /// </summary>
    Double, 
    /// <summary>
    /// An unsigned 8-bit value
    /// </summary>
    Byte, 
    /// <summary>
    /// A signed 8-bit value
    /// </summary>
    SignedByte, 
    /// <summary>
    /// An unsigned 16-bit value (short/ushort)
    /// </summary>
    Word, 
    /// <summary>
    /// A signed 16-bit value (short/ushort)
    /// </summary>
    SignedWord, 
    /// <summary>
    /// An unsigned 32-bit value (int/uint)
    /// </summary>
    DWord,
    /// <summary>
    /// A signed 32-bit value (int/uint)
    /// </summary>
    SignedDWord
}