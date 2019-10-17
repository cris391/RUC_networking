using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
  class Program
  {
    static void Main(string[] args)
    {
      var server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
      server.Start();

      while (true)
      {

        var client = server.AcceptTcpClient();

        var stream = client.GetStream();

        var buffer = new byte[client.ReceiveBufferSize];

        var rcnt = stream.Read(buffer, 0, buffer.Length);

        var msg = Encoding.UTF8.GetString(buffer, 0, rcnt);

        if (msg == "exit") break;

        Console.WriteLine($"Message: {msg}");

        msg = msg.ToUpper();

        buffer = Encoding.UTF8.GetBytes(msg);

        stream.Write(buffer, 0, buffer.Length);

        stream.Close();
      }

      server.Stop();
    }
  }
}