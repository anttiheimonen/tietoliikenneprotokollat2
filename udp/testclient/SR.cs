using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

public class SR
{
    private byte[] data;
    public int sendBase = 0;
    public int windowSize = 3;
    private int packetSize;
    public byte seqNumber = 0;
    private int numberOfTheLastPacket;
    private int chunkSize = 512;
    public DataPacket[] packetBuffer;
    private VirtualSocket virtualSocket;
    private EndPoint endPoint;
    private TimeSpan resendTime = new TimeSpan(0, 0, 1);
    bool processIsFailed;
    private int ReceiveTimeout = 1000;


    public SR(VirtualSocket virtualSocket, EndPoint endPoint)
    {
        this.virtualSocket = virtualSocket;
        this.endPoint = endPoint;
        packetBuffer = new DataPacket[windowSize];
        virtualSocket.ReceiveTimeout = 1000;
    }


    public void send(byte[] data)
    {
        this.data = data;
        numberOfTheLastPacket = data.Length / chunkSize;
        packetSize = chunkSize + 3;

        do
        {
            resendDataPackets();
            if (processIsFailed)
                return;
            sendNewDataPackets();
            receiveAcks();
            handleAckedPackets();
        } while (sendBase <= numberOfTheLastPacket);
    }


    private void sendNewDataPackets()
    {
        while (seqNumber < sendBase + windowSize && seqNumber <= numberOfTheLastPacket)
        {
            byte[] dataSegment = getNextDataSegment();
            DataPacket packet = new DataPacket(seqNumber, dataSegment, PacketType.data);
            Debug.WriteLine("Luotu paketti {0}", packet.SeqNumber);
            sendPacket(packet);
            packetBuffer[seqNumber - sendBase] = packet;
            seqNumber++;
        }
    }


    private void receiveAcks()
    {
        while (sendBase <= numberOfTheLastPacket)
        {
            byte[] buffer = new byte[packetSize];
            int bytesGot = 0;
            bytesGot = receiveFromWithoutErrors(buffer);
            if (bytesGot == 0)
            {
                break; // SocketTimeout palauttaa 0 bytea
            }
            byte[] ackBytes = Tools.trimArray(buffer, bytesGot);
            bool packetIsOK = DataCheck.checkIntegrity(ackBytes);
            if (!packetIsOK)
            {
                System.Console.WriteLine("Ack viestissa vikaa");
            }

            if (ackBytes[1] == (byte)PacketType.ack)
            {
                Debug.WriteLine("Saatu ack {0}", buffer[0]);
                packetBuffer[ackBytes[0] - sendBase].ackReceived = true;
            }
        }
    }


    private void resendDataPackets()
    {
        foreach (var packet in packetBuffer)
        {
            if (packet == null)
                continue;
            if (packet.ResendTime < DateTime.Now && packet.ackReceived == false)
                sendPacket(packet);
        }
    }


    private void handleAckedPackets()
    {
        int sendBaseRaised = 0;
        for (int i = 0; i < packetBuffer.Length; i++)
        {
            if (packetBuffer[i] == null)
                continue;

            if (packetBuffer[i].ackReceived == true)
            {
                // packetBuffer[i].ackReceived = true;
                if (packetBuffer[i].SeqNumber == sendBase)
                {
                    sendBase++;
                    sendBaseRaised++;
                }
            }
        }

        // Move packets forward in buffer array
        for (int i = 0; i < packetBuffer.Length - sendBaseRaised; i++)
        {
            packetBuffer[i] = packetBuffer[i + sendBaseRaised];
        }

        // Set rest of the array nulls
        for (int i = packetBuffer.Length - sendBaseRaised; i < packetBuffer.Length; i++)
        {
            packetBuffer[i] = null;
        }
    }


    private byte[] getNextDataSegment()
    {
        int startIndex = chunkSize * seqNumber;
        if (data.Length < (startIndex + chunkSize))
        {
            chunkSize = data.Length - startIndex;
        }
        byte[] chunk = new byte[chunkSize];
        Buffer.BlockCopy(data, startIndex, chunk, 0, chunkSize);

        return chunk;
    }


    private void sendPacket(DataPacket packet)
    {
        virtualSocket.sendToWithErrors(packet.finalData, endPoint);
        packet.SendTime = DateTime.Now;
        packet.ResendTime = DateTime.Now + resendTime;
        packet.SendAttempts++;
        Debug.WriteLine("Lahetetty paketti {0}, pituus {1}", packet.SeqNumber, packet.finalData.Length);
    }


    private int receiveFromWithoutErrors(byte[] buffer)
    {
        int bytesGot = 0;
        try
        {
            bytesGot = virtualSocket.ReceiveFrom(buffer, ref endPoint);
        }
        catch (SocketException e)
        {
        }

        return bytesGot;
    }
}