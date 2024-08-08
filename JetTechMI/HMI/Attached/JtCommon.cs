using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;

namespace JetTechMI.HMI.Attached;

public class JtCommon {
    private static readonly AttachedProperty<IJtControlData?> RegisteredControlDataProperty = AvaloniaProperty.RegisterAttached<JtCommon, Control, IJtControlData?>("RegisteredControlData");

    public static void SetRegisteredControlData(Control obj, IJtControlData? data) => obj.SetValue(RegisteredControlDataProperty, data);
    public static IJtControlData? GetRegisteredControlData(Control obj) => obj.GetValue(RegisteredControlDataProperty);

    public static bool TryGetRegisteredControlData(Control control, [NotNullWhen(true)] out IJtControlData? data) {
        return (data = GetRegisteredControlData(control)) != null;
    }
}