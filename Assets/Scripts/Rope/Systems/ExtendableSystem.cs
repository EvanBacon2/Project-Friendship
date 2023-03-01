using UnityEngine;

/*
 * Defines a system for controlling an ExtendableRope
 */
public class ExtendableSystem : RequestSystem<ExtendableState> {
    public override void OnStateReceived(object sender, ExtendableState state) {
        ExtendableRope rope = state.rope;

        //auto extend rope
        if (state.auto && !rope.extended && !rope.autoRetract && !rope.autoExtend)
            rope.autoExtend = true;

        //auto retract rope
        if (state.auto && rope.extended && !rope.autoExtend && !rope.autoRetract)
            rope.autoRetract = true;

        //wind/unwind rope 
        if (rope.extended && state.wind != 0 && rope.winchScrollBuffer == 0 && (rope.activeSegments < rope.segments.Length || state.wind < 0))
            rope.winchScrollBuffer = rope.winchFrames * System.Math.Sign(state.wind);
    }
}
