namespace JetTechMI.HMI.Controls.Entries;

public enum NumericVariableType {
    /// <summary>
    /// A 32-bit floating point number (single)
    /// </summary>
    Float, 
    /// <summary>
    /// A 64-bit floating point number (double precision)
    /// </summary>
    Double, 
    /// <summary>
    /// An 8-bit value
    /// </summary>
    Byte, 
    /// <summary>
    /// A 16-bit value (short/ushort)
    /// </summary>
    Word, 
    /// <summary>
    /// A 32-bit value (int/uint)
    /// </summary>
    DWord
}