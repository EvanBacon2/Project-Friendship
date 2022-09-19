using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SegmentRope {
	class Vector2d {
		public double x;
		public double y;

		public double magnitude { get { return System.Math.Sqrt(x * x + y * y); } }
		public double sqrmagnitude { get { return x * x + y * y; } }
		public Vector2d normalized { 
			get {
				if (magnitude > 0)
					return new Vector2d(x / magnitude, y / magnitude);
				else 
					return zero;
			} 
		}

		public Vector2d(double x, double y) {
			this.x = x;
			this.y = y;
		}

		public static Vector2d zero { get { return new Vector2d(0, 0); } }
		public static Vector2d up { get { return new Vector2d(0, 1); } }
		public static Vector2d down { get { return new Vector2d(0, -1); } }
		public static Vector2d right { get { return new Vector2d(1, 0); } }
		public static Vector2d left { get { return new Vector2d(-1, 0); } }

		public static Vector2d operator +(Vector2d v1, Vector2d v2) => new Vector2d(v1.x + v2.x, v1.y + v2.y);
		public static Vector2d operator -(Vector2d v1, Vector2d v2) => new Vector2d(v1.x - v2.x, v1.y - v2.y);
		public static Vector2d operator -(Vector2d v, double d) => new Vector2d(v.x - d, v.y - d);
		public static Vector2d operator -(double d, Vector2d v) => new Vector2d(v.x - d, v.y - d);

		public static Vector2d operator *(Vector2d v, double d) => new Vector2d(v.x * d, v.y * d);
		public static Vector2d operator *(double d, Vector2d v) => new Vector2d(v.x * d, v.y * d);
		public static Vector2d operator *(Vector2d v, int i) => new Vector2d(v.x * i, v.y * i);
		public static Vector2d operator *(int i, Vector2d v) => new Vector2d(v.x * i, v.y * i);

		public static Vector2d operator /(Vector2d v, double d) => new Vector2d(v.x / d, v.y / d);
		public static Vector2d operator /(double d, Vector2d v) => new Vector2d(v.x / d, v.y / d);
		public static Vector2d operator /(Vector2d v, int i) => new Vector2d(v.x / i, v.y / i);
		public static Vector2d operator /(int i, Vector2d v) => new Vector2d(v.x / i, v.y / i);

		public static double dot(Vector2d lhs, Vector2d rhs) {
			return lhs.x * rhs.x + lhs.y * rhs.y;
		}

		public static double cross(Vector2d lhs, Vector2d rhs) {
			return lhs.x * rhs.y - lhs.y * rhs.x;
		}

		public static double SignedAngle(Vector2d from, Vector2d to) {
			double dot = Vector2d.dot(from, to);
			double cross = Vector2d.cross(from, to);
			double denominator = System.Math.Sqrt(from.sqrmagnitude * to.sqrmagnitude);
			if (denominator < 1e-15) return 0;
			double cos = System.Math.Clamp(dot / denominator, -1, 1);
			return System.Math.Acos(cos) * System.Math.Sign(cross);
		}

		public override string ToString() {
			return "(" + x + ", " + y + ")";
		}
	}

	class Segment {
		public Vector2d _position;
		public Vector2d previousPosition;
		public Vector2d velocity;
		public double mass;

		private Vector2d _orientation;//should be a unit vector
		public Vector2d previousOrientation;
		public double angulerVelocity;
		public double inertia;

		public double length;
		private double halfLength;

		private Vector2d _p1;
		private Vector2d _p2;

		public Vector2d position {
			get { return _position; }
			set {
				_position = value;
				
				_p1.x = value.x - _orientation.x * length;
				_p1.y = value.y - _orientation.y * length;
				_p2.x = value.x + _orientation.x * length;
				_p2.y = value.y + _orientation.y * length;
			}
		}

		public Vector2d p1 { 
			get { return _p1; }
			set {
				_position.x = value.x + _orientation.x * halfLength;
				_position.y = value.y + _orientation.y * halfLength;
				_p1.x = value.x;
				_p1.y = value.y;
				_p2.x = value.x + _orientation.x * length;
				_p2.y = value.y + _orientation.y * length;
			}
		}
		public Vector2d p2 { 
			get { return _p2; }
			set {
				_position = value - _orientation * halfLength;
				_p1.x = value.x - _orientation.x * length;
				_p1.y = value.y - _orientation.y * length;
				_p2.x = value.x;
				_p2.y = value.y;
			}
		}

		public Vector2d orientation {
			get { return _orientation; }
			set {
				_orientation = value;
				_p1.x = position.x - orientation.x * halfLength;
				_p1.y = position.y - orientation.y * halfLength;
				_p2.x = position.x + orientation.x * halfLength;
				_p2.y = position.y + orientation.y * halfLength;
			}
		}

		public Segment(Vector2d position, Vector2d orientation, double mass, double inertia, double length) {
			this._p1 = position - orientation * halfLength;
			this._p2 = position + orientation * halfLength;

			this._position = position;
			this.previousPosition = position;
			this.velocity = new Vector2d(0, 0);
			this.orientation = orientation;
			this.previousOrientation = orientation;
			this.mass = mass;
			this.inertia = inertia;
			this.length = length;
			this.halfLength = length / 2;
		}
	}

	public enum RopeMode {
		FLEXIBLE,
		STIFF,
	}

	public class SegmentRope : MonoBehaviour {
		public int maxSegments = 35;
		public double length = .65;
		public double angleLimit;
		public double mass = 1;
		public double inertia = 1;
		public double stiffness = 1;
		public double maxSpeed = 6;
		public double linearDrag;
		public double angulerDrag;

		private double[] maxSpeeds = new double[] {3, 3, 4, 6};

		public int substeps = 18;
		public int iterations = 1;
		private double h;

		private Segment[] rope;
		private int extendedSegments = 0;
		private int baseSegment = -1;
		private RopeMode mode = RopeMode.FLEXIBLE;

		private Vector2d baseOrientation = Vector2d.zero;
		private Vector2d basePosition = Vector2d.zero;
		private Vector2d winchPosition = Vector2d.zero;
		private Vector2d hookPosition = Vector2d.zero;

		private double winchForce = 3.5;
		private double winchOffset = 0;
		private double winchScrollBuffer = 0;
		private bool extended = false;
		private bool autoExtend = false;
		private bool autoRetract = false;

		private double baseOffset = .65;

		private Rigidbody shipRigidbody;
		private double shipCorrection = .4;

		private void Start() {
			shipRigidbody = transform.parent.GetComponent<Rigidbody>();
			maxSpeed = maxSpeeds[Mathf.Clamp(extendedSegments, 0, 3)];
			h = Time.fixedDeltaTime / substeps;

			updateBaseOrientation();
			updateBasePosition();
			updateWinchPosition();
			updateHookPosition();

			rope = new Segment[maxSegments];
			for (int i = 0; i < maxSegments; i++) {
				rope[i] = new Segment(new Vector2d(basePosition.x, basePosition.y), Vector2d.up, mass, inertia, length);
			}

			modeFlexible();
		}

		private void modeStiff() {
			angleLimit = 6;
			linearDrag = .9985;
			angulerDrag = .985;
		}

		private void modeFlexible() {
			angleLimit = 35;
			linearDrag = .9998;
			angulerDrag = .995;
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.F)) {
				if (mode == RopeMode.FLEXIBLE) {
					modeStiff();
					mode = RopeMode.STIFF;
				} else {
					modeFlexible();
					mode = RopeMode.FLEXIBLE;
				}
			}

			if (Input.GetMouseButtonDown(1) && !extended && !autoRetract) {
				autoExtend = true;
				shipCorrection = 1;
			}

			if (Input.GetMouseButtonDown(1) && extended && !autoExtend) 
				autoRetract = true;
			
			if (extended && Input.mouseScrollDelta.y != 0 && winchScrollBuffer == 0 && (extendedSegments < maxSegments || Input.mouseScrollDelta.y < 0)) 
				winchScrollBuffer = 4 * System.Math.Sign(Input.mouseScrollDelta.y);
		}

		private void FixedUpdate() {
			updateBaseOrientation();
			updateBasePosition();
			updateWinchPosition();
			updateHookPosition();

			for (int i = baseSegment; i >= 0; i--) {
				rope[i].position.x += shipRigidbody.velocity.x * Time.fixedDeltaTime * shipCorrection;
				rope[i].position.y += shipRigidbody.velocity.y * Time.fixedDeltaTime * shipCorrection;
			}

			for (int i = 0; i < substeps; i++) {

				Simulate();
				for (int j = 0; j < iterations; j++) {
					ApplyConstraints();
				}
				adjustVelocities();
				solveVelocities();
			}

			if (winchScrollBuffer != 0) {
				adjustWinch(length / 4 * System.Math.Sign(winchScrollBuffer));
				winchScrollBuffer -= winchScrollBuffer > 0 ? 1 : -1;
			}

			if (autoExtend) {
				adjustWinch(winchForce);

				if (extendedSegments == maxSegments) {
					autoExtend = false;
					shipCorrection = .4;
				}
			}
			
			if (autoRetract) {
				adjustWinch(-length);

				if (!extended) {
					modeFlexible();
					autoRetract = false;
				}
			}
		}

		//integrate position and orientation of segments by timestep of h
		private void Simulate() {
			for (int i = baseSegment; i >= 0; i--) {
				Segment segment = rope[i];
				
				segment.previousPosition = segment.position;
				segment.position += h * segment.velocity;

				segment.previousOrientation = segment.orientation;
				rotateOrientation(segment, segment.angulerVelocity * h);
			}
		}

		private void ApplyConstraints() {
			if (extendedSegments > 0)//contrain base
				anchorConstraint(winchPosition, baseSegment, winchOffset / length, true);

			for (int i = baseSegment; i >= 1; i--) {//constrain extended segments between base and hook
				distanceConstraint(rope[i], rope[i - 1]);
				angleConstraint(rope[i], rope[i - 1]);
			}

			if (extendedSegments > 0 && autoExtend)//constrain hook
				anchorConstraint(hookPosition, 0, 1, false);

			for (int i = extendedSegments; i < rope.Length; i++) {//constrain unextended segments
				rope[i].p1 = basePosition;
				rope[i].orientation = baseOrientation;
			}
		}

		private Vector2d anchorDiff = Vector2d.zero;
		private Vector2d anchor_r = Vector2d.zero;

		//A constraint which anchors a segment to a point with infinite mass/inertia.
		private void anchorConstraint(Vector2d anchor, int segmentIndex, double limiter, bool point1) {
			if (point1) {
				anchorDiff.x = anchor.x - rope[segmentIndex].p1.x;
				anchorDiff.y = anchor.y - rope[segmentIndex].p1.y;
				anchor_r.x = rope[segmentIndex].p1.x - rope[segmentIndex].position.x;
				anchor_r.y = rope[segmentIndex].p1.y - rope[segmentIndex].position.y;
			} else {
				anchorDiff.x = anchor.x - rope[segmentIndex].p2.x;
				anchorDiff.y = anchor.y - rope[segmentIndex].p2.y;
				anchor_r.x = rope[segmentIndex].p2.x - rope[segmentIndex].position.x;
				anchor_r.y = rope[segmentIndex].p2.y - rope[segmentIndex].position.y;
			}

			double torque = Vector2d.cross(anchor_r, anchorDiff);

			if (point1)
				rope[segmentIndex].p1 += anchorDiff;
			else
				rope[segmentIndex].p2 += anchorDiff;
			rotateOrientation(rope[segmentIndex], torque);

			double angle = Vector2d.SignedAngle(baseOrientation, rope[segmentIndex].orientation);
			double limit = angleLimit * Mathf.Deg2Rad * limiter;

			if (System.Math.Abs(angle) >= limit) {
				double diff = angle - (angle > 0 ? limit : -limit);
				rotateOrientation(rope[segmentIndex], -diff);
			}
		}

		//constrains mutual orientation between two segemnts
		private void angleConstraint(Segment s1, Segment s2) {
			double angle = Vector2d.SignedAngle(s1.orientation, s2.orientation);
			double limit = angleLimit * Mathf.Deg2Rad;
			double ratio = (1 / s2.inertia) / (1 / s1.inertia + 1 / s2.inertia);

			if (System.Math.Abs(angle) > limit) {
				double difference = angle - (angle > 0 ? limit : -limit);
				rotateOrientation(s1, .5f * difference * stiffness * ratio);
				rotateOrientation(s2, .5f * -difference * stiffness * (1 - ratio));
			} 
		}

		private Vector2d s1p2 = Vector2d.zero;
		private Vector2d s2p1 = Vector2d.zero;

		private Vector2d direction = Vector2d.zero;

		private Vector2d r1 = Vector2d.zero;
		private Vector2d r2 = Vector2d.zero;

		private Vector2d correction1 = Vector2d.zero;
		private Vector2d correction2 = Vector2d.zero;

		//constrains distance between two segments
		private void distanceConstraint(Segment s1, Segment s2) {
			s1p2.x = s1.p2.x;
			s1p2.y = s1.p2.y;

			s2p1.x = s2.p1.x;
			s2p1.y = s2.p1.y;

			direction.x = s1p2.x - s2p1.x;
			direction.y = s1p2.y - s2p1.y;
			double magnitude = -direction.magnitude;
			direction = direction.normalized;

			r1.x = s1p2.x - s1.position.x;
			r1.y = s1p2.y - s1.position.y;

			r2.x = s2p1.x - s2.position.x;
			r2.y = s2p1.y - s2.position.y;

			double inverseMass1 = 1 / s1.mass + System.Math.Pow(Vector2d.cross(r1, direction), 2) / s1.inertia;
			double inverseMass2 = 1 / s2.mass + System.Math.Pow(Vector2d.cross(r2, direction), 2) / s2.inertia;
			double ratio = inverseMass1 / (inverseMass1 + inverseMass2);

			correction1.x = direction.x * magnitude * ratio;
			correction1.y = direction.y * magnitude * ratio;

			correction2.x = direction.x * magnitude * (1 - ratio);
			correction2.y = direction.y * magnitude * (1 - ratio);

			double torque1 = Vector2d.cross(r1, correction1);
			double torque2 = Vector2d.cross(r2, correction2);

			s1.p2 += correction1;
			s2.p1 -= correction2;
			rotateOrientation(s1, .5f * torque1);
			rotateOrientation(s2, .5f * -torque2);
		}

		//update velocities to account for constraints
		private void adjustVelocities() {
			for (int i = extendedSegments - 1; i >= 0; i--) {
				rope[i].velocity.x = (rope[i].position.x - rope[i].previousPosition.x) / h;
				rope[i].velocity.y = (rope[i].position.y - rope[i].previousPosition.y) / h;
				rope[i].angulerVelocity = Vector2d.SignedAngle(rope[i].previousOrientation, rope[i].orientation) / h;
			}
		}

		//clamp velocity and apply drag
		private void solveVelocities() {
			for (int i = extendedSegments - 1; i >= 0; i--) {
				rope[i].velocity = rope[i].velocity.normalized * System.Math.Clamp(rope[i].velocity.magnitude, 0, maxSpeed * (extendedSegments - i));
				rope[i].velocity.x *= linearDrag;
				rope[i].velocity.y *= linearDrag;
				rope[i].angulerVelocity *= angulerDrag;
			}
		}

		private void adjustWinch(double adjustment) {
			winchOffset += adjustment;
			if (winchOffset >= length || winchOffset < 0) {
				int segmentChange = (int)(winchOffset / length);
				if (winchOffset < 0) segmentChange -= 1;

				extendedSegments = System.Math.Clamp(extendedSegments + segmentChange, 0, maxSegments);
				baseSegment = System.Math.Clamp(baseSegment + segmentChange, 0, maxSegments - 1);
				if (winchOffset < 0) 
					winchOffset = extendedSegments > 0 ? length + winchOffset : 0;	

				winchOffset = winchOffset % length;
				maxSpeed = maxSpeeds[Mathf.Clamp(extendedSegments - 1, 0, 3)];
			}

			extended = extendedSegments != 0;
		}

		private Vector2d real = Vector2d.zero;
		private Vector2d complex = Vector2d.zero;

		//rotates segment s by the rotation r given in radians
		private void rotateOrientation(Segment s, double r) {
			real.x = System.Math.Cos(r) * s.orientation.x;
			real.y = System.Math.Cos(r) * s.orientation.y;
			complex.x = System.Math.Sin(r) * s.orientation.x;
			complex.y = System.Math.Sin(r) * s.orientation.y;

			s.orientation = new Vector2d(real.x - complex.y, real.y + complex.x).normalized;
		}

		/*private Vector2d baseOrientation() {
			double rotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
			return new Vector2d(System.Math.Cos(rotation), System.Math.Sin(rotation));
		}

		private Vector2d basePosition() {
			return new Vector2d(shipRigidbody.position.x, shipRigidbody.position.y) + new Vector2d(PlayerShipModel.impendingVelocity.x, PlayerShipModel.impendingVelocity.y) * Time.fixedDeltaTime + baseOrientation * baseOffset;
		}

		private Vector2d winchPosition() {
			return new Vector2d(shipRigidbody.position.x, shipRigidbody.position.y) + new Vector2d(PlayerShipModel.impendingVelocity.x, PlayerShipModel.impendingVelocity.y) * Time.fixedDeltaTime + baseOrientation * (baseOffset + winchOffset);
		}

		private Vector2d hookPosition() {
			return winchPosition + baseOrientation * (length * .85 * extendedSegments);
		}*/

		private void updateBaseOrientation() {
			double rotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
			baseOrientation.x = System.Math.Cos(rotation);
			baseOrientation.y = System.Math.Sin(rotation);
		}

		private void updateBasePosition() {
			basePosition.x = shipRigidbody.position.x + PlayerShipModel.impendingVelocity.x * Time.fixedDeltaTime + baseOrientation.x * baseOffset;
			basePosition.y = shipRigidbody.position.y + PlayerShipModel.impendingVelocity.y * Time.fixedDeltaTime + baseOrientation.y * baseOffset;
		}

		private void updateWinchPosition() {
			winchPosition.x = shipRigidbody.position.x + PlayerShipModel.impendingVelocity.x * Time.fixedDeltaTime + baseOrientation.x * (baseOffset + winchOffset);
			winchPosition.y = shipRigidbody.position.y + PlayerShipModel.impendingVelocity.y * Time.fixedDeltaTime + baseOrientation.y * (baseOffset + winchOffset);
		}

		private void updateHookPosition() {
			hookPosition.x = winchPosition.x + baseOrientation.x * (length * .85 * extendedSegments);
			hookPosition.y = winchPosition.y + baseOrientation.y * (length * .85 * extendedSegments);
		}

		private void OnDrawGizmos() {
			if (!Application.isPlaying) 
				return;

			for (int i = 0; i < maxSegments; i++) {
				Gizmos.color = i % 2 == 0 ? Color.green : Color.white;
				Gizmos.DrawLine(new Vector2((float)rope[i].p1.x, (float)rope[i].p1.y), 
								new Vector2((float)rope[i].p2.x, (float)rope[i].p2.y));
			}
		}
	}
}

/**Rope Modes**/

/**-- Stiff --
 * Meant for throwing enemies.
 * 
 * Angle Limit - 6
 * 
 * Stiffness - .8
 * Linear Drag - .9985
 * Anguler Drag - .985
 **/

/**-- Flexible --
 * Meant for swinging around obstacles/ pulling things.   
 * 
 * Angle Limit - 35
 * 
 * Stiffness - .8
 * Linear Drag - .9998
 * Anguler Drag - .995
 **/

/** TODO **/
/**
 * Feaures
 * - add collision to segments
 * - add ability to grab and release objects with rope
 * - add ship correction to rope wheen swinging it around in stiff mode
 * - add slight restitution force to rope when in flexible mode
 * 
 * Performance
 * - reduce gc time
 * - change underlying representation to be that of particles rather than segments
 * 
 * Bugs
 * - elimnate jitteriness that at occurs when <= 2 segments are unwound
 */