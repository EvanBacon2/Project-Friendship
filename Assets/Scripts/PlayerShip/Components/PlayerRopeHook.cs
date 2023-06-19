using UnityEngine;

public class PlayerRopeHook : Hook, RopeBehaviour {
    public PlayerRope rope;
    public Anchor anchor;
    public int hookIndex;
    public double hookLag;
    public double tightenRate;
    private bool tighten;

    private Segment hookSegment { get { return rope.segments[hookIndex]; } }
    private Vector2d autoHookPos = Vector2d.zero;//position hookSegment is constrained to during autoExtend

    private double tightLength = 0;
    private double looseLength = 0;
    private Vector2d hookSnapshot = Vector2d.zero;

    protected override void start() {
        addHookedCallback(() => {
            rope.autoExtend = false;
            rope.configure(rope.angleLimitDegrees, .97, .98, rope.maxSpeed, rope.maxSpeedScale);
            hookSegment.mass = hookMass;
            hookSegment.inertia = hookMass;
            tighten = true;
            tightenRope();
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
        Debug.Log(hookSegment.mass);
        //tighten rope
        if (tighten) {
            if (tightLength > tightenRate) {
                rope.winchOffset -= tightenRate;
                //tightLength -= tightenRate;
                tightenRope();
            } else {
                rope.winchOffset -= tightLength;
                if (rope.baseExtention > 0)
                    rope.winchOffset -= rope.winchUnit * rope.baseExtention;
                looseLength = 0;
                tightLength = 0;
                rope.stiff();
                tighten = false;
                Debug.Log("stiff");
            }
        } 
    }

    public void OnSubUpdate() {
        autoHookPos.x = anchor.attachPoint.x + anchor.orientation.x * (rope.segmentLength * hookLag * rope.activeSegments);
		autoHookPos.y = anchor.attachPoint.y + anchor.orientation.y * (rope.segmentLength * hookLag * rope.activeSegments);
    }

    public void ApplyConstraints() {
        //constrain hook while auto extending
        if (rope.extended && rope.autoExtend) {
            SegmentConstraint.pointConstraint(autoHookPos, hookSegment, false);
            SegmentConstraint.angleConstraint(anchor.anchorSegment, hookSegment, 0);
        }

        //constrain hook while tightening rope
        if (tightLength > 0) {
            //SegmentConstraint.pointConstraint(hookSnapshot, hookSegment, false);
            //SegmentConstraint.angleConstraint(anchor.anchorSegment, hookSegment, 0);
        }

        //if (isHooked)
          //  SegmentConstraint.pointConstraint(hookSnapshot, hookSegment, false);
    }

    private Vector3 hookPosition = Vector3.zero;

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

    Vector2 real = Vector2.zero;
    Vector2 complex = Vector2.zero;

    private Vector2d hook2Base = Vector2d.zero;
    private void tightenRope() {
        hook2Base.x = hookSegment.p2.x - anchor.attachPoint.x;
        hook2Base.y = hookSegment.p2.y - anchor.attachPoint.y;

        looseLength = (rope.baseExtention + rope.activeSegments - 1) * rope.segmentLength;
        tightLength = looseLength - hook2Base.magnitude;
        Debug.Log("tight " + tightLength + " loose " + looseLength);
    }
}
