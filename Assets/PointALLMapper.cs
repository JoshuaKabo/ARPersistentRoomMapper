using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using System.IO;

/*
TODO: Create classes to track one of each - EVENT updates, and ALL data.
Events might make corrections, leading to fewer floating obstacles
*/

public class PointALLMapper : MonoBehaviour
{
    public ARPointCloudManager pointCloudManager;

    public float necessaryConfidenceAmt = 0.9f;

    private List<GameObject> visuallyMarkedPoints;
    private List<Vector3> pointsForObj;

    public GameObject marker;

    public Text debugText;

    public Text confDebug;
    public Text fileDebug;

    private float mappingInitTime;

    public string fileName = "pointmapping.obj";

    // ends up in Pixel 2 XL\Internal shared storage\Android\data\com.DefaultCompany.ARMappingTool\files
    private string filePath;

    private void Start()
    {
        filePath = Application.persistentDataPath + "/" + fileName;
        visuallyMarkedPoints = new List<GameObject>();
        pointsForObj = new List<Vector3>();
        confDebug.text = "conf: " + necessaryConfidenceAmt;
        mappingInitTime = Time.time;
    }

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

        try
        {
            float timeSinceRecording = Time.time - mappingInitTime;
            string[] objLines = new string[pointsForObj.Count + 5];
            objLines[0] = "# ARZombieMapper vertex obj output (ALL POINTS, no event used.)";
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

    // delete all points
    public void Wipe()
    {
        for (int i = 0; i < visuallyMarkedPoints.Count; i++)
        {
            GameObject toDelete = visuallyMarkedPoints[i];
            Destroy(toDelete);
        }
        visuallyMarkedPoints.Clear();

        pointsForObj.Clear();
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
