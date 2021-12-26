using UnityEngine;

using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Jobs;
using UnityEngine.Assertions;

public class JobifiedServer : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NativeArray<NetworkConnection> m_Connections;

    // Keeps track of ongoing jobs
    private JobHandle ServerJobHandle;

    void Start()
    {
        // still 16 capped on connections...
        m_Connections = new NativeArray<NetworkConnection>(16, Allocator.Persistent);
        m_Driver = NetworkDriver.Create();

        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("s: Failed to bind to port 9000");
        else
            m_Driver.Listen();
    }

    // Update is called once per frame
    void Update()
    {
        ServerJobHandle.Complete();

        ServerUpdateConnectionsJob connectionJob;
        connectionJob = new ServerUpdateConnectionsJob
        {
            driver = m_Driver,
            connections = m_Connections
        };

        ServerUpdateJob serverUpdateJob;
        serverUpdateJob = new ServerUpdateJob
        {
            driver = m_Driver.ToConcurrent(),
            // this would be a deferred job array, but instead I'll just make both lists?
            connections = m_Connections
        };

        ServerJobHandle = m_Driver.ScheduleUpdate();
        ServerJobHandle = connectionJob.Schedule(ServerJobHandle);
        // batch count = 1
        // <NativeList<NetworkConnection>>
        // TODO: parallel jobs work very differently now than they used to. 
        // TODO: Set up the datastruct, then give the number of connections to update (all)
        // TODO: Then, the number of connections per thread 
        ServerJobHandle = serverUpdateJob.Schedule(m_Connections, 0, ServerJobHandle);
    }


    private void OnDestroy()
    {
        // ensure job completion before exiting
        ServerJobHandle.Complete();
        m_Connections.Dispose();
        m_Driver.Dispose();
    }


    // server jobs goal is to fan out and run the processing
    // code for all connections in parallel (hint: IJobParallelFor)
    // BUT we don't know how many connections will be recieved, so
    // we should use IJobParallelForDefer, however, that seems to 
    // be deprecated w/ no alternative. Maybe just use the OS's threads api?

    /*
    3 Options:
    1 - use OS threads api
    2 - use a fixed size, loaded with a bunch of blanks
        if there isn't a connection in that slot, skip it
    3 - just power through and see where I get stuck
    Could break up the connections into threadable groups?
    */
    struct ServerUpdateJob : IJobParallelFor
    {
        // Note the use of concurrent nw driver!
        // Allows call of the NWD from multiple threads
        public NetworkDriver.Concurrent driver;
        // Use of NativeArray instead of NativeList (would have been
        // important for a defered job)
        public NativeArray<NetworkConnection> connections;

        // Execute iterates over connections using index
        public void Execute(int index)
        {
            DataStreamReader dataStreamReader;
            // interesting assertion for ensuring index is created...
            Assert.IsTrue(connections[index].IsCreated);

            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(connections[index],
             out dataStreamReader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    uint num = dataStreamReader.ReadUInt();

                    Debug.Log("s: Got " + num + " from the Client, adding + 7 to it.");
                    num += 7;

                    DataStreamWriter dataStreamWriter;
                    driver.BeginSend(connections[index], out dataStreamWriter);
                    dataStreamWriter.WriteUInt(num);
                    driver.EndSend(dataStreamWriter);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("s: Client d/c'd from server");
                    connections[index] = default(NetworkConnection);
                }
            }
        }
    }

    // cleaning up closed connections and creating new ones cannot
    // be done in parallel, and is it's own job.
    struct ServerUpdateConnectionsJob : IJob
    {
        public NetworkDriver driver;
        public NativeList<NetworkConnection> connections;

        public void Execute()
        {
            // Clean up connections 
            for (int i = 0; i < connections.Length; i++)
            {
                if (!connections[i].IsCreated)
                {
                    connections.RemoveAtSwapBack(i);
                    --i;
                }
            }
            // Accept new connections
            NetworkConnection c;
            while ((c = driver.Accept()) != default(NetworkConnection))
            {
                connections.Add(c);
                Debug.Log("s: Accepted a connection!");
            }
        }
    }

}
