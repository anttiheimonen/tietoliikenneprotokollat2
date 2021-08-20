using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

public class ReliabilityLayerNegAck
{
    VirtualSocket virtualSocket;
    EndPoint endPoint;

    public ReliabilityLayerNegAck(VirtualSocket vs, EndPoint endPoint)
    {
        this.virtualSocket = vs;
        this.endPoint = endPoint;
    }


    public int sendToWithErrors(byte[] data)
    {
        DataPacket packet = new DataPacket(data, PacketType.data);
        byte[] ack = new byte[512];
        int bytesSent = 0;
        int bytesGot = 0;
        int sendAttemps = 0;
        bool nackReceived;

        do
        {
            sendAttemps++;
            bytesSent = virtualSocket.sendToWithErrors(packet.finalData, endPoint);
            if (!(packet.type == PacketType.data))  // Sending ack or nack
                return bytesSent;

            nackReceived = false;
            Debug.WriteLine("Sending packet, attempt {0}", sendAttemps);

            // Wait for nack
            byte[] buffer = new byte[512];
            bytesGot = ReceiveAckFromWithErrors(buffer, ref endPoint);
            if (bytesGot > 0)
                nackReceived = isNackFor(buffer);
        } while (nackReceived);

        return bytesSent;
    }


    public int ReceiveFromWithErrors(byte[] buffer, ref EndPoint remoteEP)
    {
        int byteCount = virtualSocket.ReceiveFromWithErrors(buffer, ref remoteEP);
        if (!DataCheck.checkIntegrity(buffer))
        {
            sendNack(ref remoteEP);
            System.Console.WriteLine("Paketissa on havaittu virhe!");
            return 0;
        }
        return byteCount;
    }


    public int ReceiveAckFromWithErrors(byte[] buffer, ref EndPoint remoteEP)
    {
        int byteCount = 0;
        virtualSocket.ReceiveTimeout = 5000;
        DateTime deadline = DateTime.Now.AddMilliseconds(5000);

        try
        {
            do
            {
                byteCount = virtualSocket.ReceiveFromWithErrors(buffer, ref remoteEP);
                virtualSocket.ReceiveTimeout = (int)(deadline - DateTime.Now).TotalMilliseconds;
            } while (DataCheck.checkIntegrity(buffer));
        }
        catch (SocketException)
        {
            Debug.WriteLine("Socket timed out");
        }

        return byteCount;
    }


    public bool waitAckForPacket(byte[] packet, byte[] ack)
    {
        return (ack[0] == 1);
    }


    private void sendAck(ref EndPoint remoteEP)
    {
        DataPacket ack = new DataPacket(new byte[] { 1 }, PacketType.ack);


        virtualSocket.sendToWithErrors(ack.finalData, remoteEP);
    }


    private void sendNack(ref EndPoint remoteEP)
    {
        DataPacket nack = new DataPacket(new byte[] { 2 }, PacketType.nack);
        virtualSocket.sendToWithErrors(nack.finalData, remoteEP);
    }


    private bool isNackFor(byte[] data)
    {
        if (data[0] == 2)
            return true;

        return false;
    }
}
