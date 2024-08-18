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
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HslCommunication;
using HslCommunication.BasicFramework;
using HslCommunication.Core.Types;
using HslCommunication.Devices.Melsec;
using JetTechMI.HMI;
using JetTechMI.Hsl;
using JetTechMI.Utils;

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

        LightOperationResult result;
        try {
            result = this.connection.Open();
        }
        catch (Exception ex) {
            this.connection.Close();
            this.connection.Dispose();
            this.connection = null;
            ((Button) sender!).Content = "Error";
            Console.WriteLine("Error connecting: " + ex.ToString());
            return;
        }

        if (result.IsSuccess) {
            ((Button) sender!).Content = "Disconnect";
            JetTechContext.Instance.RegisterConnection(new HslMelsecPlc(this.connection, 0));
        }
        else {
            ((Button) sender!).Content = "Error 2";
            Console.WriteLine("Error connecting: " + result.Message);
        }

        /*
        public const int CODE_STX = 0x02;
        public const int CODE_ETX = 0x03;
        public const int CODE_EOT = 0x04;
        public const int CODE_ENQ = 0x05;
        public const int CODE_ACK = 0x06;
        public const int CODE_LF = 0x0A;
        public const int CODE_CL = 0x0C;
        public const int CODE_CR = 0x0D;
        public const int CODE_NAK = 0x15;

        public const int CMD_BASIC_READ = 0x30;
        public const int CMD_BASIC_WRITE = 0x31;
        public const int CMD_FORCE_SET_BIT = 0x37;
        public const int CMD_FORCE_RESET_BIT = 0x38;
        public const int CMD_EXT_PREFIX = 0x45;
        public const int CMD_EXT_READ_PREFIX = 0x30;
        public const int CMD_EXT_WRITE_PREFIX = 0x31;
        public const int CMD_EXT_CONF = 0x30;
        public const int CMD_EXT_CODE = 0x31;

        For more info: https://github.com/KunYi/FX3U_Simulation
        BASIC READ:                 0x30
        BASIC WRITE:                0x31
        FORCE SET BIT:              0x37
        FORCE RESET BIT:            0x38
        EXTENSION READ PLC CONFIG:  0x45 0x30 0x30
        EXTENSION READ PLC CODE:    0x45 0x30 0x31
        EXTENSION WRITE PLC CONFIG: 0x45 0x31 0x30
        EXTENSION WRITE PLC CODE:   0x45 0x31 0x31
         */
        
        // No idea what this message is but GX Developer sends it and it gets a
        // valid response. I ain't got time to decode this crap lmfao
        // byte[] buffer = new byte[] {
        //     0x02, // STX?
        //     0x45, // EXTENDED COMMANDS??? Or maybe this and next byte is station number?
        //     0x30, // EXTENDED READ???
        //     0x30, // '0' // or maybe this and next byte is PC number?
        //     0x31, // '1' 
        //     0x37, // '7'  // 
        //     0x39, // '9' 
        //     0x30, // '0'
        //     0x32, // '2'
        //     0x42, // 'B'
        //     0x03, // ETX
        //     0x45, 0x44  // Checksum
        // };
        // byte[] buffer = new byte[] {
        //     0x02, // STX?
        //     0x45, // EXTENDED COMMANDS??? Or maybe this and next byte is station number?
        //     0x30, // EXTENDED READ???
        //     0x30, // '0' ???
        //     0x31, // '1' ASCII ADDRESS 1
        //     0x37, // '7' ASCII ADDRESS 2
        //     0x39, // '9' ASCII ADDRESS 3
        //     0x30, // '0' ASCII ADDRESS 4
        //     0x32, // '2' ASCII ADDR LEN 1
        //     0x42, // 'B' ASCII ADDR LEN 2
        //     0x03, // ETX
        //     0x45, 0x44  // Checksum
        // };
        
        /*
         * is 7 9 0 2 B the address in ascii?
         *
         * Is the address 0x7902 (which is 30978 decimal)?
         * Or 0x902B (decimal is 36907)
         */

        /*
         * PLC RESPONCE: (each row is an int64; 8 bytes)
         * Contains 86 data bytes, assuming the data is between STX and ETX (02 and 03)
         * Assuming for 4 bytes of control (thing and PC identifier), then that's 82 data bytes
         * 02 30 30 30 30 30 30 30
         * 30 30 41 30 30 31 30 30
         * 30 31 30 30 30 31 41 30
         * 30 30 30 30 30 30 30 30
         * 30 30 30 30 30 30 30 30
         * 30 30 30 30 30 30 30 30
         * 30 30 30 30 30 30 30 30
         * 30 30 30 30 30 30 30 30
         * 30 30 30 30 30 30 30 30
         * 30 30 30 30 30 30 31 30
         * 30 36 31 30 30 30 30 03
         * 35 30
         */
        
        // Is 86 or 82 in the message? Is it asking for bit devices?
        // 82 hex = 0x52, ascii = R
        // 86 hex = 0x56, ascii = V
        
        // 82/2 = 41 (0x29)
        // 86/2 = 43 (0x2B)
        // 82*8 = 656 (0x290 hex), 86*8 = 688 (0x2B0)
        // 82/8 == 86/8 10 (0xA)
        
        // Query data ends with 0x2B, which is 43, which is exactly how many
        // bytes the response contains divided by 2 for some reason???

        for (int i = 0; i < 24; i+= 2) {
            this.connection.Write("Y" + Convert.ToString(i, 8), true);
        }

        const ushort theActualAddress = 0;
        ushort startAddress = (theActualAddress / 8 + 0x00A0);
        byte[] asciiAddress = SoftBasic.BuildAsciiBytesFrom(startAddress);
        byte[] countBytes = SoftBasic.BuildAsciiBytesFrom(128);
        
        byte[] buffer = new byte[] {
            0x02, // STX?
            0x45, // EXTENDED COMMANDS?
            0x30, // EXTENDED READ?
            0x31, // DATA/USHORT?
            asciiAddress[0], // ASCII ADDRESS BYTE 0 (asciiAddress[1])
            asciiAddress[1], // ASCII ADDRESS BYTE 1 (asciiAddress[2])
            asciiAddress[2], // ASCII ADDRESS BYTE 2 (asciiAddress[3])
            asciiAddress[3], // ASCII ADDRESS BYTE 3 (asciiAddress[0])
            countBytes[0], // ASCII REQUESTED LENGTH BYTE 0
            countBytes[1], // ASCII REQUESTED LENGTH BYTE 1
            0x03, // ETX
            0, 0  // Checksum
        };

        int okd = 6032;

        // assuming Y0 is forced off so the above for loop setting it on is ignored, we expect this
        var expedcted1 = 0x555555;
        var expedcted2 = 0x1555554;
        // but it's ascii so we are looking for hex value of ascii char 55, 15 or 54
        byte[] thing0 = SoftBasic.BuildAsciiBytesFrom(0x55);
        byte[] thing1 = SoftBasic.BuildAsciiBytesFrom(0x54);
        byte[] thing2 = SoftBasic.BuildAsciiBytesFrom(0x15);
        

        MelsecHelper.FxCalculateCRC(buffer).CopyTo(buffer, buffer.Length - 2);
        
        LightOperationResult<byte[]> responce = this.connection.SendMessageAndGetResponce(buffer);
        byte[] x = responce.Content;
    }
    
    public void ConnectPageToView(MainView mainView) {
        this.MainView = mainView;
    }

    public void DisconnectPageFromView() {
        this.MainView = null;
    }
}