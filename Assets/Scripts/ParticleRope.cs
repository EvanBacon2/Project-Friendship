using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendShipRope {
	class Particle {
		public Vector2 position;
		public Vector2 previousPosition;
		public Vector2 velocity;
		public float mass;

		public Particle(Vector2 position, float mass) {
			this.position = position;
			this.mass = mass;
		}
	}

	class Segment {
		public Vector2 position;
		public Vector2 previousPosition;
		public Vector2 velocity;
		public float mass;

		public Vector2 orientation;
		public Vector2 previousOrientation;
		public float angulerVecocity;
		public float inertia;

		public float length;

		public Vector2 p1 { 
			get { return position - orientation.normalized * length / 2; }
			set { position = value + orientation.normalized * length / 2; }
		}
		public Vector2 p2 { 
			get { return position + orientation.normalized * length / 2; }
			set { position = value - orientation.normalized * length / 2; }
		}

		public Segment(Vector2 position, Vector2 orientation, float mass, float inertia, float length) {
			this.position = position;
			this.orientation = orientation;
			this.mass = mass;
			this.inertia = inertia;
			this.length = length;
		}
	}

	/*public class ParticleRope : MonoBehaviour {
		public int particleCount = 20;
		public float segmentLength = .5f;
		public float particleMass = 1;

		public int substeps = 20;
		public int iterations = 1;

		private Particle[] rope;

		private void Awake() {
			rope = new Particle[particleCount];

			//collision stuff

			Vector2 currentPosition = basePosition();
			for (int i = 0; i < rope.Length; i++) {
				rope[i] = new Particle(currentPosition, particleMass);
				currentPosition.y += segmentLength;
			}
		}

		private void FixedUpdate() {
			float h = Time.fixedDeltaTime / substeps;
			for (int i = 0; i < substeps; i++) {
				Simulate(h);
				for (int j = 0; j < iterations; j++) {
					ApplyConstraints();
				}
				adjustVelocities(h);
			}
		}

		private void Simulate(float h) {
			for (int i = 0; i < rope.Length; i++) {
				Particle particle = rope[i];

				particle.previousPosition = particle.position;
				particle.position += h * particle.velocity;
			}
		}

		private void ApplyConstraints() {
			rope[0].position = basePosition();

			for (int i = 1; i < rope.Length; i++) {
				distanceConstraint(rope[i - 1], rope[i]);
			}
		}

		private void distanceConstraint(Particle p1, Particle p2) {
			Vector2 direction = p1.position - p2.position;
			float magnitude = segmentLength - direction.magnitude;

			float inverseMass1 = 1 / p1.mass;
			float inverseMass2 = 1 / p2.mass;
			float ratio = inverseMass1 / (inverseMass1 + inverseMass2);

			p1.position += direction.normalized * magnitude * ratio;
			p2.position -= direction.normalized * magnitude * (1 - ratio);
		}

		private void adjustVelocities(float h) {
			for (int i = 0; i < rope.Length; i++) {
				rope[i].velocity = (rope[i].position - rope[i].previousPosition) / h;			
			}
		}

		private Vector2 basePosition() {
			float baseRotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
			return (Vector2)transform.position + new Vector2(Mathf.Cos(baseRotation), Mathf.Sin(baseRotation)) * 1.5f;
		}

		private void OnDrawGizmos() {
			if (!Application.isPlaying)
				return;

			for (int i = 0; i < rope.Length - 1; i++) {
				Gizmos.color = i % 2 == 0 ? Color.green : Color.white;
				Gizmos.DrawLine(rope[i].position, rope[i + 1].position);
			}
		}
	}*/

	public class ParticleRope : MonoBehaviour {
		public int segmentCount = 20;
		public float segmentLength = .5f;
		public float segmentAngleLimit = 30;
		public float segmentMass = 1;
		public float segmentInertia = 1;

		public int substeps = 20;
		public int iterations = 1;

		private Segment[] rope;

		private void Awake() {
			rope = new Segment[segmentCount];

			//collision stuff

			Vector2 currentPosition = basePosition();
			currentPosition.y += segmentLength / 2;
			for (int i = 0; i < rope.Length; i++) {
				rope[i] = new Segment(currentPosition, Vector2.up, segmentMass, segmentInertia, segmentLength);
				currentPosition.y += segmentLength;
			}
		}

		private void Update() {
			//more collision stuff
		}

		private void FixedUpdate() {
			float h = Time.fixedDeltaTime / substeps;
			for (int i = 0; i < substeps; i++) {
				Simulate(h);
				for (int j = 0; j < iterations; j++) {
					ApplyConstraints();
				}
				adjustVelocities(h);
				solveVelocities();
			}
		}

		private void Simulate(float h) {
			for (int i = 0; i < rope.Length; i++) {
				Segment segment = rope[i];

				segment.previousPosition = segment.position;
				segment.position += h * segment.velocity;

				segment.previousOrientation = segment.orientation;
				rotateOrientation(segment, segment.angulerVecocity);
			}
		}

		private void ApplyConstraints() {
			baseConstraint();

			for (int i = 1; i < rope.Length; i++) {
				//angleConstraint(rope[i - 1], rope[i]);
				distanceConstraint(rope[i - 1], rope[i]);
			}
		}

		private void baseConstraint() {
			Vector2 difference = basePosition() - rope[0].p1;
			rope[0].p1 += difference;

			//float angle = Mathf.Asin(Vector3.Cross(rope[0].p1.normalized, -difference.normalized).z) * Mathf.Rad2Deg;
			//rotateOrientation(rope[0], angle);
		}

		private void angleConstraint(Segment s1, Segment s2) {
			float angle = Mathf.Asin(Vector3.Cross(s1.orientation, s2.orientation).z) * Mathf.Rad2Deg;
			if (angle < -segmentAngleLimit || angle > segmentAngleLimit) {
				float difference = angle - (angle > 0 ? segmentAngleLimit : -segmentAngleLimit);
				rotateOrientation(s1, difference);
				rotateOrientation(s2, -difference);
			}
		}

		private void distanceConstraint(Segment s1, Segment s2) {
			Vector2 direction = s1.p2 - s2.p1;
			float magnitude = -direction.magnitude;
			direction = direction.normalized;

			Vector2 r1 = s1.p2 - s1.position;
			Vector2 r2 = s2.p1 - s2.position;
		
			float inverseMass1 = 1 / s1.mass + Mathf.Pow(Vector3.Cross(r1, direction).z, 2) * (1 / s1.inertia);
			float inverseMass2 = 1 / s2.mass + Mathf.Pow(Vector3.Cross(r2, direction).z, 2) * (1 / s2.inertia);
			float ratio = inverseMass1 / (inverseMass1 + inverseMass2);
			
			Vector2 correction1 = direction * magnitude * ratio;
			Vector2 correction2 = direction * magnitude * (1 - ratio);

			s1.p2 += correction1;
			s2.p1 -= correction2;

			float angle1 = correction1.magnitude != 0 ? Mathf.Asin(Mathf.Clamp(Vector3.Cross(r1, correction1).z / (r1.magnitude * correction1.magnitude), -1, 1)) : 0;
			float angle2 = correction2.magnitude != 0 ? Mathf.Asin(Mathf.Clamp(Vector3.Cross(r2, correction2).z / (r2.magnitude * correction2.magnitude), -1, 1)) : 0;

			Debug.Log(Vector3.Cross(r1, correction1).z / (r1.magnitude * correction1.magnitude));

			s1.orientation = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1));
			s2.orientation = new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2));
			//rotateOrientation(s1, angle1);
			//rotateOrientation(s2, angle2);
		}

		private void adjustVelocities(float h) {
			for (int i = 0; i < rope.Length; i++) {
				rope[i].velocity = (rope[i].position - rope[i].previousPosition) / h;
				rope[i].angulerVecocity = (Mathf.Atan(rope[i].orientation.y / rope[i].orientation.x) - Mathf.Atan(rope[i].previousOrientation.y / rope[i].previousOrientation.x)) * Mathf.Rad2Deg / h;
			}
		}

		private void solveVelocities() {
		
		}

		private Vector2 basePosition() {
			float baseRotation = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
			return (Vector2)transform.position + new Vector2(Mathf.Cos(baseRotation), Mathf.Sin(baseRotation)) * .6f;
		}

		private void rotateOrientation(Segment s, float rotation) {
			Vector2 real = Mathf.Cos(rotation) * s.orientation;
			Vector2 complex = Mathf.Sin(rotation) * s.orientation;
			s.orientation = new Vector2(real.x - complex.y, real.y + complex.x).normalized;
		}

		private void OnDrawGizmos() {
			if (!Application.isPlaying) 
				return;

			for (int i = 0; i < rope.Length; i++) {
				Gizmos.color = i % 2 == 0 ? Color.green : Color.white;
				Gizmos.DrawLine(rope[i].p1, rope[i].p2);
			}
		}
	}
}
