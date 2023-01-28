using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Defines the ExtendableRope used by the player
 */
public class PlayerRope : ExtendableRope {
    //define the list of segments that make up the rope
    protected override Segment[] buildRope() {
        return new Segment[] {};
    }
}
