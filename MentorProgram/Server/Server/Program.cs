using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private static List<Socket> clients = new List<Socket>();
        private static List<string> messages = new List<string>();
        private static List<User> users = new List<User>();
        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        class User
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Сервер запущен...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 2048));
            serverSocket.Listen(10);
            serverSocket.BeginAccept(AcceptCallBack, null);
           
            Console.ReadLine();
        }

        private static void AcceptCallBack(IAsyncResult asyncResult)
        {
            Socket socket = serverSocket.EndAccept(asyncResult);
            clients.Add(socket);
            Task task = new Task(HandleClient, socket);
            task.Start();
            serverSocket.BeginAccept(AcceptCallBack, null);
        }

        static void HandleClient(object o)
        {
            Socket client = (Socket)o;
            StringBuilder builder = new StringBuilder();
            byte[] name = new byte[1024];
            try
            {
                builder.Append(Encoding.Unicode.GetString(name, 0, client.Receive(name)));
                users.Add(new User()
                {
                    Id = client.RemoteEndPoint.ToString(),
                    Name = builder.ToString()
                });
                Console.WriteLine($"К серверу подключился {builder}");
                if (messages.Count > 0)
                {
                    client.Send(Encoding.Unicode.GetBytes(String.Join("~", messages.Skip(messages.Count - 5))));
                }
                else
                {
                    client.Send(Encoding.Unicode.GetBytes("Прошлых сообщений нет"));
                }
                while (true)
                {
                    builder.Clear();
                    byte[] messageBuff = new byte[1024];
                    var userName = users.Where(x => x.Id == client.RemoteEndPoint.ToString()).FirstOrDefault().Name;
                    try
                    {
                        builder.Append(Encoding.Unicode.GetString(messageBuff, 0, client.Receive(messageBuff)));
                        if (builder.ToString() != "/exit")
                        {

                            messages.Add($"{userName}: {builder}");
                            Console.WriteLine($"{userName}: {builder}");
                            foreach (var c in clients)
                            {
                                if (c != client)
                                {
                                    c.Send(Encoding.Unicode.GetBytes($"{userName}: {builder}"));
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"{userName} покинул чат");
                            clients.Remove(client);
                            client.Shutdown(SocketShutdown.Both);
                            client.Disconnect(true);
                            break;
                        }
                    }
                    catch (SocketException)
                    {
                        clients.Remove(client);
                        client.Shutdown(SocketShutdown.Both);
                        client.Disconnect(true);

                        Console.WriteLine($"{userName} покинул чат");
                        break;
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Что-то пошло не так");
                
            }
            
        }
    }
}
