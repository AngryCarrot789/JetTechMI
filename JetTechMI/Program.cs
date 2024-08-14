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

using Avalonia;
using System;
using System.Threading;
using Avalonia.LinuxFramebuffer.Output;
using JetTechMI.Utils;

namespace JetTechMI;

class Program {
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args) {

        // IntegerRangeMap<string>.Test();
        
        AppBuilder builder = BuildAvaloniaApp();
        if (args.Contains("--drm")) {
            SilenceConsole();
            DrmOutput drm;
            try {
                drm = new DrmOutput("/dev/dri/card0");
            }
            catch (Exception e) {
                try {
                    drm = new DrmOutput("/dev/dri/card1");
                }
                catch {
                    try {
                        drm = new DrmOutput();
                    }
                    catch {
                        try {
                            drm = new DrmOutput("/dev/dri/renderD128");
                        }
                        catch {
                            throw new Exception("Failed to find a DRM card to open", e);
                        }
                    }
                }
            }

            // OPTIONAL: SCALING ENTIRE GUI. 1.25 IS NICE
            drm.Scaling = 1.0;
            return builder.StartLinuxDirect(args: args, drm);
        }

        return builder.StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();

    private static void SilenceConsole() {
        new Thread(() => {
                Console.CursorVisible = false;
                while (true)
                    Console.ReadKey(true);
            })
            { IsBackground = true }.Start();
    }
}