using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/*
TODO: The keys to look for in the event mapper vs the all mapper:
    * Fewer? Duplicate points.
    * Higher? Overall confidence values.

    I should just set up a simple particle visualizer in Unity!!
    It'll be like loading into blender, but it will also use color to depict
    confidence values

    NOTE: confidence might not be as important when it comes to event based mapping
    But I'll leave that to observations from data.

    NOTE: I may want to map result objects as their individual point clouds rather than combining them...
*/

public class PointEVENTMapper : PointMapper
{
    public List<ARPointCloud> cloudsTracked;

    private void Awake()
    {
        pointCloudManager.pointCloudsChanged += ctx => trackPointCloud(ctx.added, ctx.updated);
    }

    private void trackPointCloud(List<ARPointCloud> added, List<ARPointCloud> updated)
    {
        // be sure to create new copies of those tracked, so they don't get deleted.
        // note - each plane has it's own subsumed by getter.
        // perhaps I can equality check <added> against <updated> to prevent double counting.
        // TODO: first try, just see if updated matches added in the end (because they should be taking care of themselves)
        // ALSO, see if moving to a fresh room wipes all I tracked


        foreach (ARPointCloud cloud in added)
        {
            // this, naievely, assumes that the updates will be reflected, and removed planes will stay in my own tracker 
            // (I want them to stay).
            cloudsTracked.Add(cloud);
        }
    }

    private void cloudsToPoints()
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
