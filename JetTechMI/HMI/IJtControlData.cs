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

using System.Threading.Tasks;
using Avalonia.Controls;

namespace JetTechMI.HMI;

/// <summary>
/// Defines an interface for the control data, that is, custom data and functionality attached
/// to a single instance of a UI control registered with the <see cref="JetTechRegistry"/>
/// </summary>
public interface IJtControlData {
    /// <summary>
    /// Gets the control associated with this data
    /// </summary>
    Control Control { get; }

    /// <summary>
    /// Scheduled update on the application's main thread
    /// </summary>
    /// <param name="batches"></param>
    /// <returns>An awaitable task for the completion of the tick</returns>
    Task UpdateAsync(BatchResultList batches);

    /// <summary>
    /// Called when the registry connects this data to a control fully
    /// </summary>
    void OnConnect();
    
    /// <summary>
    /// Called just before the registry completely de-associates this data with the control
    /// </summary>
    void OnDisconnect();

    /// <summary>
    /// This method is called on a background task. Add requests for data from the PLC
    /// </summary>
    void SubmitBatchData(BatchRequestList data);
}

/// <summary>
/// A generic version of <see cref="IJtControlData"/>, purely for convenience of
/// not having to cast <see cref="IJtControlData.Control"/> to the derived type
/// </summary>
/// <typeparam name="TControl">The type of control this interface is attached to</typeparam>
public interface IJtControlData<out TControl> : IJtControlData where TControl : Control {
    /// <summary>
    /// Gets the control associated with this data
    /// </summary>
    new TControl Control { get; }
}