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
        public float collisionRadius = 0.5f;    // Collision radius around each node.  Set high to avoid tunneling.
        public float linearInertia = 10;
        public float rotationalInertia = 10;
        public float linearScaling = 1;
        public float rotationalScaling = 1;
        public float spring = 30;

        private Vector2 oldBase = new Vector2(0,1);

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
            Vector2 newBase = new Vector2(Mathf.Cos((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad), Mathf.Sin((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad));
            Vector2 baseVelocity = newBase - oldBase;
            nodes[0].oldPosition = nodes[0].position;
            nodes[0].position = new Vector2(newBase.x + transform.position.x, newBase.y + transform.position.y);

            //float radius = 1.5f;
            //Debug.Log(transform.rotation.eulerAngles.z);
            for (int i = 1; i < nodes.Length; i++) {
                VerletNode node = nodes[i];
                Vector2 temp = node.position;
                //if (i == 1)
                    //Debug.Log("old: " + (node.oldPosition - nodes[0].position).normalized + " " + "new: " + (node.position - nodes[0].position).normalized);
                //radius += nodeDistance;
                //node.position += baseVelocity * (radius / 1.5f) * .2f * nodes.Length / i;
                Vector2 velocity = node.position - node.oldPosition;
                //float magChange = velocity.magnitude - node.oldVelocity.magnitude;
                //Vector2 dirChange = velocity.normalized - node.oldVelocity.normalized;

                //Vector2 newVelocity = (node.oldVelocity.normalized + dirChange /* (rotationalInertia* (i/10f))*/).normalized * (node.oldVelocity.magnitude + magChange /*/ (linearInertia * (i/5f))*/) * .996f; 

                node.position += velocity + new Vector2(0, 0) * Time.fixedDeltaTime * Time.fixedDeltaTime;

                //node.oldVelocity = newVelocity;
                node.oldPosition = temp;

                //if (i == 1)
                    //Debug.Log("uncon" + node.position);
                
            }

            oldBase = newBase;
        }

        private void ApplyConstraints() {
            //Vector2 axis = transform.position;

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

                Vector2 point1 = i == 0 ? (Vector2)transform.position : nodes[i - 1].position;
                Vector2 point2 = nodes[i + 1].position;

                float length1 = i == 0 ? 1.5f : (nodes[i].position - point1).magnitude;
                float length2 = (point2 - nodes[i].position).magnitude;

                float idealBridgeLength = new Vector2(length2 * .5f, length2 * Mathf.Sqrt(3) * .5f + length1).magnitude;
                Vector2 bridge = point1 - point2;

                float bridgeLength = bridge.magnitude;
                float bridgeDiff = 0;

                if (bridgeLength > 0)
                    bridgeDiff = (idealBridgeLength - bridgeLength) / bridgeLength;

                if (bridgeDiff > 0) {
                    Vector2 bridgeTranslate = bridge * ((i == 0 ? 1f : .5f) * bridgeDiff);

                    if (i > 0)
                        nodes[i - 1].position += bridgeTranslate;
                    nodes[i + 1].position -= bridgeTranslate;
                }

                    /*Vector2 minusOne = nodes[i - 1].position - nodes[i].position;
                    Vector2 plusOne = nodes[i + 1].position - nodes[i].position;

                    float angle = Vector2.SignedAngle(minusOne, plusOne);
                    float angleDiff = 180 - Mathf.Abs(angle) - nodeAngle;

                    if (angleDiff > 0) {
                        Vector2 arm = nodes[i].position - nodes[i - 1].position;
                        Vector2 bridge = (nodes[i + 1].position - nodes[i - 1].position) / 2f; 
                        Vector2 midPoint = nodes[i - 1].position + bridge;
                        float length = Mathf.Tan(nodeAngle / 2 * Mathf.Deg2Rad) * bridge.magnitude;

                        bool quad1or3 = (bridge.x >= 0 && bridge.y >= 0) || (bridge.x <= 0 && bridge.y <= 0);
                        bool bridgeAhead = Vector2.SignedAngle(arm, bridge) > 0;
                        Vector2 normal = new Vector2(bridge.y, bridge.x).normalized;
                        
                        if (quad1or3 ^ bridgeAhead) 
                            normal.x *= -1;
                        else 
                            normal.y *= -1;

                        Debug.Log("old " + nodes[i].position);
                        nodes[i].position = midPoint + normal * length;
                        Debug.Log("new " + nodes[i].position);
                    }*/
                /*} else {
                    Vector2 axis = nodes[0].position - (Vector2)transform.position;
                    Vector2 diff = node2.position - node1.position;
                    dist = Vector2.Distance(node2.position, node1.position);
                    float angle = Vector2.SignedAngle(axis, diff);
                    float baseAngle = Vector2.SignedAngle(new Vector2(1, 0), axis);
                    float angleDiff = (angle >= 0 ? nodeAngle - angle : Mathf.Abs(angle + nodeAngle)) / (spring > 0 ? spring : 1);

                    if (Math.Abs(angle) > nodeAngle) {
                        float worldAngle = baseAngle + angle + angleDiff;
                        Vector2 newPosition = node1.position + new Vector2(Mathf.Cos(worldAngle * Mathf.Deg2Rad) * dist, Mathf.Sin(worldAngle * Mathf.Deg2Rad) * dist);
                        Vector2 posChange = (newPosition - node2.position).normalized;
                        nodes[1].position = newPosition;


                        (for (int j = 1; j < nodes.Length; j++) {
                            posChange *= (nodes[j].position - nodes[j - 1].position).magnitude / posChange.magnitude;
                            nodes[j].position += posChange;
                        }
                    }
                }*/

                
                /*Vector2 minusOne = (i == 0 ? (Vector2)transform.position : nodes[i - 1].position) - nodes[i].position;
                Vector2 plusOne = nodes[i + 1].position - nodes[i].position;

                float angle = Vector2.SignedAngle(minusOne, plusOne);
                float angleDiff = 180 - Mathf.Abs(angle) - nodeAngle;

                float minusAngle = Vector2.SignedAngle(new Vector2(1, 0), minusOne);
                float plusAngle = Vector2.SignedAngle(new Vector2(1, 0), plusOne);

                //if (i == 0)
                //Debug.Log(angleDiff);

                if (angleDiff > 0) {
                    //if (i == 1)
                    //  Debug.Log("no diff " + plusAngle + " " + minusAngle);

                    angleDiff = (angle >= 0 ? 1 : -1) * .5f;

                    minusAngle = (minusAngle - angleDiff) * Mathf.Deg2Rad;

                    //if (i == 0)
                        //plusAngle = (plusAngle + 2 * angleDiff) * Mathf.Deg2Rad;
                    //else
                        plusAngle = (plusAngle + angleDiff) * Mathf.Deg2Rad;



                    /*if (i == 1) {
                        Debug.Log("diff " + plusAngle * Mathf.Rad2Deg + " " + minusAngle * Mathf.Rad2Deg);
                        Debug.Log("oldPos " + plusOne.normalized);
                    }

                    if (i > 0)
                        nodes[i - 1].position = nodes[i].position + new Vector2(Mathf.Cos(minusAngle), Mathf.Sin(minusAngle)) * minusOne.magnitude;
                    nodes[i + 1].position = nodes[i].position + new Vector2(Mathf.Cos(plusAngle), Mathf.Sin(plusAngle)) * plusOne.magnitude;

                    //if (i == 1)
                    //Debug.Log("newPos " + (nodes[i + 1].position - nodes[i].position).normalized);
                }*/
                

                /*axis = node1.position - axis;

                Vector2 diff = node2.position - node1.position;
                dist = Vector2.Distance(node1.position, node2.position);
                float angle = Vector2.SignedAngle(axis, diff);
                float baseAngle = Vector2.SignedAngle(new Vector2(1, 0), axis);
                float angleDiff = (angle >= 0 ? nodeAngle - angle : Mathf.Abs(angle + nodeAngle)) / (spring > 0 ? spring : 1);

                if (Math.Abs(angle) > nodeAngle) {
                    angle += angleDiff;
                    float worldAngle = baseAngle + angle;
                    node2.position = node1.position + new Vector2(Mathf.Cos(worldAngle * Mathf.Deg2Rad) * dist, Mathf.Sin(worldAngle * Mathf.Deg2Rad) * dist);
                }

                axis = node1.position;*/
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
