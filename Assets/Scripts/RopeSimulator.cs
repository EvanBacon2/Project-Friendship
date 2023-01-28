using UnityEngine;

/*
 * A base class which defines all of the methods needed to simulate, and apply constraints to, the segments of a 
 * rope.
 * 
 * This class does not define the actual segments of the rope.  To do so, inherit this class define them in the 
 * sub class.
 */
public abstract class RopeSimulator : MonoBehaviour {
	public double angleLimit;//max difference, in degrees, between the orientations, in relation to the origin, of two segments
	public double stiffness;//Complience of angle constraint
	public double stretchiness;//Complience of distance constraint
	public double maxSpeed = 6;//max speed of baseSegment, maxSpeed for each subsequent segment increases by a factor determined by maxSpeedScale
	public double maxSpeedScale = 1;//1 == linear, >1 == exponential, <1 == logarithmic
	public double linearDrag;
	public double angulerDrag;

	protected double _angleLimit;
	protected double _linearDrag;
	protected double _angulerDrag;

	public int substeps;
	protected double h;

	protected Segment[] rope;
	protected int activeSegments = 0;
	protected int baseSegment = -1;
	
	protected void setAngleLimit(double angleLimit) {
		_angleLimit = angleLimit * Mathf.Deg2Rad;
	}

	protected void setLinearDrag(double linearDrag) {
		_linearDrag = System.Math.Pow(linearDrag, substeps);
	}

	protected void setAngulerDrag(double angulerDrag) {
		_angulerDrag = System.Math.Pow(angulerDrag, substeps);
	}

	void Start() {
		h = Time.fixedDeltaTime / substeps;

		rope = buildRope();
		configure(this.angleLimit, this.linearDrag, this.angulerDrag);
	}

	protected abstract Segment[] buildRope();

	protected void configure(double angleLimit, double linearDrag, double angulerDrag) {
		setAngleLimit(angleLimit);
		setLinearDrag(linearDrag);
		setAngulerDrag(angulerDrag);
	}

	void FixedUpdate() {
		for (int i = 0; i < substeps; i++) {
			Simulate();
			ApplyConstraints();
			adjustVelocities();
			solveVelocities();
		}
	}

	//integrate position and orientation of segments by timestep of h
	private void Simulate() {
		for (int i = baseSegment; i >= 0; i--) {
			Segment segment = rope[i];
			
			segment.previousPosition.x = segment.position.x;
			segment.previousPosition.y = segment.position.y;
			segment.setPosition(segment.position.x + h * segment.velocity.x, segment.position.y + h * segment.velocity.y);

			segment.previousOrientation.x = segment.orientation.x;
			segment.previousOrientation.y = segment.orientation.y;
			Segment.rotateOrientation(segment, segment.angulerVelocity * h);
		}
	}
	
	//apply various positional and anguler constraints
	private void ApplyConstraints() {
		//constrain extended segments between base and hook
		for (int i = baseSegment; i >= 1; i--) {
			SegmentConstraint.distanceConstraint(rope[i], rope[i - 1]);
			SegmentConstraint.angleConstraint(rope[i], rope[i - 1], _angleLimit);
		}

		//constrain inactive segments
		for (int i = activeSegments; i < rope.Length; i++) {
			inactiveConstraint(rope[i]);
		}
	}

	/*
	 * A constraint that is applied to any inactive segment
	 */
	protected virtual void inactiveConstraint(Segment segment) {
		
	}
	
	/*
	 * Updates the velocities after constraints have been applied
	 */
	private void adjustVelocities() {
		for (int i = activeSegments - 1; i >= 0; i--) {
			rope[i].velocity.x = (rope[i].position.x - rope[i].previousPosition.x) / h;
			rope[i].velocity.y = (rope[i].position.y - rope[i].previousPosition.y) / h;
			rope[i].angulerVelocity = Vector2d.SignedAngle(rope[i].previousOrientation, rope[i].orientation) / h;
		}
	}

	Vector2d linearDiff = Vector2d.zero;
	double angulerDiff = 0;

	/*
	 * Clamps the magnitude and applies drag to all active segments
	 */
	private void solveVelocities() {
		clampMags();
		applyDrag();
	}

	protected void clampMags() {
		for (int i = activeSegments - 1; i >= 0; i--) {
			double mag = rope[i].velocity.magnitude;
			double clampedMag = System.Math.Min(mag, maxSpeed * System.Math.Pow(activeSegments - i, maxSpeedScale));

			if (mag > 0) {
				rope[i].velocity.x = rope[i].velocity.x / mag * clampedMag;
				rope[i].velocity.y = rope[i].velocity.y / mag * clampedMag;
			}
		}
	}

	protected void applyDrag() {
		for (int i = activeSegments - 1; i >= 0; i--) {
			dampJoint(rope[i], rope[i - 1]);
		}
	}

	/*
	 * Applies linear and anguler drag to the joint formed by Segments s1 and s2.
	 */
	protected void dampJoint(Segment s1, Segment s2) {
		double linRatio = s1.inverseMass / (s1.inverseMass + s2.inverseMass);
		double angRatio = s1.inverseInertia / (s1.inverseInertia + s2.inverseInertia);

		linearDiff.x = (s2.velocity.x - s1.velocity.x) * _linearDrag;
		linearDiff.y = (s2.velocity.y - s1.velocity.y) * _linearDrag;
		angulerDiff = (s2.angulerVelocity - s1.angulerVelocity) * _angulerDrag;
			
		s1.velocity.x += linearDiff.x * linRatio;
		s1.velocity.y += linearDiff.y * linRatio;
		s1.angulerVelocity += angulerDiff * angRatio;

		s2.velocity.x -= linearDiff.x * (1 - linRatio);
		s2.velocity.y -= linearDiff.y * (1 - linRatio);
		s2.angulerVelocity -= angulerDiff * (1 - angRatio);
	}

	private Vector2 giz1 = new Vector3(0, 0, 0);
	private Vector2 giz2 = new Vector3(0, 0, 0);

	private void OnDrawGizmos() {
		if (!Application.isPlaying) 
			return;

		for (int i = 0; i < rope.Length; i++) {
			Gizmos.color = i % 2 == 0 ? Color.green : Color.white;

			giz1.x = (float)rope[i].p1.x;
			giz1.y = (float)rope[i].p1.y;
			giz2.x = (float)rope[i].p2.x;
			giz2.y = (float)rope[i].p2.y;

			Gizmos.DrawLine(giz1, giz2);
		}
	}
}
