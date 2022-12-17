using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SegmentRope {
	//An implmentation of a Vector2 that uses doubles instead of floats
	class Vector2d {
		public double x;
		public double y;
		public double magnitude { get { return System.Math.Sqrt(x * x + y * y); } }
		public double sqrmagnitude { get { return x * x + y * y; } }

		public Vector2d(double x, double y) {
			this.x = x;
			this.y = y;
		}

		public static Vector2d zero { get { return new Vector2d(0, 0); } }
		public static Vector2d up { get { return new Vector2d(0, 1); } }

		public void normalize() {
			if (magnitude > 0) {
				x = x / magnitude;
				y = y / magnitude;
			}
			else {
				x = 0;
				y = 0;
			}
		}

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

	//A line segment representing a single link of rope.
	class Segment {
		public GameObject sprite;
		private Vector3 spritePos = new Vector3(0, 0, 0);

		public readonly Vector2d p1;
		public readonly Vector2d p2;

		public readonly Vector2d position;
		public Vector2d previousPosition;
		public Vector2d velocity;
		public double mass;

		public readonly Vector2d orientation;//should be a unit vector
		public Vector2d previousOrientation;
		public double angulerVelocity;
		public double inertia;

		public double length;
		private double halfLength;

		public Segment(GameObject sprite, Vector2d position, Vector2d orientation, double mass, double inertia, double length) {
			this.sprite = sprite;
			
			this.p1 = new Vector2d(position.x - orientation.x * halfLength, position.y - orientation.y * halfLength);
			this.p2 = new Vector2d(position.x + orientation.x * halfLength, position.y + orientation.y * halfLength);

			this.position = new Vector2d(position.x, position.y);
			this.previousPosition = new Vector2d(position.x, position.y);
			this.velocity = new Vector2d(0, 0);

			this.orientation = new Vector2d(orientation.x, orientation.y);
			this.previousOrientation = new Vector2d(orientation.x, orientation.y);
			this.angulerVelocity = 0;

			this.mass = mass;
			this.inertia = inertia;

			this.length = length;
			this.halfLength = length / 2;
		}

		public void updateSprite() {
			spritePos.x = (float)position.x;
			spritePos.y = (float)position.y;
			sprite.transform.position = spritePos;

			float angle = Mathf.Atan2((float)orientation.y, (float)orientation.x) * Mathf.Rad2Deg - 90;
			sprite.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}

		public void setPosition(double x, double y) {
			position.x = x;
			position.y = y;
			p1.x = x - orientation.x * length;
			p1.y = y - orientation.y * length;
			p2.x = x + orientation.x * length;
			p2.y = y + orientation.y * length;
		}

		public void setP1(double x, double y) {
			position.x = x + orientation.x * halfLength;
			position.y = y + orientation.y * halfLength;
			p1.x = x;
			p1.y = y;
			p2.x = x + orientation.x * length;
			p2.y = y + orientation.y * length;
		}

		public void setP2(double x, double y) {
			position.x = x - orientation.x * halfLength;
			position.y = y - orientation.y * halfLength;
			p1.x = x - orientation.x * length;
			p1.y = y - orientation.y * length;
			p2.x = x;
			p2.y = y;
		}

		public void setOrientation(double x, double y) {
			orientation.x = x;
			orientation.y = y;
			orientation.normalize();
			p1.x = position.x - orientation.x * halfLength;
			p1.y = position.y - orientation.y * halfLength;
			p2.x = position.x + orientation.x * halfLength;
			p2.y = position.y + orientation.y * halfLength;
		}
	}

	//Defines the physical characteristics of the rope
	public enum RopeMode {
		FLEXIBLE,
		STIFF,
	}

	//A physically simulated rope that is constructed by linking together a series of line segments
	public class SegmentRope : MonoBehaviour {
		public int maxSegments = 35;
		public double length = .65;//length of an individual segment
		public double angleLimit;//max difference, in degrees, between the rotations, in relation to the origin, of two segments
		public double mass = 1;
		public double inertia = 1;
		public double stiffness = 1;
		public double stretchiness = 1;
		public double maxSpeed = 6;
		public double linearDrag;
		public double angulerDrag;

		private double _angleLimit;
		private double[] maxSpeeds = new double[] {3, 3, 4, 6};

		public int substeps = 18;
		public int iterations = 1;
		private double h;

		public GameObject segmentSprite;
		public GameObject hook;
		private HookSegment hookSegment;

		private Segment[] rope;
		private int extendedSegments = 0;
		private int baseSegment = -1;
		private RopeMode mode = RopeMode.FLEXIBLE;

		private Rigidbody shipRigidbody;
		public double shipCorrection = .4;// reduces the effect of playerShip's translational movement on rope segments, 1 == no effect, 0 == normal

		private double baseOffset = .65;

		private double prevRotation = 0;
		private Vector2d baseOrientation = Vector2d.zero;
		private Vector2d basePosition = Vector2d.zero;
		private Vector2d winchPosition = Vector2d.zero;
		private Vector2d hookPosition = Vector2d.zero;

		private double nextRotation = 0;
		private Vector2d nextBaseOrientation = Vector2d.zero;
		private Vector2d nextBasePosition = Vector2d.zero;
		private Vector2d nextWinchPosition = Vector2d.zero;
		private Vector2d nextHookPosition = Vector2d.zero;

		private double rotationVelocity = 0;
		private Vector2d orientationVelocity = Vector2d.zero;
		private Vector2d baseVelocity = Vector2d.zero;
		private Vector2d winchVelocity = Vector2d.zero;
		private Vector2d hookVelocity = Vector2d.zero;

		private double winchForce = 3;
		private double winchOffset = 0;
		private double winchScrollBuffer = 0;
		private double winchBrakeBuffer = 0;
		private double hookLag = .92;

		private Vector2d hookSnapshot = Vector2d.zero;

		private bool extended = false;
		private bool autoExtend = false;
		private bool autoRetract = false;

		private void Start() {
			//create rope segments
			rope = new Segment[maxSegments];
			for (int i = 0; i < maxSegments; i++) {
				GameObject sprite = Instantiate(segmentSprite);
				rope[i] = new Segment(sprite, new Vector2d(basePosition.x, basePosition.y), Vector2d.up, mass, inertia, length);
			}

			//create hook segment
			GameObject hookObj = Instantiate(hook, new Vector3((float)rope[0].position.x, (float)rope[0].position.y, 0), Quaternion.identity);
			hookObj.AddComponent<HookSegment>();
			hookSegment = hookObj.GetComponent<HookSegment>();
			hookSegment.s = rope[0];

			maxSpeed = maxSpeeds[Mathf.Clamp(extendedSegments, 0, 3)];
			h = Time.fixedDeltaTime / substeps;

			shipRigidbody = transform.parent.GetComponent<Rigidbody>();

			prevRotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
			updateRotation();

			modeFlexible();
		}

		private void Update() {
			//change rope mode
			if (Input.GetKeyDown(KeyCode.F)) {
				if (mode == RopeMode.FLEXIBLE) 
					modeStiff();
				else 
					modeFlexible();
			}

			//auto extend rope
			if (Input.GetMouseButtonDown(1) && !extended && !autoRetract) {
				autoExtend = true;
				shipCorrection = 1;
				hookSegment.active = true;
			}

			//auto retract rope
			if (Input.GetMouseButtonDown(1) && extended && !autoExtend) {
				autoRetract = true;
				hookSegment.active = false;
				hookSegment.unHook();
			}

			//wind/unwind rope 
			if (extended && Input.mouseScrollDelta.y != 0 && winchScrollBuffer == 0 && (extendedSegments < maxSegments || Input.mouseScrollDelta.y < 0))
				winchScrollBuffer = 4 * System.Math.Sign(Input.mouseScrollDelta.y);
		}

		private void FixedUpdate() {
			winchBrakeBuffer -= 1;

			updateRotation();

			//apply ship correction
			for (int i = baseSegment; i >= 0; i--) {
				rope[i].setPosition(rope[i].position.x + shipRigidbody.velocity.x * Time.fixedDeltaTime * shipCorrection, 
									rope[i].position.y + shipRigidbody.velocity.y * Time.fixedDeltaTime * shipCorrection);
			}

			//main loop
			for (int i = 0; i < substeps; i++) {
				Simulate();
				for (int j = 0; j < iterations; j++) {
					ApplyConstraints();
				}
				adjustVelocities();
				solveVelocities();
				incrementPositions();
			}

			for (int i = 0; i < rope.Length; i++) {
				rope[i].updateSprite();
			}

			if (winchBrakeBuffer == 1) {
				maxSpeed = maxSpeeds[Mathf.Clamp(extendedSegments - 1, 0, 3)];
				winchBrakeBuffer = 0;
			}

			//apply scroll wind
			if (winchScrollBuffer != 0) {
				adjustWinch(length / 4 * System.Math.Sign(winchScrollBuffer));
				winchScrollBuffer -= winchScrollBuffer > 0 ? 1 : -1;
			}

			//apply auto extention
			if (autoExtend) {
				adjustWinch(winchForce);

				if (extendedSegments == maxSegments || hookSegment.isHooked) {
					autoExtend = false;
					shipCorrection = .4;
					maxSpeed = 1;
					winchBrakeBuffer = 4;
				}

				if (hookSegment.justHooked) {
					tightenRope();
					hookSnapshot.x = rope[0].position.x;
					hookSnapshot.y = rope[0].position.y;
				}
			}

			//apply auto retraction
			if (autoRetract) {
				adjustWinch(-length);

				if (!extended) {
					modeFlexible();
					autoRetract = false;
				}
			}

			if (extendedSegments == 0) {
				hookSegment.active = false;
				hookSegment.unHook();
			}

			if (tightLength > 0) {
				if (looseLength - tightLength > winchForce * .25) {
					adjustWinch(-winchForce * .25);
					looseLength -= winchForce * .25;
				}
				else {
					adjustWinch(-(looseLength - tightLength));
					looseLength = 0;
					tightLength = 0;
					modeStiff();
				}
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
				rotateOrientation(segment, segment.angulerVelocity * h);
			}
		}

		//apply various positional and anguler constraints
		private void ApplyConstraints() {
			//constrain base
			if (extendedSegments > 0)
				anchorConstraint(winchPosition, baseSegment, winchOffset / length, true);

			//constrain extended segments between base and hook
			for (int i = baseSegment; i >= 1; i--) {
				distanceConstraint(rope[i], rope[i - 1]);
				angleConstraint(rope[i], rope[i - 1]);
			}

			//constrain hook while auto extending
			if (extendedSegments > 0 && autoExtend)
				anchorConstraint(hookPosition, 0, 1, false);

			//constrain hook while tightening rope
			if (tightLength > 0)
				anchorConstraint(hookSnapshot, 0, 1, false);

			//constrain unextended segments
			for (int i = extendedSegments; i < rope.Length; i++) {
				rope[i].setP1(basePosition.x, basePosition.y);
				rope[i].setOrientation(baseOrientation.x, baseOrientation.y);
				rope[i].previousPosition.x = rope[i].position.x;
				rope[i].previousPosition.y = rope[i].position.y;
				rope[i].velocity.x = 0;
				rope[i].velocity.y = 0;
				rope[i].angulerVelocity = 0;
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
				rope[segmentIndex].setP1(rope[segmentIndex].p1.x + anchorDiff.x * stretchiness, rope[segmentIndex].p1.y + anchorDiff.y * stretchiness);
			else 
				rope[segmentIndex].setP2(rope[segmentIndex].p2.x + anchorDiff.x * stretchiness, rope[segmentIndex].p2.y + anchorDiff.y * stretchiness);
			
			rotateOrientation(rope[segmentIndex], torque);

			double angle = Vector2d.SignedAngle(baseOrientation, rope[segmentIndex].orientation);
			double limit = _angleLimit * limiter;

			if (System.Math.Abs(angle) >= limit) {
				double diff = angle - (angle > 0 ? limit : -limit);
				rotateOrientation(rope[segmentIndex], -diff);
			}
		}

		//constrains mutual orientation between two segemnts
		private void angleConstraint(Segment s1, Segment s2) {
			double angle = Vector2d.SignedAngle(s1.orientation, s2.orientation);
			double limit = _angleLimit;
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

			//direction of gap between the ends of each segment
			direction.x = s1p2.x - s2p1.x;
			direction.y = s1p2.y - s2p1.y;
			double magnitude = -direction.magnitude;
			direction.normalize();

			//calculate radius
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

			s1.setP2(s1.p2.x + correction1.x * stretchiness, s1.p2.y + correction1.y * stretchiness);
			s2.setP1(s2.p1.x - correction2.x * stretchiness, s2.p1.y - correction2.y * stretchiness);

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
			double correctedLinearDrag = System.Math.Pow(linearDrag, 1.0 / substeps);
			double correctedAngulerDrag = System.Math.Pow(angulerDrag, 1.0 / substeps);

			for (int i = extendedSegments - 1; i >= 0; i--) {
				double newMaxSpeed = System.Math.Clamp(rope[i].velocity.magnitude, 0, maxSpeed * (extendedSegments - i));
				double mag = rope[i].velocity.magnitude;

				if (mag > 0) {
					rope[i].velocity.x = rope[i].velocity.x / mag * newMaxSpeed * correctedLinearDrag;
					rope[i].velocity.y = rope[i].velocity.y / mag * newMaxSpeed * correctedLinearDrag;
				} else {
					rope[i].velocity.x = 0;
					rope[i].velocity.y = 0;
				}

				rope[i].angulerVelocity *= correctedAngulerDrag;
				//rope[i].angulerVelocity = System.Math.Clamp(rope[i].angulerVelocity, -16, 16) * correctedAngulerDrag;
			}
		}

		private Vector2d hook2Base = Vector2d.zero;
		private double tightLength = 0;
		private double looseLength = 0;

		private void tightenRope() {
			hook2Base.x = rope[0].position.x - winchPosition.x;
			hook2Base.y = rope[0].position.y - winchPosition.y;

			looseLength = extendedSegments * length;
			tightLength = hook2Base.magnitude;
		}

		//winds and unwinds rope based on adjustment
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
			double cosR = System.Math.Cos(r);
			double sinR = System.Math.Sin(r);

			real.x = cosR * s.orientation.x;
			real.y = cosR * s.orientation.y;
			complex.x = sinR * s.orientation.x;
			complex.y = sinR * s.orientation.y;

			s.setOrientation(real.x - complex.y, real.y + complex.x);
		}

		private double doublePI = 2 * System.Math.PI;
		private double PI = System.Math.PI;
		private double halfPI = .5 * System.Math.PI;

		private void updateRotation() {
			prevRotation = nextRotation;
			nextRotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;

			if (System.Math.Clamp(prevRotation, doublePI, doublePI + halfPI) == prevRotation && System.Math.Clamp(nextRotation, halfPI, PI) == nextRotation)
				prevRotation -= doublePI;
			if (System.Math.Clamp(prevRotation, halfPI, PI) == prevRotation && System.Math.Clamp(nextRotation, doublePI, doublePI + halfPI) == nextRotation)
				prevRotation += doublePI;

			rotationVelocity = (nextRotation - prevRotation) / substeps;
		}

		private void incrementPositions() {
			prevRotation += rotationVelocity;
			double impendingShipPosX = shipRigidbody.position.x + PlayerShipModel.impendingVelocity.x * Time.fixedDeltaTime;
			double impendingShipPosY = shipRigidbody.position.y + PlayerShipModel.impendingVelocity.y * Time.fixedDeltaTime;

			baseOrientation.x = System.Math.Cos(prevRotation);
			baseOrientation.y = System.Math.Sin(prevRotation);

			basePosition.x = impendingShipPosX + baseOrientation.x * baseOffset;
			basePosition.y = impendingShipPosY + baseOrientation.y * baseOffset;

			winchPosition.x = impendingShipPosX + baseOrientation.x * (baseOffset + winchOffset);
			winchPosition.y = impendingShipPosY + baseOrientation.y * (baseOffset + winchOffset);

			hookPosition.x = winchPosition.x + baseOrientation.x * (length * hookLag * extendedSegments);
			hookPosition.y = winchPosition.y + baseOrientation.y * (length * hookLag * extendedSegments);
		}

		private void modeStiff() {
			angleLimit = 6;
			_angleLimit = angleLimit * Mathf.Deg2Rad;
			linearDrag = 1;
			angulerDrag = 1;

			mode = RopeMode.STIFF;
		}

		private void modeFlexible() {
			angleLimit = 35;
			_angleLimit = angleLimit * Mathf.Deg2Rad;
			linearDrag = 1;
			angulerDrag = 1;

			mode = RopeMode.FLEXIBLE;
		}

		private Vector2 giz1 = new Vector3(0, 0, 0);
		private Vector2 giz2 = new Vector3(0, 0, 0);

		private void OnDrawGizmos() {
			if (!Application.isPlaying) 
				return;

			for (int i = 0; i < maxSegments; i++) {
				Gizmos.color = i % 2 == 0 ? Color.green : Color.white;

				giz1.x = (float)rope[i].p1.x;
				giz1.y = (float)rope[i].p1.y;
				giz2.x = (float)rope[i].p2.x;
				giz2.y = (float)rope[i].p2.y;

				Gizmos.DrawLine(giz1, giz2);
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
 * - change underlying representation to be that of particles rather than segments
 * 
 * Bugs
 * - elimnate base segment jitteriness
 * - update velocity of hooked objects to fix collision
 * 
 * Rope improvements
 * - Retract and stiffen rope when hooking enemies(in progress)
 * - Add ability to hook structures
 * - improve collision detection of hook
 * - increase mass of hook when hooking an object to prevent bounce back and other eratic movements
 * 
 * Feel
 * - Hooking is too jittering and janky.  Hooking an enemy should only result in them being pushed back lightly
 * - throwing enemies around feels sluggish and slow
 * 
 * Camera
 * - have camera follow a thrown enemy, and possible zoom out as well to ensure that it stays on screen, offset/zoom out should be enough to enemy in frame until they come to a rest.
 *
 * Random Ideas
 * - After hooking an enemy, spinning the ship around could put the ship into an auto spin state.  Exiting this state would require releasing the hooked enemy, or retracting the rope
 * - Rope could automatically retract win spinning to avoid any obstacles
 * - Could add some sort of auto spin button that would automatically spin the ship 180 or 360 degrees around
 */