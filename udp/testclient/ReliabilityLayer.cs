using System;
using System.Net;

public class ReliabilityLayer
{
    VirtualSocket virtualSocket;
    EndPoint endPoint;
    byte seqNumber = 0;
    byte waitingForSeqNumber = 0;

    public ReliabilityLayer(VirtualSocket vs, EndPoint endPoint)
    {
        this.virtualSocket = vs;
        this.endPoint = endPoint;
    }


    public int sendToWithErrors(byte[] data)
    {
        byte packetSeqNumber = seqNumber;
        DataPacket packet = new DataPacket(packetSeqNumber, data, PacketType.data);
        seqNumber++;
        byte[] ack = new byte[512];
        int bytesSent = 0;
        int sendAttempt = 0;
        do
        {
            sendAttempt++;
            System.Console.WriteLine("Lähetysyritys {0}", sendAttempt);
            packet.SendTime = DateTime.Now;
            bytesSent = virtualSocket.sendToWithErrors(packet.finalData, endPoint);
            if (!(packet.type == PacketType.data))  // Sending ack or nack
                return bytesSent;

            ReceiveAckFromWithErrors(packet, ref endPoint);
        } while (!packet.ackIsOK());

        System.Console.WriteLine("ACK saatu");

        return bytesSent;
    }


    public byte[] ReceiveFromWithErrors(ref EndPoint remoteEP)
    {
        byte[] buffer = new byte[512];
        int byteCount = virtualSocket.ReceiveFromWithErrors(buffer, ref remoteEP);

        // This can be 0 if socket times out.

        if (!DataCheck.checkIntegrity(buffer))
        {
            sendNack(0, ref remoteEP);
            System.Console.WriteLine("Paketissa on havaittu virhe!");
            return new byte[] { };
        }
        else
        {
            sendAck(buffer[0], ref remoteEP);
        }

        if (buffer[0] != waitingForSeqNumber)
        {
            // Don't send message up again
            return new byte[] { };
        }
        waitingForSeqNumber++;
        // Remove seq and check bytes
        byte[] response = new byte[byteCount - 3];
        Buffer.BlockCopy(buffer, 2, response, 0, response.Length);

        return response;
    }


    public int ReceiveAckFromWithErrors(DataPacket packet, ref EndPoint remoteEP)
    {
        byte[] buffer = new byte[512];
        int bytesGot = virtualSocket.ReceiveFromWithErrors(buffer, ref remoteEP);

        if (!DataCheck.checkIntegrity(buffer))
            return 0;

        if (bytesGot > 1)
            packet.takeAck(buffer, bytesGot);

        return bytesGot;
    }


    private void sendAck(byte AckSeqNumber, ref EndPoint remoteEP)
    {
        DataPacket ack = new DataPacket(AckSeqNumber, new byte[] { (byte)PacketType.ack }, PacketType.ack);
        virtualSocket.sendToWithErrors(ack.finalData, remoteEP);
        System.Console.WriteLine("Ack sent");
    }


    private void sendNack(byte NackSeqNumber, ref EndPoint remoteEP)
    {
        DataPacket nack = new DataPacket(NackSeqNumber, new byte[] { (byte)PacketType.nack }, PacketType.nack);
        virtualSocket.sendToWithErrors(nack.finalData, remoteEP);
        System.Console.WriteLine("Nack sent");
    }
}
