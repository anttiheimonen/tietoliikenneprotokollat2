using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace testclient
{
    class Program
    {
        static VirtualSocket vs;
        static EndPoint endPoint;
        static ReliabilityLayer rl;

        static void Main(string[] args)
        {
            int port = 12345;

            if (args.Length != 2)
            {
                System.Console.WriteLine("Ohje: kasky osoite portti");
                return;
            }

            try
            {
                vs = new VirtualSocket(AddressFamily.InterNetwork,
                                        SocketType.Dgram,
                                        ProtocolType.Udp);

                IPAddress host = IPAddress.Parse("127.0.0.1");
                IPEndPoint iep = new IPEndPoint(host, port);
                endPoint = (EndPoint)iep;

                vs.PacketDropRate = 25;
                vs.PacketDelayRate = 0;
                vs.PacketDelayTimeSeconds = 0;
                vs.PacketBitErrorRate = 0;

                vs.ReceiveTimeout = 1000;

                rl = new ReliabilityLayer(vs, endPoint);

                for (int i = 0; i < 10; i++)
                {
                    send("heissan");
                    Thread.Sleep(100);
                }
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


        static void send(string message)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            rl.sendToWithErrors(messageBytes);
        }
    }
}
