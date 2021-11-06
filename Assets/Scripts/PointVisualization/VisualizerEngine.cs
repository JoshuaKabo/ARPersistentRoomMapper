using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
nvm not going to do this in engine b/c
I want clean&management up to be as quick and easy as poss
*/



public class VisualizerEngine : MonoBehaviour
{

    public static string filePath = "Assets/MappedDemos/MappedDemo1.obj";

    private static Color[] groupColors;

    public static GameObject pointPrefab;
    // read the file
    // get a bunch of vec5s (or vec4s with shorts on an end)
    // represent group num, confidence, and 3 spacial dimensions

    private List<VisualizerPointDataObject> points;


    public static void readData()
    {
        int curLine = 0;
        try
        {
            using (System.IO.StreamReader streamReader = new System.IO.StreamReader(filePath))
            {
                string lineIn;

                while ((lineIn = streamReader.ReadLine()) != null)
                {
                    curLine++;

                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("File Read Error:");
            Debug.LogError(e.Message);
        }


        // Where 69 should actually be the ending group number
        groupColors = new Color[69];
        for (int i = 0; i < groupColors.Length; i++)
        {
            groupColors[i] = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        drawVisualization();
    }


    public static void drawVisualization()
    {

    }

    private static void drawPoint(int groupnum, float confidence)
    {
        // (at position)
        GameObject createdPoint = Instantiate(pointPrefab);
        VisualizerPoint pointVisualizer = createdPoint.GetComponent<VisualizerPoint>();
        // default to confidence for now
        pointVisualizer.initialize(groupColors[groupnum], confidence, groupnum);
    }
}

struct VisualizerPointDataObject
{
    // NOTE: not sure where groupnum is to be determined... During reading??
    public int groupnum { get; }
    public float x { get; }
    public float y { get; }
    public float z { get; }
    public float w { get; }

    public VisualizerPointDataObject(int groupnum, float x, float y, float z, float w)
    {
        this.groupnum = groupnum;
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}