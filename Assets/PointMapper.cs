using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.ARFoundation;
using UnityEngine.XR.ARFoundation;

public class PointMapper : MonoBehaviour
{
    public ARPointCloudManager pointCloudManager;

    private ARPointCloud pointCloud;

    public float necessaryConfidenceAmt = 0.9f;

    private List<GameObject> markedPoints;

    public GameObject marker;

    public Text debugText;

    public Text confDebug;

    public Text pointDebug;

    private void Start()
    {
        // pointCloud = pointCloudManager.pointCloudPrefab.GetComponent<ARPointCloud>();
        markedPoints = new List<GameObject>();
        confDebug.text = "conf: " + necessaryConfidenceAmt;
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

                ((Unity.Collections.NativeSlice<Vector3>) pointCloud.positions)
                    .CopyTo(positions);

                // Unity.Collections.NativeArray<Vector3> positions =
                //     (Unity.Collections.NativeArray<Vector3>) pointCloud.positions;
                Unity.Collections.NativeArray<float> confidences =
                    (Unity.Collections.NativeArray<float>)
                    pointCloud.confidenceValues;

                // select and spawn at suitable points
                for (int i = 0; i < confidences.Length; i++)
                {
                    if (confidences[i] >= necessaryConfidenceAmt)
                    {
                        GameObject newMarker = Instantiate(marker);
                        marker.transform.position = positions[i];
                        markedPoints.Add (newMarker);
                    }
                }

                pointDebug.text = "p: " + markedPoints[markedPoints.Count - 1];
            }
            else
            {
                debugText.text = "No Points in cloud...";
            }
        }
    }

    // delete all points
    public void Wipe()
    {
        for (int i = 0; i < markedPoints.Count; i++)
        {
            GameObject toDelete = markedPoints[i];
            Destroy (toDelete);
        }
        markedPoints.Clear();
    }

    public void setConf(float val)
    {
        necessaryConfidenceAmt = val;
        confDebug.text = "conf: " + necessaryConfidenceAmt;
    }
}
