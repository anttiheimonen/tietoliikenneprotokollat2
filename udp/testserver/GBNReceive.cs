using System.Diagnostics;
using System.Net;

public class GBNReceive
{

    private int chunkSize = 512;
    private int packetSize;
    private int windowSize = 2;
    private int lastAckSend = -1;
    private byte waitingForPacketNumber = 0;
    private VirtualSocket virtualSocket;
    private EndPoint endPoint;
    byte[] lastAckPacket;

    public GBNReceive(VirtualSocket virtualSocket, EndPoint endPoint)
    {
        this.virtualSocket = virtualSocket;
        this.endPoint = endPoint;
        this.packetSize = chunkSize + 3;
    }


    public byte[] receiveGBN()
    {
        byte[] buffer = new byte[packetSize];
        int bytesGot = 0;
        bytesGot = virtualSocket.ReceiveFromWithErrors(buffer, ref endPoint);

        byte[] packet = Tools.trimArray(buffer, bytesGot);
        bool pakettiKunnossa = DataCheck.checkIntegrity(packet);
        System.Console.WriteLine("");

        if (!pakettiKunnossa)
        {
            System.Console.WriteLine("Paketti on rikki");

            return new byte[0];
        }

        if (packet[0] < waitingForPacketNumber)
        {
            sendCurrentAck();
            return new byte[0];
        }
        if (packet[0] == waitingForPacketNumber)
        {
            Debug.Write("Seuraava paketti " + packet[0] + " saatu");
            sendNextAck();
            waitingForPacketNumber++;
            return Tools.getData(packet);
        }
        return new byte[0];
    }


    private void sendNextAck()
    {
        lastAckSend++;
        sendCurrentAck();
    }


    private void sendCurrentAck()
    {
        DataPacket ack = new DataPacket((byte)lastAckSend, new byte[] { }, PacketType.ack);
        lastAckPacket = new byte[] { (byte)lastAckSend, (byte)PacketType.ack };
        virtualSocket.sendToWithErrors(ack.finalData, endPoint);
    }
}