using UnityEngine;

public class PlayerRopeHook : Hook, RopeBehaviour {
    public PlayerRope rope;
    public Anchor anchor;
    public int hookIndex;
    public double hookLag;

    private Segment hookSegment { get { return rope.segments[hookIndex]; } }
    private Vector2d autoHookPos = Vector2d.zero;//position hookSegment is constrained to during autoExtend

    private double tightLength = 0;
    private double looseLength = 0;
    private Vector2d hookSnapshot = Vector2d.zero;

    protected override void start() {
        addHookedCallback(() => {
            rope.autoExtend = false;

            tightenRope();
            hookSnapshot.x = hookSegment.position.x;
            hookSnapshot.y = hookSegment.position.y;
        });
    }

    public void OnUpdate() {
        if (hooked != null) 
			hooked.transform.position = transform.position + hookOffset;

        //deactivate hook when rope isn't extended
        if (!rope.extended && !rope.autoExtend) {
		    active = false;
			unHook();
		}

        //tighten rope
        if (tightLength > 0) {
            if (looseLength - tightLength > rope.autoExtendRate * .25) {
                rope.winchOffset -= rope.autoExtendRate * .25;
                looseLength -= rope.autoExtendRate * .25;
            } else {
                rope.winchOffset -= looseLength - tightLength;
                looseLength = 0;
                tightLength = 0;
                rope.stiff();
            }
        }
    }

    public void OnSubUpdate() {
        autoHookPos.x = anchor.attachPoint.x + anchor.orientation.x * (rope.segmentLength * hookLag * rope.activeSegments);
		autoHookPos.y = anchor.attachPoint.y + anchor.orientation.y * (rope.segmentLength * hookLag * rope.activeSegments);
    }

    public void ApplyConstraints() {
        //constrain hook while auto extending
        if (rope.extended && rope.autoExtend)
            SegmentConstraint.pointConstraint(autoHookPos, hookSegment, false);

        //constrain hook while tightening rope
        if (tightLength > 0)
            SegmentConstraint.pointConstraint(hookSnapshot, hookSegment, false);
    }

    private Vector3 hookPosition = Vector3.zero;

    public void OnUpdateLate() {
        hookPosition.x = (float)hookSegment.position.x;
        hookPosition.y = (float)hookSegment.position.y;
        transform.position = hookPosition;
        
        float angle = Mathf.Atan2((float)hookSegment.orientation.y, (float)hookSegment.orientation.x) * Mathf.Rad2Deg - 90;
		Quaternion hookRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = hookRotation;
       
        hookVelocity.x = (float)hookSegment.velocity.x;
        hookVelocity.y = (float)hookSegment.velocity.y;
    }

    private Vector2d hook2Base = Vector2d.zero;
    private void tightenRope() {
        hook2Base.x = hookSegment.position.x - anchor.attachPoint.x;
        hook2Base.y = hookSegment.position.y - anchor.attachPoint.y;

        looseLength = rope.activeSegments * rope.segmentLength;
        tightLength = hook2Base.magnitude;
    }
}
