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

using HslCommunication.Core.Types;

namespace JetTechMI.HMI;

/// <summary>
/// An interface for interacting with a PLC device
/// </summary>
public interface ILogicController : IReadableAddressMemory, IWriteableAddressMemory {
    /// <summary>
    /// Gets whether or not the PLC is currently connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the ID for this PLC
    /// </summary>
    ushort Id { get; }

    /// <summary>
    /// Gets whether double-precision floating point numbers are supported. Read and write
    /// operations will default to float operations if unsupported
    /// </summary>
    bool IsDoubleSupported { get; }

    /// <summary>
    /// Gets whether 64-bit integers are supported. Read and write
    /// operations will default to int32 operations if unsupported
    /// </summary>
    bool IsInt64Supported { get; }
    
    /// <summary>
    /// Called every so often to ensure the PLC is still connected. If not, attempt reconnection
    /// </summary>
    /// <returns>
    /// True if already connected or the reconnection was successful.
    /// Unsuccessful if an exception occurred, e.g. timeout while trying to connect
    /// </returns>
    LightOperationResult CheckConnection();

    /// <summary>
    /// Factory method to create an instance of the PLC-specific request storage object
    /// </summary>
    /// <returns>A new request info object</returns>
    BatchRequestInfo CreateRequestData();

    /// <summary>
    /// Factory method to create an instance of the PLC-specific result storage object 
    /// </summary>
    /// <returns>A new result data object</returns>
    BatchResultData CreateResultData();
    
    /// <summary>
    /// Reads multiple addresses from this PLC device and places the resulting data in the result data object
    /// </summary>
    /// <param name="requests">The request storage object</param>
    /// <param name="results">The response/result storage object</param>
    void ReadBatchedData(BatchRequestInfo requests, BatchResultData results);
}