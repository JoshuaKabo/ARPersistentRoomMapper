using UnityEngine;
using UnityEngine.XR.ARFoundation;

/*
    The idea is that ALL mapper just grabs everything (maybe even duplicates?), while EVENT mapper might make corrections
*/

public class PointALLMapper : PointMapper
{
    private void Update()
    {
        foreach (ARPointCloud pointCloud in pointCloudManager.trackables)
        {
            // Do something with the ARPointCloud
            // null check first
            if (
                pointCloud.positions != null &&
                pointCloud.confidenceValues != null
            )
            {
                int cloudSize =
                    (
                    (Unity.Collections.NativeSlice<Vector3>)
                    pointCloud.positions
                    ).Length;

                debugText.text = cloudSize + " Points found in cloud!";

                Vector3[] positions = new Vector3[cloudSize];

                ((Unity.Collections.NativeSlice<Vector3>)pointCloud.positions)
                    .CopyTo(positions);

                Unity.Collections.NativeArray<float> confidences =
                    (Unity.Collections.NativeArray<float>)
                    pointCloud.confidenceValues;

                // marks points at high enough confidence
                selectivelyMarkPoints(confidences, positions);

            }
            else
            {
                debugText.text = "No Points in cloud...";
            }
        }
    }




}
