using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

public class VirtualSocket : Socket
{

    private int _packetDropRate;
    public int PacketDropRate
    {
        get => _packetDropRate;
        set
        {
            if ((value >= 0) && (value <= 100))
            {
                _packetDropRate = value;
            }
        }
    }

    private int _packetDelayRate;
    public int PacketDelayRate
    {
        get => _packetDelayRate;
        set
        {
            if ((value >= 0) && (value <= 100))
                _packetDelayRate = value;
        }
    }

    private int _packetDelayTimeSeconds;
    public int PacketDelayTimeSeconds
    {
        get => _packetDelayTimeSeconds;
        set
        {
            if ((value >= 0) && (value <= 100))
                _packetDelayTimeSeconds = value;
        }
    }

    private int _packetBitErrorRate;
    public int PacketBitErrorRate
    {
        get => _packetBitErrorRate;
        set
        {
            if ((value >= 0) && (value <= 100))
                _packetBitErrorRate = value;
        }
    }

    Random rand = new Random();


    public VirtualSocket(AddressFamily addressFamily,
                            SocketType socketType,
                            ProtocolType protocolType) :
                        base(addressFamily,
                            socketType,
                            protocolType)
    {
        this.ReceiveTimeout = 0;
    }


    public int ReceiveFromWithErrors(byte[] buffer, ref EndPoint remoteEP)
    {
        int bytesGot = 0;
        try
        {
            if (dropPacket())
            {
                byte[] blackhole = new byte[2048];
                ReceiveFrom(blackhole, ref remoteEP);
                Debug.WriteLine("Incoming packet dropped");
                buffer = new byte[2048];
                return 0;
            }

            bytesGot = ReceiveFrom(buffer, ref remoteEP);

            if (makeBitErrorToPacket())
            {
                Debug.WriteLine("Incomig packet sabotaged");
                buffer = makeBitError(buffer, bytesGot);
            }

            addPossibleDelay();

        }
        catch (SocketException e)
        {
            Debug.WriteLine("Socket timed out");
        }

        return bytesGot;
    }


    public int sendToWithErrors(byte[] buffer, EndPoint remoteEP)
    {
        // Array changes go back up, so don't mess with original data!
        byte[] bufferWitErrors = (byte[])buffer.Clone();
        if (dropPacket())
        {
            Debug.WriteLine("Outgoing packet dropped");
            return 0;
        }

        addPossibleDelay();
        if (makeBitErrorToPacket())
        {
            Debug.WriteLine("Outgoing packet sabotaged");
            buffer = makeBitError(bufferWitErrors, bufferWitErrors.Length);
        }

        return SendTo(bufferWitErrors, remoteEP);
    }


    private void addPossibleDelay()
    {
        if (rand.Next(100) < PacketDelayRate)
            Thread.Sleep(PacketDelayTimeSeconds * 1000);
    }


    private byte[] makeBitError(byte[] buffer, int packetLength)
    {
        int errorIndex = rand.Next(0, packetLength);
        buffer[errorIndex] = (byte)(buffer[errorIndex] - 1);

        return buffer;
    }


    private bool dropPacket()
    {
        int random = rand.Next(100);
        // Debug.WriteLine("pudotetaanko paketti: {0} {1}", random, PacketDropRate);
        return (random < PacketDropRate);
    }


    private bool delayPacket()
    {
        return (rand.Next(100) < PacketDelayRate);
    }


    private bool makeBitErrorToPacket()
    {
        return (rand.Next(100) < PacketBitErrorRate);
    }
}
