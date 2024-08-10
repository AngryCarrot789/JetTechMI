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

namespace JetTechMI.HMI.Controls.Buttons;

public enum JtButtonMode {
    /// <summary>
    /// This button is pressed by the user and released by the user
    /// </summary>
    Momentary,
    /// <summary>
    /// This button is pressed and released each click
    /// </summary>
    Toggle,
    /// <summary>
    /// This button activates a signal via write variable
    /// </summary>
    Set,
    /// <summary>
    /// This button deactivates a signal via write variable
    /// </summary>
    Reset
}