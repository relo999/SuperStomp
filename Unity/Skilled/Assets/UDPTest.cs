﻿using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDPTest : MonoBehaviour {

    private const int listenPort = 11000;
    UdpClient listener;
    IPEndPoint groupEP;

    // Use this for initialization
    void Start () {
        listener = new UdpClient(listenPort);
        groupEP = new IPEndPoint(IPAddress.Any, listenPort);
    }
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyDown(KeyCode.N))
        {
            //start server
            //StartListening();
            listener.BeginReceive(new AsyncCallback(recv), null);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            //start client/connect to server
        }
        /*
        string received_data;
        byte[] receive_byte_array;

        receive_byte_array = listener.Receive(ref groupEP);

        Debug.Log("Received a broadcast from "+ groupEP.ToString());
        received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
        Debug.Log("data follows \n" + received_data);
        */
    }

    private void recv(IAsyncResult res)
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8000);
        byte[] received = listener.EndReceive(res, ref RemoteIpEndPoint);

        //Process codes

        Debug.Log(Encoding.UTF8.GetString(received));
        Debug.Log(DateTime.Now.ToUniversalTime());
        listener.BeginReceive(new AsyncCallback(recv), null);
    }














    public int StartListening()
    {
        //bool done = false;
        //UdpClient listener = new UdpClient(listenPort);
        //IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
        
        /*try
        {
            while (!done)
            {
                Console.WriteLine("Waiting for broadcast");
                // this is the line of code that receives the broadcase message.
                // It calls the receive function from the object listener (class UdpClient)
                // It passes to listener the end point groupEP.
                // It puts the data from the broadcast message into the byte array
                // named received_byte_array.
                // I don't know why this uses the class UdpClient and IPEndPoint like this.
                // Contrast this with the talker code. It does not pass by reference.
                // Note that this is a synchronous or blocking call.
                receive_byte_array = listener.Receive(ref groupEP);
                Console.WriteLine("Received a broadcast from {0}", groupEP.ToString());
                received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                Console.WriteLine("data follows \n{0}\n\n", received_data);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }*/
        listener.Close();
        return 0;
    }


















    public void HolePunch(String ServerIp, Int32 Port)
    {
        IPEndPoint LocalPt = new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], Port);
        UdpClient Client = new UdpClient();
        Client.ExclusiveAddressUse = false;
        Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        Client.Client.Bind(LocalPt);

        IPEndPoint RemotePt = new IPEndPoint(IPAddress.Parse(ServerIp), Port);

        // This Part Sends your local endpoint to the server so if the two peers are on the same nat they can bypass it, you can omit this if you wish to just use the remote endpoint.
        byte[] IPBuffer = System.Text.Encoding.UTF8.GetBytes(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString());
        byte[] LengthBuffer = BitConverter.GetBytes(IPBuffer.Length);
        byte[] PortBuffer = BitConverter.GetBytes(Port);
        byte[] Buffer = new byte[IPBuffer.Length + LengthBuffer.Length + PortBuffer.Length];
        LengthBuffer.CopyTo(Buffer, 0);
        IPBuffer.CopyTo(Buffer, LengthBuffer.Length);
        PortBuffer.CopyTo(Buffer, IPBuffer.Length + LengthBuffer.Length);
        Client.BeginSend(Buffer, Buffer.Length, RemotePt, new AsyncCallback(SendCallback), Client);

        // Wait to receve something
        BeginReceive(Client, Port);

        // you may want to use a auto or manual ResetEvent here and have the server send back a confirmation, the server should have now stored your local (you sent it) and remote endpoint.

        // you now need to work out who you need to connect to then ask the server for there remote and local end point then need to try to connect to the local first then the remote.
        // if the server knows who you need to connect to you could just have it send you the endpoints as the confirmation.

        // you may also need to keep this open with a keepalive packet untill it is time to connect to the peer or peers.

        // once you have the endpoints of the peer you can close this connection unless you need to keep asking the server for other endpoints

        Client.Close();
    }

    public void ConnectToPeer(String PeerIp, Int32 Port)
    {
        IPEndPoint LocalPt = new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], Port);
        UdpClient Client = new UdpClient();
        Client.ExclusiveAddressUse = false;
        Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        Client.Client.Bind(LocalPt);
        IPEndPoint RemotePt = new IPEndPoint(IPAddress.Parse(PeerIp), Port);
        Client.Connect(RemotePt);
        //you may want to keep the peer client connections in a list.

        BeginReceive(Client, Port);
    }

    public void SendCallback(IAsyncResult ar)
    {
        UdpClient Client = (UdpClient)ar.AsyncState;
        Client.EndSend(ar);
    }

    public void BeginReceive(UdpClient Client, Int32 Port)
    {
        IPEndPoint ListenPt = new IPEndPoint(IPAddress.Any, Port);

        System.Object[] State = new System.Object[] { Client, ListenPt };

        Client.BeginReceive(new AsyncCallback(ReceiveCallback), State);
    }

    public void ReceiveCallback(IAsyncResult ar)
    {
        UdpClient Client = (UdpClient)((System.Object[])ar.AsyncState)[0];
        IPEndPoint ListenPt = (IPEndPoint)((System.Object[])ar.AsyncState)[0];

        Byte[] receiveBytes = Client.EndReceive(ar, ref ListenPt);
    }


}
