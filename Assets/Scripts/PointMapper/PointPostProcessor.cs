using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    TODO: Level floors
    Be sure that all "floor" points are on same y, so not accidentally a slope

    TODO: visually mark points with particles in real time
    (replace the old marker)
*/

public class PointPostProcessor : MonoBehaviour
{
    /*
    Could grab point groups by y difference, to determine floor
    or stairs, etc

    could move a rectangle of extrema size up and down to find
    and contain the floor, then size down to the floor
    
    could just group all points of similar y into the floor
    with the flat height as the average of all the points - 50% or so
    establish tiers? 
    each tier should have a larger and more connected surface area than

    could train a machine learning model
    using y and relationships as inputs

    there might be an algorithm or two that will help quite a bit
    clutering algorithm?
    really good clustering could establish:
    0) a group for floor
    0.5) individual furniture gets own cluster
    0.75) stairs by stair characteristics
    1) by cross sectional area, this is floor, the others are furniture
    
    */

    public void cleanUpPoints()
    {
        /* Do something */
        extractDeeperMeaning();
    }

    public void extractDeeperMeaning()
    {
        /* Do something */
        // NOW WRITE TO FILE
    }
}
