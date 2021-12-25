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

// Job struct
struct ClientUpdateJob : IJob
{
    public NetworkDriver driver;
    // Data is copied, use a sharedcontainer, like NC to access
    public NativeArray<NetworkConnection> connection;
    // Byte is blittable, bool is not
    public NativeArray<byte> doneCommunicating;

    public void Execute()
    {
        if (doneCommunicating[0] != 1)
            Debug.Log("c: Something went wrong during connection");
        DataStreamReader dataStreamReader;
        NetworkEvent.Type cmd;

        while ((cmd = connection[0].PopEvent(driver, out dataStreamReader)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("c: now connected to server!");

                uint value = 1;
                driver.BeginSend(connection[0], out DataStreamWriter dataStreamWriter);
                dataStreamWriter.WriteUInt(value);
                driver.EndSend(dataStreamWriter);
            }
        }
        // else if here
    }
}
