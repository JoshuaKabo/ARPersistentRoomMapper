using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Floor defining:
    create a flat polygon with few internal points by
    removing internal points as they are created, resulting in
    an expanding polygon to represent the floor
    could have the player specify floor if necessary

    Furniture comes from outlying high points
    can be extruded down

    stairs can be raw data, or at low refinement
*/

/*
    Does live processing, for the sake of quick optimizations
    and displaying to the phone
*/
public static class PointLiveProcessor
{
    // Make points more coarse
    public static float truncateFloat(float f, int decimalPlaces)
    {
        // Note: this could be unrolled to use little extra operations by using same truncator for all data
        float truncator = Mathf.Pow(10f, decimalPlaces);

        return Mathf.Round(f * truncator) / truncator;
    }

    public static void markPointVisually(Vector3[] positions, int i, List<GameObject> visuallyMarkedPoints, GameObject marker)
    {
        GameObject newMarker = GameObject.Instantiate(marker);
        marker.transform.position = positions[i];
        visuallyMarkedPoints.Add(newMarker);
    }
}
