using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ApplicationUDP
{
    class Program
    {
        static void Main(string[] args)
        {
            const int port = 8001;
            IPAddress ip = GetLocalIP();
            var udpEndPoint = new IPEndPoint(ip, port);

            var udpSocet = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                udpSocet.Bind(udpEndPoint);

                WriteWithTime("Сервер запущен. Ожидание подключений...");
                while (true)
                {
                    IPEndPoint senderIp;
                    EndPoint senderEndPoin;
                    byte codeOperation;
                    {
                        var size = 0;
                        byte[] buffer = new byte[1];

                        senderEndPoin = new IPEndPoint(IPAddress.Any, 0);

                        size = udpSocet.ReceiveFrom(buffer, ref senderEndPoin);
                        senderIp = senderEndPoin as IPEndPoint;

                        codeOperation = buffer[0];
                    }

                    //WriteWithTime("Подключение установленно");

                    if (codeOperation == 1 || codeOperation == 2 || codeOperation == 3)
                    {
                        if (codeOperation == 1)
                        {
                            string filePath = GetMessage(udpSocet).ToString();

                            WriteWithTime(senderIp.Address.ToString() + ": Запрос на обработку файла: " + filePath);

                            byte[] array;
                            using (FileStream fStriam = new FileStream(filePath, FileMode.Open))
                            {
                                array = new byte[fStriam.Length];
                                fStriam.Read(array, 0, array.Length);
                            }
                            udpSocet.SendTo(array, senderEndPoin);
                        }
                        else if (codeOperation == 2)
                        {
                            WriteWithTime(senderIp.Address.ToString() + ": " + GetMessage(udpSocet).ToString());
                        }
                        else if (codeOperation == 3)
                        {
                            WriteWithTime(senderIp.Address.ToString() + ": " + "Клиент отключен");
                            //udpSocet.Shutdown(SocketShutdown.Both);
                            //udpSocet.Close();

                            WriteWithTime("Ожидание подключений...");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static StringBuilder GetMessage(Socket listeningSocket)
        {
            byte[] buffer = new byte[256];
            int messageSize;
            StringBuilder data = new StringBuilder();

            EndPoint senderEndPoin = new IPEndPoint(IPAddress.Any, 0);
            do
            {
                messageSize = listeningSocket.ReceiveFrom(buffer, ref senderEndPoin);
                data.Append(Encoding.UTF8.GetString(buffer, 0, messageSize));
            }
            while (listeningSocket.Available > 0);

            return data;
        }

        private static IPAddress GetLocalIP()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address;
            }
        }

        private static void WriteWithTime(string str)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + str);
        }
    }
}
