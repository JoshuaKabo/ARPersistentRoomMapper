using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;

public class ClientBehavior : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;

    public bool communicationDone;


    private void Start()
    {
        // Driver & connection creation
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        // Endpoint and connection setup
        NetworkEndPoint endPoint = NetworkEndPoint.LoopbackIpv4;
        endPoint.Port = 9000;
        m_Connection = m_Driver.Connect(endPoint);
    }

    private void OnDestroy()
    {
        // NetworkDriver allocates unmanaged memory, need to be disposed
        m_Driver.Dispose();

    }

    private void Update()
    {
        // Transport uses Unity C# Job System internally.
        // has a ScheduleUpdate method call, in Update, it should complete the JobHandle.
        m_Driver.ScheduleUpdate().Complete();

        //if a connection is not made, and communication should be ongoing, the connect failed 
        if (!m_Connection.IsCreated)
        {
            if (!communicationDone)
                Debug.Log("Something went wrong during connect");
        }

        DataStreamReader dataStreamReader;
        NetworkEvent.Type cmd;


        // PopEvent is designed for a single connection
        // Pops network events off the stack for processing
        while ((cmd = m_Connection.PopEvent(m_Driver, out dataStreamReader)) != NetworkEvent.Type.Empty)
        {
            // NetworkEvent.Type.Connect event - tells that
            // you recieved a Connection accept message, now connected to the remote peer
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("I, the Client, connected to the server!");

                uint value = 1;
                // NOTE: This may not be the BeginSend paramlist I want
                m_Driver.BeginSend(m_Connection, out DataStreamWriter dataStreamWriter);
                dataStreamWriter.WriteUInt(value);
                m_Driver.EndSend(dataStreamWriter);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                uint value = dataStreamReader.ReadUInt();
            }
        }

    }
}
