using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpedCursor : MonoBehaviour
{
    Vector2 cursorPos;

    void Update() {
        cursorPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 70));
        transform.position = new Vector3(cursorPos.x, cursorPos.y, 70);        
    }
}
