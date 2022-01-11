using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class LocationDebug : MonoBehaviour
{
    public TextMeshProUGUI locationDebugUIText;

    // On update, debug display name, then position and rotation
    void Update()
    {
        locationDebugUIText.text = gameObject.name + " P: " + "( " + transform.position.x + ", "
        + transform.position.y + ", " + transform.position.z + ")"
        + " R: " + transform.rotation.x + ", " + transform.rotation.y
         + ", " + transform.rotation.z + ")";
    }
}
