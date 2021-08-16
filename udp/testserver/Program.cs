using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace testserver
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 12345;
            Console.WriteLine("Olen testiserveri");

            EndPoint endPoint = null;
            VirtualSocket vs = null;
            // ReliabilityLayer rl = null;
            GBNReceive gbn;

            try
            {
                IPEndPoint client = new IPEndPoint(IPAddress.Any, port);
                endPoint = (EndPoint)client;

                vs = new VirtualSocket(AddressFamily.InterNetwork,
                       SocketType.Dgram,
                       ProtocolType.Udp);

                vs.Bind(endPoint);
                vs.PacketDropRate = 0;
                vs.PacketDelayRate = 0;
                vs.PacketDelayTimeSeconds = 0;
                gbn = new GBNReceive(vs, endPoint);

                int packetsGot = 0;
                StringBuilder stringBuilder = new StringBuilder();
                while (packetsGot < 5)
                {
                    byte[] message = gbn.receiveGBN();
                    if (message.Length > 0)
                    {
                        string s = Encoding.ASCII.GetString(message, 0, message.Length);
                        stringBuilder.Append(s);
                        packetsGot++;
                    }
                }
                System.Console.Write(stringBuilder.ToString());

                // rl = new ReliabilityLayer(vs, endPoint);

                // while (true)
                // {
                //     byte[] message = rl.ReceiveFromWithErrors(ref endPoint);

                //     if (message.Length > 0)
                //     {
                //         string s = Encoding.ASCII.GetString(message, 0, message.Length);
                //         System.Console.WriteLine(s);
                //     }

                // }
            }
            catch (System.Exception)
            {
                throw;
            }
            finally
            {
                vs.Close();
            }
        }
    }
}
