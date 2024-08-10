using System;
using Avalonia;
using Avalonia.Controls;
using HslCommunication.Devices.Melsec;
using JetTechMI.Views.Pages;
using JetTechMI.Views.Pages.MainPage;

namespace JetTechMI.Views;

public partial class MainView : UserControl, IMainView {
    public static readonly StyledProperty<ActivePage> ActivePageProperty = AvaloniaProperty.Register<MainView, ActivePage>("ActivePage");

    public ActivePage ActivePage {
        get => this.GetValue(ActivePageProperty);
        set => this.SetValue(ActivePageProperty, value);
    }
    
    private ActivePage? theActivePage;
    private MainPageView? mainPageView;

    public MainView() {
        this.InitializeComponent();
        this.UpdatePage(ActivePage.Main);
    }
    static MainView() {
        ActivePageProperty.Changed.AddClassHandler<MainView, ActivePage>((c, e) => c.UpdatePage(e.NewValue.GetValueOrDefault()));
    }

    private void UpdatePage(ActivePage page) {
        if (this.theActivePage == page)
            return;

        if (this.PART_RootContentControl.Content is IPage currentPage) {
            currentPage.DisconnectPageFromView();
        }
        
        Control control;
        switch (page) {
            case ActivePage.Main:
                control = this.mainPageView ??= new MainPageView();
                break;
            default: throw new ArgumentOutOfRangeException(nameof(page), page, null);
        }

        ((IPage) control).ConnectPageToView(this);

        this.theActivePage = page;
        this.PART_RootContentControl.Content = control;
    }

    public void AddTopControl(Control control) {
        this.PART_RootGrid.Children.Add(control);
    }

    public void RemoveTopControl(Control control) {
        this.PART_RootGrid.Children.Remove(control);
    }
}