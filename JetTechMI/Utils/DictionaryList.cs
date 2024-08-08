using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace JetTechMI.Utils;

public class DictionaryList<TValue> {
    public List<TValue?> List { get; }

    public DictionaryList(List<TValue?> list) {
        this.List = list;
    }

    public DictionaryList() : this(new List<TValue?>()) {
    }
    
    public void Set(int index, TValue value) {
        if ((index + 1) > this.List.Count)
            this.List.FillToCapacity(index + 1);
        else if (this.List[index] != null)
            throw new InvalidOperationException("Index already in use: " + index);
        this.List[index] = value;
    }
    
    public bool Unset(int index) {
        if (index < 0 || index >= this.List.Count) {
            return false;
        }
        
        if (index == (this.List.Count - 1)) {
            this.List.RemoveAt(index);
            
            // Remove extra null
            for (int i = index - 1; i > 0; i--) {
                if (this.List[index] == null)
                    this.List.RemoveAt(i);
            } 
            
            this.List.TrimExcess();
        }
        else {
            this.List[index] = default;
        }

        return true;
    }

    public bool TryGet(int index, [NotNullWhen(true)] out TValue? value) {
        if (index >= 0 && index < this.List.Count && (value = this.List[index]) != null) {
            return true;
        }

        value = default;
        return false;
    }
}