using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;

namespace JetTechMI.Utils;

public static class RepeatUtils {
    private static readonly FieldInfo TimerField = typeof(RepeatButton).GetField("_repeatTimer", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Missing _repeatTimer field in RepeatButton");

    public static bool IsTimerRunning(this RepeatButton button) {
        DispatcherTimer timer = (DispatcherTimer) TimerField.GetValue(button);
        return timer != null && timer.IsEnabled;
    }
}