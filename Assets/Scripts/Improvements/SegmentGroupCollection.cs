using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface SegmentGroupCollection {
    public SegmentGroup[] toArray();
}

public class PlayerSegmentGroupCollection : SegmentGroupCollection {
    public SegmentGroup Rope;
    public SegmentGroup RBAnalogSegment;
    public SegmentGroup Hooked;

    private SegmentGroup[] _array;

    public PlayerSegmentGroupCollection() {
        this._array = new SegmentGroup[3] {Rope, RBAnalogSegment, Hooked };
    }
    
    public SegmentGroup[] toArray() {
        return this._array;
    }
}
