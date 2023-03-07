using UnityEngine;

public enum RopeMode {
    STIFF,
    FLEXIBLE
}

public class PlayerRope : ExtendableRope {
	public RopeMode mode;
    public int segmentCount;
    public double segmentLength;
    public Anchor anchor;
    public Hook hook;

	public void stiff() {
        configure(3, .98, .98, 6, 1);
        mode = RopeMode.STIFF;
    }

    public void flexible() {
        configure(35, 0, .95, 25, .1);
        mode = RopeMode.FLEXIBLE;
    }

    protected override void start() {
        base.start();
        buildRope(new Segment(Vector2d.zero, Vector2d.up, segmentLength), segmentCount);

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

    public override void OnSubUpdate() {
        base.OnSubUpdate();
        setInactivePosition(anchor.position.x, anchor.position.y);
        setInactiveOrientation(anchor.orientation.x, anchor.orientation.y);
    }

    protected override void onValidate() {
        base.onValidate();

        if (mode == RopeMode.STIFF)
            stiff();
        else
            flexible();
    }
}
