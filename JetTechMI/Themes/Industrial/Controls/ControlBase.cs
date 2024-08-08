using Avalonia;
using Avalonia.Controls;

namespace JetTechMI.Themes.Industrial.Controls;

/// <summary>
/// A base that a control can sit on top of
/// </summary>
public class ControlBase : ContentControl {
    public static readonly StyledProperty<bool> IsPressedInProperty = AvaloniaProperty.Register<ControlBase, bool>("IsPressedIn");

    public bool IsPressedIn {
        get => this.GetValue(IsPressedInProperty);
        set => this.SetValue(IsPressedInProperty, value);
    }
}