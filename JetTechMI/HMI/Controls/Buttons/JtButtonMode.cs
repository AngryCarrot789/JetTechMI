namespace JetTechMI.HMI.Controls.Buttons;

public enum JtButtonMode {
    /// <summary>
    /// This button is pressed by the user and released by the user
    /// </summary>
    Momentary,
    /// <summary>
    /// This button is pressed and released each click
    /// </summary>
    Toggle,
    /// <summary>
    /// This button activates a signal via write variable
    /// </summary>
    Set,
    /// <summary>
    /// This button deactivates a signal via write variable
    /// </summary>
    Reset
}