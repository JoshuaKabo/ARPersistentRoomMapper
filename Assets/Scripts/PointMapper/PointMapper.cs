using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

/*
    Collector dispatches the other point classes

    TODO: Level floors
    Be sure that all "floor" points are on same y, so not accidentally a slope

    TODO: visually mark points with particles in real time
    (replace the old marker)
*/

/*
    Pack mule to carry all point mapping functionality for inheritance
    TODO: Remember to request permissions nicely
    TODO: Turn off rotate
*/

public class PointMapper : MonoBehaviour
{
    public Text pointsTrackedDebug;
    public ARPointCloudManager pointCloudManager;

    public float necessaryConfidenceAmt = 0.9f;

    protected List<GameObject> visuallyMarkedPoints;
    protected List<Vector4> pointsForObj;

    public GameObject marker;

    public Text debugText;

    public Text confDebug;
    public Text fileDebug;
    protected float mappingInitTime;

    // Used to be: private List<PointDataObject> trackedPoints; (hashset eliminates dupes)
    private HashSet<Vector3> trackedPoints;

    private void Awake()
    {
        pointCloudManager.pointCloudsChanged += ctx => extractNewPoints(ctx.added, ctx.updated);
    }

    protected virtual void Start()
    {
        visuallyMarkedPoints = new List<GameObject>();
        pointsForObj = new List<Vector4>();
        confDebug.text = "conf: " + necessaryConfidenceAmt;
        mappingInitTime = Time.time;
    }

    private void trackAllPointsInCloud(ARPointCloud cloud)
    {
        Unity.Collections.NativeArray<float> confidencesToTrack =
                    (Unity.Collections.NativeArray<float>)
                    cloud.confidenceValues;

        int cloudSize = confidencesToTrack.Length;

        Vector3[] pointsToTrack = new Vector3[cloudSize];
        ((Unity.Collections.NativeSlice<Vector3>)cloud.positions)
            .CopyTo(pointsToTrack);

        for (int i = 0; i < cloudSize; i++)
        {
            if (confidencesToTrack[i] >= necessaryConfidenceAmt)
            {
                trackedPoints.Add(new Vector3(
                PointLiveProcessor.truncateFloat(pointsToTrack[i].x, 2),
                PointLiveProcessor.truncateFloat(pointsToTrack[i].y, 2),
                PointLiveProcessor.truncateFloat(pointsToTrack[i].z, 2)));
            }
        }
    }

    // Tracks points from all clouds that might exist only when they update
    private void extractNewPoints(List<ARPointCloud> added, List<ARPointCloud> updated)
    {
        // note: added should really only be relevant on camera start...
        foreach (ARPointCloud cloud in added)
        {
            trackAllPointsInCloud(cloud);
        }

        foreach (ARPointCloud cloud in updated)
        {
            trackAllPointsInCloud(cloud);
        }

        pointsTrackedDebug.text = "Points Tracked: " + trackedPoints.Count;
    }

    // delete all points
    public void setConf(float val)
    {
        // Wipe and restart timer for the sake of pure data
        Wipe();
        necessaryConfidenceAmt = val;
        confDebug.text = "conf: " + necessaryConfidenceAmt;
        mappingInitTime = Time.time;
    }

    // Note, some code above this call is reused.
    // This function is definitely generic between the two mappers. Logic above may be different between the two.
    protected virtual void selectivelyMarkPoints(Unity.Collections.NativeArray<float> confidences, Vector3[] positions, float threshold)
    {
        // threshdebug.text = "thresh " + threshold;
        // select and spawn at suitable points
        for (int i = 0; i < confidences.Length; i++)
        {
            if (confidences[i] >= threshold)
            {
                // foundconf.text = "foundconf " + confidences[i];

                // markPointVisually(positions, i);
                pointsForObj.Add(new Vector4(positions[i].x, positions[i].y, positions[i].z, confidences[i]));
            }
        }
    }

    public void Wipe()
    {
        if (trackedPoints != null)
        {
            for (int i = 0; i < visuallyMarkedPoints.Count; i++)
            {
                GameObject toDelete = visuallyMarkedPoints[i];
                Destroy(toDelete);
            }
            visuallyMarkedPoints.Clear();

            trackedPoints.Clear();

            pointsTrackedDebug.text = "Points Tracked: 0";
        }
    }

    public void WritePoints()
    {
        PointFileWriter.writeToObj(trackedPoints, mappingInitTime, necessaryConfidenceAmt, fileDebug);
    }

}
