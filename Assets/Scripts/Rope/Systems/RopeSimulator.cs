/*
 * A base class which defines all of the methods needed to simulate, and apply constraints to, the segments of a 
 * rope.
 * 
 * This class does not define the actual segments of the rope.  To do so, inherit this class define them in the 
 * sub class.
 */
public abstract class RopeSimulator {
	protected Rope rope;
	protected Segment[] segments;
	protected double h;

	public RopeSimulator(Rope rope) {
		this.rope = rope;
		this.segments = rope.segments;
		this.h = rope.h;
	}

	protected virtual void mainLoop() {
		for (int i = 0; i < rope.substeps; i++) {
			OnSubUpdate();
			Simulate();
			ApplyConstraints();
			adjustVelocities();
			solveVelocities();
		}
	}

	private bool OnUpdate() { return false; }

	protected virtual void OnSubUpdate() {}
	protected virtual void ApplyConstraints() {}
	private bool OnUpdateLate() { return false; }

	/*
	 * Integrate position and orientation of segments by timestep of h
	 */
	private void Simulate() {
		for (int i = rope.baseSegment; i >= 0; i--) {
			Segment segment = rope.segments[i];
			
			segment.previousPosition.x = segment.position.x;
			segment.previousPosition.y = segment.position.y;
			segment.setPosition(segment.position.x + h * segment.velocity.x, segment.position.y + h * segment.velocity.y);

			segment.previousOrientation.x = segment.orientation.x;
			segment.previousOrientation.y = segment.orientation.y;
			Segment.rotate(segment, segment.angulerVelocity * h);
		}
	}
	
	/*
	 * Updates the velocities after constraints have been applied
	 */
	private void adjustVelocities() {
		for (int i = rope.activeSegments - 1; i >= 0; i--) {
			segments[i].velocity.x = (segments[i].position.x - segments[i].previousPosition.x) / h;
			segments[i].velocity.y = (segments[i].position.y - segments[i].previousPosition.y) / h;
			segments[i].angulerVelocity = Vector2d.SignedAngle(segments[i].previousOrientation, segments[i].orientation) / h;
		}
	}

	protected Vector2d linearDiff = Vector2d.zero;
	protected double angulerDiff = 0;

	/*
	 * Clamps the magnitude and applies drag to all active segments
	 */
	private void solveVelocities() {
		clampMags();
		applyDrag();
	}

	protected void clampMags() {
		for (int i = rope.activeSegments - 1; i >= 0; i--) {
			double mag = segments[i].velocity.magnitude;
			double clampedMag = System.Math.Min(mag, rope.maxSpeed * System.Math.Pow(rope.activeSegments - i, rope.maxSpeedScale));

			if (mag > 0) {
				segments[i].velocity.x = segments[i].velocity.x / mag * clampedMag;
				segments[i].velocity.y = segments[i].velocity.y / mag * clampedMag;
			}
		}
	}

	protected virtual void applyDrag() {
		for (int i = rope.activeSegments - 1; i >= 1; i--) {
			Segment s1 = segments[i];
			Segment s2 = segments[i - 1];

			double linearRatio = s1.inverseMass / (s1.inverseMass + s2.inverseMass);
			double angulerRatio = s1.inverseInertia / (s1.inverseInertia + s2.inverseInertia);

			linearDiff.x = (s2.velocity.x - s1.velocity.x) * rope.subLinearDrag;
			linearDiff.y = (s2.velocity.y - s1.velocity.y) * rope.subLinearDrag;
			angulerDiff = (s2.angulerVelocity - s1.angulerVelocity) * rope.subAngulerDrag;
				
			s1.velocity.x += linearDiff.x * linearRatio;
			s1.velocity.y += linearDiff.y * linearRatio;
			s1.angulerVelocity += angulerDiff * angulerRatio;

			s2.velocity.x -= linearDiff.x * (1 - linearRatio);
			s2.velocity.y -= linearDiff.y * (1 - linearRatio);
			s2.angulerVelocity -= angulerDiff * (1 - angulerRatio);
		}
	}
}

public interface RopeBehaviour {
	public void OnUpdate() {}
	public void OnSubUpdate() {}
	public void ApplyContraints() {}
	public void OnUpdateLate() {}
}