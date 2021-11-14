using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private void LateUpdate()
    {
        // end of frame, orient same as cam
        transform.forward = Camera.main.transform.forward;
    }
}
