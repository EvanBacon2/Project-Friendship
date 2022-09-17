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
				_p1 = value - _orientation * length / 2;
				_p2 = value + _orientation * length / 2;
			}
		}

		public Vector2d p1 { 
			get { return _p1; }
			set {
				_p1 = value;
				_p2 = value + _orientation * length;
				_position = value + _orientation * length / 2; 
			}
		}
		public Vector2d p2 { 
			get { return _p2; }
			set {
				_p1 = value - _orientation * length;
				_p2 = value;
				_position = value - _orientation * length / 2; 
			}
		}

		public Vector2d orientation {
			get { return _orientation; }
			set {
				_orientation = value;
				_p1 = position - orientation * halfLength;
				_p2 = position + orientation * halfLength;
			}
		}

		public Segment(Vector2d position, Vector2d orientation, double mass, double inertia, double length) {
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

		private Segment[] rope;
		private int extendedSegments = 1;
		private int baseSegment = 0;
		private RopeMode mode = RopeMode.FLEXIBLE;
		private double h;

		private double winchForce = 3.5;
		private double winchOffset = 0;
		private double winchAdjustment = 0;
		private double winchScrollBuffer = 0;
		private bool extended = false;
		private bool extending = false;
		private bool retracting = false;

		private double baseOffset = .65;
		private Vector2d basePos;
		private Vector2d nextBasePos;
		private Vector2d baseVelocity;

		private Rigidbody shipRigidbody;
		private double shipCorrection = .4;

		private void Start() {
			shipRigidbody = transform.parent.GetComponent<Rigidbody>();
			maxSpeed = maxSpeeds[Mathf.Clamp(extendedSegments - 1, 0, 3)];
			h = Time.fixedDeltaTime / substeps;
			basePos = basePosition();

			rope = new Segment[maxSegments];
			Vector2d initPos = basePosition();
			for (int i = 0; i < maxSegments; i++) {
				rope[i] = new Segment(new Vector2d(initPos.x, initPos.y), Vector2d.up, mass, inertia, length);
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

			if (Input.GetMouseButtonDown(1) && !extended && !retracting) {
				extended = true;
				extending = true;
				shipCorrection = 1;
			}

			if (Input.GetMouseButtonDown(1) && extended && !extending) {
				extended = false;
				retracting = true;
			}

			if (extended && Input.mouseScrollDelta.y != 0 && winchScrollBuffer == 0) {
				if (extendedSegments < maxSegments || Input.mouseScrollDelta.y < 0)
					winchScrollBuffer = 4 * System.Math.Sign(Input.mouseScrollDelta.y);
				//winchAdjustment = Mathf.Clamp(Input.mouseScrollDelta.y, (float)-length, (float)length);
				//adjustWinch(Mathf.Clamp(Input.mouseScrollDelta.y, (float)-length, (float)length));
			}
		}

		private void FixedUpdate() {
			nextBasePos = basePosition();
			baseVelocity = (nextBasePos - basePos) / substeps;

			for (int i = extendedSegments - 1; i >= 0; i--) {
				rope[i].position.x += shipRigidbody.velocity.x * Time.fixedDeltaTime * shipCorrection;
				rope[i].position.y += shipRigidbody.velocity.y * Time.fixedDeltaTime * shipCorrection;
			}

			for (int i = 0; i < substeps; i++) {
				basePos += baseVelocity;

				Simulate();
				for (int j = 0; j < iterations; j++) {
					ApplyConstraints(basePos);
				}
				adjustVelocities();
				solveVelocities();
			}

			if (winchScrollBuffer != 0) {
				adjustWinch(length / 4 * System.Math.Sign(winchScrollBuffer));
				winchScrollBuffer -= winchScrollBuffer > 0 ? 1 : -1;
			}
			

			if (extending) {
				winchOffset += winchForce;
				if (winchOffset >= length) {
					int segmentIncrease = (int)(winchOffset / length);
					extendedSegments = System.Math.Min(extendedSegments + segmentIncrease, maxSegments);
					baseSegment = System.Math.Min(baseSegment + segmentIncrease, maxSegments - 1);
					winchOffset = winchOffset % length;
					maxSpeed = maxSpeeds[Mathf.Clamp(extendedSegments - 1, 0, 3)];
				}

				if (extendedSegments == maxSegments) {
					extending = false;
					shipCorrection = .4;
				}
			}

			if (retracting) {
				winchOffset -= winchForce;
				if (winchOffset <= 0) {
					baseSegment -= 1;
					extendedSegments -= 1;
					winchOffset = length;
					maxSpeed = maxSpeeds[Mathf.Clamp(extendedSegments - 1, 0, 3)];
				}

				if (extendedSegments == 1) {
					modeFlexible();
					retracting = false;
					winchOffset = 0;
				}
			}
		}

		//integrate position and orientation of segments by timestep of h
		private void Simulate() {
			for (int i = extendedSegments - 1; i >= 0; i--) {
				Segment segment = rope[i];
				
				segment.previousPosition = segment.position;
				segment.position += h * segment.velocity;

				segment.previousOrientation = segment.orientation;
				rotateOrientation(segment, segment.angulerVelocity * h);
			}
		}

		private void ApplyConstraints(Vector2d basePos) {
			Vector2d orientation = baseOrientation();

			if (extendedSegments > 0)
				anchorConstraint(orientation, basePos + winchAddition(), baseSegment, winchOffset / length, true);

			for (int i = extendedSegments -  2; i >= 0; i--) {
				distanceConstraint(rope[i + 1], rope[i]);
				angleConstraint(rope[i + 1], rope[i]);
			}

			if (extendedSegments > 1 && extending)
				anchorConstraint(orientation, hookPosition(), 0, 1, false);

			for (int i = extendedSegments; i < rope.Length; i++) {
				rope[i].p1 = basePos;
				rope[i].orientation = orientation;
			}
		}

		//A constraint which anchors a segment to a point with infinite mass/inertia.
		private void anchorConstraint(Vector2d orientation, Vector2d anchor, int segmentIndex, double limiter, bool point1) {
			Vector2d difference;
			Vector2d r;
			if (point1) {
				difference = anchor - rope[segmentIndex].p1;
				r = rope[segmentIndex].p1 - rope[segmentIndex].position;
			} else {
				difference = anchor - rope[segmentIndex].p2;
				r = rope[segmentIndex].p2 - rope[segmentIndex].position;
			}
			double torque = Vector2d.cross(r, difference);

			if (point1)
				rope[segmentIndex].p1 += difference;
			else
				rope[segmentIndex].p2 += difference;
			rotateOrientation(rope[segmentIndex], torque);

			double angle = Vector2d.SignedAngle(orientation, rope[segmentIndex].orientation);
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

		//constrains distance between two segments
		private void distanceConstraint(Segment s1, Segment s2) {
			s1p2.x = s1.p2.x;
			s1p2.y = s1.p2.y;

			s2p1.x = s2.p1.x;
			s2p1.y = s2.p1.y;

			Vector2d direction = s1p2 - s2p1;
			double magnitude = -direction.magnitude;
			direction = direction.normalized;

			Vector2d r1 = s1p2 - s1.position;
			Vector2d r2 = s2p1 - s2.position;
		
			double inverseMass1 = 1 / s1.mass + System.Math.Pow(Vector2d.cross(r1, direction), 2) / s1.inertia;
			double inverseMass2 = 1 / s2.mass + System.Math.Pow(Vector2d.cross(r2, direction), 2) / s2.inertia;
			double ratio = inverseMass1 / (inverseMass1 + inverseMass2);
			
			Vector2d correction1 = direction * magnitude * ratio;
			Vector2d correction2 = direction * magnitude * (1 - ratio);

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
				rope[i].velocity = (rope[i].position - rope[i].previousPosition) / h;
				rope[i].angulerVelocity = Vector2d.SignedAngle(rope[i].previousOrientation, rope[i].orientation) / h;
			}
		}

		//clamp velocity and apply drag
		private void solveVelocities() {
			for (int i = extendedSegments - 1; i >= 0; i--) {
				rope[i].velocity = rope[i].velocity.normalized * System.Math.Clamp(rope[i].velocity.magnitude, 0, maxSpeed * (extendedSegments - i));
				rope[i].velocity *= linearDrag;
				rope[i].angulerVelocity *= angulerDrag;
			}
		}

		private void adjustWinch(double adjustment) {
			winchOffset += adjustment;
			if (winchOffset >= length || winchOffset < 0) {
				int segmentIncrease = (int)(winchOffset / length);
				if (winchOffset < 0) segmentIncrease -= 1;
				extendedSegments = System.Math.Clamp(extendedSegments + segmentIncrease, 1, maxSegments);
				baseSegment = System.Math.Clamp(baseSegment + segmentIncrease, 0, maxSegments - 1);
				if (winchOffset < 0) {
					 if (extendedSegments >1)
						winchOffset = length + winchOffset;
					else
						winchOffset = 0;
				}
				winchOffset = winchOffset % length;
				maxSpeed = maxSpeeds[Mathf.Clamp(extendedSegments - 1, 0, 3)];
			}

			if (extendedSegments ==1)
				extended = false;
			Debug.Log(extendedSegments);
			//if (extendedSegments == maxSegments) {
			//	extending = false;
			//	shipCorrection = .4;
			//}
		}
		
		//rotates segment s by the rotation r given in radians
		private void rotateOrientation(Segment s, double r) {
			Vector2d real = System.Math.Cos(r) * s.orientation;
			Vector2d complex = System.Math.Sin(r) * s.orientation;
			s.orientation = new Vector2d(real.x - complex.y, real.y + complex.x).normalized;
		}

		private Vector2d basePosition() {
			return new Vector2d(shipRigidbody.position.x, shipRigidbody.position.y) + new Vector2d(PlayerShipModel.impendingVelocity.x, PlayerShipModel.impendingVelocity.y) * Time.fixedDeltaTime + baseOrientation() * baseOffset;
		}

		private Vector2d winchPosition() {
			return new Vector2d(shipRigidbody.position.x, shipRigidbody.position.y) + new Vector2d(PlayerShipModel.impendingVelocity.x, PlayerShipModel.impendingVelocity.y) * Time.fixedDeltaTime + baseOrientation() * (baseOffset + winchOffset);
		}

		private Vector2d winchAddition() {
			return baseOrientation() * winchOffset;
		}

		private Vector2d hookPosition() {
			return winchPosition() + baseOrientation() * (length * .85 * (extendedSegments - 1));
		}
		
		private Vector2d baseOrientation() {
			double rotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
			return new Vector2d(System.Math.Cos(rotation), System.Math.Sin(rotation));
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
 * - add ability to grab objects with rope
 * - add ability to wind/unwind the rope via mousewheel
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


//extended == extendedSegments > 1