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
using Avalonia;
using JetTechMI.HMI.Attached;
using JetTechMI.Utils;

namespace JetTechMI.HMI.Controls.Entries;

public class JtNumericEntry : NumericEntry {
    public static readonly StyledProperty<string?> WriteVariableProperty = AvaloniaProperty.Register<JtNumericEntry, string?>("WriteVariable");
    public static readonly StyledProperty<string?> ReadVariableProperty = AvaloniaProperty.Register<JtNumericEntry, string?>("ReadVariable");
    public static readonly StyledProperty<NumericDataType> DataTypeProperty = AvaloniaProperty.Register<JtNumericEntry, NumericDataType>("DataType");

    public string? WriteVariable {
        get => this.GetValue(WriteVariableProperty);
        set => this.SetValue(WriteVariableProperty, value);
    }
    
    public string? ReadVariable {
        get => this.GetValue(ReadVariableProperty);
        set => this.SetValue(ReadVariableProperty, value);
    }
    
    public NumericDataType DateType {
        get => this.GetValue(DataTypeProperty);
        set => this.SetValue(DataTypeProperty, value);
    }

    static JtNumericEntry() {
        JetTechRegistry.Instance.RegisterControlType(typeof(JtNumericEntry), (c) => new JtNumericEntryControlData((JtNumericEntry) c));
        WriteVariableProperty.Changed.AddClassHandler<JtNumericEntry, string?>((c, e) => {
            JetTechRegistry.GetOrCreateControlData<JtNumericEntryControlData>(c).WriteVariable = DeviceAddress.Parse(e.NewValue.GetValueOrDefault());
        });

        ReadVariableProperty.Changed.AddClassHandler<JtNumericEntry, string?>((c, e) => {
            JetTechRegistry.GetOrCreateControlData<JtNumericEntryControlData>(c).ReadVariable = DeviceAddress.Parse(e.NewValue.GetValueOrDefault());
        });
    }

    protected override void OnChangeValueFromNumberPad(double newValue) {
        base.OnChangeValueFromNumberPad(newValue);
        if (JtCommon.GetRegisteredControlData(this) is JtNumericEntryControlData data) {
            data.SendNewValue(newValue);
        }
    }

    private class JtNumericEntryControlData : BaseControlData<JtNumericEntry> {
        public DeviceAddress WriteVariable { get; set; }
        public DeviceAddress ReadVariable { get; set; }
        public DeviceAddress EnablingVariable { get; set; }

        public JtNumericEntryControlData(JtNumericEntry control) : base(control) {
        }

        public override void SubmitBatchData(PlcBatchRequest data) {
            base.SubmitBatchData(data);
            data.TryRequest(this.EnablingVariable);
            data.TryRequest(this.ReadVariable);
        }

        public override async Task UpdateAsync(PlcBatchResults batches) {
            double? val;
            switch (this.Control.DateType) {
                case NumericDataType.Float:  val = this.Context.ReadFloat(this.ReadVariable, false); break;
                case NumericDataType.Double: val = this.Context.ReadFloat(this.ReadVariable, true); break;
                case NumericDataType.Byte:   val = this.Context.ReadInteger(this.ReadVariable, 1); break;
                case NumericDataType.Word:   val = this.Context.ReadInteger(this.ReadVariable, 2); break;
                case NumericDataType.DWord:  val = this.Context.ReadInteger(this.ReadVariable, 4); break;
                default: throw new ArgumentOutOfRangeException();
            }

            if (val.HasValue) {
                this.Control.Value = val.Value;
            }
        }

        public override void OnConnect() {
        }

        public override void OnDisconnect() {
        }

        public void SendNewValue(double value) {
            if (this.WriteVariable.IsValid && this.Context.TryGetPLC(this.WriteVariable, out IPlcApi? plc)) {
                switch (this.Control.DateType) {
                    case NumericDataType.Float:  plc.WriteFloat(this.WriteVariable.FullAddress, (float) value); break;
                    case NumericDataType.Double: plc.WriteDouble(this.WriteVariable.FullAddress, value); break;
                    case NumericDataType.Byte:   plc.WriteByte(this.WriteVariable.FullAddress, (byte) value); break;
                    case NumericDataType.Word:   plc.WriteInt16(this.WriteVariable.FullAddress, (short) value); break;
                    case NumericDataType.DWord:  plc.WriteInt32(this.WriteVariable.FullAddress, (int) value); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}