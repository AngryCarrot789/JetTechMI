﻿// 
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
/// An interface that provides basic 
/// </summary>
public interface IWriteableAddressMemory {
    LightOperationResult WriteBool(string address, bool value);
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
