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

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;

namespace JetTechMI.HMI.Attached;

public class JtCommon {
    private static readonly AttachedProperty<IJtControlData?> RegisteredControlDataProperty = AvaloniaProperty.RegisterAttached<JtCommon, Control, IJtControlData?>("RegisteredControlData");

    public static void SetRegisteredControlData(Control obj, IJtControlData? data) => obj.SetValue(RegisteredControlDataProperty, data);
    public static IJtControlData? GetRegisteredControlData(Control obj) => obj.GetValue(RegisteredControlDataProperty);

    public static bool TryGetRegisteredControlData(Control control, [NotNullWhen(true)] out IJtControlData? data) {
        return (data = GetRegisteredControlData(control)) != null;
    }
}