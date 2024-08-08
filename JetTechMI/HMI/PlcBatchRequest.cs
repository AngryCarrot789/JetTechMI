using JetTechMI.Utils;

namespace JetTechMI.HMI;

/// <summary>
/// Contains a list of addresses that are being requested for batched data
/// </summary>
public class PlcBatchRequest {
    public DictionaryList<PlcBatchRequestData> Requests { get; }

    public PlcBatchRequest() {
        this.Requests = new DictionaryList<PlcBatchRequestData>();
    }

    public void Clear() {
        foreach (PlcBatchRequestData? device in this.Requests.List)
            device?.Clear();
        this.Requests.List.Clear();
    }

    public void Submit(JetTechContext context, PlcBatchResults batchesData) => context.SubmitBatchRequestData(this, batchesData);

    public void TryRequest(DeviceAddress address) {
        if (!address.IsValid)
            return;
        
        if (!this.Requests.TryGet(address.Device, out PlcBatchRequestData? data))
            this.Requests.Set(address.Device, data = new PlcBatchRequestData(address.Device));

        switch (address.AddressPrefix) {
            case 'M': data.GetListForM().Add(address.AddressSlot); break;
            case 'S': data.GetListForS().Add(address.AddressSlot); break;
            case 'T': data.GetListForT().Add(address.AddressSlot); break;
            case 'C': (address.AddressSlot > 199 ? data.GetListForC32() : data.GetListForC16()).Add(address.AddressSlot); break;
            // case 'D': data.GetListForD(); break;
            case 'X': data.GetListForX().Add(address.AddressSlot); break;
            case 'Y': data.GetListForY().Add(address.AddressSlot); break;
            // case 'R': data.GetListForR(); break;
        }
    }
}

/// <summary>
/// A struct which contains batch request information in relation to a specific PLC device
/// </summary>
public class PlcBatchRequestData {
    public readonly int Device;

    public IntegerRangeList? ListForC16; // 16-bit counter
    public IntegerRangeList? ListForC32; // 32-bit counter
    public IntegerRangeList? ListForM; // aux relays/internal mem bits
    public IntegerRangeList? ListForS; // state
    public IntegerRangeList? ListForT; // timer
    public IntegerRangeList? ListForX; // input
    public IntegerRangeList? ListForY; // output

    public PlcBatchRequestData(int device) {
        this.Device = device;
    }

    public void Clear() {
        this.ListForM = null;
        this.ListForS = null;
        this.ListForT = null;
        this.ListForX = null;
        this.ListForY = null;
    }

    public IntegerRangeList GetListForC16() => (this.ListForC16 ??= new IntegerRangeList());
    public IntegerRangeList GetListForC32() => (this.ListForC32 ??= new IntegerRangeList());
    public IntegerRangeList GetListForM() => (this.ListForM ??= new IntegerRangeList());
    public IntegerRangeList GetListForS() => (this.ListForS ??= new IntegerRangeList());
    public IntegerRangeList GetListForT() => (this.ListForT ??= new IntegerRangeList());
    public IntegerRangeList GetListForX() => (this.ListForX ??= new IntegerRangeList());
    public IntegerRangeList GetListForY() => (this.ListForY ??= new IntegerRangeList());
}