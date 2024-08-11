// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of JetTechMI.
// 
// JetTechMI is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// JetTechMI is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with JetTechMI. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.IO.Ports;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HslCommunication.Devices.Melsec;
using JetTechMI.HMI;
using JetTechMI.Hsl;

namespace JetTechMI.Views.Pages.MainPage;

public partial class MainPageView : UserControl, IPage {
    private MelsecFxSerial? connection;
    
    public MainView? MainView { get; set; }
    
    public MainPageView() {
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
        
        this.connection = new MelsecFxSerial() { SleepTime = 0 };
        this.connection.SetupSerial(selectedPort, 38400, 7, StopBits.One, Parity.Even);
        
        try {
            this.connection.Open();
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
    
    public void ConnectPageToView(MainView mainView) {
        this.MainView = mainView;
    }

    public void DisconnectPageFromView() {
        this.MainView = null;
    }
}