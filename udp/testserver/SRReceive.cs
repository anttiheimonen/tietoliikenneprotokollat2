using System;
using System.Net;
using System.Net.Sockets;

public class SRReceive
{
    public int rcvBase = 0;
    public int packetsReturned = 0;
    public int inWindow = 0;
    public int windowSize = 3;
    public DataPacket[] packetBuffer;
    public bool[] ackBuffer;
    private VirtualSocket virtualSocket;
    private EndPoint endPoint;


    public SRReceive(VirtualSocket virtualSocket, EndPoint endPoint)
    {
        packetBuffer = new DataPacket[windowSize];
        ackBuffer = new bool[windowSize];
        this.virtualSocket = virtualSocket;
        this.endPoint = endPoint;
        virtualSocket.ReceiveTimeout = 500;
    }


    public byte[] receive()
    {
        byte[] dataPackets;

        receiveToPacketBuffer();
        dataPackets = handleReadyToReturnPackets();

        return dataPackets;
    }


    private void addPacketToPacketBuffer(DataPacket packet)
    {
        if (packet.SeqNumber < rcvBase || packet.SeqNumber >= rcvBase + windowSize)
            return;

        int index = packet.SeqNumber - rcvBase;
        packetBuffer[index] = packet;
    }


    private void receiveToPacketBuffer()
    {
        byte[] buffer = new byte[1024];
        int packetsInWindow = 0;
        DateTime endTime = DateTime.Now + new TimeSpan(0, 0, 1);
        while (DateTime.Now < endTime && packetsInWindow < windowSize)
        {
            int bytesGot = receiveFromSocket(buffer);
            if (bytesGot == 0)
                continue;

            byte[] data = Tools.trimArray(buffer, bytesGot);

            if (!DataCheck.checkIntegrity(data))
            {
                System.Console.WriteLine("Viellinen paketti hylatty");
                continue;
            }
            packetsInWindow++;
            DataPacket packet = new DataPacket(data);
            addPacketToPacketBuffer(packet);
            sendAckFor(packet);
        }
    }


    private int receiveFromSocket(byte[] buffer)
    {
        int bytesGot = 0;
        try
        {
            bytesGot = virtualSocket.ReceiveFromWithErrors(buffer, ref endPoint);
        }
        catch (SocketException e)
        {

        }

        return bytesGot;
    }


    private byte[] handleReadyToReturnPackets()
    {
        byte[] combinedBuffer = new byte[0];
        int packetsHandled = 0;
        System.Console.WriteLine("rcvBase {0}", rcvBase);
        for (int i = 0; i < windowSize; i++)
        {
            if (packetBuffer[i] == null || packetBuffer[i].SeqNumber != rcvBase)
                break;

            combinedBuffer = Tools.combineArrays(combinedBuffer, packetBuffer[i].initData);
            packetsHandled++;
            rcvBase++;
        }

        packetBuffer = Tools.moveArrayObjectsLeftAndNullRest(packetBuffer, packetsHandled);

        return combinedBuffer;
    }


    private void sendAckFor(DataPacket packet)
    {
        if (packet.SeqNumber >= rcvBase + windowSize)
            return;

        send(packet.getAck());
    }


    private void send(byte[] data)
    {
        virtualSocket.SendTo(data, endPoint);
    }

}