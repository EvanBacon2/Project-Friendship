using UnityEngine;

public class PlayerRope : ExtendableRope {
	public RopeMode mode;
    public int segmentCount;
    public double segmentLength;
    public Anchor anchor;
    public Hook hook;

	public void stiff() {
        configure(6, .98, .98, 6, 1);
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
		    maxSpeed = 25;
        });

        addAutoRetractStartCallback(() => {
            hook.active = false;
		    hook.unHook();

		    flexible();
		    angulerDrag = .99;
		    maxSpeed = 55;
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
