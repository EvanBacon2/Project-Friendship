using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SegmentRope;

class HookSegment : MonoBehaviour {
	public Segment s;

	private Vector3 hookPos = new Vector3(0,0,70);

	private GameObject hooked;//object attatched to hook
	private Vector3 hookedPos = new Vector3(0, 0, 70);

	public bool active;

	private void FixedUpdate() {
		hookPos.x = (float)s.position.x;
		hookPos.y = (float)s.position.y;
		transform.position = hookPos;

		float angle = Mathf.Atan2((float)s.orientation.y, (float)s.orientation.x) * Mathf.Rad2Deg - 90;
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

		if (hooked != null) {
			hookedPos.x = (float)s.p2.x;
			hookedPos.y = (float)s.p2.y;

			hooked.transform.position = hookedPos;
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (active)
			hooked = other.gameObject;
	}

	public void unHook() {
		hooked = null;
	}
}
