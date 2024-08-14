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
    public static readonly StyledProperty<DataType> DataTypeProperty = AvaloniaProperty.Register<JtNumericEntry, DataType>("DataType");

    public string? WriteVariable {
        get => this.GetValue(WriteVariableProperty);
        set => this.SetValue(WriteVariableProperty, value);
    }
    
    public string? ReadVariable {
        get => this.GetValue(ReadVariableProperty);
        set => this.SetValue(ReadVariableProperty, value);
    }
    
    public DataType DataType {
        get => this.GetValue(DataTypeProperty);
        set => this.SetValue(DataTypeProperty, value);
    }

    static JtNumericEntry() {
        JtControlManager.Instance.RegisterControlType(typeof(JtNumericEntry), (c) => new JtNumericEntryControlData((JtNumericEntry) c));
        WriteVariableProperty.Changed.AddClassHandler<JtNumericEntry, string?>((c, e) => {
            JtControlManager.GetOrCreateControlData<JtNumericEntryControlData>(c).WriteVariable = DeviceAddress.Parse(e.NewValue.GetValueOrDefault());
        });

        ReadVariableProperty.Changed.AddClassHandler<JtNumericEntry, string?>((c, e) => {
            JtControlManager.GetOrCreateControlData<JtNumericEntryControlData>(c).ReadVariable = DeviceAddress.Parse(e.NewValue.GetValueOrDefault());
        });

        DataTypeProperty.Changed.AddClassHandler<JtNumericEntry, DataType>((c, e) => {
            JtControlManager.GetOrCreateControlData<JtNumericEntryControlData>(c).DataType = e.NewValue.GetValueOrDefault();
        });
    }

    protected override void OnChangeValueFromNumberPad(double newValue) {
        base.OnChangeValueFromNumberPad(newValue);
        if (JtCommon.GetRegisteredControlData(this) is JtNumericEntryControlData data) {
            data.SendNewValue(newValue);
        }
    }

    private class JtNumericEntryControlData : BaseControlData<JtNumericEntry> {
        public DeviceAddress? WriteVariable { get; set; }
        public DeviceAddress? ReadVariable { get; set; }
        public DeviceAddress? EnablingVariable { get; set; }
        public DataType DataType { get; set; }

        public JtNumericEntryControlData(JtNumericEntry control) : base(control) {
        }

        public override void SubmitBatchData(BatchRequestList data) {
            base.SubmitBatchData(data);
            data.TryRequest(this.EnablingVariable, DataSize.Bit);
            data.TryRequest(this.ReadVariable, this.DataType.GetDataSize());
        }

        public override async Task UpdateAsync(BatchResultList batches) {
            if (this.ReadVariable == null) {
                return;
            }
            
            double val;
            switch (this.Control.DataType) {
                case DataType.Bool: {val = this.TryReadBool(batches, this.ReadVariable, out bool value) && value ? 1.0 : 0.0; break; }
                case DataType.Byte: {val = this.TryReadByte(batches, this.ReadVariable, out byte value) ? value : 0.0; break; }
                case DataType.Short: {val = this.TryReadInt16(batches, this.ReadVariable, out short value) ? value : 0.0; break; }
                case DataType.Int: {val = this.TryReadInt32(batches, this.ReadVariable, out int value) ? value : 0.0; break; }
                case DataType.Long: {val = this.TryReadInt64(batches, this.ReadVariable, out long value) ? value : 0.0; break; }
                case DataType.Float: {val = this.TryReadFloat(batches, this.ReadVariable, out float value) ? value : 0.0; break; }
                case DataType.Double: {val = this.TryReadDouble(batches, this.ReadVariable, out double value) ? value : 0.0; break; }
                default: throw new ArgumentOutOfRangeException();
            }
            
            this.Control.Value = val;
        }

        public override void OnConnect() {
        }

        public override void OnDisconnect() {
        }

        public void SendNewValue(double value) {
            if (this.WriteVariable != null && this.Context.TryGetPLC(this.WriteVariable, out ILogicController? plc)) {
                switch (this.Control.DataType) {
                    case DataType.Bool:   plc.WriteBool(this.WriteVariable.Address, Maths.Equals(value, 1.0)); break;
                    case DataType.Byte:   plc.WriteByte(this.WriteVariable.Address, (byte) value); break;
                    case DataType.Short:  plc.WriteInt16(this.WriteVariable.Address, (short) value); break;
                    case DataType.Int:    plc.WriteInt32(this.WriteVariable.Address, (int) value); break;
                    case DataType.Long:   plc.WriteInt64(this.WriteVariable.Address, (long) value); break;
                    case DataType.Float:  plc.WriteFloat(this.WriteVariable.Address, (float) value); break;
                    case DataType.Double: plc.WriteDouble(this.WriteVariable.Address, value); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}