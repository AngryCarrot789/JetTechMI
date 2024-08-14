namespace JetTechMI.HMI;

/// <summary>
/// The size of a piece of data. A bit is treated differently from a byte, since a PLC will typically pack 8 booleans into a single byte
/// </summary>
public enum DataSize {
    /// <summary>
    /// There are 8 bits packed into a single byte
    /// </summary>
    Bit,
    /// <summary>
    /// One byte
    /// </summary>
    Byte,
    /// <summary>
    /// Two bytes
    /// </summary>
    Word,
    /// <summary>
    /// Four bytes
    /// </summary>
    DWord,
    /// <summary>
    /// Eight bytes
    /// </summary>
    QWord
}