
using System;
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

    private double _stiffAngle = 3;
    private double _flexAngle = 35;

    private bool hookBoost;

	public void stiff() {
        configure(3, .98, .98, 6, 1);
        for (int i = (segments?.Length ?? 0) - 1; i >= 0; i--) {
            _angleConstraints[i] = _stiffAngle;
        }
        mode = RopeMode.STIFF;
    }

    public void flexible() {
        configure(35, 0, .95, 25, .1);
        for (int i = (segments?.Length ?? 0) - 1; i >= 0; i--) {
            _angleConstraints[i] = _flexAngle;
        }
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

            hookBoost = true;
        });

        addAutoExtendEndCallback(() => {
            anchor.velocityCorrection = 0;
            anchor.mass = 1;
            anchor.inertia = .05;
		    maxSpeed = 25;

            segments[0].mass = 1;
            hookBoost = false;
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

    public override void OnUpdateLate() {
        base.OnUpdateLate();

        if (hookBoost && activeSegments != 0) {
            float rbAngV = anchor.rb.AngularVelocity.pendingValue().z;

            float shipAngle = 90 + (anchor.rb.Rotation.pendingValue().eulerAngles.z + rbAngV * Time.fixedDeltaTime);

            segments[0].velocity.x = anchor.rb.Velocity.value.x + 4000 * Math.Cos(shipAngle * Mathf.Deg2Rad);
            segments[0].velocity.y = anchor.rb.Velocity.value.y + 4000 * Math.Sin(shipAngle * Mathf.Deg2Rad);

            segments[0].previousPosition.x = segments[0].position.x - segments[0].velocity.x;
            segments[0].previousPosition.x = segments[0].position.y - segments[0].velocity.y;
            segments[0].mass = 100000;

            Debug.Log("hookBoost");

            //if (activeSegments > 15)
              //  hookBoost = false;
        }
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

		for (int i = 0; i < segments.Length; i++) {
			Color modeColor = _angleConstraints[i] == 3 ? Color.red : Color.green;
			Gizmos.color = i % 2 == 0 ? modeColor : Color.white;


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
