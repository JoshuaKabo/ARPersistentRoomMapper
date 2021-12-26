using UnityEngine;

using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Jobs;

public class JobifiedServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // server jobs goal is to fan out and run the processing
    // code for all connections in parallel (hint: IJobParallelFor)
    // BUT we don't know how many connections will be recieved, so
    // we use IJobParallelForDefer
    struct ServerUpdateJob : IJobParallelFor
    {

    }

}
