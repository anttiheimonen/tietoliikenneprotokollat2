public static class Crc8
{
    static byte[] table = new byte[256];
    // x8 + x7 + x6 + x4 + x2 + 1

    public static byte ComputeChecksum(params byte[] bytes)
    {
        byte crc = 0;
        if (bytes != null && bytes.Length > 0)
        {
            foreach (byte b in bytes)
            {
                crc = table[crc ^ b];
            }
        }
        return crc;
    }
}
