using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.IO;
using UnityEngine.UI;

/*
    TODO: visually mark points with particles in real time
    (replace the old marker)
*/

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
            if (confidencesToTrack[i] >= necessaryConfidenceAmt)
            {
                trackedPoints.Add(new PointDataObject(0, pointsToTrack[i].x, pointsToTrack[i].y, pointsToTrack[i].z, confidencesToTrack[i]));
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

    public override void writeToObj()
    {
        // ends up in Pixel 2 XL\Internal shared storage\Android\data\com.DefaultCompany.ARMappingTool\files
        // https://github.com/HookJabs/CS240_3DRenderer/blob/master/crystals.obj
        // https://answers.unity.com/questions/539339/saving-data-in-to-files-android.html

        // Request permission here where necessary


        string filePath = findFreshPath();

        try
        {
            int prevGroupNum = -1;
            float timeSinceRecording = Time.time - mappingInitTime;
            // use a list because unsure of number of groups
            List<string> objLines = new List<string>();
            objLines.Add("# ARZombieMapper vertex obj output (ALL POINTS, no event used.)");
            objLines.Add("# Confidence threshold: " + necessaryConfidenceAmt);
            objLines.Add("# Time spent collecting data: " + timeSinceRecording);
            objLines.Add("mtllib pointmapping.mtl");

            for (int i = 0; i < trackedPoints.Count; i++)
            {
                // Note/Warning: assumes group numbers will increase in order
                if (trackedPoints[i].groupnum > prevGroupNum)
                {
                    prevGroupNum++;
                    objLines.Add("o PtMappingG" + prevGroupNum);
                }

                // confidences are thresholded earlier, so I can just grab everything prevetted
                objLines.Add("v " + trackedPoints[i].x + ' ' + trackedPoints[i].y + ' ' + trackedPoints[i].z + ' ' + trackedPoints[i].w);
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

    public override void Wipe()
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

}
