using System;
using UnityEngine;

public enum RopeMode {
    STIFF,
    FLEXIBLE
}

[RequireComponent(typeof(Rope))]
[RequireComponent(typeof(ExtendRopeNoIn))]
public class PlayerRopeNoIn : MonoBehaviour, RopeBehaviour {
    private Rope rope;
    private ExtendRopeNoIn extender;

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
    private TightenRope tighty = new TightenRope();

	public void stiff() {
        rope.configure(3, .98, .98, 6, 1);
        for (int i = (rope.segments?.Length ?? 0) - 1; i >= 0; i--) {
            rope.setAngleConstraint(_stiffAngle, i);
        }
        mode = RopeMode.STIFF;
    }

    public void flexible() {
        rope.configure(35, 0, .95, 25, .1);
        rope.tightEnd = false;
        for (int i = (rope.segments?.Length ?? 0) - 1; i >= 0; i--) {
            rope.setAngleConstraint(_flexAngle, i);
        }
        mode = RopeMode.FLEXIBLE;
    }

    private void Awake() {
        this.rope = GetComponent<Rope>();
        this.extender = GetComponent<ExtendRopeNoIn>();
    }

    private void Start() {
        rope.buildRope(new Segment(Vector2d.zero, Vector2d.up, segmentLength), segmentCount);

        anchor.substeps = rope.substeps;

        extender.addAutoExtendStartCallback(() => {
            flexible();
		    rope.maxSpeed = 40;

            anchor.velocityCorrection = 1;
		    hook.active = true;
        });

        extender.addAutoExtendEndCallback(() => {
            anchor.velocityCorrection = 0;
            anchor.mass = 1;
            anchor.inertia = .05;
		    rope.maxSpeed = 25;
        });

        extender.addAutoRetractStartCallback(() => {
            hook.active = false;
		    hook.unHook();

            anchor.mass = double.PositiveInfinity;
            anchor.inertia = double.PositiveInfinity;

		    flexible();
		    rope.angulerDrag = .995;
		    rope.maxSpeed = 30;
        });

        extender.addAutoRetractEndCallback(() => {
            flexible();
        });
    }

    /*public void ApplyConstraints(){
        base.ApplyConstraints();
    }*/

    public void OnUpdate() {
        anchor.correctVelocity(rope);
        
        if (tighten)
            tighty.rotateToRope(rope, anchor);
    }

    public void OnSubUpdate() {
        anchor.setAngleLimit(rope.angleLimitRadians * extender.baseExtention);
        anchor.setAttachSegment(rope.baseSegment > -1 ? rope.segments[rope.baseSegment] : null);
        anchor.setOffset(0, .00000001 + extender.winchOffset);
        anchor.subLinearDrag = rope.subLinearDrag;
        anchor.subAngulerDrag = rope.subAngulerDrag;

        extender.setInactivePosition(anchor.position.x, anchor.position.y);
        extender.setInactiveOrientation(anchor.orientation.x, anchor.orientation.y);

        if (tighten)
            tighten = tighty.execute(rope, anchor, extender, this);
    }

    void OnValidate() {
        /*if (mode == RopeMode.STIFF)
            stiff();
        else
            flexible();*/
    }

    private Vector2 pGiz1 = new Vector3(0, 0, 0);
	private Vector2 pGiz2 = new Vector3(0, 0, 0);

    private void OnDrawGizmos() {
        if (!Application.isPlaying) 
			return;
        
        for (int i = 1; i < rope.segments.Length; i++) {
			Color modeColor = rope.getAngleConstraint(i) == 3 ? Color.magenta : Color.cyan;
			Gizmos.color = i % 2 == 1 ? modeColor : Color.yellow;


			pGiz1.x = (float)rope.segments[i].p1.x;
			pGiz1.y = (float)rope.segments[i].p1.y;
			pGiz2.x = (float)rope.segments[i].p2.x;
			pGiz2.y = (float)rope.segments[i].p2.y;

			Gizmos.DrawLine(pGiz1, pGiz2);
		}

        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(new Vector2((float)segments[0].p2.x, (float)segments[0].p2.y), 
        //                new Vector2((float)anchor.attachPoint.x, (float)anchor.attachPoint.y));
    }
}
