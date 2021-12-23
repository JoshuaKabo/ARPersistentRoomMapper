using UnityEngine;

using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Jobs;

public class JobifiedClient : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

struct ClientUpdateJob : IJob
{
    public NetworkDriver driver;
    public NativeArray<NetworkConnection> connection;
    public NativeArray<byte> doneCommunicating;

    public void Execute()
    {
        throw new System.NotImplementedException();
    }
}
