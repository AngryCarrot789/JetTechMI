using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using JetTechMI.HMI.Attached;

namespace JetTechMI.HMI.Controls.Buttons;

public class JtButton : ContentControl {
    public static readonly StyledProperty<string?> WriteVariableProperty = AvaloniaProperty.Register<JtButton, string?>("WriteVariable");
    public static readonly StyledProperty<string?> ReadVariableProperty = AvaloniaProperty.Register<JtButton, string?>("ReadVariable");
    public static readonly StyledProperty<string?> EnablingVariableProperty = AvaloniaProperty.Register<JtButton, string?>("EnablingVariable");
    public static readonly StyledProperty<JtButtonMode> ButtonModeProperty = AvaloniaProperty.Register<JtButton, JtButtonMode>("ButtonMode");
    public static readonly DirectProperty<JtButton, bool> IsVisuallyPressedProperty = AvaloniaProperty.RegisterDirect<JtButton, bool>("IsVisuallyPressed", o => o.IsVisuallyPressed);
    public static readonly DirectProperty<JtButton, bool> IsPhysicallyPressedProperty = AvaloniaProperty.RegisterDirect<JtButton, bool>("IsPhysicallyPressed", o => o.IsPhysicallyPressed);
    public static readonly StyledProperty<bool> IsPhysicalPressingEnabledProperty = AvaloniaProperty.Register<JtButton, bool>("IsPhysicalPressingEnabled", true);
    
    public string? WriteVariable {
        get => this.GetValue(WriteVariableProperty);
        set => this.SetValue(WriteVariableProperty, value);
    }

    public string? ReadVariable {
        get => this.GetValue(ReadVariableProperty);
        set => this.SetValue(ReadVariableProperty, value);
    }

    public string? EnablingVariable {
        get => this.GetValue(EnablingVariableProperty);
        set => this.SetValue(EnablingVariableProperty, value);
    }

    public JtButtonMode ButtonMode {
        get => this.GetValue(ButtonModeProperty);
        set => this.SetValue(ButtonModeProperty, value);
    }

    public bool IsVisuallyPressed {
        get => this.isVisuallyPressed;
        protected set => this.SetAndRaise(IsVisuallyPressedProperty, ref this.isVisuallyPressed, value);
    }

    public bool IsPhysicallyPressed {
        get => this.isPhysicallyPressed;
        protected set => this.SetAndRaise(IsPhysicallyPressedProperty, ref this.isPhysicallyPressed, value);
    }
    
    public bool IsPhysicalPressingEnabled {
        get => this.GetValue(IsPhysicalPressingEnabledProperty);
        set => this.SetValue(IsPhysicalPressingEnabledProperty, value);
    }

    private bool isVisuallyPressed, isPhysicallyPressed, isToggleActive;

    public JtButton() {
    }

    static JtButton() {
        JetTechRegistry.Instance.RegisterControlType(typeof(JtButton), (c) => new JtButtonControlData((JtButton) c));
        WriteVariableProperty.Changed.AddClassHandler<JtButton, string?>((c, e) => {
            JetTechRegistry.GetOrCreateControlData<JtButtonControlData>(c).WriteVariable = DeviceAddress.Parse(e.NewValue.GetValueOrDefault());
        });

        ReadVariableProperty.Changed.AddClassHandler<JtButton, string?>((c, e) => {
            JetTechRegistry.GetOrCreateControlData<JtButtonControlData>(c).ReadVariable = DeviceAddress.Parse(e.NewValue.GetValueOrDefault());
        });

        EnablingVariableProperty.Changed.AddClassHandler<JtButton, string?>((c, e) => {
            JetTechRegistry.GetOrCreateControlData<JtButtonControlData>(c).EnablingVariable = DeviceAddress.Parse(e.NewValue.GetValueOrDefault());
        });
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);

        if (this.IsPhysicalPressingEnabled && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            e.Handled = true;
            this.OnPressedCore();
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);

