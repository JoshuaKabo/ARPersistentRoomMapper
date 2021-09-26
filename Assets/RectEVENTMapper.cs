using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.ARFoundation;
using UnityEngine.XR.ARFoundation;
using System.IO;

/*






NOTE (FINAL): After experimenting with rectangle mapping more, I have concluded that its limited data yield 
and high risk of incorrect polygons generated in favor of realtime interactive surfaces is not in my best interest.
I will be using pointclouds. Maybe I'll combine pointclouds w/ ARPLanes if I'm desparate.
Give up on ARPlanes.









TODO: Create classes to track one of each - EVENT updates, and ALL data.
Events might make corrections, leading to fewer floating obstacles

TODO: because planemanger planes are removed, I might need to heavily customize planemanager to prevent this.

NOTE: Maybe I'm looking into it too far. It appears there isn't more granular data as far as the rects go, they really
are 2 dimesional boundaries with a worldspace center point.
or maybe the boundary field has a polygon with higher dimensional data?

Hierarchy: 
(This Class)
ARPlaneManager
    Subsystem and XRRaycasts develop an ARPlane
ARPlaneMeshVisualizer(Attached to the "ARPlane" Prefabs!)
ARPlaneMeshGenerators(Static) GenerateMesh


Finally - could look into Google ARCore's image depth features paired with anchors to stitch together a 3d model
of depth images, then peek into those images to give the model more detail.
-BUT I want to be as cross plat as possible, and I think what unity gives me will be good enough-
*/

public class RectEVENTMapper : MonoBehaviour
{
    public ARPlaneManager planeManager;
    // TODO: if I'm using direct comparisons and pointer storage, then planes that get removed might be wiped from memory...
    public List<ARPlane> planesTracked;

    public float necessaryConfidenceAmt = 0.9f;

    private List<Vector3> pointsForObj;

    public GameObject marker;

    public Text debugText;

    public Text confDebug;
    public Text fileDebug;

    private float mappingInitTime;

    public string fileName = "rectmapping.obj";

    // ends up in Pixel 2 XL\Internal shared storage\Android\data\com.DefaultCompany.ARMappingTool\files
    private string filePath;

    private void Awake()
    {
        // Track the planemanager's change event
        // public ARPlanesChangedEventArgs(List<ARPlane> added, List<ARPlane> updated, List<ARPlane> removed)
        planeManager.planesChanged += ctx => trackRects(ctx.added, ctx.updated);
    }

    private void trackRects(List<ARPlane> added, List<ARPlane> updated)
    {
        // be sure to create new copies of those tracked, so they don't get deleted.
        // note - each plane has it's own subsumed by getter.
        // perhaps I can equality check <added> against <updated> to prevent double counting. (in n^2 time)
        // there don't seem to be many rects in a location, but reducing from n^2 is a TODO for production code...
        // TODO: first try, just see if updated matches added in the end (because they should be taking care of themselves)
        // ALSO, see if moving to a fresh room wipes all I tracked

        // NOTE: planes have a nativepointer - original and new should probably have the same nativepointer
        // Or maybe I need to keep a track of all nativepointers rather than default pointers


        foreach (ARPlane plane in added)
        {
            // this, naievely, assumes that the updates will be reflected, and removed planes will stay in my own tracker 
            // (I want them to stay).
            planesTracked.Add(plane);
            // plane.
        }
    }

    // NOTE - goal 1 : writeToObj Goal 2 : check on updated, removed, etc
    // NOTE - rects will need some toworld conversion, with all the data I seem to be able to squeeze being local.
    public void writeToObj()
    {
        // https://github.com/HookJabs/CS240_3DRenderer/blob/master/crystals.obj
        // https://answers.unity.com/questions/539339/saving-data-in-to-files-android.html

        // are all of these planes 4 vertices???

        try
        {
            float timeSinceRecording = Time.time - mappingInitTime;

            // TODO: this shouldn't be * 16...
            string[] objLines = new string[planesTracked.Count * 16];
            objLines[0] = "# ARZombieMapper rectange EVENT obj output";
            objLines[1] = "# Time spent collecting data: " + timeSinceRecording;
            objLines[2] = "mtllib rectmapping.mtl";

            for (int index = 0; index < planesTracked.Count; index++)
            {
                // blender, why is it x, z, -y??
                ARPlane currentPlane = planesTracked[index];
                objLines[index + 2] = "o Rectmapping" + index; //make each rect it's own object


                Vector3 planeNormal = currentPlane.normal;
                objLines[index + 3] = "vn " + planeNormal.x + " " + planeNormal.z + " " + -1 * planeNormal.y;
                // Debug.Log(currentPlane.normal);
                // currentPlane
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

    // delete all points
    public void Wipe()
    {
        planesTracked.Clear();
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
