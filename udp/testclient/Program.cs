using System.Net;
using System.Net.Sockets;
using System.Text;

namespace testclient
{
    class Program
    {
        static VirtualSocket vs;
        static EndPoint endPoint;
        // static ReliabilityLayer rl;
        static GBN gbn;

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

                vs.PacketDropRate = 50;
                vs.PacketDelayRate = 0;
                vs.PacketDelayTimeSeconds = 0;
                vs.PacketBitErrorRate = 0;

                // vs.ReceiveTimeout = 1000;

                string alice = @"Alice was beginning to get very tired of sitting by her sister on
the bank, and of having nothing to do: once or twice she had
peeped into the book her sister was reading, but it had no
pictures or conversations in it, and where is the use of a book,
thought Alice, without pictures or conversations? So she was
considering in her own mind, (as well as she could, for the hot
day made her feel very sleepy and stupid,) whether the pleasure
of making a daisy-chain was worth the trouble of getting up and
picking the daisies, when a white rabbit with pink eyes ran close 
by her.

There was nothing very remarkable in that, nor did Alice think it
so very much out of the way to hear the rabbit say to itself
dear, dear! I shall be too late! (when she thought it over
afterwards, it occurred to her that she ought to have wondered at
this, but at the time it all seemed quite natural); but when the
rabbit actually took a watch out of its waistcoat-pocket, looked
at it, and then hurried on, Alice started to her feet, for it
flashed across her mind that she had never before seen a rabbit
with either a waistcoat-pocket or a watch to take out of it, and,
full of curiosity, she hurried across the field after it, and was
just in time to see it pop down a large rabbit-hole under the
hedge. In a moment down went Alice after it, never once
considering how in the world she was to get out again.

There were doors all round the hall, but they were all locked,
and when Alice had been all round it, and tried them all, she
walked sadly down the middle, wondering how she was ever to get
out again: suddenly she came upon a little three-legged table,
all made of solid glass; there was nothing lying upon it, but a
tiny golden key, and Alice's first idea was that it might belong
to one of the doors of the hall, but alas! either the locks were
too large, or the key too small, but at any rate it would open
none of them. However, on the second time round, she came to a
low curtain, behind which was a door about eighteen inches high:
she tried the little key in the keyhole, and it fitted! Alice
opened the door, and looked down a small passage, not larger than
a rat-hole, into the loveliest garden you ever saw.";

                byte[] messageBytes = Encoding.ASCII.GetBytes(alice);

                gbn = new GBN(vs, endPoint);
                gbn.sendData(messageBytes);

                // rl = new ReliabilityLayer(vs, endPoint);

                // for (int i = 0; i < 10; i++)
                // {
                //     send("heissan");
                //     Thread.Sleep(100);
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


        // static void send(string message)
        // {
        //     byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        //     rl.sendToWithErrors(messageBytes);
        // }
    }
}
