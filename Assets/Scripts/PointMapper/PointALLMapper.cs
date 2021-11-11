using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using System.IO;

/*
    The idea is that ALL mapper just grabs everything (maybe even duplicates?), while EVENT mapper might make corrections
*/

public class PointALLMapper : MonoBehaviour
{
    public ARPointCloudManager pointCloudManager;

    public float necessaryConfidenceAmt = 0.9f;

    private List<GameObject> visuallyMarkedPoints;
    private List<Vector4> pointsForObj;

    public GameObject marker;

    public Text debugText;

    public Text confDebug;
    public Text fileDebug;

    private float mappingInitTime;



    private void Start()
    {
        visuallyMarkedPoints = new List<GameObject>();
        pointsForObj = new List<Vector4>();
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
                        markPointForObj(new Vector4(positions[i].x, positions[i].y, positions[i].z, confidences[i]));
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


        // ends up in Pixel 2 XL\Internal shared storage\Android\data\com.DefaultCompany.ARMappingTool\files
        string fileName = "pointmapping" + System.DateTime.Now + ".obj";
        string filePath = Application.persistentDataPath + "/" + fileName;

        try
        {
            // TODO: add confidence values into the obj file. I can then color particles based on conf
            //       in my own visualizer.
            float timeSinceRecording = Time.time - mappingInitTime;
            string[] objLines = new string[pointsForObj.Count + 5];
            objLines[0] = "# ARZombieMapper vertex obj output (ALL POINTS, no event used.)";
            objLines[1] = "# Confidence threshold: " + necessaryConfidenceAmt;
            objLines[2] = "# Time spent collecting data: " + timeSinceRecording;
            objLines[3] = "mtllib pointmapping.mtl";
            objLines[4] = "o Pointmapping";

            for (int index = 0; index < pointsForObj.Count; index++)
            {
                Vector4 currentPoint = pointsForObj[index];
                // prepare x, y, z, then confidence
                objLines[index + 5] = "v " + currentPoint.x + ' ' + currentPoint.y + ' ' + currentPoint.z + ' ' + currentPoint.w;
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
