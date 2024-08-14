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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using JetTechMI.HMI;
using JetTechMI.HMI.Controls;
using JetTechMI.Services;
using JetTechMI.Services.Numeric;
using JetTechMI.Utils;
using JetTechMI.Views;

namespace JetTechMI;

public partial class App : Application {
    private static readonly ServiceManager manager = new ServiceManager();
    
    public static IServiceManager Services => manager;
    
    public App() {
        IntegerRangeList.Test(); 
    }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        JetTechRegistry.Instance.Run();

        IMainView? view = null;
        
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
            view = ((MainWindow) desktop.MainWindow).PART_MainView;
        }
        else if (this.ApplicationLifetime is ISingleViewApplicationLifetime singleView) {
            singleView.MainView = new MainView();
            view = (MainView) singleView.MainView;
        }

        base.OnFrameworkInitializationCompleted();
        manager.Register<ITouchEntry>(new TouchEntryImpl(view));
    }
    
    private class TouchEntryImpl : ITouchEntry {
        private readonly IMainView? rootView;
        private readonly EventHandler eventHandler;
        private NumberPadControl? activeNumPad;
        private TaskCompletionSource<double?>? currentTask;

        public TouchEntryImpl(IMainView? rootView) {
            this.rootView = rootView;
            this.eventHandler = (sender, e) => {
                NumberPadControl control = (NumberPadControl) sender!;
                control.DialogResultChanged -= this.eventHandler;
                
                bool result = control.DialogResult;
                double value = control.Value;
                this.rootView?.RemoveTopControl(control);

                if (control == this.activeNumPad)
                    this.activeNumPad = null;

                this.currentTask?.SetResult(result ? value : null);
                this.currentTask = null;
            };
        }

        public Task<double?> ShowNumericAsync(double initialValue, double minValue = 0, double maxValue = Int32.MaxValue) {
            if (this.rootView == null)
                return Task.FromResult<double?>(null);

            this.activeNumPad?.SetDialogResult(false);
            if (this.currentTask != null)
                throw new Exception("Did not expect current TCS to still exist");

            this.activeNumPad = new NumberPadControl(minValue, maxValue, initialValue) {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Effect = new DropShadowEffect() { BlurRadius = 75, Color = Colors.Black, OffsetX = 0, OffsetY = 0, Opacity = 1 }
            };

            this.currentTask = new TaskCompletionSource<double?>();

            this.rootView.AddTopControl(this.activeNumPad);
            this.activeNumPad.DialogResultChanged += this.eventHandler;
            return this.currentTask.Task;
        }
    }
}