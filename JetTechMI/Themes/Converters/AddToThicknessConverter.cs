using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace JetTechMI.Themes.Converters;

public class AddToThicknessConverter : IValueConverter {
    public Thickness Thickness { get; set; }

    public double Uniform { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value == AvaloniaProperty.UnsetValue)
            return value;

        if (value is Thickness t)
            return new Thickness(
                t.Left + this.Thickness.Left + this.Uniform,
                t.Top + this.Thickness.Top + this.Uniform,
                t.Right + this.Thickness.Right + this.Uniform,
                t.Bottom + this.Thickness.Bottom + this.Uniform);

        throw new Exception("Invalid value: " + value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}