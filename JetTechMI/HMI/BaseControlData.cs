using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace JetTechMI.HMI;

public abstract class BaseControlData<T> : IJtControlData<T> where T : Control {
    Control IJtControlData.Control => this.Control;

    public T Control { get; }

    /// <summary>
    /// Helper property for quickly accessing the jet tech context
    /// </summary>
    public JetTechContext Context => JetTechContext.Instance;

    protected BaseControlData(T control) {
        this.Control = control ?? throw new ArgumentNullException(nameof(control), "Control cannot be null");
    }

    public abstract Task UpdateAsync(PlcBatchResults batches);
    
    public abstract void OnConnect();

    public abstract void OnDisconnect();
    
    public virtual void SubmitBatchData(PlcBatchRequest data) {
        
    }
}