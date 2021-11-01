using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizerEngine : MonoBehaviour
{

    private Color[] groupColors;

    public GameObject pointPrefab;
    // read the file
    // get a bunch of vec5s (or vec4s with shorts on an end)
    // represent group num, confidence, and 3 spacial dimensions

    private void readData()
    {
        // Where 69 should actually be the ending group number
        groupColors = new Color[69];
        for (int i = 0; i < groupColors.Length; i++)
        {
            groupColors[i] = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
    }


    public void drawVisualization()
    {

    }

    void drawPoint(int groupnum, float confidence)
    {
        // (at position)
        GameObject createdPoint = Instantiate(pointPrefab);
        VisualizerPoint pointVisualizer = createdPoint.GetComponent<VisualizerPoint>();
        // default to confidence for now
        pointVisualizer.initialize(groupColors[groupnum], confidence, groupnum);


    }
}
