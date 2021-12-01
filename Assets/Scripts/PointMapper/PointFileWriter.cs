using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;


public static class PointFileWriter
{

    public static string findFreshPath()
    {
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

        return filePath;
    }

    public static void writeToObjWithConf(List<Vector4> pointsForObj, float mappingInitTime, float necessaryConfidenceAmt, Text fileDebug)
    {
        // ends up in Pixel 2 XL\Internal shared storage\Android\data\com.DefaultCompany.ARMappingTool\files
        // https://github.com/HookJabs/CS240_3DRenderer/blob/master/crystals.obj
        // https://answers.unity.com/questions/539339/saving-data-in-to-files-android.html

        string filePath = findFreshPath();

        try
        {
            // TODO: add confidence values into the obj file. I can then color particles based on conf
            //       in my own visualizer.
            float timeSinceRecording = Time.time - mappingInitTime;
            string[] objLines = new string[pointsForObj.Count + 5];
            objLines[0] = "# ARZombieMapper vertex obj output";
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

    public static void writeToObj(HashSet<Vector3> trackedPoints, float mappingInitTime, float necessaryConfidenceAmt, Text fileDebug)
    {
        // ends up in Pixel 2 XL\Internal shared storage\Android\data\com.DefaultCompany.ARMappingTool\files
        // https://github.com/HookJabs/CS240_3DRenderer/blob/master/crystals.obj
        // https://answers.unity.com/questions/539339/saving-data-in-to-files-android.html

        // Request permission here where necessary


        string filePath = findFreshPath();

        try
        {
            float timeSinceRecording = Time.time - mappingInitTime;
            // use a list because unsure of number of groups
            List<string> objLines = new List<string>();
            objLines.Add("# ARZombieMapper vertex obj output (ALL POINTS, no event used.)");
            objLines.Add("# Confidence threshold: " + necessaryConfidenceAmt);
            objLines.Add("# Time spent collecting data: " + timeSinceRecording);
            objLines.Add("mtllib pointmapping.mtl");

            foreach (Vector3 point in trackedPoints)
            {
                // NOTE: removed group number functionality
                //// Note / Warning: assumes group numbers will increase in order
                //// if (trackedPoints[i].groupnum > prevGroupNum)
                //// {
                ////     prevGroupNum++;
                ////     objLines.Add("o PtMappingG" + prevGroupNum);
                //// }
                // Note - to get really serious abt saving data, could try a custom float of my own
                objLines.Add("v " + point.x + ' ' + point.y + ' ' + point.z);
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
