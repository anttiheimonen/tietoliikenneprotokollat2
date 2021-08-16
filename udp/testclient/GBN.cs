using System;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Sockets;

public class GBN
{
    private byte[] data;
    private int chunkSize = 512;
    private int packetSize;
    private int windowSize = 2;
    private int ackReceived = -1;
    byte segmentNumber = 0;
    int numberOfTheLastPacket;
    private int ReceiveTimeout = 10;
    private VirtualSocket virtualSocket;
    private EndPoint endPoint;
    private Queue<DataPacket> queue;
    private TimeSpan resendTime = new TimeSpan(0, 0, 1);
    private int maxResendsPerPacket = 5;
    private bool processIsFailed = false;


    public GBN(VirtualSocket virtualSocket, EndPoint endPoint)
    {
        this.virtualSocket = virtualSocket;
        this.endPoint = endPoint;
        this.packetSize = chunkSize + 3;
        this.queue = new Queue<DataPacket>();
        virtualSocket.ReceiveTimeout = ReceiveTimeout;
    }


    public void sendData(byte[] data)
    {
        this.data = data;
        numberOfTheLastPacket = data.Length / chunkSize;
        Debug.WriteLine("Viimeisen paketin numero {0}", numberOfTheLastPacket);
        do
        {
            resendDataPackets();
            if (processIsFailed)
                return;
            sendNewDataPackets();
            receiveAcks();
        } while (ackReceived < numberOfTheLastPacket);
    }


    public void receiveAcks()
    {
        while (ackReceived < segmentNumber)
        {
            byte[] buffer = new byte[packetSize];
            int bytesGot = 0;
            // bytesGot = virtualSocket.ReceiveFromWithErrors(buffer, ref endPoint);
            bytesGot = receiveFromWithoutErrors(buffer);
            if (bytesGot == 0)
            {
                break;
            }
            byte[] packet = Tools.trimArray(buffer, bytesGot);
            bool ackIsOK = DataCheck.checkIntegrity(packet);
            if (!ackIsOK)
            {
                System.Console.WriteLine("Ack viestissa vikaa");
            }

            if (buffer[1] == (byte)PacketType.ack)
            {
                Debug.WriteLine("Saatu ack {0}", buffer[0]);
                ackReceived = (buffer[0] > ackReceived ? buffer[0] : ackReceived);
            }
        }
    }


    public void resendDataPackets()
    {
        Queue<DataPacket> newQueue = new Queue<DataPacket>();

        DateTime time = DateTime.Now;
        while (queue.Count > 0)
        {
            DataPacket packet = queue.Dequeue();
            System.Console.Write("Resend: packet {0}, ", packet.SeqNumber);
            if (packet.SeqNumber <= ackReceived)
            {
                System.Console.WriteLine("Ack saatu, paketti hoidettu");
                continue;
            }

            if (time > packet.SendTime + resendTime)
            {
                if (packet.SendAttempts > maxResendsPerPacket)
                {
                    System.Console.WriteLine("Too many resends, abort ");
                    processIsFailed = true;
                    return;
                }
                System.Console.Write("Ack missing, resending");
                sendPacket(packet);
            }
            else
            {
                System.Console.Write("Wait time is not over yet");
            }
            newQueue.Enqueue(packet);
            System.Console.WriteLine("");
        }
        queue = newQueue;
    }


    private void sendNewDataPackets()
    {
        while (segmentNumber <= ackReceived + windowSize && segmentNumber <= numberOfTheLastPacket)
        {
            byte[] dataSegment = getNextDataSegment();
            DataPacket packet = new DataPacket(segmentNumber, dataSegment, PacketType.data);
            sendPacket(packet);
            queue.Enqueue(packet);
            segmentNumber++;
        }
    }


    private void sendPacket(DataPacket packet)
    {
        virtualSocket.sendToWithErrors(packet.finalData, endPoint);
        packet.SendTime = DateTime.Now;
        packet.SendAttempts++;
        Debug.WriteLine("Lahetetty paketti {0}", packet.SeqNumber);
    }


    private byte[] getNextDataSegment()
    {
        int startIndex = chunkSize * segmentNumber;
        if (data.Length < (startIndex + chunkSize))
        {
            chunkSize = data.Length - startIndex;
        }
        byte[] chunk = new byte[chunkSize];
        Buffer.BlockCopy(data, startIndex, chunk, 0, chunkSize);

        return chunk;
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
