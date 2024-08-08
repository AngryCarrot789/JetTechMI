using System;

namespace JetTechMI.Utils;

public class PercentUtils {
    public static void ValidateHopperRange(Percent value) {
        if (value.Numerator < 0)
            throw new ArgumentException("Percent cannot be negative");
        if (value.Numerator > 10000)
            throw new ArgumentException("Percent exceeds 100%");
    }
}