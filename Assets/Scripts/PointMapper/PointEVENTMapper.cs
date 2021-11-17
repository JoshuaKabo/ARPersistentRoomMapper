using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.IO;
using UnityEngine.UI;

/*
TODO: The keys to look for in the event mapper vs the all mapper:
    * Fewer? Duplicate points.
    * Higher? Overall confidence values.

    NOTE: confidence might not be as important when it comes to event based mapping
    But I'll leave that to observations from data.

    NOTE: I may want to map result objects as their individual point clouds rather than combining them...

    unsure of approach here...
    could hold on to all clouds and wait until write time to really look at the points
    could update all points on update... Nah...
    could hold on to clouds and evaluate the points every x seconds...

    Optimization Research:
    Gaussian Decomposition
    Space Partitioning
    Probablistic selection
    occupancy grid map generation

    GRID based processing using 3-D lidar and fusion with radar measurement
In this method, proposed by Philipp Lindner and Gerd Wanielik, laser data is processed using a multidimensional occupancy grid.[98] Data from a four-layer laser is pre-processed at the signal level and then processed at a higher level to extract the features of the obstacles. A combination two- and three-dimensional grid structure is used and the space in these structures is tessellated into several discrete cells. This method allows a huge amount of raw measurement data to be effectively handled by collecting it in spatial containers, the cells of the evidence grid. Each cell is associated with a probability measure that identifies the cell occupation. This probability is calculated by using the range measurement of the lidar sensor obtained over time and a new range measurement, which are related using Bayes' theorem. A two-dimensional grid can observe an obstacle in front of it, but cannot observe the space behind the obstacle. To address this, the unknown state behind the obstacle is assigned a probability of 0.5. By introducing the third dimension or in other terms using a multi-layer laser, the spatial configuration of an object could be mapped into the grid structure to a degree of complexity. This is achieved by transferring the measurement points into a three-dimensional grid. The grid cells which are occupied will possess a probability greater than 0.5 and the mapping would be color-coded based on the probability. The cells that are not occupied will possess a probability less than 0.5 and this area will usually be white space. This measurement is then transformed to a grid coordinate system by using the sensor position on the vehicle and the vehicle position in the world coordinate system. The coordinates of the sensor depend upon its location on the vehicle and the coordinates of the vehicle are computed using egomotion estimation, which is estimating the vehicle motion relative to a rigid scene. For this method, the grid profile must be defined. The grid cells touched by the transmitted laser beam are calculated by applying Bresenham's line algorithm. To obtain the spatially extended structure, a connected component analysis of these cells is performed. This information is then passed on to a rotating caliper algorithm to obtain the spatial characteristics of the object. In addition to the lidar detection, RADAR data obtained by using two short-range radars is integrated to get additional dynamic properties of the object, such as its velocity. The measurements are assigned to the object using a potential distance function.

Advantages and disadvantages
The geometric features of the objects are extracted efficiently, from the measurements obtained by the 3-D occupancy grid, using rotating caliper algorithm. Fusing the radar data to the lidar measurements give information about the dynamic properties of the obstacle such as velocity and location of the obstacle for the sensor location which helps the vehicle or the driver decide the action to be performed in order to ensure safety. The only concern is the computational requirement to implement this data processing technique. It can be implemented in real time and has been proven efficient if the 3-D occupancy grid size is considerably restricted. But this can be improved to an even wider range by using dedicated spatial data structures that manipulate the spatial data more effectively, for the 3-D grid representation.

Fusion of 3-D lidar and color camera for multiple object detection and tracking (this talks about some algorithms too)
The framework proposed in this method by Soonmin Hwang et al.,[99] is split into four steps. First, the data from the camera and 3-D lidar is input into the system. Both inputs from lidar and camera are parallelly obtained and the color image from the camera is calibrated with the lidar. To improve the efficiency, horizontal 3-D point sampling is applied as pre-processing. Second, the segmentation stage is where the entire 3-D points are divided into several groups per the distance from the sensor and local planes from close plane to far plane are sequentially estimated. The local planes are estimated using statistical analysis. The group of points closer to the sensor are used to compute the initial plane. By using the current local plane, the next local plane is estimated by an iterative update. The object proposals in the 2-D image are used to separate foreground objects from background. For faster and accurate detection and tracking Binarized Normed Gradients (BING) for Objectness Estimation at 300fps is used.[100] BING is a combination of normed gradient and its binarized version which speeds up the feature extraction and testing process, to estimate the objectness of an image window. This way the foreground and background objects are separated. To form objects after estimating the objectness of an image using BING, the 3-D points are grouped or clustered. Clustering is done using DBSCAN (Density-Based Spatial Clustering of Applications with Noise) algorithm which could be robust due to its less-parametric characteristic. Using the clustered 3-D points, i.e. 3-D segment, more accurate region-of-interests (RoIs) are generated by projecting 3-D points on the 2-D image.[101] The third step is detection, which is broadly divided into two parts. First is object detection in 2-D image which is achieved using Fast R-CNN[102] as this method doesn't need training and it also considers an image and several regions of interest. Second is object detection in 3-D space that is done by using the spin image method.[103] This method extracts local and global histograms to represent a certain object. To merge the results of 2-D image and 3-D space object detection, same 3-D region is considered and two independent classifiers from 2-D image and 3-D space are applied to the considered region. Scores calibration[104] is done to get a single confidence score from both detectors. This single score is obtained in the form of probability. The final step is tracking. This is done by associating moving objects in present and past frame. For object tracking, segment matching is adopted. Features such as mean, standard deviation, quantized color histograms, volume size and number of 3-D points of a segment are computed. Euclidean distance is used to measure differences between segments. To judge the appearance and disappearance of an object, similar segments (obtained based on the Euclidean distance) from two different frames are taken and the physical distance and dissimilarity scores are calculated. If the scores go beyond a range for every segment in the previous frame, the object being tracked is considered to have disappeared.

Should look into the process by which apple and google give these point clouds
might give some insight into my approach, how to use the points, where to edit the process.

simultaneous localization and mapping
rangefinding, situational awareness
Computer stereo vision
*/


