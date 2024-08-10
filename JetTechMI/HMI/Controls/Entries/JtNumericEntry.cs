using System;
using System.Threading.Tasks;
using Avalonia;
using JetTechMI.HMI.Attached;

namespace JetTechMI.HMI.Controls.Entries;

public class JtNumericEntry : NumericEntry {
    public static readonly StyledProperty<string?> WriteVariableProperty = AvaloniaProperty.Register<JtNumericEntry, string?>("WriteVariable");
    public static readonly StyledProperty<string?> ReadVariableProperty = AvaloniaProperty.Register<JtNumericEntry, string?>("ReadVariable");
    public static readonly StyledProperty<NumericVariableType> NumericVariableTypeProperty = AvaloniaProperty.Register<JtNumericEntry, NumericVariableType>("NumericVariableType");

    public string? WriteVariable {
        get => this.GetValue(WriteVariableProperty);
        set => this.SetValue(WriteVariableProperty, value);
    }
    
    public string? ReadVariable {
        get => this.GetValue(ReadVariableProperty);
        set => this.SetValue(ReadVariableProperty, value);
    }
    
    public NumericVariableType NumericVariableType {
        get => this.GetValue(NumericVariableTypeProperty);
        set => this.SetValue(NumericVariableTypeProperty, value);
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
            switch (this.Control.NumericVariableType) {
                case NumericVariableType.Float:  val = this.Context.ReadFloat(this.ReadVariable, false); break;
                case NumericVariableType.Double: val = this.Context.ReadFloat(this.ReadVariable, true); break;
                case NumericVariableType.Byte:   val = this.Context.ReadInteger(this.ReadVariable, 1); break;
                case NumericVariableType.Word:   val = this.Context.ReadInteger(this.ReadVariable, 2); break;
                case NumericVariableType.DWord:  val = this.Context.ReadInteger(this.ReadVariable, 4); break;
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
                switch (this.Control.NumericVariableType) {
                    case NumericVariableType.Float:  plc.WriteFloat(this.WriteVariable.FullAddress, (float) value); break;
                    case NumericVariableType.Double: plc.WriteDouble(this.WriteVariable.FullAddress, value); break;
                    case NumericVariableType.Byte:   plc.WriteByte(this.WriteVariable.FullAddress, (byte) value); break;
                    case NumericVariableType.Word:   plc.WriteInt16(this.WriteVariable.FullAddress, (short) value); break;
                    case NumericVariableType.DWord:  plc.WriteInt32(this.WriteVariable.FullAddress, (int) value); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}