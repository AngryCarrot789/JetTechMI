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
    private readonly PlcBatchRequest batchesRequest;
    private readonly PlcBatchResults batchesData;

    public JetTechRegistry() {
        this.controlTypes = new Dictionary<Type, ControlTypeRegistration>();
        this.allControls = new List<IJtControlData>();
        this.batchesRequest = new PlcBatchRequest();
        this.batchesData = new PlcBatchResults();
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
                try {
                    DateTime startTime = DateTime.Now;
                    this.ProcessAndSubmitBatchRequests();
                    double millisTaken = (DateTime.Now - startTime).TotalMilliseconds;

                    await Dispatcher.UIThread.InvokeAsync(this.UpdateAllAsync);
                    // await Task.Delay(new TimeSpan(Math.Max(TimeSpan.FromMilliseconds(50).Ticks - timeTaken.Ticks, TimeSpan.FromMilliseconds(25).Ticks)));
                    await Task.Delay(50);
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
        for (int i = 0; i < this.allControls.Count; i++) {
            IJtControlData data = this.allControls[i];

            try {
                data.SubmitBatchData(this.batchesRequest);
            }
            catch (Exception e) {
                Debug.WriteLine($"Exception while querying batch data from control '{data.Control}': " + e);
            }
        }
        
        this.batchesRequest.Submit(JetTechContext.Instance, this.batchesData);
    }

    private async Task UpdateAllAsync() {
        // ReSharper disable once ForCanBeConvertedToForeach
        // This method must use a for-loop so that we don't run into concurrent list modification issues
        for (int i = 0; i < this.allControls.Count;) {
            i = await this.UpdateGroup(i);
            await Task.Delay(4);
        }

        this.batchesData.Clear();
    }

    private async Task<int> UpdateGroup(int index) {
        const int groupCount = 5;
        
        int i = index;
        for (; i < Math.Min(this.allControls.Count, index + groupCount); i++) {
            IJtControlData data = this.allControls[i];

            try {
                await data.UpdateAsync(this.batchesData);
            }
            catch (Exception e) {
                Debug.WriteLine($"Exception while updating control '{data.Control}': " + e);
            }   
        }

        return i;
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