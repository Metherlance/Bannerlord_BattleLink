using System.Threading;
using System;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using BattleLink.Web.Common;
using System.Net.Sockets;
using TaleWorlds.Library;
using System.Runtime.Remoting.Channels;
using TaleWorlds.Diamond;

namespace BattleLink.Singleplayer
{
    public class UdpSpSw
    {
        private static NetManager client;
        private static EventBasedNetListener listener;
        private static Thread networkThread;

        public static void start()
        {
            try
            {
                listener = new EventBasedNetListener();
                client = new NetManager(listener);
                client.Start();
                client.Connect("localhost", 9050, "SomeConnectionKey");
                listener.NetworkReceiveEvent += (NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod) =>
                {
                    var mes = reader.GetString(100 /* max length of string */);
                    InformationManager.DisplayMessage(new InformationMessage(" "+ mes));
                    Console.WriteLine("We got: {0}", mes);
                    reader.Recycle();
                };

                listener.PeerDisconnectedEvent += (NetPeer peer, DisconnectInfo disconnectInfo) =>
                {
                    Console.WriteLine($"We got disconnection: {peer.Address} {disconnectInfo.Reason}");
                    Reconnect();
                };

                networkThread = new Thread(() =>
                {
                    while (true)
                    {
                        client.PollEvents();
                        Thread.Sleep(50);
                    }
                });

                networkThread.IsBackground = true;
                networkThread.Start();
                //networkThread.Abort();


                var writer = new NetDataWriter();
                writer.Put("afdgfjdj");
                client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered); //ReliableUnordered

            }
            catch (Exception e)
            {
                Console.WriteLine($"RPC failed: {e}");
            }

        }

        private static void Reconnect()
        {
            try
            {
                client.Stop();
                client.Start();
                client.Connect("localhost", 9050, "SomeConnectionKey");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Reconnect failed: {e}");
            }
        }

        //private static void SendMessage(NetManager client, string message)
        //{
        //    var writer = new NetDataWriter();
        //    writer.Put(message);
        //    client.FirstPeer.Send(writer, DeliveryMethod.ReliableUnordered);
        //}

        public static void Send(PacketSp2SwType type, InitCampaignData data, DeliveryMethod deliveryMethod)
        {
            // look at CampaignEvents

            try
            {
                byte[] dataBin = ProtoUtils.Serialize(data);
               // byte[] packet = ProtoUtils.Serialize(new PacketSp2Sw { PacketType = type, Data = dataBin });

                var writer = new NetDataWriter();
                writer.Put((ushort)type);
                writer.Put(dataBin);

                if (client==null)
                {
                    start();
                }
                else if (client.FirstPeer == null)
                {
                    //client.DisconnectAll();
                    //start();
                    //client.Connect("localhost", 9050, "SomeConnectionKey");
                    Reconnect();
                }
                client.FirstPeer.Send(writer, DeliveryMethod.ReliableUnordered);//deliveryMethod DeliveryMethod.ReliableOrdered
            }
            catch (Exception e)
            {
                Console.WriteLine($"RPC failed: {e}");
            }
        }


    }
}