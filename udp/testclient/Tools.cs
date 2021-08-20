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


    public static DataPacket[] moveArrayObjectsLeftAndNullRest(DataPacket[] array, int count)
    {
        for (int i = 0; i < array.Length - count; i++)
        {
            array[i] = array[i + count];
        }


        for (int i = array.Length - count; i < array.Length; i++)
        {
            array[i] = null;
        }

        return array;
    }


    public static byte[] combineArrays(byte[] LeftArray, byte[] RightArray)
    {
        int length = LeftArray.Length + RightArray.Length;
        byte[] combined = new byte[length];
        Buffer.BlockCopy(LeftArray, 0, combined, 0, LeftArray.Length);
        Buffer.BlockCopy(RightArray, 0, combined, LeftArray.Length, RightArray.Length);

        return combined;
    }
}