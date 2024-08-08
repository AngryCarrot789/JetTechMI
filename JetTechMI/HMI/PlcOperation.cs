using System;
using System.Diagnostics.CodeAnalysis;

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
}