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
        public Vector2 newPosition;
        public Vector2 oldPosition;
        private float spriteHeight = 0;

        public VerletNode(GameObject sprite) {
            this.sprite = sprite;
            this.position = sprite.transform.position;
            this.oldPosition = sprite.transform.position;
            if (sprite.GetComponent<SpriteRenderer>() != null)
                this.spriteHeight = sprite.GetComponent<SpriteRenderer>().bounds.size.y / 2;
        }

        public void updateSpritePosition() {
            sprite.transform.position = newPosition - new Vector2(Mathf.Cos((sprite.transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad) * spriteHeight, 
                                                                  Mathf.Sin((sprite.transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad) * spriteHeight);
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

        private float angleDistance;
        private float angleDistance2;

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
            Vector2 pos = new Vector2(Mathf.Cos((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad) * .6f + transform.position.x,
                                                Mathf.Sin((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad) * .6f + transform.position.y);

            nodes[0] = new VerletNode(new GameObject("ChainStart"));
            nodes[0].position = pos;
            pos.y += nodeDistance;

            for (int i = 1; i < totalNodes+1; i++) {
                nodes[i] = new VerletNode(Instantiate(grappleChain, pos, Quaternion.identity));
                pos.y += nodeDistance;
            }

            nodes[totalNodes+1] = new VerletNode(new GameObject("ChainEnd"));
            nodes[totalNodes+1].position = pos;

            angleDistance = Mathf.Sin(nodeAngle / 2 * Mathf.Deg2Rad) * nodeDistance;
            angleDistance2 = Mathf.Cos(nodeAngle / 2 * Mathf.Deg2Rad) * nodeDistance * 2;
        }

        private void Update() {
            if (shouldSnapshotCollision) {
                SnapshotCollision();
            }
        } 

        private void FixedUpdate() {
            shouldSnapshotCollision = true;

            Vector2 basePosition = new Vector2(Mathf.Cos((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad) * 1f + transform.position.x,
                                                Mathf.Sin((transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad) * 1f + transform.position.y);

            Simulate();
            for (int i = 0; i < iterations; i++) {
                ApplyConstraints(basePosition);
                AdjustCollisions();
            }

            for (int i = 1; i < nodes.Length - 1; i++) {
                Vector2 diff = nodes[i].position - nodes[i + 1].position;
                nodes[i].sprite.transform.RotateAround(nodes[i].position, Vector3.forward, Vector2.SignedAngle(Vector2.up, diff) - nodes[i].sprite.transform.rotation.eulerAngles.z);
            }

            for (int i = 0; i < nodes.Length; i++) {
                nodes[i].updateSpritePosition();
            }
        }

        private void Simulate() {
            for (int i = 1; i < nodes.Length; i++) {
                VerletNode node = nodes[i];
                Vector2 temp = node.position;
                node.position += (node.position - node.oldPosition) * .996f;
                node.oldPosition = temp;
            }
        }

        /*private void ApplyConstraints(Vector2 basePosition) {
            nodes[0].position = basePosition;

            for (int i = 0; i < nodes.Length - 1; i++) {
                VerletNode node1 = i == 0 ? null : nodes[i - 1];
                VerletNode node2 = nodes[i];
                VerletNode node3 = nodes[i + 1];

                Vector2 pos1 = i == 0 ? (Vector2)transform.position : nodes[i - 1].position;
                Vector2 pos2 = nodes[i].position;
                Vector2 pos3 = nodes[i + 1].position;

                for (int j = 0; j < 30; j++) {
                    Vector2 stickTransform1 = StickConstraint(pos1, pos2);
                    Vector2 stickTransform2 = StickConstraint(pos2, pos3);
                    if (i > 0) node1.position += stickTransform1;
                    node2.position += stickTransform2 - stickTransform1;
                    node3.position -= stickTransform2;

                    Vector2 angleTransform = AngleConstraint(pos1, pos2, pos3);
                    if (i > 0) node1.position -= angleTransform;
                    node2.position += angleTransform;
                    node3.position -= angleTransform;

                    Vector2 stickTransform3 = StickConstraint(pos1, pos2);
                    Vector2 stickTransform4 = StickConstraint(pos2, pos3);
                    if (i > 0) node1.position += stickTransform3;
                    node2.position += stickTransform4 - stickTransform3;
                    node3.position -= stickTransform4;
                }
            }
        }*/

        private void ApplyConstraints(Vector2 basePosition) {
            float angleLimit = 180 - nodeAngle;
            nodes[0].position = basePosition;
            for (int i = 0; i < nodes.Length - 1; i++) {
                for (int k = 0; k < 30; k++) {

                    Vector2 stick1Translate = Vector2.zero;
                    Vector2 stick2Translate;

                    if (i > 0)
                        stick1Translate = StickConstraint(nodes[i - 1].position, nodes[i].position);
                    stick2Translate = StickConstraint(nodes[i].position, nodes[i + 1].position);

                    if (i > 0)
                        nodes[i - 1].position += stick1Translate;
                    else
                        nodes[i - 1].position = basePosition;

                    nodes[i].position += stick2Translate - stick1Translate;
                    nodes[i + 1].position -= stick2Translate;

                    VerletNode node1 = i == 0 ? null : nodes[i - 1];
                    VerletNode node2 = nodes[i];
                    VerletNode node3 = nodes[i + 1];

                    Vector2 pos1 = i == 0 ? (Vector2)transform.position : nodes[i - 1].position;
                    Vector2 pos2 = nodes[i].position;
                    Vector2 pos3 = nodes[i + 1].position;

                    /*Vector2 angleTransform = AngleConstraint(pos1, pos2, pos3);
                    if (i > 0) node1.position -= angleTransform;
                    node2.position += angleTransform;
                    node3.position -= angleTransform;*/

                    Vector2 angleTransform2 = AngleConstraint2(pos1, pos3);
                    if (i > 0) node1.position += angleTransform2;
                    node3.position -= angleTransform2;

                    /*if (i > 0)
                        stick1Translate = StickConstraint(nodes[i - 1].position, nodes[i].position);
                    stick2Translate = StickConstraint(nodes[i].position, nodes[i + 1].position);

                    if (i > 0)
                        nodes[i - 1].position += stick1Translate;
                    nodes[i].position += stick2Translate - stick1Translate;
                    nodes[i + 1].position -= stick2Translate;*/

                        
                }

                /*Vector2 point1 = i == 0 ? (Vector2)transform.position : nodes[i - 1].position;
                Vector2 point2 = nodes[i + 1].position;

                if (Mathf.Abs(Vector2.SignedAngle(point2 - nodes[i].position, point1 - nodes[i].position)) < angleLimit) {
                    Vector2 bridge = point1 - point2;

                    float a1 = Mathf.Abs(Vector2.SignedAngle(-bridge, nodes[i].position - point1));
                    float a2 = Mathf.Abs(Vector2.SignedAngle(bridge, nodes[i].position - point2));
                    //fails when a1 == 0 and a2 == 180
                    float baseMag = Mathf.Sin(a1 * Mathf.Deg2Rad) * (i == 0 ? 1.5f : (nodes[i].position - point1).magnitude);
                    float bridgeLength1 = baseMag / Mathf.Tan(a1 * Mathf.Deg2Rad);
                    float bridgeLength2 = baseMag / Mathf.Tan(a2 * Mathf.Deg2Rad);

                    if (a1 != 0 && a2 != 0) {
                        float angleRatio = nodeAngle / (a1 + a2);
                        a1 *= angleRatio;
                        a2 *= angleRatio;

                        float idealBridgeLength1 = baseMag / Mathf.Tan(a1 * Mathf.Deg2Rad);
                        float idealBridgeLength2 = baseMag / Mathf.Tan(a2 * Mathf.Deg2Rad);

                        float bridgeDiff1 = (idealBridgeLength1 - bridgeLength1) / bridgeLength1;
                        float bridgeDiff2 = (idealBridgeLength2 - bridgeLength2) / bridgeLength2;

                        Vector2 bridgeTranslate1 = bridge.normalized * bridgeLength1 * bridgeDiff1;
                        Vector2 bridgeTranslate2 = bridge.normalized * bridgeLength2 * bridgeDiff2;

                        if (i > 0) {
                            nodes[i - 1].position += bridgeTranslate1 * .8f;
                            nodes[i + 1].position -= bridgeTranslate2 * .8f;
                        }
                        else {
                            nodes[i + 1].position -= (bridgeTranslate2 + bridgeTranslate1) * .8f;
                        }
                    }
                }*/
            }
        }

        private Vector2 StickConstraint(Vector2 node1, Vector2 node2) {
            Vector2 diff = node1 - node2;
            float dist = Vector2.Distance(node1, node2);
            float difference = 0;

            if (dist > 0) 
                difference = (nodeDistance - dist) / dist;

            return new Vector2(diff.x, diff.y) * (.5f * difference);
        }

        private Vector2 AngleConstraint(Vector2 node1, Vector2 node2, Vector2 node3) {
            Vector2 bridge = node3 - node1;
            Vector2 joint = node2 - node1;
            float intersectionLength = Mathf.Cos(Vector2.Angle(joint, bridge)) * joint.magnitude;

            Vector2 intersect = node1 + bridge.normalized * intersectionLength;
            Vector2 diff = node2 - intersect;

            float difference = 0;

            if (diff.magnitude > angleDistance)
                difference = (angleDistance - diff.magnitude) / diff.magnitude;

            return diff * .5f * difference;
        }

        private Vector2 AngleConstraint2(Vector2 node1, Vector2 node2) {
            Vector2 diff = node1 - node2;
            float dist = Vector2.Distance(node1, node2);
            float difference = 0;

            if (dist > 0 && dist < angleDistance2)
                difference = (angleDistance2 - dist) / dist;

            return new Vector2(diff.x, diff.y) * (.5f * difference);
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
