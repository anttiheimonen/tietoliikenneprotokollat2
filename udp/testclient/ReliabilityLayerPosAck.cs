using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

public class ReliabilityLayerPosAck
{
    VirtualSocket virtualSocket;
    EndPoint endPoint;
    int ackNumber = 0;

    public ReliabilityLayerPosAck(VirtualSocket vs, EndPoint endPoint)
    {
        this.virtualSocket = vs;
        this.endPoint = endPoint;
    }


    // public int sendToWithErrors(byte[] data)
    // {
    //     DataPacket packet = new DataPacket(data, PacketType.data);
    //     byte[] ack = new byte[512];
    //     int bytesSent = 0;
    //     int bytesGot = 0;
    //     int sendAttemps = 0;

    //     do
    //     {
    //         sendAttemps++;
    //         bytesSent = virtualSocket.sendToWithErrors(packet.finalData, endPoint);
    //         if (!(packet.type == PacketType.data))  // Sending ack or nack
    //             return bytesSent;

    //         Debug.WriteLine("Sending packet, attempt {0}", sendAttemps);
    //         // Wait for ack
    //         byte[] buffer = new byte[512];
    //         bytesGot = ReceiveAckFromWithErrors(buffer, ref endPoint);
    //         if (bytesGot > 0)
    //             packet.takeAck(buffer, bytesGot);
    //     } while (!packet.ackIsOK());

    //     return bytesSent;
    // }


    public int ReceiveFromWithErrors(byte[] buffer, ref EndPoint remoteEP)
    {
        int byteCount = virtualSocket.ReceiveFromWithErrors(buffer, ref remoteEP);
        if (!DataCheck.checkIntegrity(buffer))
        {
            // sendNack(ref remoteEP);
            System.Console.WriteLine("Paketissa on havaittu virhe!");
            return 0;
        }
        else
        {
            sendAck(ref remoteEP);
        }
        return byteCount;
    }


    public int ReceiveAckFromWithErrors(byte[] buffer, ref EndPoint remoteEP)
    {
        int byteCount = 0;

        try
        {
            byteCount = virtualSocket.ReceiveFromWithErrors(buffer, ref remoteEP);
            if (!DataCheck.checkIntegrity(buffer))
                return 0;
        }
        catch (SocketException e)
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
        DataPacket ack = new DataPacket((byte)ackNumber, new byte[] { }, PacketType.ack);
        ackNumber++;
        virtualSocket.sendToWithErrors(ack.finalData, remoteEP);
    }
}
