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
/// A class which contains a registry of every type of HMI element and how to handle interactions of data
/// </summary>
public class JetTechRegistry {
    public static JetTechRegistry Instance { get; } = new JetTechRegistry();

    private readonly Dictionary<Type, ControlTypeRegistration> controlTypes;
    private readonly List<IJtControlData> allControls;

    public JetTechRegistry() {
        this.controlTypes = new Dictionary<Type, ControlTypeRegistration>();
        this.allControls = new List<IJtControlData>();
    }
    
    public void RegisterControlType(Type controlType, Func<Control, IJtControlData> dataConstructor) {
        if (!typeof(Control).IsAssignableFrom(controlType))
            throw new ArgumentException("Gives control type is not assignable to type control");
        
        if (this.controlTypes.ContainsKey(controlType))
            throw new InvalidOperationException("Control already registered: " + controlType.Name);

        this.controlTypes[controlType] = new ControlTypeRegistration(controlType, dataConstructor);
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
                    
                    this.ProcessAndSubmitBatchRequests();
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
    public static T GetOrCreateControlData<T>(Control control) where T : IJtControlData {
        return (T) Instance.GetOrCreateControlData(control);
    }

    public IJtControlData GetOrCreateControlData(Control control) {
        if (JtCommon.TryGetRegisteredControlData(control, out IJtControlData data))
            return data;

        ControlTypeRegistration? registration = null;
        for (Type? type = control.GetType(); !ReferenceEquals(type, null); type = type.BaseType) {
            if (this.controlTypes.TryGetValue(type, out registration)) {
                break;
            }
        }

        if (registration == null)
            throw new InvalidOperationException("No registered control information for " + control.GetType());
        
        JtCommon.SetRegisteredControlData(control, data = registration.CreateData(control));
        this.allControls.Add(data);
        data.OnConnect();
        return data;
    }
    
    public void Unregister(Control control) {
        if (JtCommon.TryGetRegisteredControlData(control, out IJtControlData data)) {
            data.OnDisconnect();
            this.allControls.Remove(data);
            JtCommon.SetRegisteredControlData(control, null);
        }
    }

    private void ProcessAndSubmitBatchRequests() {
        BatchRequestList requestList = JetTechContext.Instance.BatchRequestList;
        requestList.Prepare();
        
        for (int i = 0; i < this.allControls.Count; i++) {
            IJtControlData data = this.allControls[i];

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
        for (int i = 0; i < this.allControls.Count;) {
            for (; i < Math.Min(this.allControls.Count, i + ControlsPerGroup); i++) {
                IJtControlData data = this.allControls[i];

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