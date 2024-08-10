namespace JetTechMI.Views.Pages;

/// <summary>
/// An interface for a page control
/// </summary>
public interface IPage {
    MainView? MainView { get; }

    void ConnectPageToView(MainView mainView);
    
    void DisconnectPageFromView();
}