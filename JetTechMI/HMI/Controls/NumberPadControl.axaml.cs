using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Utilities;

namespace JetTechMI.HMI.Controls;

public partial class NumberPadControl : UserControl {
    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<NumberPadControl, double>("Value", defaultBindingMode: BindingMode.TwoWay, coerce: CoerceValue);
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<NumberPadControl, double>("Minimum", int.MinValue, coerce: CoerceMinimum);
    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<NumberPadControl, double>("Maximum", int.MaxValue, coerce: CoerceMaximum);
    public static readonly DirectProperty<NumberPadControl, bool> DialogResultProperty = AvaloniaProperty.RegisterDirect<NumberPadControl, bool>("DialogResult", o => o.DialogResult, (o, v) => o.DialogResult = v);

    public bool DialogResult {
        get => this._dialogResult;
        private set => this.SetAndRaise(DialogResultProperty, ref this._dialogResult, value);
    }

    public double Value {
        get => this.GetValue(ValueProperty);
        set => this.SetValue(ValueProperty, value);
    }

    public double Minimum {
        get => this.GetValue(MinimumProperty);
        set => this.SetValue(MinimumProperty, value);
    }

    public double Maximum {
        get => this.GetValue(MaximumProperty);
        set => this.SetValue(MaximumProperty, value);
    }

    private bool _dialogResult;
    private string? _textBuffer;
    private bool ignoreValueChangedInternal;
    private bool canReplaceWithEnteredNumber;

    public event EventHandler? DialogResultChanged;

    public NumberPadControl() {
        this.InitializeComponent();
        this.UpdateForValueChanged(0);
    }

    public NumberPadControl(double min, double max, double initialValue) {
        this.InitializeComponent();
        this.Minimum = min;
        this.Maximum = max;
        this.Value = initialValue;
        this.UpdateForValueChanged(initialValue);

        this.canReplaceWithEnteredNumber = true;
    }

    static NumberPadControl() {
        MinimumProperty.Changed.AddClassHandler<NumberPadControl>((control, _) => control.UpdateRangeDisplay());
        MaximumProperty.Changed.AddClassHandler<NumberPadControl>((control, _) => control.UpdateRangeDisplay());
        ValueProperty.Changed.AddClassHandler<NumberPadControl, double>((control, args) => control.OnValueChanged(args));
    }

    private void OnValueChanged(AvaloniaPropertyChangedEventArgs<double> e) {
        this.UpdateForValueChanged(e.NewValue.GetValueOrDefault());
    }

    private void UpdateForValueChanged(double? newValue) {
        if (this.ignoreValueChangedInternal)
            return;

        this.SetTextPreview((newValue ?? this.Value).ToString());
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.UpdateRangeDisplay();
        if (this.PART_ValueBlock != null)
            this.PART_ValueBlock.Text = this._textBuffer;
    }

    private void OnGridButtonClicked(object? sender, RoutedEventArgs e) {
        if (ReferenceEquals(sender, this.PART_Button0))
            this.OnClickNumber(0);
        else if (ReferenceEquals(sender, this.PART_Button1))
            this.OnClickNumber(1);
        else if (ReferenceEquals(sender, this.PART_Button2))
            this.OnClickNumber(2);
        else if (ReferenceEquals(sender, this.PART_Button3))
            this.OnClickNumber(3);
        else if (ReferenceEquals(sender, this.PART_Button4))
            this.OnClickNumber(4);
        else if (ReferenceEquals(sender, this.PART_Button5))
            this.OnClickNumber(5);
        else if (ReferenceEquals(sender, this.PART_Button6))
            this.OnClickNumber(6);
        else if (ReferenceEquals(sender, this.PART_Button7))
            this.OnClickNumber(7);
        else if (ReferenceEquals(sender, this.PART_Button8))
            this.OnClickNumber(8);
        else if (ReferenceEquals(sender, this.PART_Button9))
            this.OnClickNumber(9);
        else if (ReferenceEquals(sender, this.PART_ButtonCLR)) {
            this.canReplaceWithEnteredNumber = false;
            this.SetTextPreview("0");
        }
        else if (ReferenceEquals(sender, this.PART_ToggleNegativity)) {
            this.canReplaceWithEnteredNumber = false;
            if (!string.IsNullOrEmpty(this._textBuffer)) {
                this.SetTextPreview(this._textBuffer[0] == '-' ? this._textBuffer.Substring(1) : ('-' + this._textBuffer));
            }
        }
        else if (ReferenceEquals(sender, this.PART_ButtonESC))
            this.SetDialogResult(false);
        else if (ReferenceEquals(sender, this.PART_ButtonDEL))
            this.OnDeleteLastNumber();
        else if (ReferenceEquals(sender, this.PART_ButtonENT))
            this.SetDialogResult(true);
        else if (ReferenceEquals(sender, this.PART_InsertDot)) {
            this.canReplaceWithEnteredNumber = false;
            if (!string.IsNullOrEmpty(this._textBuffer)) {
                if (!this._textBuffer.Contains('.'))
                    this.SetTextPreview(this._textBuffer + ".");
            }
            else {
                this.SetTextPreview("0.");
            }
        }
    }

    private void OnClickNumber(int number) {
        if (this.canReplaceWithEnteredNumber) {
            this.canReplaceWithEnteredNumber = false;
            this.SetTextPreview(Math.Max(this.Minimum, Math.Min(this.Maximum, number)).ToString());
        }
        else {
            bool isDotZero = number == 0 && this._textBuffer!.Length > 0 && this._textBuffer[this._textBuffer.Length - 1] == '.';
            double newValue = double.TryParse(this._textBuffer + number, out double d) ? d : this.Maximum;
            newValue = Math.Max(this.Minimum, Math.Min(this.Maximum, newValue));
            this.SetTextPreview(newValue.ToString() + (isDotZero ? ".0" : ""));
        }
    }

    public void SetDialogResult(bool value) {
        this.DialogResult = value;
        if (value && double.TryParse(this._textBuffer, out double val)) {
            this.ignoreValueChangedInternal = true;
            this.Value = val;
            this.ignoreValueChangedInternal = false;
        }

        this.DialogResultChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnDeleteLastNumber() {
        this.canReplaceWithEnteredNumber = false;
        string? str = this._textBuffer;
        if (str == null)
            return;

        switch (str.Length) {
            case 0: return;
            case 1:
                this.SetTextPreview("0");
                break;
            default:
                this.SetTextPreview(str.Substring(0, str.Length - 1));
                break;
        }
    }

    private void UpdateRangeDisplay() {
        this.PART_RangeBlock.Text = $"{this.Minimum}~{this.Maximum}";
    }

    private void SetTextPreview(string text) {
        this._textBuffer = text;
        if (this.PART_ValueBlock != null)
            this.PART_ValueBlock.Text = text;
    }

    private static double CoerceMinimum(AvaloniaObject sender, double value) {
        return ValidateDouble(value) ? value : sender.GetValue(MinimumProperty);
    }

    private static double CoerceMaximum(AvaloniaObject sender, double value) {
        return ValidateDouble(value)
            ? Math.Max(value, sender.GetValue(MinimumProperty))
            : sender.GetValue(MaximumProperty);
    }

    private static double CoerceValue(AvaloniaObject sender, double value) {
        return ValidateDouble(value)
            ? MathUtilities.Clamp(value, sender.GetValue(MinimumProperty), sender.GetValue(MaximumProperty))
            : sender.GetValue(ValueProperty);
    }

    private static bool ValidateDouble(double value) => !double.IsInfinity(value) && !double.IsNaN(value);
}