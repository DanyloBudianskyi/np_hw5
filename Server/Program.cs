using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class Server
    {
        private TcpListener Listener { get; set; }
        private Dictionary<string,TcpClient> Clients = new Dictionary<string,TcpClient>();
        public void Start()
        {
            Listener = new TcpListener(IPAddress.Any, 5000);
            Listener.Start();
            while (true)
            {
                TcpClient client = Listener.AcceptTcpClient();
                Console.WriteLine("The client is connected...");
                _ = HandleClient(client);
            }
        }

        protected async Task HandleClient(object? obj)
        {
            TcpClient client = obj as TcpClient;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            string clientName = null;
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                clientName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                if (!Clients.TryAdd(clientName, client))
                {
                    Console.WriteLine("Client already exists");
                    client.Close();
                    return;
                }
                Console.WriteLine("Client connected " + clientName);
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var parts = message.Split(":", 2);
                    if (parts.Length == 2)
                    {
                        string targetId = parts[0].Trim();
                        string msg = parts[1].Trim();
                        SendMessage(targetId, $"{clientName}: {msg}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Client disconnected");
                Clients.Remove(clientName);
                client.Close();
            }
        }

        private async Task SendMessage(string targetId, string message)
        {
            if (Clients.TryGetValue(targetId, out var targetClient)) {
                NetworkStream stream = targetClient.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length); 
            }
        }
        public void Stop()
        {
            Listener.Stop();
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();
        }
    }
}
