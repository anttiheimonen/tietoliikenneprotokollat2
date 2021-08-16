using System;

public class DataCheck
{
    public static bool checkIntegrity(byte[] bytes)
    {
        // byte CheckSum = Crc8.ComputeChecksum(bytes);
        return (Crc8.ComputeChecksum(bytes) == 0);
    }


    public static byte[] addCrc8(byte[] bytes)
    {
        byte CheckSum = Crc8.ComputeChecksum(bytes);
        byte[] withCrc8 = new byte[bytes.Length + 1];

        Buffer.BlockCopy(bytes, 0, withCrc8, 0, bytes.Length);
        withCrc8[withCrc8.Length - 1] = (byte)(CheckSum);

        return withCrc8;
    }
}