/*
TODO 11/17
Get the number of points in these clouds
*/

public class PointEVENTMapper : PointMapper
{
    /*
    TODO: determine how often updates are mapped,
    determine if I can compare original to update w/ pointer comparison
    */

    public Text cloudsTrackedDebug;
    public List<ARPointCloud> cloudsTracked;

    private void Awake()
    {
        pointCloudManager.pointCloudsChanged += ctx => trackPointCloud(ctx.added, ctx.updated, ctx.removed);
    }

    protected override void Start()
    {
        cloudsTracked = new List<ARPointCloud>();
        visuallyMarkedPoints = new List<GameObject>();
        pointsForObj = new List<Vector4>();
        confDebug.text = "conf: " + necessaryConfidenceAmt;
        mappingInitTime = Time.time;
    }

    private void trackPointCloud(List<ARPointCloud> added, List<ARPointCloud> updated, List<ARPointCloud> removed)
    {
        // be sure to create new copies of those tracked, so they don't get deleted.
        // note - each plane has it's own subsumed by getter.
        // perhaps I can equality check <added> against <updated> to prevent double counting.
        // TODO: first try, just see if updated matches added in the end (because they should be taking care of themselves)
        // ALSO, see if moving to a fresh room wipes all I tracked


        /*
        foreach (ARPointCloud cloud in removed)
        {
            MAYBE IT WAS REMOVED BECAUSE IT WAS REPLACED W/ SOMETHING BETTER
            (...most likely it was removed because not in focus anymore...)
        }
        */

        /*
        foreach (ARPointCloud cloud in added)
        {
            if(in tracked)
                display debug message, replace the original
        }
        */


        foreach (ARPointCloud cloud in added)
        {
            Debug.Log("cloudAdded");
            // this, naievely, assumes that the updates will be reflected, and removed planes will stay in my own tracker 
            // (I want them to stay).
            cloudsTracked.Add(cloud);
            cloudsTrackedDebug.text = "Clouds Tracked:" + cloudsTracked.Count;
        }
    }

    /*
    Difference w/ event will be that it will write all those clouds it grabbed
    */
    public override void writeToObj()
    {
        // ends up in Pixel 2 XL\Internal shared storage\Android\data\com.DefaultCompany.ARMappingTool\files
        // https://github.com/HookJabs/CS240_3DRenderer/blob/master/crystals.obj
        // https://answers.unity.com/questions/539339/saving-data-in-to-files-android.html

        // fileDebug.text = "Requests permission here where necessary";


        string filePath = findFreshPath();

        int groupNum = -1;

        try
        {
            float timeSinceRecording = Time.time - mappingInitTime;
            List<string> objLines = new List<string>();
            objLines.Add("# ARZombieMapper vertex obj output (ALL POINTS, no event used.)");
            objLines.Add("# Confidence threshold: " + necessaryConfidenceAmt);
            objLines.Add("# Time spent collecting data: " + timeSinceRecording);
            objLines.Add("mtllib pointmapping.mtl");


            foreach (ARPointCloud pointCloud in cloudsTracked)
            {
                Debug.Log("mapping a cloud");

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

                    Vector3[] positions = new Vector3[cloudSize];

                    // save the positions to a native slice of V3s
                    ((Unity.Collections.NativeSlice<Vector3>)pointCloud.positions)
                        .CopyTo(positions);

                    // save the confs to a native array of floats
                    Unity.Collections.NativeArray<float> confidences =
                        (Unity.Collections.NativeArray<float>)
                        pointCloud.confidenceValues;

                    objLines.Add("o PtMappingG" + groupNum);

                    for (int index = 0; index < confidences.Length; index++)
                    {
                        if (confidences[index] > necessaryConfidenceAmt)
                        {
                            Debug.Log("mapping a point");

                            // prepare x, y, z, then confidence
                            objLines.Add("v " + positions[index].x + ' ' + positions[index].y + ' ' + positions[index].z + ' ' + confidences[index]);
                        }
                    }

                }

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
