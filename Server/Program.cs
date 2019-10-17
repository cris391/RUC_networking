using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;




namespace Server
{


    // State object for reading client dat asynchrounusly 
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public AsynchronousSocketListener()
        {

        }
        public static void StartListening()
        {
            // Establish the local endpoint for the socket
            // The DNS name of comp.
            // running the listener is "host.contoso.com"
            IPHostEntry iPHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = iPHostInfo.AddressList[0];
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 5000); // change to loopback or other IP == MODIFY TO IP 127.0.0.1 port 5000

            // Create a TCO/IP socket.
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listing for incoming connections
            try
            {
                listener.Bind(localEndpoint);
                listener.Listen(5000); // MODIFY PORT?  5000

                while (true)
                {
                    // Set the event to nonsignaled state. 
                    allDone.Reset();

                    // Start an asychronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallBack),
                        listener);

                    // Wait until a connection is made before contiuning
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\n Press ENTER to continue...");
            Console.Read();
        }



        public static void AcceptCallBack(IAsyncResult ar)
        {
            Console.WriteLine("Accept call back async"); 
            // Signal the main thread to continue
            allDone.Set();

            // GEt the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallBack), state);
        }


        public static void ReadCallBack(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // From the asynchronous state object
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // THere might be more data, so store the data received so far
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not htere , read
                // more data
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the Client
                    // Display to the console
                    Console.WriteLine("Read {0} bytes from socket. \n Data: {1}", content.Length, content);
                    // Echo the data back to the client.
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. get more
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallBack), state);
                }
            }

        }
        private static void Send(Socket handler, String data)
        {

            Console.WriteLine("SEND ");
            // Convert the string data to byte data using ASCII encoding
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallBack), handler);
        }

        private static void SendCallBack(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        class Program
        {


            public static int Main(string[] args)
            {
                
                StartListening();
                return 0;

                /*
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

                */
            }
        }
    }
}