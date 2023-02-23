using UnityEngine;

/*
 * Defines a system for controlling an ExtendableRope
 */
public class ExtendableSystem : RequestSystem<ExtendableState> {
    //private ExtendableRopeState state;
    //private ExtendableRope rope;
    //private Anchor anchor;

    //private Vector2d inactivePosition = Vector2d.zero;
    //private Vector2d inactiveOrientation = Vector2d.zero;

    public override void OnStateReceived(object sender, ExtendableState state) {
        ExtendableRope rope = state.rope;

        //rope.setInactivePosition(state.inactivePosition.x, state.inactivePosition.y);
        //rope.setInactiveOrientation(state.inactiveOrientation.x, state.inactiveOrientation.y);

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

    /*public override void OnUpdate() {
        //apply scroll wind
        if (rope.winchScrollBuffer != 0) {
            rope.winchOffset += rope.winchUnit / rope.winchFrames * System.Math.Sign(rope.winchScrollBuffer);
            rope.winchScrollBuffer -= rope.winchScrollBuffer > 0 ? 1 : -1;
        }

        //apply auto extention
        if (rope.autoExtend) {
            rope.winchOffset += rope.winchUnit;
            if (rope.activeSegments == rope.segments.Length) {//stop extending
                rope.autoExtend = false;
                state.onAutoExtendStart();
            }
        }

        //apply auto retraction
        if (rope.autoRetract) {
            rope.winchOffset -= rope.winchUnit;
            if (!rope.extended) {//stop retracting
                rope.autoRetract = false;
                state.onAutoRetractStart();
            }
        }

        anchor.setOffset(0, rope.winchOffset);
    }

    public override void ApplyConstraints() {
        for (int i = rope.activeSegments; i < rope.segments.Length; i++) {
            inactiveConstraint(rope.segments[i]);
        }
    }

    protected void inactiveConstraint(Segment segment) {
        segment.setP1(inactivePosition.x, inactivePosition.y);
		segment.setOrientation(inactiveOrientation.x, inactiveOrientation.y);
		segment.previousPosition.x = segment.position.x;
		segment.previousPosition.y = segment.position.y;
	    segment.velocity.x = 0;
		segment.velocity.y = 0;
		segment.angulerVelocity = 0;
    }*/
}
