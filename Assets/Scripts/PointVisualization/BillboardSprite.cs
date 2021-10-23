using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    public static Camera mainCam;

    private void LateUpdate()
    {
        // end of frame, orient same as cam
        transform.forward = mainCam.transform.forward;
    }
}
