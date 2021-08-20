﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace testclient
{
    class Program
    {
        static VirtualSocket vs;
        static EndPoint endPoint;
        // static ReliabilityLayer rl;
        // static GBN gbn;
        static SR sr;

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

The rabbit-hole went straight on like a tunnel for some way, and
then dipped suddenly down, so suddenly, that Alice had not a
moment to think about stopping herself, before she found herself
falling down what seemed a deep well. Either the well was very
deep, or she fell very slowly, for she had plenty of time as she
went down to look about her, and to wonder what would happen
next. First, she tried to look down and make out what she was
coming to, but it was too dark to see anything: then, she looked
at the sides of the well, and noticed that they were filled with
cupboards and book-shelves: here and there were maps and pictures
hung on pegs. She took a jar down off one of the shelves as she
passed: it was labelled Orange Marmalade, but to her great
disappointment it was empty: she did not like to drop the jar,
for fear of killing somebody underneath, so managed to put it
into one of the cupboards as she fell past it.

Well! thought Alice to herself, after such a fall as this, I
shall think nothing of tumbling down stairs! How brave they'll
all think me at home! Why, I wouldn't say anything about it, even
if I fell off the top of the house! (which was most likely
true.)

Down, down, down. Would the fall never come to an end? I wonder
how many miles I've fallen by this time? she said aloud, I must
be getting somewhere near the centre of the earth. Let me see:
that would be four thousand miles down, I think-- (for you see
Alice had learnt several things of this sort in her lessons in
the schoolroom, and though this was not a very good opportunity
of showing off her knowledge, as there was no one to hear her,
still it was good practice to say it over,) yes that's the right
distance, but then what Longitude or Latitude-line shall I be
in? (Alice had no idea what Longitude was, or Latitude either,
but she thought they were nice grand words to say.)

Presently she began again: I wonder if I shall fall right
through the earth! How funny it'll be to come out among the
people that walk with their heads downwards! But I shall have to
ask them what the name of the country is, you know. Please,
Ma'am, is this New Zealand or Australia?--and she tried to
curtsey as she spoke (fancy curtseying as you're falling through
the air! do you think you could manage it?) and what an ignorant
little girl she'll think me for asking! No, it'll never do to
ask: perhaps I shall see it written up somewhere.

Down, down, down: there was nothing else to do, so Alice soon
began talking again. Dinah will miss me very much tonight, I
should think! (Dinah was the cat.) I hope they'll remember her
saucer of milk at tea-time! Oh, dear Dinah, I wish I had you
here! There are no mice in the air, I'm afraid, but you might
catch a bat, and that's very like a mouse, you know, my dear. But
do cats eat bats, I wonder? And here Alice began to get rather
sleepy, and kept on saying to herself, in a dreamy sort of way
do cats eat bats? do cats eat bats? and sometimes, do bats
eat cats? for, as she couldn't answer either question, it didn't
much matter which way she put it. She felt that she was dozing
off, and had just begun to dream that she was walking hand in
hand with Dinah, and was saying to her very earnestly, Now,
Dinah, my dear, tell me the truth. Did you ever eat a bat? when
suddenly, bump! bump! down she came upon a heap of sticks and
shavings, and the fall was over.";

                byte[] messageBytes = Encoding.ASCII.GetBytes(alice);

                sr = new SR(vs, endPoint);
                sr.send(messageBytes);

                // gbn = new GBN(vs, endPoint);
                // gbn.sendData(messageBytes);

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
