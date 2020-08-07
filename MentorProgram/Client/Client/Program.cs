using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        
        static string[] names = new string[] { "Liam", "Noah", "Mason", "Ethan", "Logan", "Lucas", "Jackson", "Aiden", "Oliver", "Jacob" };
        static string[] messages = new string[] { "Привет!", "/exit", "Как дела?", "Как погода?", "Что делаешь?", "Как настроение?", 
            "Здравствуйте", "Добрый день!", "Всем пока", "До встречи" };
        static Random rnd = new Random();
        static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        static CancellationToken token = cancelTokenSource.Token;
        static void Main(string[] args)
        {
            while (true)
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect("127.0.0.1", 2048);
              
                Console.WriteLine("Успешное подключение к серверу");

                clientSocket.Send(Encoding.Unicode.GetBytes(names[rnd.Next(0, 10)]));

                byte[] historyMessagesBuff = new byte[1024];
                string[] historyMessages = Encoding.Unicode.GetString(historyMessagesBuff, 0, clientSocket.Receive(historyMessagesBuff))
                                                                                .Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in historyMessages)
                {
                    Console.WriteLine(item);
                }

                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            byte[] realTimeMessagesBuff = new byte[1024];
                            Console.WriteLine(Encoding.Unicode.GetString(realTimeMessagesBuff, 0, clientSocket.Receive(realTimeMessagesBuff)));

                        }
                        else
                        {
                            clientSocket.Shutdown(SocketShutdown.Both);
                            clientSocket.Close();
                            break;
                        }
                    }
                }, token);

                while (true)
                {
                    Thread.Sleep(rnd.Next(0, 1000));
                    string message = messages[rnd.Next(0, 9)]; 
                    
                    clientSocket.Send(Encoding.Unicode.GetBytes(message));
                    if (message == "/exit")
                    {
                        cancelTokenSource.Cancel();
                        break;
                    }
                    
                }
            }
        }
    }
}
