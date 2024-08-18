using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using JetTechMI.HMI.Attached;
using JetTechMI.Utils;

namespace JetTechMI.HMI.Controls.Buttons;

public class JtRepeatButton : RepeatButton {
    public static readonly StyledProperty<string?> VariableProperty = AvaloniaProperty.Register<JtRepeatButton, string?>("Variable");
    public static readonly StyledProperty<string?> EnablingVariableProperty = JtButton.EnablingVariableProperty.AddOwner<JtRepeatButton>();
    public static readonly StyledProperty<JtRepeatType> RepeatTypeProperty = AvaloniaProperty.Register<JtRepeatButton, JtRepeatType>("RepeatType");
    public static readonly StyledProperty<DataType> DataTypeProperty = AvaloniaProperty.Register<JtRepeatButton, DataType>("DataType");

    public string? Variable {
        get => this.GetValue(VariableProperty);
        set => this.SetValue(VariableProperty, value);
    }

    public string? EnablingVariable {
        get => this.GetValue(EnablingVariableProperty);
        set => this.SetValue(EnablingVariableProperty, value);
    }
    
    public JtRepeatType RepeatType {
        get => this.GetValue(RepeatTypeProperty);
        set => this.SetValue(RepeatTypeProperty, value);
    }
    
    public DataType DataType {
        get => this.GetValue(DataTypeProperty);
        set => this.SetValue(DataTypeProperty, value);
    }

    static JtRepeatButton() {
        JtControlManager.Instance.RegisterControlType(typeof(JtRepeatButton), (c) => new JtRepeatButtonControlData((JtRepeatButton) c));
        VariableProperty.Changed.AddClassHandler<JtRepeatButton, string?>((c, e) => JtControlManager.GetOrCreateControlData<JtRepeatButtonControlData>(c).Variable = DeviceAddress.Parse(e.NewValue.GetValueOrDefault()));
        EnablingVariableProperty.Changed.AddClassHandler<JtRepeatButton, string?>((c, e) => JtControlManager.GetOrCreateControlData<JtRepeatButtonControlData>(c).EnablingVariable = DeviceAddress.Parse(e.NewValue.GetValueOrDefault()));
        RepeatTypeProperty.Changed.AddClassHandler<JtRepeatButton, JtRepeatType>((c, e) => JtControlManager.GetOrCreateControlData<JtRepeatButtonControlData>(c).RepeatType = e.NewValue.GetValueOrDefault());
        DataTypeProperty.Changed.AddClassHandler<JtRepeatButton, DataType>((c, e) => JtControlManager.GetOrCreateControlData<JtRepeatButtonControlData>(c).DataType = e.NewValue.GetValueOrDefault());
    }
    
    protected override void OnClick() {
        base.OnClick();
        if (JtCommon.TryGetRegisteredControlData(this, out JtRepeatButtonControlData? data)) {
            if (this.RepeatType == JtRepeatType.Increment)
                data.Increment();
            else
                data.Decrement();
        }
    }
    
    private class JtRepeatButtonControlData : BaseControlData<JtRepeatButton> {
        public DeviceAddress? Variable { get; set; }
        public DeviceAddress? EnablingVariable { get; set; }
        
        public JtRepeatType RepeatType { get; set; }
        public DataType DataType { get; set; }

        private double lastDataValue;

        public JtRepeatButtonControlData(JtRepeatButton control) : base(control) {
        }

        protected override void OnConnectedCore() {
        }

        protected override void OnDisconnectedCore() {
        }

        public override void SubmitBatchData(BatchRequestList data) {
            base.SubmitBatchData(data);
            data.TryRequest(this.EnablingVariable, DataSize.Bit);
            data.TryRequest(this.Variable, this.DataType.GetDataSize());
        }

        public override async Task UpdateAsync(BatchResultList batches) {
            // A cool light show that shows how the updating happens in real time
            // if (this.oldBrush == null) {
            //     this.oldBrush = this.Control.Background;
            //     this.Control.Background = Brushes.Orange;
            // }
            // else {
            //     this.Control.Background = this.oldBrush;
            //     this.oldBrush = null;
            // }

            if (this.EnablingVariable == null)
                this.Control.IsEnabled = true;
            else if (this.EnablingVariable == null || this.Context.TryGetPLC(this.EnablingVariable.Device, out _))
                this.Control.IsEnabled = (this.TryReadBool(batches, this.EnablingVariable, out bool value) && value);
            else
                this.Control.IsEnabled = false;
            
            if (this.Variable != null && this.Context.TryGetPLC(this.Variable, out ILogicController? plc)) {
                switch (this.Control.DataType) {
                    case DataType.Bool:   this.lastDataValue = plc.ReadBool(this.Variable.Address).GetResultOr() ? 1.0 : 0.0; break;
                    case DataType.Byte:   this.lastDataValue = plc.ReadByte(this.Variable.Address).GetResultOr(); break;
                    case DataType.Short:  this.lastDataValue = plc.ReadInt16(this.Variable.Address).GetResultOr(); break;
                    case DataType.Int:    this.lastDataValue = plc.ReadInt32(this.Variable.Address).GetResultOr(); break;
                    case DataType.Long:   this.lastDataValue = plc.ReadInt64(this.Variable.Address).GetResultOr(); break;
                    case DataType.Float:  this.lastDataValue = plc.ReadFloat(this.Variable.Address).GetResultOr(); break;
                    case DataType.Double: this.lastDataValue = plc.ReadDouble(this.Variable.Address).GetResultOr(); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Increment() => this.AddValue(1);

        public void Decrement() => this.AddValue(-1);

        public void AddValue(int value) {
            if (this.Variable != null && this.Context.TryGetPLC(this.Variable, out ILogicController? plc)) {
                switch (this.Control.DataType) {
                    case DataType.Bool:   plc.WriteBool(this.Variable.Address, Maths.Equals(this.lastDataValue = ((this.lastDataValue + value) % 1), 1)); break;
                    case DataType.Byte:   plc.WriteByte(this.Variable.Address, (byte) (this.lastDataValue = (byte) (this.lastDataValue + value))); break;
                    case DataType.Short:  plc.WriteInt16(this.Variable.Address, (short) (this.lastDataValue = (short) (this.lastDataValue + value))); break;
                    case DataType.Int:    plc.WriteInt32(this.Variable.Address, (int) (this.lastDataValue = (int) (this.lastDataValue + value))); break;
                    case DataType.Long:   plc.WriteInt64(this.Variable.Address, (long) (this.lastDataValue = (long) (this.lastDataValue + value))); break;
                    case DataType.Float:  plc.WriteFloat(this.Variable.Address, (float) (this.lastDataValue = (float) (this.lastDataValue + value))); break;
                    case DataType.Double: plc.WriteDouble(this.Variable.Address, this.lastDataValue = (this.lastDataValue + value)); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}