using Avalonia.Controls;

namespace JetTechMI;

public interface IMainView {
    void AddTopControl(Control control);
    void RemoveTopControl(Control control);
}