using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Rope {
    class VerletNode {
        
        public Vector2 position {
            get { return newPosition; }
            set { newPosition = value; }
        }
        public GameObject sprite;
        private Vector2 newPosition;
        public Vector2 oldPosition;
        public float oldAngle = 0f;
        public float oldAngleVelocity = 0f;
        public Vector2 oldVelocity;

        public VerletNode(GameObject sprite) {
            this.sprite = sprite;
            this.oldPosition = sprite.transform.position;
        }

        public void updateSpritePosition() {
            sprite.transform.position = newPosition;
        }
    }

    enum ColliderType {
        Circle,
        Box,
        None,
    }

    class CollisionInfo {
        public int id;

        public ColliderType colliderType;
        public Vector2 colliderSize;
        public Vector2 position;
        public Vector2 scale;
        public Matrix4x4 wtl;
        public Matrix4x4 ltw;
        public int numCollisions;
        public int[] collidingNodes;

        public CollisionInfo(int maxCollisions) {
            this.id = -1;
            this.colliderType = ColliderType.None;
            this.colliderSize = Vector2.zero;
            this.position = Vector2.zero;
            this.scale = Vector2.zero;
            this.wtl = Matrix4x4.zero;
            this.ltw = Matrix4x4.zero;

            this.numCollisions = 0;
            this.collidingNodes = new int[maxCollisions];
        }
    }

    public class Rope : MonoBehaviour {
        // Maximum number of colliders hitting the rope at once.
        private const int MAX_ROPE_COLLISIONS = 32;
        // Size of the collider buffer, also the maximum number of colliders that a single node can touch at once.
        private const int COLLIDER_BUFFER_SIZE = 8;

        [Min(2)]
        public int totalNodes = 200;
        public int iterations = 80;
        public float nodeDistance = 0.1f;
        public float nodeAngle = 30;
        [Min(0.001f)]
        public float stepTime = 0.01f;
        public float maxStep = 0.1f;

        public float drawWidth = 0.025f;
        public Vector2 gravity = new Vector2(0, -20f);
        public float collisionRadius = 0.5f;    // Collision radius around each node.  Set high to avoid tunneling.

        private VerletNode[] nodes;
        private CollisionInfo[] collisionInfos;
        private int numCollisions;
        private bool shouldSnapshotCollision;

        private Collider2D[] colliderBuffer;

        public GameObject grappleChain;

        private void Awake() {
            nodes = new VerletNode[totalNodes + 2];
            collisionInfos = new CollisionInfo[MAX_ROPE_COLLISIONS];
            for (int i = 0; i < collisionInfos.Length; i++) {
                collisionInfos[i] = new CollisionInfo(totalNodes);
            }

            // Buffer for OverlapCircleNonAlloc.
            colliderBuffer = new Collider2D[COLLIDER_BUFFER_SIZE];

            // Spawn nodes starting from the transform position and working down.
            Vector2 pos = transform.position;

            nodes[0] = new VerletNode(new GameObject("ChainStart"));
            nodes[0].position = pos;
            pos.y += nodeDistance;

            for (int i = 1; i < totalNodes+1; i++) {
                nodes[i] = new VerletNode(Instantiate(grappleChain, pos, Quaternion.identity));
                pos.y += nodeDistance;
            }

            nodes[totalNodes+1] = new VerletNode(new GameObject("ChainEnd"));
            nodes[totalNodes+1].position = pos;
        }

        private void Update() {
            if (shouldSnapshotCollision) {
                SnapshotCollision();
            }
        } 

        private void FixedUpdate() {
            shouldSnapshotCollision = true;

            Simulate();
            for (int i = 0; i < iterations; i++) {
                ApplyConstraints();
                AdjustCollisions();
            }

            for (int i = 0; i < nodes.Length; i++) {
                nodes[i].updateSpritePosition();
            }

            Vector2 axis = new Vector2(0, 1);
            for (int i = 1; i < nodes.Length - 1; i++) {
                Vector2 diff = nodes[i].position - nodes[i + 1].position;
                nodes[i].sprite.transform.rotation = Quaternion.AngleAxis(Vector2.SignedAngle(diff, axis) * -1f, Vector3.forward);
            }
        }

        private void SnapshotCollision() {
            Profiler.BeginSample("Snapshot");

            numCollisions = 0;
            // Loop through each node and get collisions within a radius.
            for (int i = 0; i < nodes.Length; i++) {
                int collisions =
                    Physics2D.OverlapCircleNonAlloc(nodes[i].position, collisionRadius, colliderBuffer);

                for (int j = 0; j < collisions; j++) {
                    Collider2D col = colliderBuffer[j];
                    int id = col.GetInstanceID();

                    int idx = -1;
                    for (int k = 0; k < numCollisions; k++) {
                        if (collisionInfos[k].id == id) {
                            idx = k;
                            break;
                        }
                    }

                    // If we didn't have the collider, we need to add it.
                    if (idx < 0) {
                        // Record all the data we need to use into our classes.
                        CollisionInfo ci = collisionInfos[numCollisions];
                        ci.id = id;
                        ci.wtl = col.transform.worldToLocalMatrix;
                        ci.ltw = col.transform.localToWorldMatrix;
                        ci.scale.x = ci.ltw.GetColumn(0).magnitude;
                        ci.scale.y = ci.ltw.GetColumn(1).magnitude;
                        ci.position = col.transform.position;
                        ci.numCollisions = 1; // 1 collision, this one.
                        ci.collidingNodes[0] = i;

                        switch (col) {
                            case CircleCollider2D c:
                                ci.colliderType = ColliderType.Circle;
                                ci.colliderSize.x = ci.colliderSize.y = c.radius;
                                break;
                            case BoxCollider2D b:
                                ci.colliderType = ColliderType.Box;
                                ci.colliderSize = b.size;
                                break;
                            default:
                                ci.colliderType = ColliderType.None;
                                break;
                        }

                        numCollisions++;
                        if (numCollisions >= MAX_ROPE_COLLISIONS) {
                            Profiler.EndSample();
                            return;
                        }

                        // If we found the collider, then we just have to increment the collisions and add our node.
                    }
                    else {
                        CollisionInfo ci = collisionInfos[idx];
                        if (ci.numCollisions >= totalNodes) {
                            continue;
                        }

                        ci.collidingNodes[ci.numCollisions++] = i;
                    }
                }
            }

            shouldSnapshotCollision = false;

            Profiler.EndSample();
        }

        private void Simulate() {
            
            nodes[0].oldPosition = nodes[0].position;
            nodes[0].position = new Vector2(Mathf.Cos((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad) * 1.5f + transform.position.x,
                                                 Mathf.Sin((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad) * 1.5f + transform.position.y);
            
            for (int i = 1; i < nodes.Length; i++) {
                VerletNode node = nodes[i];
                Vector2 temp = node.position;
                Vector2 velocity = node.position - node.oldPosition;

                Vector2 newVelocity = node.oldVelocity + (velocity - node.oldVelocity) / 150.0f;

                node.position += newVelocity.normalized * Mathf.Min(newVelocity.magnitude, .15f) * .7f; 
                node.oldVelocity = node.position - node.oldPosition;
                node.oldPosition = temp;
            }
        }

        private void ApplyConstraints() {
            Vector2 axis = transform.position;

            nodes[0].position = new Vector2(Mathf.Cos((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad) * 1.5f + transform.position.x,
                                            Mathf.Sin((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad) * 1.5f + transform.position.y);

            for (int i = 0; i < nodes.Length - 1; i++) {
                VerletNode node1 = nodes[i];
                VerletNode node2 = nodes[i + 1];

                //Current distance between rope nodes.
                float diffX = node1.position.x - node2.position.x;
                float diffY = node1.position.y - node2.position.y;
                float dist = Vector2.Distance(node1.position, node2.position);
                float difference = 0;
                // Guard against divide by 0.
                if (dist > 0) {
                    difference = (nodeDistance - dist) / dist;
                }

                Vector2 translate = new Vector2(diffX, diffY) * (.5f * difference);

                node1.position += translate;
                node2.position -= translate;

                axis = node1.position - axis;

                Vector2 diff = node2.position - node1.position;
                dist = Vector2.Distance(node1.position, node2.position);
                float angle = Vector2.SignedAngle(diff, axis) * -1;
                float baseAngle = Vector2.SignedAngle(axis, new Vector2(0, 1)) * -1;

                if (Math.Abs(angle) > nodeAngle) {
                    float worldAngle = baseAngle + (angle >= 0 ? nodeAngle : -nodeAngle) + 90;
                    node2.position = node1.position + new Vector2(Mathf.Cos(worldAngle * Mathf.Deg2Rad) * dist, Mathf.Sin(worldAngle * Mathf.Deg2Rad) * dist);
                    node2.oldAngleVelocity = 0;
                    node2.oldAngle = angle > 0 ? nodeAngle : -nodeAngle;
                }

                axis = node1.position;
            }
        }

        private void AdjustCollisions() {
            Profiler.BeginSample("Collision");

            for (int i = 0; i < numCollisions; i++) {
                CollisionInfo ci = collisionInfos[i];

                switch (ci.colliderType) {
                    case ColliderType.Circle: {
                            float radius = ci.colliderSize.x * Mathf.Max(ci.scale.x, ci.scale.y);

                            for (int j = 0; j < ci.numCollisions; j++) {
                                VerletNode node = nodes[ci.collidingNodes[j]];
                                float distance = Vector2.Distance(ci.position, node.position);

                                // Early out if we're not colliding.
                                if (distance - radius > 0) {
                                    continue;
                                }

                                Vector2 dir = (node.position - ci.position).normalized;
                                Vector2 hitPos = ci.position + dir * radius;
                                node.position = hitPos;
                            }
                        }
                        break;

                    case ColliderType.Box: {
                            for (int j = 0; j < ci.numCollisions; j++) {
                                VerletNode node = nodes[ci.collidingNodes[j]];
                                Vector2 localPoint = ci.wtl.MultiplyPoint(node.position);

                                // If distance from center is more than box "radius", then we can't be colliding.
                                Vector2 half = ci.colliderSize * .5f;
                                Vector2 scalar = ci.scale;
                                float dx = localPoint.x;
                                float px = half.x - Mathf.Abs(dx);
                                if (px <= 0) {
                                    continue;
                                }

                                float dy = localPoint.y;
                                float py = half.y - Mathf.Abs(dy);
                                if (py <= 0) {
                                    continue;
                                }

                                // Need to multiply distance by scale or we'll mess up on scaled box corners.
                                if (px * scalar.x < py * scalar.y) {
                                    float sx = Mathf.Sign(dx);
                                    localPoint.x = half.x * sx;
                                }
                                else {
                                    float sy = Mathf.Sign(dy);
                                    localPoint.y = half.y * sy;
                                }

                                Vector2 hitPos = ci.ltw.MultiplyPoint(localPoint);
                                node.position = hitPos;
                            }
                        }
                        break;
                }
            }

            Profiler.EndSample();
        }

        private void OnDrawGizmos() {
            if (!Application.isPlaying) {
                return;
            }
            for (int i = 0; i < nodes.Length - 1; i++) {
                if (i % 2 == 0) {
                    Gizmos.color = Color.green;
                } else {
                    Gizmos.color = Color.white;
                }
                Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
            }
        }
    }
}
