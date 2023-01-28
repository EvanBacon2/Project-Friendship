using UnityEngine;
public enum RopeMode {
	FLEXIBLE,
	STIFF,
}

namespace SegmentRope {
	//Defines the physical characteristics of the rope
	

	//A physically simulated rope that is constructed by linking together a series of line segments
	public class SegmentRope : MonoBehaviour {
		public int maxSegments;
		public double length;//length of an individual segment
		public double angleLimit;//max difference, in degrees, between the rotations, in relation to the origin, of two segments
		public double mass;
		public double inertia;
		public double stiffness;
		public double stretchiness;
		public double maxSpeed = 6;// max speed of baseSegment, maxSpeed for each subsequent segment increases by a factor determined by maxSpeedScale
		public double maxSpeedScale = 1;// 1 == linear, >1 == exponential, <1 == logarithmic
		public double linearDrag;
		public double angulerDrag;

		private double _angleLimit;
		private double _inverseMass;
		private double _inverseInertia;
		private double _linearDrag;
		private double _angulerDrag;

		public int substeps = 18;
		private double h;

		public GameObject segmentSprite;
		public GameObject hook;
		private HookSegment hookSegment;

		private Segment[] rope;
		public int extendedSegments = 0;
		private int baseSegment = -1;
		private RopeMode mode = RopeMode.FLEXIBLE;

		private Rigidbody baseRB;
		public double shipCorrection = 1;// reduces the effect of playerShip's translational movement on rope segments, 1 == no effect, 0 == full effect

		private double baseOffset = .65;

		private Vector2d baseOrientation = Vector2d.zero;// orientation of player ship
		private Vector2d basePosition = Vector2d.zero;// position of any unextended segment's p1
		private Vector2d winchPosition = Vector2d.zero;// position of baseSegment's p1
		private Vector2d hookPosition = Vector2d.zero;//  position of hookSegment's p2 while auto extending rope.
		
		private double interRotation = 0;
		private double rotationStep = 0;

		private Vector2d interPosition = Vector2d.zero;
		private Vector2d positionStep = Vector2d.zero;

		private double winchForce = 3;
		private double winchOffset = 0;
		private double winchScrollBuffer = 0;
		private double winchBrakeBuffer = 0;
		private double hookLag = .92;

		private Vector2d hookSnapshot = Vector2d.zero;

		private bool extended = false;
		private bool autoExtend = false;
		private bool autoRetract = false;

		private void setAngleLimit(double angleLimit) {
			_angleLimit = angleLimit * Mathf.Deg2Rad;
		}

		private void setLinearDrag(double linearDrag) {
			_linearDrag = System.Math.Pow(linearDrag, substeps);
		}

		private void setAngulerDrag(double angulerDrag) {
			_angulerDrag = System.Math.Pow(angulerDrag, substeps);
		}

		private void Start() {
			setAngleLimit(angleLimit);
			_inverseMass = 1.0 / mass;
			_inverseInertia = 1.0 / inertia;
			setLinearDrag(linearDrag);
			setAngulerDrag(angulerDrag);

			//create rope segments
			rope = new Segment[maxSegments];
			for (int i = 0; i < maxSegments; i++) {
				//GameObject sprite = Instantiate(segmentSprite);
				rope[i] = new Segment(new Vector2d(basePosition.x, basePosition.y), Vector2d.up, mass, inertia, length);
			}

			//create hook segment
			GameObject hookObj = Instantiate(hook, new Vector3((float)rope[0].position.x, (float)rope[0].position.y, 0), Quaternion.identity);
			hookObj.AddComponent<HookSegment>();
			hookSegment = hookObj.GetComponent<HookSegment>();
			hookSegment.s = rope[0];

			h = Time.fixedDeltaTime / substeps;

			baseRB = transform.parent.GetComponent<Rigidbody>();

			interPosition.x = baseRB.position.x;
			interPosition.y = baseRB.position.y;
			interRotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
			updateShipInterpolation();

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

				modeFlexible();
				setAngulerDrag(.99);
				maxSpeed = 55;
			}

			//wind/unwind rope 
			if (extended && Input.mouseScrollDelta.y != 0 && winchScrollBuffer == 0 && (extendedSegments < maxSegments || Input.mouseScrollDelta.y < 0))
				winchScrollBuffer = 4 * System.Math.Sign(Input.mouseScrollDelta.y);
		}

