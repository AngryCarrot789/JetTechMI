using System.Threading.Tasks;
using Avalonia.Controls;

namespace JetTechMI.HMI;

/// <summary>
/// Defines an interface for the control data, that is, custom data and functionality attached
/// to a single instance of a UI control registered with the <see cref="JetTechRegistry"/>
/// </summary>
public interface IJtControlData {
    /// <summary>
    /// Gets the control associated with this data
    /// </summary>
    Control Control { get; }

    /// <summary>
    /// Scheduled update on the application's main thread
    /// </summary>
    /// <param name="batches"></param>
    /// <returns>An awaitable task for the completion of the tick</returns>
    Task UpdateAsync(PlcBatchResults batches);

    /// <summary>
    /// Called when the registry connects this data to a control fully
    /// </summary>
    void OnConnect();
    
    /// <summary>
    /// Called just before the registry completely de-associates this data with the control
    /// </summary>
    void OnDisconnect();

    /// <summary>
    /// This method is called on a background task. Add requests for data from the PLC
    /// </summary>
    void SubmitBatchData(PlcBatchRequest data);
}

/// <summary>
/// A generic version of <see cref="IJtControlData"/>, purely for convenience of
/// not having to cast <see cref="IJtControlData.Control"/> to the derived type
/// </summary>
/// <typeparam name="TControl">The type of control this interface is attached to</typeparam>
public interface IJtControlData<out TControl> : IJtControlData where TControl : Control {
    /// <summary>
    /// Gets the control associated with this data
    /// </summary>
    new TControl Control { get; }
}