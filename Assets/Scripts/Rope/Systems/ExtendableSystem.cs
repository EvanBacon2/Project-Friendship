using UnityEngine;

/*
 * Defines a system for controlling an ExtendableRope
 */
public class ExtendableSystem : RequestSystem<ExtendableState> {
    public override void OnStateReceived(object sender, ExtendableState state) {//rope extender
        Rope rope = state.rope;
        ExtendRopeNoIn extender = state.extender;
        
        if (state.auto) {
            Debug.Log("extended: " + extender.extended + " autoRetract: " + extender.autoRetract + " autoExtend: " + extender.autoExtend);
        }

        //auto extend rope
        if (state.auto && !extender.extended && !extender.autoRetract && !extender.autoExtend) 
            extender.autoExtend = true;

        //auto retract rope
        if (state.auto && extender.extended && !extender.autoExtend && !extender.autoRetract)
            extender.autoRetract = true;

        //wind/unwind rope 
        if (extender.extended && state.wind != 0 && extender.winchScrollBuffer == 0 && (rope.activeSegments < rope.segments.Length || state.wind < 0))
            extender.winchScrollBuffer = extender.winchFrames * System.Math.Sign(state.wind);
    }
}
