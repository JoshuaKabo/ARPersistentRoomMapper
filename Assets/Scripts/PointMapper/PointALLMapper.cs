using UnityEngine;
using UnityEngine.XR.ARFoundation;

/*
    The idea is that ALL mapper just grabs everything (maybe even duplicates?), while EVENT mapper might make corrections

    =========__LOGS__=========

    ~~~~~quickupdownstarrs.11.13.21 (13mb, 337,000 pts, 0.6534375 conf)~~~~~

    The good:   
    is promising, shows the livingroom, stairs, and upstairs prominently

    The Bad:    
    there are way too many points, and a lot of duplicates.
    also there seem to be some points that drift out in space...

    Conclusion:
    I hope that event mapper will grab fewer dupes. If not, I'll come up with a space partitioning algorithm.
    I should run more analysis on the points to determine how to optimize them, starting w/ dupe elimination.
    
    
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
                selectivelyMarkPoints(confidences, positions, necessaryConfidenceAmt);

            }
            else
            {
                debugText.text = "No Points in cloud...";
            }
        }
    }




}
