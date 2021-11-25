using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Does live processing, for the sake of quick optimizations
    and displaying to the phone
*/
public static class PointLiveProcessor
{
    // Make points more coarse
    static float truncateFloat(float f, int decimalPlaces)
    {
        // Note: this could be unrolled to use little extra operations by using same truncator for all data
        float truncator = Mathf.Pow(10f, decimalPlaces);

        return Mathf.Round(f * truncator) / truncator;
    }

    private static void markPointVisually(Vector3[] positions, int i, List<GameObject> visuallyMarkedPoints)
    {
        GameObject newMarker = Instantiate(marker);
        marker.transform.position = positions[i];
        visuallyMarkedPoints.Add(newMarker);
    }
}
