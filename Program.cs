using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;

class Program
{
    private static SerialPort? serialPort;
    private const byte StartByte = 0xF0; // Start of packet
    private const byte StopByte = 0x55;  // End of packet
    private static bool moduleNameRetrieved = false;
    private static string moduleName = "Unknown";

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [STAThread]
    static void Main(string[] args)
    {
        AllocConsole();
        string comPort = "COM11"; // Replace with your COM port
        int baudRate = 92160;

        try
        {
            InitializeSerialPort(comPort, baudRate);
            serialPort!.Open();
            Console.WriteLine($"Connected to {comPort}. Setting module name...");
            SetModuleName(); // Set the module name to "11"

            while (true) // Keeps the console running until manually closed
            {
                Thread.Sleep(100);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            CleanupSerialPort();
            Console.WriteLine("Program terminated.");
        }
    }

    private static void InitializeSerialPort(string comPort, int baudRate)
    {
        serialPort = new SerialPort(comPort, baudRate)
        {
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            ReadTimeout = 5000,
            WriteTimeout = 5000
        };
        ConfigureBTS1("8");
        ConfigureBTS234("1");
        ConfigureBTS8("*,2");
        serialPort.DataReceived += DataReceivedHandler;
    }

    private static void ConfigureBTS8(string config) => SendCommand($"BTS8={config}");
    private static void ConfigureBTS1(string config) => SendCommand($"BTS1={config}");
    private static void ConfigureBTS234(string config)
    {
        SendCommand($"BTS2={config}");
        SendCommand($"BTS3={config}");
        SendCommand($"BTS4={config}");
    }

    private static void SendCommand(string command)
    {
        try
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                string fullCommand = command + "\r";
                serialPort.WriteLine(fullCommand);
                Console.WriteLine($"Command sent: {fullCommand}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending command '{command}': {ex.Message}");
        }
    }

    private static void SetModuleName()
    {
        try
        {
            byte[] command = Encoding.ASCII.GetBytes("BTS6=13\r");
            serialPort?.Write(command, 0, command.Length);
            Console.WriteLine("Command sent: BTS6=...");
            SendCommand("BT^TRES");
            Console.WriteLine("Reset command sent to apply changes.");
            Thread.Sleep(500);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting module name: {ex.Message}");
        }
    }

    private static void CleanupSerialPort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                SendCommand("BT^STOP");
                serialPort.Close();
                Console.WriteLine("Serial port closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing serial port: {ex.Message}");
            }
        }
    }

    private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        if (serialPort == null) return;

        try
        {
            int bytesToRead = serialPort.BytesToRead;
            byte[] incomingData = new byte[bytesToRead];
            serialPort.Read(incomingData, 0, bytesToRead);

            if (!moduleNameRetrieved)
            {
                RetrieveModuleName();
                moduleNameRetrieved = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during data reception: {ex.Message}");
        }
    }

    private static void RetrieveModuleName()
    {
        try
        {
            Console.WriteLine("Retrieving module name...");
            serialPort?.DiscardInBuffer();
            SendCommand("BTS6?");
            Thread.Sleep(500);

            int bytesToRead = serialPort?.BytesToRead ?? 0;
            if (bytesToRead > 0)
            {
                byte[] incomingData = new byte[bytesToRead];
                serialPort?.Read(incomingData, 0, bytesToRead);
                string response = Encoding.ASCII.GetString(incomingData).Trim();
                Console.WriteLine($"Parsed Response: {response}");
            }
            else
            {
                Console.WriteLine("No data received for module name.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving module name: {ex.Message}");
        }
    }
}
