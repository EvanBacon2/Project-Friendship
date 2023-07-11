using UnityEngine;

public class PlayerRopeHook : Hook, RopeBehaviour {
    //public PlayerRope rope;
    public Rope rope;
    public ExtendRopeNoIn extender;
    public PlayerRopeNoIn playerRope;

    public Anchor anchor;
    public int hookIndex;
    public double hookLag;

    private Segment hookSegment { get { return rope.segments[hookIndex]; } }
    private Vector2d autoHookPos = Vector2d.zero;//position hookSegment is constrained to during autoExtend

    private bool constrainHook = false;

    protected override void start() {
        addHookedCallback(() => {
            //rope.autoExtend = false;
            Debug.Log("///////////////hooked");
            constrainHook = false;
            rope.configure(rope.angleLimitDegrees, .97, .98, rope.maxSpeed, rope.maxSpeedScale);
            hookSegment.mass = 500;
            hookSegment.inertia = 500;
            hookSegment.velocity.x /= 500;
            hookSegment.velocity.y /= 500;
            hookSegment.previousPosition.x = hookSegment.position.x - hookSegment.velocity.x;
            hookSegment.previousPosition.y = hookSegment.position.y - hookSegment.velocity.y;
        });

        addUnHookedCallback(() => {
            hookSegment.mass = 1;
        });

        extender.addAutoExtendStartCallback(() => {
            constrainHook = true;
        });
    }

    public void OnUpdate() {
        //deactivate hook when rope isn't extended
        if (!extender.extended && !extender.autoExtend) {
		    active = false;
			unHook();
		}   
    }

    public void OnSubUpdate() {
        autoHookPos.x = anchor.attachPoint.x + anchor.orientation.x * (playerRope.segmentLength * hookLag * rope.activeSegments);
		autoHookPos.y = anchor.attachPoint.y + anchor.orientation.y * (playerRope.segmentLength * hookLag * rope.activeSegments);
    }

    public void ApplyConstraints() {
        //constrain hook while auto extending
        if (extender.extended && extender.autoExtend && constrainHook) {
            SegmentConstraint.pointConstraint(autoHookPos, hookSegment, false);
            SegmentConstraint.angleConstraint(anchor.anchorSegment, hookSegment, 0);
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
