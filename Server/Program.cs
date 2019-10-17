using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace Server
{
  class Program
  {
    static void Main(string[] args)
    {
      new Server(IPAddress.Parse("127.0.0.1"), 5000);
      Console.WriteLine("Server Started");
    }

  }
  class Server
  {
    TcpListener server = null;
    public Server(IPAddress localAddr, int port)
    {
      server = new TcpListener(localAddr, port);
      server.Start();
      StartListener();
    }

    public void StartListener()
    {
      try
      {
        while (true)
        {
          Console.WriteLine("Waiting for a connection...");
          var client = server.AcceptTcpClient();
          Console.WriteLine($"A client connected!");

          Thread t = new Thread(new ParameterizedThreadStart(HandleDevice));
          t.Start(client);
        }
      }
      catch (SocketException e)
      {
        Console.WriteLine("SocketException: {0}", e);
        server.Stop();
      }
    }

    public void HandleDevice(object obj)
    {
      TcpClient client = (TcpClient)obj;
      var stream = client.GetStream();

      var buffer = new byte[client.ReceiveBufferSize];
      int receiveBufferCount;
      try
      {
        while ((receiveBufferCount = stream.Read(buffer, 0, buffer.Length)) != 0)
        {
          var msg = Encoding.UTF8.GetString(buffer, 0, receiveBufferCount);
          // implement some kind of cleanup if client sends close message(server sid)
          // if (msg == "exit2") client.Close();
          Console.WriteLine("{1}: Received: {0}", msg, Thread.CurrentThread.ManagedThreadId);

          string replyMsg = "Hey Device!";
          var byteReplyMsg = Encoding.UTF8.GetBytes(replyMsg);
          stream.Write(byteReplyMsg, 0, byteReplyMsg.Length);
          Console.WriteLine("{1}: Sent: {0}", replyMsg, Thread.CurrentThread.ManagedThreadId);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine("Exception: {0}", e.ToString());
        client.Close();
      }
    }
  }
}