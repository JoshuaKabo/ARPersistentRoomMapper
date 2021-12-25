using UnityEngine;

using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Jobs;

public class JobifiedClient : MonoBehaviour
{
    public NetworkDriver m_Driver;
    // containerized connection and done for reading from job worker
    public NativeArray<NetworkConnection> m_Connection;
    public NativeArray<byte> m_DoneCommunicating;
    // tracks ongoing jobs
    public JobHandle ClientJobHandle;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        // persistent = long as you want, not just a couple frames
        m_Connection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);
        m_DoneCommunicating = new NativeArray<byte>(1, Allocator.Persistent);

        NetworkEndPoint endPoint = NetworkEndPoint.LoopbackIpv4;
        endPoint.Port = 9000;

        m_Connection[0] = m_Driver.Connect(endPoint);
    }

    // Update is called once per frame
    void Update()
    {
        // before running a new frame, ensure last frame was completed
        // note the perferred use of handle, not driver.scheduleupdate.
        ClientJobHandle.Complete();

        // job struct instance
        ClientUpdateJob job = new ClientUpdateJob
        {
            driver = m_Driver,
            connection = m_Connection,
            doneCommunicating = m_DoneCommunicating
        };

        // scheduleupdate makes networkdriver give a handle for the job
        ClientJobHandle = m_Driver.ScheduleUpdate();
        // now give the handle to the job to a new schedule
        ClientJobHandle = job.Schedule(ClientJobHandle);
    }

    private void OnDestroy()
    {
        // ensure job is complete before cleaning up data
        ClientJobHandle.Complete();

        // important to dispose all nativearrays
        m_Connection.Dispose();
        m_Driver.Dispose();
        m_DoneCommunicating.Dispose();
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
            else if (cmd == NetworkEvent.Type.Data)
            {
                uint value = dataStreamReader.ReadUInt();
                Debug.Log("c: Got the value " + value + " back from the server!");
                // set done to true, only accept one transmission
                doneCommunicating[0] = 1;
                connection[0].Disconnect(driver);
                connection[0] = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                Debug.Log("c: client got d/c'd");
                connection[0] = default(NetworkConnection);
            }
        }
    }
}
