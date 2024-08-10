using System.Threading.Tasks;

namespace JetTechMI.Services.Numeric;

/// <summary>
/// An interface for touch-screen suitable keypads and numpads 
/// </summary>
public interface ITouchEntry {
    Task<double?> ShowNumericAsync(double initialValue, double minValue = 0, double maxValue = int.MaxValue);
}