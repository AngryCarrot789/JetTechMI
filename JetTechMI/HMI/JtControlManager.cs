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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using JetTechMI.HMI.Attached;

namespace JetTechMI.HMI;

/// <summary>
/// A class which contains a registry of registered control types, and a list of active HMI control element
/// </summary>
public class JtControlManager {
    public static JtControlManager Instance { get; } = new JtControlManager();

    private readonly Dictionary<Type, ControlTypeRegistration> controlTypes;
    private readonly List<IJtControlData> activeControls;

    /// <summary>
    /// Gets an enumerable of the controls that are currently active in this application
    /// </summary>
    public IEnumerable<IJtControlData> ActiveControls => this.activeControls;
    
    public JtControlManager() {
        this.controlTypes = new Dictionary<Type, ControlTypeRegistration>();
        this.activeControls = new List<IJtControlData>();
    }
    
    public void RegisterControlType(Type controlType, Func<Control, IJtControlData> dataConstructor) {
        if (!typeof(Control).IsAssignableFrom(controlType))
            throw new ArgumentException("Gives control type is not assignable to type control");
        
        if (this.controlTypes.ContainsKey(controlType))
            throw new InvalidOperationException("Control already registered: " + controlType.Name);

        this.controlTypes[controlType] = new ControlTypeRegistration(controlType, dataConstructor);
    }
    
    /// <summary>
    /// Gets or creates the control data. This uses the registered control type to create an instance automatically
    /// </summary>
    /// <param name="control">The control</param>
    /// <param name="autoConnectToManager">
    /// If the control has data associated with it via the avalonia property
    /// but is not connected, then if this is true, add it to this manager
    /// </param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">No </exception>
    public IJtControlData GetOrCreateControlData(Control control, bool autoConnectToManager = false) {
        if (JtCommon.TryGetRegisteredControlData(control, out IJtControlData data)) {
            if (autoConnectToManager && !data.IsConnected)
                this.Register(control, data, false);
            return data;
        }

        ControlTypeRegistration? registration = null;
        for (Type? type = control.GetType(); !ReferenceEquals(type, null); type = type.BaseType) {
            if (this.controlTypes.TryGetValue(type, out registration)) {
                break;
            }
        }

        if (registration == null)
            throw new InvalidOperationException("No registered control information for " + control.GetType());
        
        this.Register(control, data = registration.CreateData(control));
        return data;
    }

    /// <summary>
    /// Adds the control to this control manager
    /// </summary>
    /// <param name="control">The control. This is only used when updateAvaloniaProperty is true so it may be null</param>
    /// <param name="data">The data being added</param>
    /// <param name="updateAvaloniaProperty">True to associate the data with the control via the avalonia property</param>
    /// <exception cref="InvalidOperationException">Already added</exception>
    public void Register(Control control, IJtControlData data, bool updateAvaloniaProperty = true) {
        if (data.IsConnected)
            throw new InvalidOperationException("Data is already connected to the manager");
        
        if (updateAvaloniaProperty)
            JtCommon.SetRegisteredControlData(control, data);
        
        this.activeControls.Add(data);
        data.OnConnectToManager();
    }
    
    /// <summary>
    /// Removes the control's data, if it has any
    /// </summary>
    public void Unregister(Control control) {
        if (JtCommon.TryGetRegisteredControlData(control, out IJtControlData? data)) {
            this.Unregister(control, data);
        }
    }
    
    public void Unregister(Control control, IJtControlData controlData, bool updateAvaloniaProperty = true) {
        if (!controlData.IsConnected)
            throw new InvalidOperationException("Data is not connected to the manager");
            
        controlData.OnDisconnectFromManager();
        this.activeControls.Remove(controlData);
        
        if (updateAvaloniaProperty)
            JtCommon.SetRegisteredControlData(control, null);
    }

    public void Run() {
        Task.Run(async () => {
            while (true) {
                // The time between full refreshes. Too slow becomes visually unresponsive. Too fast becomes visually glitchy
                TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(300);
                
                // The absolute minimum time we can wait if the refresh took too long. This is to prevent stalling the UI
                TimeSpan MinSleepForLongRefresh = TimeSpan.FromMilliseconds(25);
                
                try {
                    DateTime startTime = DateTime.Now;
                    
                    // Process all controls for batch requests and then submit
                    this.ProcessAndSubmitBatchRequests();
                    
                    // Update all control states
                    await Dispatcher.UIThread.InvokeAsync(this.UpdateAllAsync);

                    TimeSpan fullRefreshTime = (DateTime.Now - startTime);
                    await Task.Delay(new TimeSpan(Math.Max(RefreshInterval.Ticks - fullRefreshTime.Ticks, MinSleepForLongRefresh.Ticks)));
                }
                catch (Exception e) {
                    Debug.WriteLine("Error during tick of registry: " + e);
                }
            }
        });
    }
    
    /// <summary>
    /// A convenience method for the non-static <see cref="GetOrCreateControlData"/> method
    /// </summary>
    /// <param name="control"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetOrCreateControlData<T>(Control control, bool autoConnectToManager = false) where T : IJtControlData {
        return (T) Instance.GetOrCreateControlData(control, autoConnectToManager);
    }

    private void ProcessAndSubmitBatchRequests() {
        BatchRequestList requestList = JetTechContext.Instance.BatchRequestList;
        requestList.Prepare();
        
        for (int i = 0; i < this.activeControls.Count; i++) {
            IJtControlData data = this.activeControls[i];

            try {
                data.SubmitBatchData(requestList);
            }
            catch (Exception e) {
                Debug.WriteLine($"Exception while querying batch data from control '{data.Control}': " + e);
            }
        }
        
        JetTechContext.Instance.BatchResultList.Clear();
        requestList.Submit(JetTechContext.Instance, JetTechContext.Instance.BatchResultList);
    }

    private async Task UpdateAllAsync() {
        BatchResultList batches = JetTechContext.Instance.BatchResultList;
        const int ControlsPerGroup = 4;
        for (int i = 0; i < this.activeControls.Count;) {
            for (; i < Math.Min(this.activeControls.Count, i + ControlsPerGroup); i++) {
                IJtControlData data = this.activeControls[i];

                try {
                    await data.UpdateAsync(batches);
                }
                catch (Exception e) {
                    Debug.WriteLine($"Exception while updating control '{data.Control}': " + e);
                }
            }
            
            await Task.Delay(2);
        }

        batches.Clear();
    }

    private class ControlTypeRegistration {
        private readonly Type controlType;
        private readonly Func<Control, IJtControlData> dataConstructor;

        public ControlTypeRegistration(Type controlType, Func<Control, IJtControlData> dataConstructor) {
            this.controlType = controlType;
            this.dataConstructor = dataConstructor;
        }

        public IJtControlData CreateData(Control control) {
            return this.dataConstructor(control);
        }
    }
}