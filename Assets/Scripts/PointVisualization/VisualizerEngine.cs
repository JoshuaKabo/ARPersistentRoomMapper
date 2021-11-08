using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
nvm not going to do this in engine b/c
I want clean&management up to be as quick and easy as poss
*/



public class VisualizerEngine : MonoBehaviour
{

    public string filePath = "Assets/MappedDemos/MappedDemo1.obj";

    private Color[] groupColors;

    public GameObject pointPrefab;
    // read the file
    // get a bunch of vec5s (or vec4s with shorts on an end)
    // represent group num, confidence, and 3 spacial dimensions

    // NOTE: this is memory inefficient, do something better in production
    private List<VisualizerPointDataObject> uninitializedPoints;
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
                    else
                    {
                        // TODO: bake actual geometry into the production models, don't rebuild geo each time


                    }


                    /*
                    proper data reading and initialization goes here.
                    */

                    curLineNo++;
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

    // ignore metadata, blank, and definition lines...
    private bool shouldSkipLine(string line, int lineNo)
    {
        return lineNo <= NUM_METADATA_LINES || line.Substring(0, 6).Equals("mtllib") || line.Length <= 0;
    }

    public void drawVisualization()
    {

    }

    private void drawPoint(int groupnum, float confidence)
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