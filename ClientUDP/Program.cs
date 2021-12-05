﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ApplicationUDP
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string serverIp;
                {
                    Console.Write("Введите ip сервера: ");
                    serverIp = Console.ReadLine();
                }
                int clientPort = 8002, serverPort;
                {
                    Console.Write("Введите номер порта сервера (8001): ");
                    serverPort = Convert.ToInt32(Console.ReadLine());
                }

                var udpEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), clientPort);
                var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpSocket.Bind(udpEndPoint);

                while (true)
                {
                    try
                    {
                        byte codeOperation;
                        {
                            Console.WriteLine("1 - Найти среднее арифметическое чисел файла");
                            Console.WriteLine("2 - Написать сообщение");
                            Console.WriteLine("3 - Выйти");
                            Console.Write("Введите код операции: ");
                            codeOperation = Convert.ToByte(Console.ReadLine());
                        }

                        if (codeOperation == 1 || codeOperation == 2 || codeOperation == 3)
                        {
                            var serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
                            udpSocket.SendTo(new byte[] { codeOperation }, serverEndPoint);

                            if (codeOperation == 1)
                            {
                                byte[] data;
                                {
                                    Console.Write("Путь к файлу: ");
                                    var path = Console.ReadLine();
                                    data = Encoding.UTF8.GetBytes(path);
                                }
                                udpSocket.SendTo(data, serverEndPoint);

                                StringBuilder fileText = GetMessage(udpSocket);
                                Console.WriteLine("Среднее арифметическое чисел файла = " + ArithmeticMean(fileText));
                            }
                            else if (codeOperation == 2)
                            {
                                byte[] data;
                                {
                                    Console.Write("Введите сообщение: ");
                                    string message = Console.ReadLine();
                                    data = Encoding.UTF8.GetBytes(message);
                                }
                                udpSocket.SendTo(data, serverEndPoint);
                            }
                            else if (codeOperation == 3)
                            {
                                udpSocket.Shutdown(SocketShutdown.Both);
                                udpSocket.Close();
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static StringBuilder GetMessage(Socket listener)
        {
            byte[] buffer = new byte[256];
            int messageSize;
            StringBuilder data = new StringBuilder();

            EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

            do
            {
                messageSize = listener.ReceiveFrom(buffer, ref senderEndPoint);
                data.Append(Encoding.UTF8.GetString(buffer, 0, messageSize));
            }
            while (listener.Available > 0);
            return data;
        }

        private static double ArithmeticMean(StringBuilder fileText)
        {
            fileText.Append("\n");

            Regex regex = new Regex(@"(.*?)\n");
            MatchCollection matchCollection = regex.Matches(fileText.ToString());

            double arithmeticMean = default(double);
            if (matchCollection.Count > 0)
            {
                try
                {
                    for (int i = 0; i < matchCollection.Count; i++)
                    {
                        arithmeticMean += Convert.ToDouble(matchCollection[i].Groups[1].Value.Trim());
                    }
                    arithmeticMean /= matchCollection.Count;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return arithmeticMean;
        }
    }
}