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
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JetTechMI.HMI;

/// <summary>
/// A class which represents the state of a PLC communication operation
/// </summary>
public readonly struct PlcOperation {
    public bool IsSuccessful { get; }

    public int ErrorCode { get; }

    public string? ErrorMessagte { get; }

    public static PlcOperation Success => new PlcOperation(true, 0, null);
    
    public PlcOperation(bool isSuccessful, int errorCode, string? errorMessagte) {
        this.IsSuccessful = isSuccessful;
        this.ErrorCode = errorCode;
        this.ErrorMessagte = errorMessagte;
    }
}

/// <summary>
/// A class which represents the state of a PLC communication operation
/// </summary>
public readonly struct PlcOperation<TResult> {
    public bool IsSuccessful { get; }
    public int ErrorCode { get; }
    public string? ErrorMessagte { get; }

    public TResult Result { get; }
    
    public PlcOperation(bool isSuccessful, TResult result, int errorCode, string? errorMessagte) {
        this.IsSuccessful = isSuccessful;
        this.Result = result;
        this.ErrorCode = errorCode;
        this.ErrorMessagte = errorMessagte;
    }

    public bool TryGetResult([NotNullWhen(true)] out TResult? result) {
        if (this.IsSuccessful) {
            result = this.Result!;
            return true;
        }

        result = default;
        return false;
    }

    public PlcOperation<T> Select<T>(Func<TResult, T> func) {
        if (!this.IsSuccessful)
            return new PlcOperation<T>(false, default!, this.ErrorCode, this.ErrorMessagte);
        return new PlcOperation<T>(true, func(this.Result), 0, null);
    }

    public TResult? GetResultOr(TResult? def = default) => this.IsSuccessful ? this.Result : def;
}