using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JetTechMI.HMI;
using JetTechMI.Utils;

namespace JetTechMI;

public partial class App : Application {
    public App() {
        IntegerRangeList.Test(); 
    }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        JetTechRegistry.Instance.Run();
        
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
        }
        else if (this.ApplicationLifetime is ISingleViewApplicationLifetime singleView) {
            singleView.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}