using System;

public class DataPacket
{
    public byte[] initData;
    public byte[] finalData;
    public byte[] ack;
    private byte seqNumber;
    public bool ackReceived = false;
    public PacketType type;

    private DateTime _sendTime;
    public DateTime SendTime
    {
        get => _sendTime;
        set
        {
            _sendTime = value;
        }
    }


    public DataPacket(byte seqNumber, byte[] data, PacketType packetType)
    {
        this.seqNumber = seqNumber;
        this.type = packetType;
        this.initData = data;
        byte[] temp = addSeqNumberAndType(initData);
        this.finalData = DataCheck.addCrc8(temp);
    }


    private byte[] addSeqNumberAndType(byte[] initData)
    {
        byte[] tempData = new byte[initData.Length + 2];
        tempData[0] = seqNumber;
        tempData[1] = (byte)type;
        Buffer.BlockCopy(initData, 0, tempData, 2, initData.Length);
        return tempData;
    }


    public bool ackIsOK()
    {
        if (ack == null || ack.Length < 2)
            return false;

        if (ack[0] == seqNumber && ack[1] == (byte)PacketType.ack)
            return true;

        return false;
    }


    public bool takeAck(byte[] ackMessage, int numberOfBytes)
    {
        if (numberOfBytes < 2)
            return false;

        ack = new byte[numberOfBytes - 1];
        Buffer.BlockCopy(ackMessage, 0, ack, 0, numberOfBytes - 1);
        return ackIsOK();
    }
}
