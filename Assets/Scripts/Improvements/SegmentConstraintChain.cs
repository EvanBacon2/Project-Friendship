using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentConstraintChain : ResponsibilityLink<SegmentGroupCollection, SegmentGroupCollection> {
    public bool execute(SegmentGroupCollection inState, SegmentGroupCollection outState) {
        return true;
    }
}