		private void FixedUpdate() {
			winchBrakeBuffer -= 1;

			updateShipInterpolation();

			//apply ship correction
			for (int i = baseSegment; i >= 0; i--) {
				rope[i].setPosition(rope[i].position.x + baseRB.velocity.x * Time.fixedDeltaTime * shipCorrection, 
									rope[i].position.y + baseRB.velocity.y * Time.fixedDeltaTime * shipCorrection);
			}

			//main loop
			for (int i = 0; i < substeps; i++) {
				Simulate();
				ApplyConstraints();
				adjustVelocities();
				solveVelocities();
				interpolateShipPositions();
			}
			
			for (int i = 0; i < rope.Length; i++) {
				//rope[i].updateSprite();
			}

			if (winchBrakeBuffer == 1) {
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

				modeFlexible();
				maxSpeed = 40;

				if (extendedSegments == maxSegments || hookSegment.isHooked) {//stop extending
					autoExtend = false;
					shipCorrection = 0;
					winchBrakeBuffer = 4;

					maxSpeed = 25;
				}

				if (hookSegment.justHooked) {//tighten rope after hooking object
					tightenRope();
					hookSnapshot.x = rope[0].position.x;
					hookSnapshot.y = rope[0].position.y;
				}
			}

			//apply auto retraction
			if (autoRetract) {
				adjustWinch(-length);
				if (!extended) {//stop retracting
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

		/*
		 * A constraint which anchors a segment to a point with infinite mass/inertia.
		 */
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
				rotateOrientation(rope[segmentIndex], -diff * stiffness);
			}
		}

		/*
		* Constrains the mutual orientation between two segemnts
		*/
		private void angleConstraint(Segment s1, Segment s2) {
			double angle = Vector2d.SignedAngle(s1.orientation, s2.orientation);
			double limit = _angleLimit;
			double ratio = s2.inverseInertia / (s1.inverseInertia + s2.inverseInertia);

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

		/*
		 * Constrains the distance between two segments
		 */
		private void distanceConstraint(Segment s1, Segment s2) {
			s1p2.x = s1.p2.x;
			s1p2.y = s1.p2.y;

			s2p1.x = s2.p1.x;
			s2p1.y = s2.p1.y;

			//direction of gap between the ends of each segment
			direction.x = s2p1.x - s1p2.x;
			direction.y = s2p1.y - s1p2.y;

			//calculate radius
			r1.x = s1p2.x - s1.position.x;
			r1.y = s1p2.y - s1.position.y;

			r2.x = s2p1.x - s2.position.x;
			r2.y = s2p1.y - s2.position.y;

			double inverseMass1 = s1.inverseMass + System.Math.Pow(Vector2d.cross(r1, direction), 2) * s1.inverseInertia;
			double inverseMass2 = s2.inverseMass + System.Math.Pow(Vector2d.cross(r2, direction), 2) * s2.inverseInertia;
			double ratio = inverseMass1 / (inverseMass1 + inverseMass2);
			
			correction1.x = direction.x * ratio;
			correction1.y = direction.y * ratio;

			correction2.x = direction.x * (1 - ratio);
			correction2.y = direction.y * (1 - ratio);

			double torque1 = Vector2d.cross(r1, correction1);
			double torque2 = Vector2d.cross(r2, correction2);

			s1.setP2(s1.p2.x + correction1.x * stretchiness, s1.p2.y + correction1.y * stretchiness);
			s2.setP1(s2.p1.x - correction2.x * stretchiness, s2.p1.y - correction2.y * stretchiness);

			rotateOrientation(s1, .5f * torque1);
			rotateOrientation(s2, .5f * -torque2);
		}
		
		/*
		 * Updates the velocities to account for constraints
		 */
		private void adjustVelocities() {
			for (int i = extendedSegments - 1; i >= 0; i--) {
				rope[i].velocity.x = (rope[i].position.x - rope[i].previousPosition.x) / h;
				rope[i].velocity.y = (rope[i].position.y - rope[i].previousPosition.y) / h;
				rope[i].angulerVelocity = Vector2d.SignedAngle(rope[i].previousOrientation, rope[i].orientation) / h;
			}
		}

		Vector2d linearDiff = Vector2d.zero;
		double angulerDiff = 0;

		/*
		 * Clamps the velocity and applies drag
		 */
		private void solveVelocities() {
			//clamp magnitude of all extended segments
			for (int i = extendedSegments - 1; i >= 0; i--) {
				double mag = rope[i].velocity.magnitude;
				double clampedMag = System.Math.Min(mag, maxSpeed * System.Math.Pow(extendedSegments - i, maxSpeedScale));

				if (mag > 0) {
					rope[i].velocity.x = rope[i].velocity.x / mag * clampedMag;
					rope[i].velocity.y = rope[i].velocity.y / mag * clampedMag;
				}
			}

			//apply drag to base segment
			if (extendedSegments >= 1) {
				linearDiff.x = (rope[baseSegment].velocity.x - baseRB.velocity.x) * _linearDrag;
				linearDiff.y = (rope[baseSegment].velocity.y - baseRB.velocity.y) * _linearDrag;
				angulerDiff = (rope[baseSegment].angulerVelocity - rotationStep / h) * _angulerDrag;
				
				rope[baseSegment].velocity.x -= linearDiff.x;
				rope[baseSegment].velocity.y -= linearDiff.y;
				rope[baseSegment].angulerVelocity -= angulerDiff;
			}
			
			//apply drag to all other extended segments
			for (int i = extendedSegments - 1; i >= 1; i--) {
				linearDiff.x = (rope[i - 1].velocity.x - rope[i].velocity.x) * _linearDrag;
				linearDiff.y = (rope[i - 1].velocity.y - rope[i].velocity.y) * _linearDrag;
				angulerDiff = (rope[i - 1].angulerVelocity - rope[i].angulerVelocity) * _angulerDrag;
				
				rope[i].velocity.x += linearDiff.x * .5;
				rope[i].velocity.y += linearDiff.y * .5;
				rope[i].angulerVelocity += angulerDiff * .5;

				rope[i - 1].velocity.x -= linearDiff.x * .5;
				rope[i - 1].velocity.y -= linearDiff.y * .5;
				rope[i - 1].angulerVelocity -= angulerDiff * .5;
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

		private void updateShipInterpolation() {
			double nextRotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;

			//When prev and next cross over the 0 degree mark angle goes from doublePI to zero and vice versa.  To ensure angle difference is accurate
			//prevRotation is adjusted so that both it and nextRotation are either > or < doublePI.    
			if (System.Math.Clamp(interRotation, doublePI, doublePI + halfPI) == interRotation && System.Math.Clamp(nextRotation, halfPI, PI) == nextRotation)
				interRotation -= doublePI;
			if (System.Math.Clamp(interRotation, halfPI, PI) == interRotation && System.Math.Clamp(nextRotation, doublePI, doublePI + halfPI) == nextRotation)
				interRotation += doublePI;

			rotationStep = (nextRotation - interRotation) / substeps;

			double nextPositionX = baseRB.position.x + RECSRigidBody.impendingVelocity.x * Time.fixedDeltaTime;
			double nextPositionY = baseRB.position.y + RECSRigidBody.impendingVelocity.y * Time.fixedDeltaTime;

			positionStep.x = (nextPositionX - interPosition.x) / substeps;
			positionStep.y = (nextPositionY - interPosition.y) / substeps;
		}

		private void interpolateShipPositions() {
			interRotation += rotationStep;
			interPosition.x += positionStep.x;
			interPosition.y += positionStep.y;

			baseOrientation.x = System.Math.Cos(interRotation);
			baseOrientation.y = System.Math.Sin(interRotation);

			basePosition.x = interPosition.x + positionStep.x + baseOrientation.x * baseOffset;
			basePosition.y = interPosition.y + positionStep.y + baseOrientation.y * baseOffset;

			winchPosition.x = interPosition.x + positionStep.x + baseOrientation.x * (baseOffset + winchOffset);
			winchPosition.y = interPosition.y + positionStep.y + baseOrientation.y * (baseOffset + winchOffset);

			hookPosition.x = winchPosition.x + baseOrientation.x * (length * hookLag * extendedSegments);
			hookPosition.y = winchPosition.y + baseOrientation.y * (length * hookLag * extendedSegments);
		}

		private void modeStiff() {
			setAngleLimit(6);
			maxSpeed = 6;
			maxSpeedScale = 1;
			setLinearDrag(.98);
			setAngulerDrag(.98);

			mode = RopeMode.STIFF;
		}

		private void modeFlexible() {
			setAngleLimit(35);
			maxSpeed = 25;
			maxSpeedScale = .1;
			setLinearDrag(0);
			setAngulerDrag(.95);

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
 * 
 * Bugs
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