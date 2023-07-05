using UnityEngine;

public class PlayerRopeHook : Hook, RopeBehaviour {
    public PlayerRope rope;
    public Anchor anchor;
    public int hookIndex;
    public double hookLag;

    private Segment hookSegment { get { return rope.segments[hookIndex]; } }
    private Vector2d autoHookPos = Vector2d.zero;//position hookSegment is constrained to during autoExtend

    private Vector2d hookSnapshot = Vector2d.zero;

    protected override void start() {
        addHookedCallback(() => {
            //rope.autoExtend = false;
            rope.configure(rope.angleLimitDegrees, .97, .98, rope.maxSpeed, rope.maxSpeedScale);
            hookSegment.mass = hookMass;
            hookSegment.inertia = hookMass;
            //hookSegment.velocity.x = 0;
            //hookSegment.velocity.y = 0;

            hookSnapshot.x = hookSegment.p2.x;
            hookSnapshot.y = hookSegment.p2.y;
        });

        addUnHookedCallback(() => {
            hookSegment.mass = 1;
        });
    }

    public void OnUpdate() {
        //deactivate hook when rope isn't extended
        if (!rope.extended && !rope.autoExtend) {
		    active = false;
			unHook();
		}   
    }

    public void OnSubUpdate() {
        autoHookPos.x = anchor.attachPoint.x + anchor.orientation.x * (rope.segmentLength * hookLag * rope.activeSegments);
		autoHookPos.y = anchor.attachPoint.y + anchor.orientation.y * (rope.segmentLength * hookLag * rope.activeSegments);
    }

    public void ApplyConstraints() {
        //constrain hook while auto extending
        if (rope.extended && rope.autoExtend) {
            //SegmentConstraint.pointConstraint(autoHookPos, hookSegment, false);
            //SegmentConstraint.angleConstraint(anchor.anchorSegment, hookSegment, 0);
        }
    }

    private Vector3 hookPosition = Vector3.zero;
    Vector2 real = Vector2.zero;
    Vector2 complex = Vector2.zero;

    public void OnUpdateLate() {
        //update hook position
        hookPosition.x = (float)hookSegment.position.x;
        hookPosition.y = (float)hookSegment.position.y;
        transform.position = hookPosition;
        
        //update hook rotation
        float angle = Mathf.Atan2((float)hookSegment.orientation.y, (float)hookSegment.orientation.x) * Mathf.Rad2Deg - 90;
		Quaternion hookRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = hookRotation;

        //update hook velocity
        hookVelocity.x = (float)hookSegment.velocity.x;
        hookVelocity.y = (float)hookSegment.velocity.y;

        //update hooked 
        if (hooked != null) {
            float angleChange = (transform.rotation.eulerAngles.z - baseRotation.eulerAngles.z) * Mathf.Deg2Rad;

            float cosR = Mathf.Cos(angleChange);
            float sinR = Mathf.Sin(angleChange);

            real.x = cosR * hookOffset.x;
            real.y = cosR * hookOffset.y;
            complex.x = sinR * hookOffset.x;
            complex.y = sinR * hookOffset.y;
            
            hookOffset.x = real.x - complex.y; 
            hookOffset.y = real.y + complex.x;

			hooked.transform.position = transform.position + hookOffset;
            hooked.transform.rotation = Quaternion.AngleAxis(angleChange * Mathf.Rad2Deg + hooked.transform.rotation.eulerAngles.z, Vector3.forward);

            baseRotation = transform.rotation;
        }
    }
}
