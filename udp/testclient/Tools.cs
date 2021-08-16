using System;

public static class Tools
{
    public static byte[] trimArray(byte[] array, int length)
    {
        byte[] packet = new byte[length];
        Buffer.BlockCopy(array, 0, packet, 0, length);
        return packet;
    }

    public static byte[] getData(byte[] packet)
    {
        byte[] data = new byte[packet.Length - 3];
        Buffer.BlockCopy(packet, 2, data, 0, packet.Length - 3);
        return data;
    }
}