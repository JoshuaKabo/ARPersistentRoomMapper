using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.IO;
using UnityEngine.UI;

public class PointEVENTMapper : PointMapper
{
    public Text pointsTrackedDebug;

    private List<PointDataObject> trackedPoints;


    private void Awake()
    {
        pointCloudManager.pointCloudsChanged += ctx => extractNewPoints(ctx.added, ctx.updated);
    }

    protected override void Start()
    {
        trackedPoints = new List<PointDataObject>();
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
            trackedPoints.Add(new PointDataObject(0, pointsToTrack[i].x, pointsToTrack[i].y, pointsToTrack[i].z, confidencesToTrack[i]));
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

    public override void writeToObj()
    {
        // ends up in Pixel 2 XL\Internal shared storage\Android\data\com.DefaultCompany.ARMappingTool\files
        // https://github.com/HookJabs/CS240_3DRenderer/blob/master/crystals.obj
        // https://answers.unity.com/questions/539339/saving-data-in-to-files-android.html

        // fileDebug.text = "Requests permission here where necessary";


        string filePath = findFreshPath();

        int groupNum = -1;

        try
        {
            int writeIndex = 0;
            float timeSinceRecording = Time.time - mappingInitTime;
            string[] objLines = new string[trackedPoints.Count];
            objLines[writeIndex++] = "# ARZombieMapper vertex obj output (ALL POINTS, no event used.)";
            objLines[writeIndex++] = "# Confidence threshold: " + necessaryConfidenceAmt;
            objLines[writeIndex++] = "# Time spent collecting data: " + timeSinceRecording;
            objLines[writeIndex++] = "mtllib pointmapping.mtl";

            // TODO: Set up grouping, fix this code

            foreach (PointDataObject point in trackedPoints)
            {
                int cloudSize =
                    (
                    (Unity.Collections.NativeSlice<Vector3>)
                    pointCloud.positions
                    ).Length;

                Vector3[] positions = new Vector3[cloudSize];

                // save the positions to a native slice of V3s
                ((Unity.Collections.NativeSlice<Vector3>)pointCloud.positions)
                    .CopyTo(positions);

                // save the confs to a native array of floats
                Unity.Collections.NativeArray<float> confidences =
                    (Unity.Collections.NativeArray<float>)
                    pointCloud.confidenceValues;

                objLines.Add("o PtMappingG" + groupNum);

                for (int index = 0; index < confidences.Length; index++)
                {
                    if (confidences[index] > necessaryConfidenceAmt)
                    {
                        Debug.Log("mapping a point");

                        // prepare x, y, z, then confidence
                        objLines.Add("v " + positions[index].x + ' ' + positions[index].y + ' ' + positions[index].z + ' ' + confidences[index]);
                    }
                }

            }


            File.WriteAllLines(filePath, objLines);
            fileDebug.text = objLines.Count + " Lines written successfully!";
        }
        catch (System.Exception e)
        {
            fileDebug.text = "Write to file threw " + e.Message;
            Debug.Log(e);
        }
    }

}
