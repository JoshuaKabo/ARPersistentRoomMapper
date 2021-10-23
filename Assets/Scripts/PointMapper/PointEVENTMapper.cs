using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using System.IO;

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

public class PointEVENTMapper : MonoBehaviour
{
    public ARPointCloudManager pointCloudManager;
    public List<ARPointCloud> cloudsTracked;

    public float necessaryConfidenceAmt = 0.9f;

    private List<GameObject> visuallyMarkedPoints;
    private List<Vector3> pointsForObj;

    public GameObject marker;

    public Text debugText;

    public Text confDebug;
    public Text fileDebug;

    private float mappingInitTime;

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

    private void Start()
    {
        visuallyMarkedPoints = new List<GameObject>();
        pointsForObj = new List<Vector3>();
        confDebug.text = "conf: " + necessaryConfidenceAmt;
        mappingInitTime = Time.time;
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

                List<Vector3> sessionPoints = new List<Vector3>();

                Vector3[] positions = new Vector3[cloudSize];

                ((Unity.Collections.NativeSlice<Vector3>)pointCloud.positions)
                    .CopyTo(positions);

                Unity.Collections.NativeArray<float> confidences =
                    (Unity.Collections.NativeArray<float>)
                    pointCloud.confidenceValues;

                // select and spawn at suitable points
                for (int i = 0; i < confidences.Length; i++)
                {
                    if (confidences[i] >= necessaryConfidenceAmt)
                    {
                        // markPointVisually(positions, i);
                        markPointForObj(new Vector3(positions[i].x, positions[i].y, positions[i].z));
                    }
                }

            }
            else
            {
                debugText.text = "No Points in cloud...";
            }
        }
    }

    public void writeToObj()
    {
        // https://github.com/HookJabs/CS240_3DRenderer/blob/master/crystals.obj
        // https://answers.unity.com/questions/539339/saving-data-in-to-files-android.html

        // get all points out of the cloud collection
        cloudsToPoints();

        // ends up in Pixel 2 XL\Internal shared storage\Android\data\com.DefaultCompany.ARMappingTool\files
        string fileName = "pointmapping" + System.DateTime.Now + ".obj";
        string filePath = Application.persistentDataPath + "/" + fileName;

        try
        {
            // TODO: add confidence values into the obj file. I can then color particles based on conf
            //       in my own visualizer.
            float timeSinceRecording = Time.time - mappingInitTime;
            string[] objLines = new string[pointsForObj.Count + 4];
            objLines[0] = "# ARZombieMapper vertex obj output EVENT points)";
            objLines[1] = "# Confidence threshold: " + necessaryConfidenceAmt;
            objLines[2] = "# Time spent collecting data: " + timeSinceRecording;
            objLines[3] = "mtllib pointmapping.mtl";
            objLines[4] = "o Pointmapping";

            for (int index = 0; index < pointsForObj.Count; index++)
            {
                Vector3 currentPoint = pointsForObj[index];
                // blender, why is it x, z, -y??
                objLines[index + 5] = "v " + currentPoint.x + " " + currentPoint.z + " " + -1 * currentPoint.y;
            }

            File.WriteAllLines(filePath, objLines);
            fileDebug.text = objLines.Length + " Lines written successfully!";
        }
        catch (System.Exception e)
        {
            fileDebug.text = "Write to file threw an error.";
            Debug.Log(e);
        }
    }

    private void markPointForObj(Vector3 point)
    {
        pointsForObj.Add(point);
    }

    private void markPointVisually(Vector3[] positions, int i)
    {
        GameObject newMarker = Instantiate(marker);
        marker.transform.position = positions[i];
        visuallyMarkedPoints.Add(newMarker);
    }

    // delete all tracked things
    public void Wipe()
    {
        for (int i = 0; i < visuallyMarkedPoints.Count; i++)
        {
            GameObject toDelete = visuallyMarkedPoints[i];
            Destroy(toDelete);
        }
        visuallyMarkedPoints.Clear();

        pointsForObj.Clear();
        cloudsTracked.Clear();
    }

    public void setConf(float val)
    {
        // Wipe and restart timer for the sake of pure data
        Wipe();
        necessaryConfidenceAmt = val;
        confDebug.text = "conf: " + necessaryConfidenceAmt;
        mappingInitTime = Time.time;
    }
}
