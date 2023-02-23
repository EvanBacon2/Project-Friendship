using UnityEngine;

/*
 * Defines the ExtendableRope used by the player
 */
public class HookSystem {
    /*public int segmentCount;
    public double length;

    public Hook hook;
	//private HookSegment hookSegment;
    private double hookLag = .92;
    private Vector2d hookSnapshot = Vector2d.zero;

    //private Vector2d winchPosition = Vector2d.zero;//replace with set offset
    private Vector2d hookPosition = Vector2d.zero;
    
    private Vector3 hookPos = Vector3.zero;
    private Quaternion hookRotation = new Quaternion();
    private Vector3 hookVelocity = Vector3.zero;

    private double tightLength = 0;
    private double looseLength = 0;

    private PlayerRope rope;
    private Anchor anchor;*?

    /*public void OnStartState(object sender, RopeHookState state) {
        state.hook.addHookedCallback(() => {
            tightenRope();
            hookSnapshot.x = state.rope.segments[0].position.x;
            hookSnapshot.y = state.rope.segments[0].position.y;
        });
    }*/

    public void OnStateReceived(object sender) {
        //hook = state.hook;
        //rope = state.rope;
    }

    protected void StartCallbackLate() {

        //ship = new RopeAnchor(shipRB, double.PositiveInfinity, double.PositiveInfinity, new Vector2(0, .65f), substeps); 

        /*GameObject hookObj = Instantiate(hook, new Vector3((float)rope[0].position.x, (float)rope[0].position.y, 0), Quaternion.identity);
		hookObj.AddComponent<HookSegment>();
		hookSegment = hookObj.GetComponent<HookSegment>();
		hookSegment.s = rope[0];*/

        /*ship.addInterpolationCallback((interOrientation, interPosition) => {
            winchPosition.x = ship.anchorSegment.p2.x + (ship.anchorSegment.orientation.x * winchOffset);
		    winchPosition.y = ship.anchorSegment.p2.y * (ship.anchorSegment.orientation.y * winchOffset);
        });
        
        ship.addInterpolationCallback((interOrientation, interPosition) => {
            hookPosition.x = rope[baseSegment].p1.x + ship.anchorSegment.orientation.x * (length * hookLag * activeSegments);
		    hookPosition.y = rope[baseSegment].p1.y + ship.anchorSegment.orientation.y * (length * hookLag * activeSegments);
        });*/
    }

    /*public override void OnUpdate() {
        hookPos.Set((float)hookPosition.x, (float)hookPosition.y, hookPos.z);
        float angle = Mathf.Atan2((float)rope.segments[0].orientation.y, (float)rope.segments[0].orientation.x) * Mathf.Rad2Deg - 90;
		hookRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        hookVelocity.Set((float)rope.segments[0].velocity.x, (float)rope.segments[0].velocity.y, 0);

        hook.updateHook(hookPos, hookRotation, hookVelocity);

        //deactivate hook when rope isn't extended
        if (!rope.extended) {
			hook.active = false;
			hook.unHook();
		}

        //tighten rope
        if (tightLength > 0) {
            if (looseLength - tightLength > rope.winchForce * .25) {
                rope.winchOffset -= rope.winchForce * .25;
                looseLength -= rope.winchForce * .25;
            }
            else {
                rope.winchOffset -= looseLength - tightLength;
                looseLength = 0;
                tightLength = 0;
                rope.stiff();
            }
        }

        anchor.setOffset(0, rope.winchOffset);
    }*/

    /*public override void OnSubUpdate() {
        hookPosition.x = rope.segments[rope.baseSegment].p1.x + anchor.orientation.x * (length * hookLag * rope.activeSegments);
		hookPosition.y = rope.segments[rope.baseSegment].p1.y + anchor.orientation.y * (length * hookLag * rope.activeSegments);
    }

    public override void ApplyConstraints() {
        //constrain hook while auto extending
        if (rope.extended && rope.autoExtend)
            SegmentConstraint.pointConstraint(hookPosition, rope.segments[0], false);

        //constrain hook while tightening rope
        if (tightLength > 0)
            SegmentConstraint.pointConstraint(hookSnapshot, rope.segments[0], false);
    }*/

    /*protected override void onAutoExtendStart(){
        modeFlexible();
		maxSpeed = 40;

        shipCorrection = 1;
		hookSegment.active = true;
    }

    protected override void onAutoExtendEnd() {
        shipCorrection = 0;
		maxSpeed = 25;
    }

    protected override void onAutoRetractStart() {
        hookSegment.active = false;
		hookSegment.unHook();

		modeFlexible();
		angulerDrag = .99;
		maxSpeed = 55;
    }

    protected override void onAutoRetractEnd() {
        modeFlexible();
    }*/

    /*private Vector2d hook2Base = Vector2d.zero;
    private void tightenRope() {
        hook2Base.x = rope.segments[0].position.x - anchor.attachPoint.x;
        hook2Base.y = rope.segments[0].position.y - anchor.attachPoint.y;

        looseLength = rope.activeSegments * length;
        tightLength = hook2Base.magnitude;
    }*/
}
