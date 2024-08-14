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
/// The size of a piece of data. A bit is treated differently from a byte, since a PLC will typically pack 8 booleans into a single byte
/// </summary>
public enum DataSize {
    /// <summary>
    /// There are 8 bits packed into a single byte
    /// </summary>
    Bit,
    /// <summary>
    /// One byte
    /// </summary>
    Byte,
    /// <summary>
    /// Two bytes - Primarily for short/ushort
    /// </summary>
    Word,
    /// <summary>
    /// Four bytes - Primarily for int, uint and float
    /// </summary>
    DWord,
    /// <summary>
    /// Eight bytes. Primarily for long, ulong and double
    /// </summary>
    QWord
}