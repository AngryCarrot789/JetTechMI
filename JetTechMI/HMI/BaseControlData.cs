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
using System.Threading.Tasks;
using Avalonia.Controls;

namespace JetTechMI.HMI;

public abstract class BaseControlData<T> : IJtControlData<T> where T : Control {
    Control IJtControlData.Control => this.Control;

    public T Control { get; }

    /// <summary>
    /// Helper property for quickly accessing the jet tech context
    /// </summary>
    public JetTechContext Context => JetTechContext.Instance;

    protected BaseControlData(T control) {
        this.Control = control ?? throw new ArgumentNullException(nameof(control), "Control cannot be null");
    }

    public abstract Task UpdateAsync(PlcBatchResults batches);
    
    public abstract void OnConnect();

    public abstract void OnDisconnect();
    
    public virtual void SubmitBatchData(PlcBatchRequest data) {
        
    }
}