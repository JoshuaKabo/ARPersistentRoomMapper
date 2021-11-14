using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using System.IO;
using UnityEngine.Android;

/*
    Pack mule to carry all point mapping functionality for inheritance
*/

public class PointMapper : MonoBehaviour
{
    public ARPointCloudManager pointCloudManager;

    public float necessaryConfidenceAmt = 0.9f;

    protected List<GameObject> visuallyMarkedPoints;
    protected List<Vector4> pointsForObj;

    public GameObject marker;

    public Text debugText;

    public Text confDebug;
    public Text fileDebug;
    protected float mappingInitTime;


    public Text foundconf;
    public Text threshdebug;


    protected void Start()
    {
        visuallyMarkedPoints = new List<GameObject>();
        pointsForObj = new List<Vector4>();
        confDebug.text = "conf: " + necessaryConfidenceAmt;
        mappingInitTime = Time.time;
    }


    public void writeToObj()
    {
        // ends up in Pixel 2 XL\Internal shared storage\Android\data\com.DefaultCompany.ARMappingTool\files
        // https://github.com/HookJabs/CS240_3DRenderer/blob/master/crystals.obj
        // https://answers.unity.com/questions/539339/saving-data-in-to-files-android.html

        // fileDebug.text = "Requests permission here where necessary";

        // Get to the right file iteration (version 1 2 3, etc)
        int currentIteration = 0;
        string fileName = "pointmapping" + currentIteration + ".obj";
        string filePath = Application.persistentDataPath + "/" + fileName;
        while (File.Exists(filePath))
        {
            currentIteration += 1;
            fileName = "pointmapping" + currentIteration + ".obj";
            filePath = Application.persistentDataPath + "/" + fileName;
        }


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
            fileDebug.text = "Write to file threw " + e.Message;
            Debug.Log(e);
        }
    }

    protected void markPointVisually(Vector3[] positions, int i)
    {
        GameObject newMarker = Instantiate(marker);
        marker.transform.position = positions[i];
        visuallyMarkedPoints.Add(newMarker);
    }

    // delete all points
    public void Wipe()
    {
        if (visuallyMarkedPoints != null)
        {
            for (int i = 0; i < visuallyMarkedPoints.Count; i++)
            {
                GameObject toDelete = visuallyMarkedPoints[i];
                Destroy(toDelete);
            }
            visuallyMarkedPoints.Clear();

            pointsForObj.Clear();
        }
    }

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
    protected void selectivelyMarkPoints(Unity.Collections.NativeArray<float> confidences, Vector3[] positions, float threshold)
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


}