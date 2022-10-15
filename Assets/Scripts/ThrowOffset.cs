using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThrowOffset : MonoBehaviour {
    public GameObject player;
    public static GameObject tracked;

    private CinemachineFramingTransposer cam;

	private void Start() {
        cam = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        cam.m_ScreenX = .5f;
        cam.m_ScreenY = .5f;
    }

    void Update() {
        if (tracked != null) {
            Vector2 trackedVelocity = (tracked.GetComponent<Rigidbody>().velocity - player.GetComponent<Rigidbody>().velocity).normalized * 25;
            Vector2 offset = Camera.main.WorldToViewportPoint(new Vector3(player.transform.position.x - trackedVelocity.x, player.transform.position.y + trackedVelocity.y, 70));
            Debug.Log(Camera.main.transform.position);
            Debug.Log(player.transform.position);
            Debug.Log(Camera.main.WorldToViewportPoint(player.transform.position));
            Debug.Log("pos " + new Vector2(player.transform.position.x - trackedVelocity.x, player.transform.position.y + trackedVelocity.y));
            Debug.Log("vel " + trackedVelocity);
            Debug.Log("off " + offset);

            cam.m_ScreenX = offset.x;
            cam.m_ScreenY = offset.y;

            StartCoroutine(reCenter());

            tracked = null;
        }
    }

    private IEnumerator reCenter() {
        yield return new WaitForSeconds(.7f);

        cam.m_ScreenX = .5f;
        cam.m_ScreenY = .5f;
    }
}

//record viewport pos of tracked object
//if values outside the range [0,1] than find the difference and add it to cam's screen x/y
//if screen x/y exceeds [0,1] range than stop following tracked and reset offset to .5, .5
