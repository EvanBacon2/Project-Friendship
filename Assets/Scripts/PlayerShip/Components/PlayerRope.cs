using System;
using UnityEngine;

/*public enum RopeMode {
    STIFF,
    FLEXIBLE
}*/

public class PlayerRope : ExtendableRope {
	public RopeMode mode;
    public int segmentCount;
    public double segmentLength;
    public Anchor anchor;
    public Hook hook;

    public double stiffAngle { get { return _stiffAngle; } }
    public double flexAngle { get { return _flexAngle; } }

    private double _stiffAngle = 3;
    private double _flexAngle = 35;

    public bool tighten = false;
    //private TightenRope tighty = new TightenRope();

	public void stiff() {
        configure(3, .98, .98, 6, 1);
        for (int i = (segments?.Length ?? 0) - 1; i >= 0; i--) {
            _angleConstraints[i] = _stiffAngle;
        }
        mode = RopeMode.STIFF;
    }

    public void flexible() {
        configure(35, 0, .95, 25, .1);
        tightEnd = false;
        for (int i = (segments?.Length ?? 0) - 1; i >= 0; i--) {
            _angleConstraints[i] = _flexAngle;
        }
        mode = RopeMode.FLEXIBLE;
    }

    protected override void start() {
        base.start();
        buildRope(new Segment(Vector2d.zero, Vector2d.up, segmentLength), segmentCount);

        anchor.substeps = substeps;

        addAutoExtendStartCallback(() => {
            flexible();
		    maxSpeed = 40;

            anchor.velocityCorrection = 1;
		    hook.active = true;
        });

        addAutoExtendEndCallback(() => {
            anchor.velocityCorrection = 0;
            anchor.mass = 1;
            anchor.inertia = .05;
		    maxSpeed = 25;
        });

        addAutoRetractStartCallback(() => {
            hook.active = false;
		    hook.unHook();

            anchor.mass = double.PositiveInfinity;
            anchor.inertia = double.PositiveInfinity;

		    flexible();
		    angulerDrag = .995;
		    maxSpeed = 30;
        });

        addAutoRetractEndCallback(() => {
            flexible();
        });
    }

    public override void ApplyConstraints(){
        base.ApplyConstraints();
    }

    public void OnUpdate() {
        //base.OnUpdate();
        anchor.correctVelocity(this);
        
        //if (tighten)
        //    tighty.rotateToRope(this, anchor);
    }

    public void OnSubUpdate() {
        //base.OnSubUpdate();
        anchor.setAngleLimit(this.angleLimitRadians * this.baseExtention);
        anchor.setAttachSegment(baseSegment > -1 ? segments[baseSegment] : null);
        anchor.setOffset(0, .00000001 + winchOffset);
        anchor.subLinearDrag = subLinearDrag;
        anchor.subAngulerDrag = subAngulerDrag;

        setInactivePosition(anchor.position.x, anchor.position.y);
        setInactiveOrientation(anchor.orientation.x, anchor.orientation.y);

        //if (tighten)
        //    tighten = tighty.execute(this);
    }

    public override void OnUpdateLate() {
        base.OnUpdateLate();
    }

    protected override void onValidate() {
        base.onValidate();

        if (mode == RopeMode.STIFF)
            stiff();
        else
            flexible();
    }

    private Vector2 pGiz1 = new Vector3(0, 0, 0);
	private Vector2 pGiz2 = new Vector3(0, 0, 0);

    private void OnDrawGizmos() {
        if (!Application.isPlaying) 
			return;
        
        for (int i = 1; i < segments.Length; i++) {
			Color modeColor = _angleConstraints[i] == 3 ? Color.magenta : Color.cyan;
			Gizmos.color = i % 2 == 1 ? modeColor : Color.yellow;


			pGiz1.x = (float)segments[i].p1.x;
			pGiz1.y = (float)segments[i].p1.y;
			pGiz2.x = (float)segments[i].p2.x;
			pGiz2.y = (float)segments[i].p2.y;

			Gizmos.DrawLine(pGiz1, pGiz2);
		}

        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(new Vector2((float)segments[0].p2.x, (float)segments[0].p2.y), 
        //                new Vector2((float)anchor.attachPoint.x, (float)anchor.attachPoint.y));
    }
}
