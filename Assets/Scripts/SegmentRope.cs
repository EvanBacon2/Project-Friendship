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
		public Vector2d position;
		public Vector2d previousPosition;
		public Vector2d velocity;
		public double mass;

		private Vector2d _orientation;//only update via rotateOrientation to guarentee normalized magnitude
		public Vector2d previousOrientation;
		public double angulerVelocity;
		public double inertia;

		public double length;
		private double halfLength;

		private Vector2d _p1;
		private Vector2d _p2;

		public Vector2d p1 { 
			get { return _p1; }
			set { position = value + _orientation * length / 2; }
		}
		public Vector2d p2 { 
			get { return _p2; }
			set { position = value - _orientation * length / 2; }
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
			this.position = position;
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

	public class SegmentRope : MonoBehaviour {
		public int count = 35;
		public double length = .65;
		public double angleLimit = 6;
		public double mass = 1;
		public double inertia = 1;
		public double stiffness = .8;
		public double maxSpeed = 6;
		public double linearDrag = .9985;
		public double angulerDrag = .985;

		public int substeps = 20;
		public int iterations = 1;

		public bool flexible = false;

		private Segment[] rope;
		private Vector2d basePos;

		private void Start() {
			rope = new Segment[count];

			//collision stuff

			Vector2d currentPosition = basePosition();
			currentPosition.y += length / 2;
			for (int i = 0; i < rope.Length; i++) {
				rope[i] = new Segment(new Vector2d(currentPosition.x, currentPosition.y), Vector2d.up, mass, inertia, length);
				currentPosition.y += length;
			}
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
			//more collision stuff
			if (Input.GetKeyDown(KeyCode.F)) {
				if (angleLimit == 35)
					modeStiff();
				else
					modeFlexible();
			}
		}

		private void FixedUpdate() {
			double h = Time.fixedDeltaTime / substeps;
			basePos = basePosition();

			for (int i = 0; i < substeps; i++) {
				Simulate(h);
				for (int j = 0; j < iterations; j++) {
					ApplyConstraints();
				}
				adjustVelocities(h);
				solveVelocities();
			}
		}

		private void Simulate(double h) {
			for (int i = 0; i < rope.Length; i++) {
				Segment segment = rope[i];
		
				segment.previousPosition = segment.position;
				segment.position += h * segment.velocity;

				segment.previousOrientation = segment.orientation;
				rotateOrientation(segment, segment.angulerVelocity * h);
			}
		}

		private void ApplyConstraints() {
			baseConstraint();

			for (int i = 1; i < rope.Length; i++) {
				angleConstraint(rope[i - 1], rope[i]);
				distanceConstraint(rope[i - 1], rope[i]);
			}
		}

		private void baseConstraint() {
			Vector2d baseOrientation = basePos - new Vector2d(transform.position.x, transform.position.y);//**
			double angle = Vector2d.SignedAngle(baseOrientation, rope[0].orientation);
			double limit = angleLimit * Mathf.Deg2Rad;

			if (System.Math.Abs(angle) > limit) {
				double diff = angle - (angle > 0 ? limit : -limit);
				rotateOrientation(rope[0], .5f * -diff);
			}

			Vector2d difference = basePos - rope[0].p1;//**
			Vector2d r = rope[0].p1 - rope[0].position;//**
			double torque = Vector2d.cross(r, difference);

			rope[0].p1 += difference;
			rotateOrientation(rope[0], .5f * torque * stiffness);
		}

		private double angle = 0;
		private double limit = 0;

		private void angleConstraint(Segment s1, Segment s2) {
			angle = Vector2d.SignedAngle(s1.orientation, s2.orientation);
			limit = angleLimit * Mathf.Deg2Rad;

			if (System.Math.Abs(angle) > limit) {
				double difference = angle - (angle > 0 ? limit : -limit);
				rotateOrientation(s1, .5f * difference * stiffness);
				rotateOrientation(s2, .5f * -difference * stiffness);
			}
		}

		private Vector2d s1p2 = Vector2d.zero;
		private Vector2d s2p1 = Vector2d.zero;

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
		
			double inverseMass1 = 1 + System.Math.Pow(Vector2d.cross(r1, direction), 2);
			double inverseMass2 = 1 + System.Math.Pow(Vector2d.cross(r2, direction), 2);
			double ratio = inverseMass1 / (inverseMass1 + inverseMass2);
			
			Vector2d correction1 = direction * magnitude * ratio;//**
			Vector2d correction2 = direction * magnitude * (1 - ratio);//**

			double torque1 = Vector2d.cross(r1, correction1);
			double torque2 = Vector2d.cross(r2, correction2);

			s1.p2 += correction1;
			s2.p1 -= correction2;
			rotateOrientation(s1, .5f * torque1);
			rotateOrientation(s2, .5f * -torque2);
		}

		private void adjustVelocities(double h) {
			for (int i = 0; i < rope.Length; i++) {
				rope[i].velocity = (rope[i].position - rope[i].previousPosition) / h;
				rope[i].angulerVelocity = Vector2d.SignedAngle(rope[i].previousOrientation, rope[i].orientation) / h;
			}
		}

		private void solveVelocities() {
			for (int i = 0; i < rope.Length; i++) {
				rope[i].velocity = rope[i].velocity.normalized * System.Math.Clamp(rope[i].velocity.magnitude, 0, maxSpeed * (i + 1));
				rope[i].velocity *= linearDrag;
				rope[i].angulerVelocity *= angulerDrag;
			}
		}

		private Vector2d basePosition() {
			double baseRotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
			return new Vector2d(transform.position.x, transform.position.y) + new Vector2d(System.Math.Cos(baseRotation), System.Math.Sin(baseRotation)) * 1.2;
		}

		private void rotateOrientation(Segment s, double rotation) {
			Vector2d real = System.Math.Cos(rotation) * s.orientation;
			Vector2d complex = System.Math.Sin(rotation) * s.orientation;
			s.orientation = new Vector2d(real.x - complex.y, real.y + complex.x).normalized;
		}

		private void OnDrawGizmos() {
			if (!Application.isPlaying) 
				return;

			for (int i = 0; i < rope.Length; i++) {
				Gizmos.color = i % 2 == 0 ? Color.green : Color.white;
				Gizmos.DrawLine(new Vector2((float)rope[i].p1.x, (float)rope[i].p1.y), new Vector2((float)rope[i].p2.x, (float)rope[i].p2.y));
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
 * Max Speed - 6
 * Linear Drag - .9985
 * Anguler Drag - .985
 **/

/**-- Flexible --
 * Meant for swinging around asteroids/ pulling things.   
 * 
 * Angle Limit - 35
 * 
 * Stiffness - .8
 * Max Speed - 6
 * Linear Drag - .9998
 * Anguler Drag - .995
 **/
