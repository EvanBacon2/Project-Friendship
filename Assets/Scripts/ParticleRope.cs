using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParticleRope {
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

		public static double distance(Vector2d lhs, Vector2d rhs) {
			return System.Math.Sqrt(System.Math.Pow(rhs.x - lhs.x, 2) + System.Math.Pow(rhs.y - lhs.y, 2));
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

	class Particle {
		public readonly Vector2d position;
		public Vector2d previousPosition;
		public Vector2d velocity;

		public double mass;

		public Particle(Vector2d position, Vector2d velocity, double mass) {
			this.position = new Vector2d(position.x, position.y);
			this.previousPosition = new Vector2d(position.x, position.y);
			this.velocity = new Vector2d(velocity.x, velocity.y);
			this.mass = mass;
		}

		public void setPosition(double x, double y) {
			position.x = x;
			position.y = y;
		}
	}

	//A line segment consisting of two particles representing a single link of rope.
	class Segment {
		public GameObject sprite;
		private Vector3 spritePos = new Vector3(0, 0, 70);

		public readonly Particle p1;
		public readonly Particle p2;

		private Vector2d position;

		public readonly Vector2d orientation;//should be a unit vector
		public double inertia;

		public Segment(GameObject sprite, Particle p1, Particle p2, double inertia) {
			this.sprite = sprite;

			this.p1 = p1;
			this.p2 = p2;

			this.position = new Vector2d((p1.position.x + p2.position.x) / 2, (p1.position.y + p2.position.y) + 2);

			this.orientation = new Vector2d(p2.position.x - p1.position.x, p2.position.y - p1.position.y);
			this.orientation.normalize();
			this.inertia = inertia;
		}

		public void updateSprite() {
			position.x = (p1.position.x + p2.position.x) / 2;
			position.y = (p1.position.y + p2.position.y) / 2;

			spritePos.x = (float)position.x;
			spritePos.y = (float)position.y;
			sprite.transform.position = spritePos;

			float angle = Mathf.Atan2((float)orientation.y, (float)orientation.x) * Mathf.Rad2Deg - 90;
			sprite.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}

		public void setOrientation(double x, double y) {
			orientation.x = x;
			orientation.y = y;
			orientation.normalize();

			//update incase position of p1 or p2 has changed
			position.x = (p1.position.x + p2.position.x) / 2;
			position.y = (p1.position.y + p2.position.y) / 2;

			double halfLength = Vector2d.distance(p1.position, p2.position) / 2.0;

			p1.setPosition(position.x - orientation.x * halfLength, position.y - orientation.y * halfLength);
			p2.setPosition(position.x + orientation.x * halfLength, position.y + orientation.y * halfLength);
		}
	}

	public enum RopeMode {
		FLEXIBLE,
		STIFF,
	}

	public class ParticleRope : MonoBehaviour {
		public int maxSegments = 35;
		public double length = .65;
		public double angleLimit;
		public double mass = 1;
		public double inertia = 1;
		public double stiffness = 1;
		public double stretchiness = 1;
		public double maxSpeed = 6;
		public double linearDrag;
		public double angulerDrag;

		private double _angleLimit;
		private double[] maxSpeeds = new double[] { 3, 3, 4, 6 };

		public int substeps = 18;
		public int iterations = 1;
		private double h;

		public GameObject segmentSprite;
		public GameObject hook;
		private HookSegment hookSegment;

		private Segment[] segments;//index 0 == end of rope
		private Particle[] particles;
		private int extendedSegments = 0;
		private int baseSegment = -1;
		private RopeMode mode = RopeMode.FLEXIBLE;

		private Rigidbody shipRigidbody;
		private double shipCorrection = .4;// reduces the effect of playerShip's translational movement on rope segments, 1 == no effect, 0 == normal

		private double baseOffset = .65;

		private Vector2d baseOrientation = Vector2d.zero;
		private Vector2d basePosition = Vector2d.zero;
		private Vector2d winchPosition = Vector2d.zero;
		private Vector2d hookPosition = Vector2d.zero;

		private Vector2d nextBaseOrientation = Vector2d.zero;
		private Vector2d nextBasePosition = Vector2d.zero;
		private Vector2d nextWinchPosition = Vector2d.zero;
		private Vector2d nextHookPosition = Vector2d.zero;

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
			segments = new Segment[maxSegments];
			particles = new Particle[maxSegments + 1];

			for (int i = 0; i < maxSegments + 1; i++) {//init particles
				particles[i] = new Particle(new Vector2d(basePosition.x, basePosition.y), Vector2d.zero, mass);
			}

			for (int i = 0; i < maxSegments; i++) {//init segments
				GameObject sprite = Instantiate(segmentSprite);
				segments[i] = new Segment(sprite, particles[i + 1], particles[i], inertia);	
			}

			//init hook
			GameObject hookObj = Instantiate(hook, new Vector3((float)particles[0].position.x, (float)particles[0].position.y, 0), Quaternion.identity);
			hookObj.AddComponent<HookSegment>();
			hookSegment = hookObj.GetComponent<HookSegment>();
			hookSegment.s = segments[0];

			maxSpeed = maxSpeeds[Mathf.Clamp(extendedSegments, 0, 3)];
			h = Time.fixedDeltaTime / substeps;

			shipRigidbody = transform.parent.GetComponent<Rigidbody>();

			updateBaseOrientation();
			updateBasePosition();
			updateWinchPosition();
			updateHookPosition();

			modeFlexible();
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.F)) {//change rope mode
				if (mode == RopeMode.FLEXIBLE)
					modeStiff();
				else
					modeFlexible();
			}

			if (Input.GetMouseButtonDown(1) && !extended && !autoRetract) {//auto extend rope
				autoExtend = true;
				shipCorrection = 1;
				hookSegment.active = true;
			}

			if (Input.GetMouseButtonDown(1) && extended && !autoExtend) {//auto retract rope
				autoRetract = true;
				hookSegment.active = false;
				hookSegment.unHook();
			}

			if (extended && Input.mouseScrollDelta.y != 0 && winchScrollBuffer == 0 && (extendedSegments < maxSegments || Input.mouseScrollDelta.y < 0))//wind/unwind rope 
				winchScrollBuffer = 4 * System.Math.Sign(Input.mouseScrollDelta.y);
		}

		private void FixedUpdate() {
			winchBrakeBuffer -= 1;

			updateBaseOrientation();
			updateBasePosition();
			updateWinchPosition();
			updateHookPosition();

			for (int i = baseSegment < 0 ? baseSegment : baseSegment + 1; i >= 0; i--) {//apply ship correction
				particles[i].setPosition(particles[i].position.x + shipRigidbody.velocity.x * Time.fixedDeltaTime * shipCorrection,
									     particles[i].position.y + shipRigidbody.velocity.y * Time.fixedDeltaTime * shipCorrection);
			}

			for (int i = 0; i < substeps; i++) {//main loop
				Simulate();
				for (int j = 0; j < iterations; j++) {
					ApplyConstraints();
				}
				adjustVelocities();
				solveVelocities();

				incrementPositions();
			}

			Debug.Log(particles[0].position);

			for (int i = 0; i < segments.Length; i++) {
				segments[i].updateSprite();
			}

			if (winchBrakeBuffer == 1) {
				maxSpeed = maxSpeeds[Mathf.Clamp(extendedSegments - 1, 0, 3)];
				winchBrakeBuffer = 0;
			}

			if (winchScrollBuffer != 0) {//apply scroll wind
				adjustWinch(length / 4 * System.Math.Sign(winchScrollBuffer));
				winchScrollBuffer -= winchScrollBuffer > 0 ? 1 : -1;
			}

			if (autoExtend) {//apply auto extention
				adjustWinch(winchForce);

				if (extendedSegments == maxSegments || hookSegment.isHooked) {
					autoExtend = false;
					shipCorrection = .4;
					maxSpeed = 1;
					winchBrakeBuffer = 4;
				}

				if (hookSegment.justHooked) {
					tightenRope();
					hookSnapshot.x = particles[0].position.x;
					hookSnapshot.y = particles[0].position.y;
				}
			}

			if (autoRetract) {//apply auto retraction
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
			for (int i = baseSegment < 0 ? baseSegment : baseSegment + 1; i >= 0; i--) {
				Particle p = particles[i];
				p.previousPosition.x = p.position.x;
				p.previousPosition.y = p.position.y;
				p.setPosition(p.position.x + h * p.velocity.x, p.position.y + h * p.velocity.y);
			}
		}

		//apply various positional and anguler constraints
		private void ApplyConstraints() {
			if (extendedSegments > 0) {//constrain base 
				anchorDistanceConstraint(winchPosition, segments[baseSegment].p1);
				distanceConstraint(segments[baseSegment], winchOffset);
				anchorAngleConstraint(baseOrientation, segments[baseSegment], winchOffset / length);
			}

			for (int i = baseSegment - 1; i >= 1; i--) {//constrain extended segments between base and hook
				distanceConstraint(segments[i - 1], length);
				distanceConstraint(segments[i], length);
				angleConstraint(segments[i - 1], segments[i]);
			}

			if (extendedSegments > 0 && autoExtend)//constrain hook
				anchorDistanceConstraint(hookPosition, particles[0]);

			if (tightLength > 0)
				anchorDistanceConstraint(hookSnapshot, particles[0]);

			for (int i = extendedSegments; i < segments.Length; i++) {
				segments[i].setOrientation(baseOrientation.x, baseOrientation.y);
			}

			for (int i = extendedSegments; i < particles.Length; i++) {//constrain unextended segments
				Particle p = particles[i];

				p.setPosition(basePosition.x, basePosition.y);
				p.previousPosition.x = p.position.x;
				p.previousPosition.y = p.position.y;
				p.velocity.x = 0;
				p.velocity.y = 0;
			}
		}

		private Vector2d anchorDiff = Vector2d.zero;
		
		//anchors a particle to a point with infinite mass/inertia
		private void anchorDistanceConstraint(Vector2d anchor, Particle p) {
			p.setPosition(anchor.x, anchor.y);
		}

		//constrains a segment's relative orientation to an anchor segment with infinite mass/inertia
		private void anchorAngleConstraint(Vector2d anchor, Segment s, double limiter) {
			double angle = Vector2d.SignedAngle(anchor, s.orientation);
			double limit = _angleLimit * limiter;

			if (System.Math.Abs(angle) >= limit) {
				double diff = angle - (angle > 0 ? limit : -limit);
				rotateOrientation(s, -diff);
			}
		}

		//constrains mutual orientation between two segemnts
		private void angleConstraint(Segment s1, Segment s2) {
			double angle = Vector2d.SignedAngle(s1.orientation, s2.orientation);
			double limit = _angleLimit;
			double ratio = (1 / s2.inertia) / (1 / s1.inertia + 1 / s2.inertia);

			if (System.Math.Abs(angle) > limit) {
				double difference = angle - (angle > 0 ? limit : -limit);
				//rotateOrientation(s1, .25f * difference * stiffness);
				rotateOrientation(s2, .5f * -difference * stiffness);
			}
		}

		private Vector2d sp2 = Vector2d.zero;
		private Vector2d sp1 = Vector2d.zero;

		private Vector2d direction = Vector2d.zero;

		private Vector2d correction1 = Vector2d.zero;
		private Vector2d correction2 = Vector2d.zero;

		//constrains distance between two segments
		private void distanceConstraint(Segment s, double distance) {
			sp1.x = s.p1.position.x;
			sp1.y = s.p1.position.y;

			sp2.x = s.p2.position.x;
			sp2.y = s.p2.position.y;

			direction.x = sp2.x - sp1.x;
			direction.y = sp2.y - sp1.y;
			double magnitude = distance - direction.magnitude;
			direction.normalize();

			correction1.x = direction.x * magnitude * .5;
			correction1.y = direction.y * magnitude * .5;

			correction2.x = direction.x * magnitude * .5;
			correction2.y = direction.y * magnitude * .5;

			s.p1.setPosition(sp1.x - correction1.x, sp1.y - correction1.y);
			s.p2.setPosition(sp2.x + correction2.x, sp2.y + correction2.y);
		}

		//update velocities to account for constraints
		private void adjustVelocities() {
			for (int i = extendedSegments; i >= 0; i--) {
				particles[i].velocity.x = (particles[i].position.x - particles[i].previousPosition.x) / h;
				particles[i].velocity.y = (particles[i].position.y - particles[i].previousPosition.y) / h;
			}
		}

		//clamp velocity and apply drag
		private void solveVelocities() {
			for (int i = extendedSegments; i >= 0; i--) {
				double newMaxSpeed = System.Math.Clamp(particles[i].velocity.magnitude, 0, maxSpeed * (extendedSegments - i));
				double mag = particles[i].velocity.magnitude;
				if (mag > 0) {
					particles[i].velocity.x = particles[i].velocity.x / mag * newMaxSpeed * linearDrag;
					particles[i].velocity.y = particles[i].velocity.y / mag * newMaxSpeed * linearDrag;
				}
				else {
					particles[i].velocity.x = 0;
					particles[i].velocity.y = 0;
				}
			}
		}

		private Vector2d hook2Base = Vector2d.zero;
		private double tightLength = 0;
		private double looseLength = 0;

		private void tightenRope() {
			hook2Base.x = particles[0].position.x - winchPosition.x;
			hook2Base.y = particles[0].position.y - winchPosition.y;

			looseLength = extendedSegments * length;
			tightLength = hook2Base.magnitude;
		}

		//winds/unwinds rope based on adjustment
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

			s.setOrientation(real.x - complex.y, real.y + complex.x);
		}

		private void updateBaseOrientation() {
			double rotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
			nextBaseOrientation.x = System.Math.Cos(rotation);
			nextBaseOrientation.y = System.Math.Sin(rotation);

			orientationVelocity.x = (nextBaseOrientation.x - baseOrientation.x) / substeps;
			orientationVelocity.y = (nextBaseOrientation.y - baseOrientation.y) / substeps;
		}

		private void updateBasePosition() {
			nextBasePosition.x = shipRigidbody.position.x + PlayerShipModel.impendingVelocity.x * Time.fixedDeltaTime + nextBaseOrientation.x * baseOffset;
			nextBasePosition.y = shipRigidbody.position.y + PlayerShipModel.impendingVelocity.y * Time.fixedDeltaTime + nextBaseOrientation.y * baseOffset;

			baseVelocity.x = (nextBasePosition.x - basePosition.x) / substeps;
			baseVelocity.y = (nextBasePosition.y - basePosition.y) / substeps;
		}

		private void updateWinchPosition() {
			nextWinchPosition.x = shipRigidbody.position.x + PlayerShipModel.impendingVelocity.x * Time.fixedDeltaTime + nextBaseOrientation.x * (baseOffset + winchOffset);
			nextWinchPosition.y = shipRigidbody.position.y + PlayerShipModel.impendingVelocity.y * Time.fixedDeltaTime + nextBaseOrientation.y * (baseOffset + winchOffset);

			winchVelocity.x = (nextWinchPosition.x - winchPosition.x) / substeps;
			winchVelocity.y = (nextWinchPosition.y - winchPosition.y) / substeps;
		}

		private void updateHookPosition() {
			nextHookPosition.x = winchPosition.x + nextBaseOrientation.x * (length * hookLag * extendedSegments);
			nextHookPosition.y = winchPosition.y + nextBaseOrientation.y * (length * hookLag * extendedSegments);

			hookVelocity.x = (nextHookPosition.x - hookPosition.x) / substeps;
			hookVelocity.y = (nextHookPosition.y - hookPosition.y) / substeps;
		}

		private void incrementPositions() {
			baseOrientation.x += orientationVelocity.x;
			baseOrientation.y += orientationVelocity.y;

			basePosition.x += baseVelocity.x;
			basePosition.y += baseVelocity.y;

			winchPosition.x += winchVelocity.x;
			winchPosition.y += winchVelocity.y;

			hookPosition.x += hookVelocity.x;
			hookPosition.y += hookVelocity.y;
		}

		private void modeStiff() {
			angleLimit = 6;
			_angleLimit = angleLimit * Mathf.Deg2Rad;
			linearDrag = .9985;
			angulerDrag = .985;

			mode = RopeMode.STIFF;
		}

		private void modeFlexible() {
			angleLimit = 35;
			_angleLimit = angleLimit * Mathf.Deg2Rad;
			linearDrag = .9998;
			angulerDrag = .995;

			mode = RopeMode.FLEXIBLE;
		}

		private Vector2 gizmo1 = new Vector3(0, 0, 70);
		private Vector2 gizmo2 = new Vector3(0, 0, 70);

		private void OnDrawGizmos() {
			if (!Application.isPlaying)
				return;

			for (int i = 0; i < maxSegments; i++) {
				Gizmos.color = i % 2 == 0 ? Color.green : Color.white;

				gizmo1.x = (float)segments[i].p1.position.x;
				gizmo1.y = (float)segments[i].p1.position.y;
				gizmo2.x = (float)segments[i].p2.position.x;
				gizmo2.y = (float)segments[i].p2.position.y;

				Gizmos.DrawLine(gizmo1, gizmo2);
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
 * - After hooking an enemy, spinning the ship around could but the ship into an auto spin state.  Exiting this state would require releasing the hooked enemy, or retracting the rope
 * - Rope could automatically retract win spinning to avoid any obstacles
 * - Could add some sort of auto spin button that would automatically spin the ship 180 or 360 degrees around
 */
