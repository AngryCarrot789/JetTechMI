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
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using JetTechMI.Services.Numeric;
using JetTechMI.Themes.Industrial.Controls;
using JetTechMI.Utils;
using JetTechMI.Utils.Commands;

namespace JetTechMI.HMI.Controls;

public class NumericEntry : RangeBase {
    public static readonly StyledProperty<string?> DisplayTextProperty = AvaloniaProperty.Register<NumericEntry, string?>("DisplayText");
    public static readonly StyledProperty<string?> ValueFormatProperty = AvaloniaProperty.Register<NumericEntry, string?>("ValueFormat");
    public static readonly DirectProperty<NumericEntry, bool> HasDisplayTextProperty = AvaloniaProperty.RegisterDirect<NumericEntry, bool>("HasDisplayText", o => o.HasDisplayText);
    public static readonly DirectProperty<NumericEntry, bool> IsPointerPressedProperty = AvaloniaProperty.RegisterDirect<NumericEntry, bool>("IsPointerPressed", o => o.IsPointerPressed);
    public static readonly DirectProperty<NumericEntry, bool> IsKeypadOpenProperty = AvaloniaProperty.RegisterDirect<NumericEntry, bool>("IsKeypadOpen", o => o.IsKeypadOpen);

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty = AvaloniaProperty.Register<NumericEntry, VerticalAlignment>("VerticalContentAlignment");
    public static readonly StyledProperty<HorizontalAlignment> DisplayTextAlignmentProperty = AvaloniaProperty.Register<NumericEntry, HorizontalAlignment>("DisplayTextAlignment", HorizontalAlignment.Center);
    public static readonly StyledProperty<HorizontalAlignment> ValueTextAlignmentProperty = AvaloniaProperty.Register<NumericEntry, HorizontalAlignment>("ValueTextAlignment", HorizontalAlignment.Right);
    public static readonly StyledProperty<Dock> DisplayTextPositionProperty = AvaloniaProperty.Register<NumericEntry, Dock>("DisplayTextPosition", Dock.Top);
    public static readonly StyledProperty<Dock> ValueTextPositionProperty = AvaloniaProperty.Register<NumericEntry, Dock>("ValueTextPosition", Dock.Right);

    public VerticalAlignment VerticalContentAlignment {
        get => this.GetValue(VerticalContentAlignmentProperty);
        set => this.SetValue(VerticalContentAlignmentProperty, value);
    }

    public HorizontalAlignment DisplayTextAlignment {
        get => this.GetValue(DisplayTextAlignmentProperty);
        set => this.SetValue(DisplayTextAlignmentProperty, value);
    }

    public HorizontalAlignment ValueTextAlignment {
        get => this.GetValue(ValueTextAlignmentProperty);
        set => this.SetValue(ValueTextAlignmentProperty, value);
    }

    public Dock DisplayTextPosition {
        get => this.GetValue(DisplayTextPositionProperty);
        set => this.SetValue(DisplayTextPositionProperty, value);
    }

    public Dock ValueTextPosition {
        get => this.GetValue(ValueTextPositionProperty);
        set => this.SetValue(ValueTextPositionProperty, value);
    }

    public string? DisplayText {
        get => this.GetValue(DisplayTextProperty);
        set => this.SetValue(DisplayTextProperty, value);
    }

    public string? ValueFormat {
        get => this.GetValue(ValueFormatProperty);
        set => this.SetValue(ValueFormatProperty, value);
    }

    public bool HasDisplayText {
        get => this._hasDisplayText;
        set => this.SetAndRaise(HasDisplayTextProperty, ref this._hasDisplayText, value);
    }

    public bool IsPointerPressed {
        get => this.isPointerPressed;
        private set => this.SetAndRaise(IsPointerPressedProperty, ref this.isPointerPressed, value);
    }

    public bool IsKeypadOpen {
        get => this._isKeypadOpen;
        private set => this.SetAndRaise(IsKeypadOpenProperty, ref this._isKeypadOpen, value);
    }

    private ControlBase PART_Root;
    private ControlBase PART_ButtonPresser;
    private TextBlock PART_DisplayTextBlock;
    private TextBlock PART_ValueTextBlock;
    private bool isPointerPressed;
    private bool _hasDisplayText;
    private string textInternal;
    private bool _isKeypadOpen;

    private static readonly AsyncRelayCommand<NumericEntry> ShowNumPadCommand;

    public NumericEntry() {
    }

    static NumericEntry() {
        DisplayTextProperty.Changed.AddClassHandler<NumericEntry, string?>((o, args) => {
            string? text = args.NewValue.GetValueOrDefault();
            o.HasDisplayText = !string.IsNullOrWhiteSpace(text);
        });

        ValueProperty.Changed.AddClassHandler<NumericEntry, double>((o, args) => o.UpdateValueText());
        ValueFormatProperty.Changed.AddClassHandler<NumericEntry, string?>((o, args) => o.UpdateValueText());

        // Static to help reduce performance overhead
        ShowNumPadCommand = new AsyncRelayCommand<NumericEntry>(async (e) => {
            if (e == null)
                throw new InvalidOperationException("NumericEntry control not passed to command");

            double? value = await App.Services.GetService<ITouchEntry>().ShowNumericAsync(e.Value, e.Minimum, e.Maximum);
            if (value.HasValue)
                e.OnChangeValueFromNumberPad(value.Value);

            e.IsPointerPressed = false;
            e.IsKeypadOpen = false;
        });

        // DisplayTextPositionProperty.Changed.AddClassHandler<NumericEntry, Dock>((o, e) => o.UpdateTextBlockPositions());
    }

    private void UpdateValueText() {
        string? format = this.ValueFormat;
        this.textInternal = format != null ? string.Format(format, this.Value) : this.Value.ToString("F2");

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (this.PART_ValueTextBlock != null)
            this.PART_ValueTextBlock.Text = this.textInternal;
    }

    protected virtual void OnChangeValueFromNumberPad(double newValue) {
        this.Value = newValue;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild(nameof(this.PART_Root), out this.PART_Root);
        e.NameScope.GetTemplateChild(nameof(this.PART_ButtonPresser), out this.PART_ButtonPresser);
        e.NameScope.GetTemplateChild(nameof(this.PART_DisplayTextBlock), out this.PART_DisplayTextBlock);
        e.NameScope.GetTemplateChild(nameof(this.PART_ValueTextBlock), out this.PART_ValueTextBlock);

        this.PART_ValueTextBlock.Text = this.textInternal;
        this.PART_Root.PointerPressed += this.OnRootPressed;
        this.PART_Root.PointerReleased += this.OnRootReleased;
        this.PART_Root.PointerExited += this.OnRootPointerExited;
    }

    private void OnRootPointerExited(object? sender, PointerEventArgs e) {
        if (this.IsKeypadOpen)
            return;
        this.IsPointerPressed = false;
    }

    private void OnRootPressed(object? sender, PointerPressedEventArgs e) {
        this.IsKeypadOpen = false;
        this.IsPointerPressed = true;

        // e.Pointer.Type == PointerType.Touch
    }

    private void OnRootReleased(object? sender, PointerReleasedEventArgs e) {
        if (this.IsKeypadOpen)
            return;

        if (!this.IsPointerPressed)
            return;

        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        e.Handled = true;
        if (!this.GetVisualsAt(e.GetPosition(this)).Any(c => this == c || this.IsVisualAncestorOf(c))) {
            this.IsPointerPressed = false;
            return;
        }

        ShowNumPadCommand.Execute(this);
    }
}