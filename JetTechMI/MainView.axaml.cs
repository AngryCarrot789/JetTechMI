using System;
using System.IO.Ports;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using HslCommunication.Devices.Melsec;
using JetTechMI.HMI;
using JetTechMI.Hsl;

namespace JetTechMI;

public partial class MainView : UserControl {
    private MelsecFxSerial? connection;
    
    public MainView() {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        return;

        void OnLoaded(object? sender, RoutedEventArgs e) {
            this.RefreshPortNames();
        }
    }

    private void RefreshPortNames() {
        this.PART_PortNameListBox.Items.Clear();
        foreach (string name in SerialPort.GetPortNames()) {
            this.PART_PortNameListBox.Items.Add(name);
        }
    }

    private void RefreshPortListButton_OnClick(object? sender, RoutedEventArgs e) {
        this.RefreshPortNames();
    }

    private void ConnectOrDisconnectButton_OnClick(object? sender, RoutedEventArgs e) {
        if (this.connection != null) {
            this.connection.Dispose();
            this.connection = null;
            JetTechContext.Instance.UnregisterConnection(0);
            ((Button) sender!).Content = "Connect"; 
            return;
        }

        if (!(this.PART_PortNameListBox.SelectedItem is string selectedPort) || string.IsNullOrWhiteSpace(selectedPort)) {
            return;
        }
        
        this.connection = new MelsecFxSerial() {
            SleepTime = 0//, IsNewVersion = false
        };
        
        this.connection.SerialPortInni(selectedPort, 38400, 7, StopBits.One, Parity.Even);
        try {
            this.connection.Open();
            // var operation = this.connection.Open();
            // if (operation == null || !operation.IsSuccess) {
            //     this.connection.Dispose();
            //     this.connection = null;
            // 
            //     ((Button) sender!).Content = "Error";
            //     Console.WriteLine("Error connecting: " + operation?.ToMessageShowString());
            //     return;
            // }
        }
        catch (Exception ex) {
            this.connection.Close();
            this.connection.Dispose();
            this.connection = null;
            ((Button) sender!).Content = "Error";
            Console.WriteLine("Error connecting: " + ex.ToString());
            return;
        }

        ((Button) sender!).Content = "Disconnect";
        JetTechContext.Instance.RegisterConnection(0, new HslMelsecPlc(this.connection));
    }

    private void ToggleStateButton_Click(object? sender, RoutedEventArgs e) {
        if (this.connection == null)
            return;
        
        int channel = int.Parse(((ToggleButton) sender!).Tag!.ToString()!);
        this.connection.Write("M" + channel, ((ToggleButton) sender!).IsChecked ?? false);
    }
}