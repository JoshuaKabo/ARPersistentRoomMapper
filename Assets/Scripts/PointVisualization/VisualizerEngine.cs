using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    options for confidence long term use:
    1) leave confidence in production for the sake of model dependability
    2) take confidence back out, no need to waste space
*/



public class VisualizerEngine : MonoBehaviour
{

    public string filePath = "Assets/MappedDemos/MappedDemo1.obj";

    private Color[] groupColors;

    public GameObject pointPrefab;
    // read the file
    // each is group num, confidence, and 3 spacial dimensions

    // NOTE: this is memory inefficient, do something better in production
    private List<VisPointDataObject> uninitializedPoints;
    private List<GameObject> createdPoints;

    // start @ -1 because the first readable line of any of these files will be a fresh object
    private int currGroupNum = -1;

    private const int NUM_METADATA_LINES = 3;


    public void readData()
    {
        int curLineNo = 0;
        try
        {
            using (System.IO.StreamReader streamReader = new System.IO.StreamReader(filePath))
            {
                string lineIn;

                while ((lineIn = streamReader.ReadLine()) != null)
                {
                    lineIn = lineIn.Trim();
                    lineIn = lineIn.ToLower();

                    if (shouldSkipLine(lineIn, curLineNo))
                    {
                        continue;
                    }
                    // TODO: test if char comparison plays nicely with == here
                    // this is a new object, meaning it's part of a fresh group
                    else if (lineIn[0] == 'o')
                    {
                        currGroupNum++;
                        continue;
                    }
                    else if (lineIn[0] == 'v')
                    {
                        // NOTE (Much later): bake actual geometry into the production models, don't rebuild geo each time

                        string[] positioning = lineIn.Split(' ');

                        float x, y, z, conf;

                        Debug.Log("x: " + positioning[0]);
                        x = float.Parse(positioning[0]);
                        Debug.Log("y: " + positioning[1]);
                        y = float.Parse(positioning[1]);
                        Debug.Log("z: " + positioning[2]);
                        z = float.Parse(positioning[2]);
                        Debug.Log("conf: " + positioning[3]);
                        conf = float.Parse(positioning[3]);

                        instantiateVisPoint(x, y, z, conf);
                    }

                    curLineNo++;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("File Read Error:");
            Debug.LogError(e.Message);
        }


        if (currGroupNum >= 0)
        {
            groupColors = new Color[currGroupNum];
            for (int i = 0; i < groupColors.Length; i++)
            {
                groupColors[i] = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            }
        }

        // colors all the points appropriately
        finalizeVisPoints();
    }


    private void instantiateVisPoint(float x, float y, float z)
    {
        //TODO: Implementation
    }

    // ignore metadata, blank, and definition lines...
    private bool shouldSkipLine(string line, int lineNo)
    {
        return lineNo <= NUM_METADATA_LINES || line.Substring(0, 6).Equals("mtllib") || line.Length <= 0;
    }

    public void finalizeVisPoints()
    {
        //TODO: Implementation
        // calls my fancy initialize on the instantiated points

        // default to confidence for now
        // pointVisualizer.initialize(groupColors[groupnum], confidence, groupnum);
    }

    private void drawPoint(int groupnum, float confidence)
    {
        // (at position)
        GameObject createdPoint = Instantiate(pointPrefab);
        VisPoint pointVisualizer = createdPoint.GetComponent<VisPoint>();
    }
}

struct VisPointDataObject
{
    // NOTE: not sure where groupnum is to be determined... During reading??
    public int groupnum { get; }
    public float x { get; }
    public float y { get; }
    public float z { get; }
    public float w { get; }

    public VisPointDataObject(int groupnum, float x, float y, float z, float w)
    {
        this.groupnum = groupnum;
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}