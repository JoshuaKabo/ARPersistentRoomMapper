using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;

public class ServerBehavior : MonoBehaviour
{
    // NetworkDriver - similar between clients & servers
    public NetworkDriver m_Driver;

    // Holds connections
    private NativeList<NetworkConnection> m_Connections;

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
        endPoint.Port = 9000;

        // Try to bind to a specific port and any ipv4
        if (m_Driver.Bind(endPoint) != 0)
            Debug.Log("s: Failed to bind to port 9000");
        else
        {
            // Now actively listening for connections
            m_Driver.Listen();
        }

        // Create a list to hold the 16 connections
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        // NetworkDriver & NativeList allocate unmanaged memory, need to be disposed
        m_Driver.Dispose();
        m_Connections.Dispose();
    }


    // Update is called once per frame
    void Update()
    {
        // Transport uses Unity C# Job System internally.
        // has a ScheduleUpdate method call, in Update, it should complete the JobHandle.
        m_Driver.ScheduleUpdate().Complete();

        // Force synch on main thread to update and handle
        // data later in the MonoBehavior::Update call
        // Jobified Client Server is probably better

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new connections
        NetworkConnection connection;
        while ((connection = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(connection);
            Debug.Log("s: Accepted a connection!");
        }

        // Start querying the driver for events that might have happened
        // since the last update

        // Stream will hold data on data event
        DataStreamReader dataStreamReader;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;

            // For each connection, call PopEventForConnection while there
            // are more events still needing to get processed
            // note theres also a non "forconnection" call for processing events
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out dataStreamReader)) != NetworkEvent.Type.Empty)
            {
                // process events, starting w/ Data event
                if (cmd == NetworkEvent.Type.Data)
                {
                    // Try to read a uint from stream, output it
                    uint number = dataStreamReader.ReadUInt();
                    Debug.Log("s: Got" + number + " from the Client. Adding + 2 to it!");

                    // Now add 2 and send it back (using a DataStreamWriter)
                    number += 2;

                    DataStreamWriter dataStreamWriter = new DataStreamWriter();

                    // status int (unused)
                    int sendStatusInt = m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out dataStreamWriter);
                    dataStreamWriter.WriteUInt(number);
                    m_Driver.EndSend(dataStreamWriter);
                }

                // disconnect case
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("s: Client disconnected from server");
                    // resets the connection
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }


    }
}
