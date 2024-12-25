using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Client
    {
        private TcpClient _Client { get; set; }
        private NetworkStream _Stream { get; set; }
        public async Task Start()
        {
            _Client = new TcpClient("127.0.0.1", 5000);
            _Stream = _Client.GetStream();
            Console.WriteLine("Enter your name:");
            string clientName = Console.ReadLine();
            await SendMessage(clientName);
            _ = ReceiveMessage();
            while (true)
            {
                Console.WriteLine("Please input message(format Name:message):");
                string message = Console.ReadLine();
                SendMessage(message);
            }

        }
        private async Task ReceiveMessage()
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead = await _Stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine("Disconnected from server.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Message received: {message}");
            }
        }

        private async Task SendMessage(string message)
        {
            if (_Stream != null)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await _Stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            client.Start();
        }
    }
}
