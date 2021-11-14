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

    public string filePath = "Assets/MappedDemos/pointmapping0.obj";

    private List<Color> groupColors;

    public GameObject groupOrganizerPrefab;
    private GameObject currentGroupParent;
    public GameObject pointPrefab;
    // read the file
    // each is group num, confidence, and 3 spacial dimensions

    // NOTE: this is memory inefficient, do something better in production
    private List<VisPointDataObject> uninitializedPoints;
    private List<GameObject> createdPoints;

    // start @ -1 because the first readable line of any of these files will be a fresh object
    private int currGroupNum = -1;

    private const int NUM_METADATA_LINES = 3;

    private void Start()
    {
        groupColors = new List<Color>();
        createdPoints = new List<GameObject>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            readData();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            changeColorMode();
        }
    }


    public void readData()
    {

        Debug.Log("reading: " + filePath);

        int curLineNo = 0;
        try
        {
            using (System.IO.StreamReader streamReader = new System.IO.StreamReader(filePath))
            {
                string lineIn;

                while ((lineIn = streamReader.ReadLine()) != null)
                {
                    // Debug.Log("line: " + lineIn);
                    lineIn = lineIn.Trim();
                    lineIn = lineIn.ToLower();

                    // if (shouldSkipLine(lineIn, curLineNo)) { }
                    // TODO: test if char comparison plays nicely with == here
                    // this is a new object, meaning it's part of a fresh group
                    if (lineIn.Length > 0 && lineIn[0] == 'o')
                    {
                        // Debug.Log("In branch to make a group");

                        currGroupNum++;
                        groupColors.Add(UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));

                        // WARNING: Rotation of points may be altered by the parent's initial rotation
                        currentGroupParent = Instantiate(groupOrganizerPrefab);
                        currentGroupParent.name = "Point Group " + currGroupNum;

                    }
                    else if (lineIn.Length > 0 && lineIn[0] == 'v')
                    {
                        // NOTE (Much later): bake actual geometry into the production models, don't rebuild geo each time

                        // Debug.Log("In branch to read a line");

                        string[] positioning = lineIn.Split(' ');

                        float x, y, z, conf;

                        // 1 offset to ignore the 'v'
                        // Debug.Log("x: " + positioning[0 + 1]);
                        x = float.Parse(positioning[0 + 1]);
                        // Debug.Log("y: " + positioning[1 + 1]);
                        y = float.Parse(positioning[1 + 1]);
                        // Debug.Log("z: " + positioning[2 + 1]);
                        z = float.Parse(positioning[2 + 1]);
                        // Debug.Log("conf: " + positioning[3 + 1]);
                        conf = float.Parse(positioning[3 + 1]);

                        createVisPoint(x, y, z, conf, currGroupNum);
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
    }

    public void changeColorMode()
    {
        foreach (GameObject visPointGameObj in createdPoints)
        {
            visPointGameObj.GetComponent<VisualizerPoint>().changeColorMode();
        }
    }

    // Note: now I am applying the coloring at the same time as creation
    private void createVisPoint(float x, float y, float z, float conf, int groupNum)
    {
        Debug.Log("instantiating a pt");
        GameObject visPointGameObj = Instantiate(pointPrefab);
        visPointGameObj.transform.position = new Vector3(x, y, z);
        // for group organization (hopefully this doesn't alter position)
        visPointGameObj.transform.parent = currentGroupParent.transform;
        visPointGameObj.GetComponent<VisualizerPoint>().initialize(groupColors[groupNum], conf, groupNum);
        createdPoints.Add(visPointGameObj);
    }

    // ignore metadata, blank, and definition lines...
    // private bool shouldSkipLine(string line, int lineNo)
    // {
    //     return lineNo <= NUM_METADATA_LINES || line.Substring(0, 6).Equals("mtllib") || line.Length <= 0 || line[0] == '#';
    // }
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