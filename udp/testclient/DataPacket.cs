using System;

public class DataPacket
{
    public byte[] initData;
    public byte[] finalData;
    public byte[] ack;
    private byte _seqNumber;
    public byte SeqNumber { get => _seqNumber; }

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
    private DateTime _resendTime;
    public DateTime ResendTime
    {
        get => _resendTime;
        set
        {
            _resendTime = value;
        }
    }

    private int _sendAttempts = 0;
    public int SendAttempts
    {
        get => _sendAttempts;
        set
        {
            _sendAttempts = value;
        }
    }


    public DataPacket(byte seqNumber, byte[] data, PacketType packetType)
    {
        this._seqNumber = seqNumber;
        this.type = packetType;
        this.initData = data;
        byte[] temp = addSeqNumberAndType(initData);
        this.finalData = DataCheck.addCrc8(temp);
    }


    public DataPacket(byte[] finalData)
    {
        if (finalData.Length < 3)
            return;

        this.finalData = finalData;
        this._seqNumber = finalData[0];
        this.type = (PacketType)finalData[1];
        extractInitDataFromFinal(finalData);
    }


    private byte[] addSeqNumberAndType(byte[] initData)
    {
        byte[] tempData = new byte[initData.Length + 2];
        tempData[0] = _seqNumber;
        tempData[1] = (byte)type;
        Buffer.BlockCopy(initData, 0, tempData, 2, initData.Length);
        return tempData;
    }


    public bool ackIsOK()
    {
        if (ack == null || ack.Length < 2)
            return false;

        if (ack[0] == _seqNumber && ack[1] == (byte)PacketType.ack)
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


    private void extractInitDataFromFinal(byte[] finaldata)
    {
        var data = new byte[finalData.Length - 3];
        Buffer.BlockCopy(finaldata, 2, data, 0, data.Length);
        this.initData = data;
    }


    public byte[] getAck()
    {
        var ack = new byte[] { this.SeqNumber, (byte)PacketType.ack };
        return DataCheck.addCrc8(ack);
    }
}
