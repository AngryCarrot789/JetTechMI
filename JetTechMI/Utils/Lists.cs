using System.Collections.Generic;

namespace JetTechMI.Utils;

public static class Lists {
    public static void FillToCapacity<T>(this IList<T?> list, int capacity, T? fill = default) {
        for (int i = list.Count; i < capacity; i++) {
            list.Add(fill);
        }
    }
}