        if (this.IsPhysicalPressingEnabled && this.IsPhysicallyPressed && e.InitialPressMouseButton == MouseButton.Left) {
            // this.OnReleasedCore(this.GetVisualsAt(e.GetPosition(this)).Any(c => this == c || this.IsVisualAncestorOf(c)));
            this.OnReleasedCore();
        }
    }

    private void OnPressedCore() {
        this.IsPhysicallyPressed = true;
        
        JtButtonControlData? data = (JtButtonControlData?) JtCommon.GetRegisteredControlData(this);
        if (data != null) {
            switch (this.ButtonMode) {
                case JtButtonMode.Momentary:
                    if (data.CanUpdateVisualPressedForWrite)
                        this.IsVisuallyPressed = true;
                    data.SendActivate();
                    break;
                case JtButtonMode.Toggle:
                    if (this.isToggleActive) {
                        this.isToggleActive = false;
                        if (data.CanUpdateVisualPressedForWrite)
                            this.IsVisuallyPressed = false;
                        data.SendDeactivate();
                    }
                    else {
                        this.isToggleActive = true;
                        if (data.CanUpdateVisualPressedForWrite)
                            this.IsVisuallyPressed = true;
                        data.SendActivate();
                    }

                    break;
                case JtButtonMode.Set:
                    if (data.CanUpdateVisualPressedForWrite)
                        this.IsVisuallyPressed = true;
                    data.SendActivate();
                    break;
                case JtButtonMode.Reset:
                    if (data.CanUpdateVisualPressedForWrite)
                        this.IsVisuallyPressed = false;
                    data.SendDeactivate();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void OnReleasedCore() {
        this.IsPhysicallyPressed = false;
        
        JtButtonControlData? data = (JtButtonControlData?) JtCommon.GetRegisteredControlData(this); 
        if (data != null) {
            if (this.ButtonMode == JtButtonMode.Momentary) {
                if (data.CanUpdateVisualPressedForWrite)
                    this.IsVisuallyPressed = false;
                
                data.SendDeactivate();
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e) {
        base.OnPointerCaptureLost(e);
        if (this.IsPhysicallyPressed) {
            this.OnReleasedCore();
            this.IsPhysicallyPressed = false;
        }
    }

    /// <inheritdoc/>
    protected override void OnLostFocus(RoutedEventArgs e) {
        base.OnLostFocus(e);
        if (this.IsPhysicallyPressed) {
            this.OnReleasedCore();
            this.IsPhysicallyPressed = false;
        }
    }

    private class JtButtonControlData : BaseControlData<JtButton> {
        public DeviceAddress WriteVariable { get; set; }
        public DeviceAddress ReadVariable { get; set; }
        public DeviceAddress EnablingVariable { get; set; }

        private IBrush? oldBrush;

        // public bool CanUpdateVisualPressedForWrite => string.IsNullOrEmpty(this.ReadVariable) || this.ReadVariable.Equals(this.WriteVariable);
        // public bool CanUpdateVisualPressedForWrite => true;
        public bool CanUpdateVisualPressedForWrite => !this.ReadVariable.IsValid;

        public JtButtonControlData(JtButton control) : base(control) {
        }

        public override void SubmitBatchData(PlcBatchRequest data) {
            base.SubmitBatchData(data);
            data.TryRequest(this.EnablingVariable);
            data.TryRequest(this.ReadVariable);
        }

        public override async Task UpdateAsync(PlcBatchResults batches) {
            // A cool light show that shows how the updating happens in real time
            if (this.oldBrush == null) {
                this.oldBrush = this.Control.Background;
                this.Control.Background = Brushes.Orange;
            }
            else {
                this.Control.Background = this.oldBrush;
                this.oldBrush = null;
            }

            if (this.Context.TryGetPLC(this.EnablingVariable.Device, out _))
                this.Control.IsEnabled = !this.EnablingVariable.IsValid || (this.Context.TryReadBool(batches, this.EnablingVariable, out bool value) && value);
            else
                this.Control.IsEnabled = false;

            if (this.ReadVariable.IsValid && this.Context.TryReadBool(batches, this.ReadVariable, out bool isPressed)) {
                this.Control.IsVisuallyPressed = isPressed;
                this.Control.isToggleActive = isPressed;
            }
        }

        public override void OnConnect() {
            // HslCommunication.Authorization.SetAuthorizationCode()
        }

        public override void OnDisconnect() {
        }

        public void SendActivate() => this.SendSignal(true);

        public void SendDeactivate() => this.SendSignal(false);

        private void SendSignal(bool signal) {
            if (this.WriteVariable.IsValid && this.Context.TryGetPLC(this.WriteVariable, out IPlcApi? plc)) {
                Debug.WriteLine("Sending " + (signal ? "HIGH" : "LOW"));
                PlcOperation operation = plc.WriteBool(this.WriteVariable.FullAddress, signal);
                if (!operation.IsSuccessful) {
                    Debug.WriteLine("Error sending bool: " + operation.ErrorMessagte);
                }
            }
        }
    }
}