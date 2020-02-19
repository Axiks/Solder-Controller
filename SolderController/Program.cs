using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using SharpDX.XInput;

namespace SolderController
{
    class Program
    {
        static SerialPort port;
        static bool ok = true;

        private static void Control()
        {
            XInputController controller = new XInputController();

            ConsoleKeyInfo keyinfo;
            do
            {
                keyinfo = Console.ReadKey();
                Console.WriteLine(keyinfo.Key + " was pressed");

                string msg = port.ReadExisting();
                //Console.WriteLine(msg);
                if (msg.Equals("ok\r\n")) ok = true;

                if (ok)
                {
                    switch (keyinfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            port.WriteLine("G21G91G0X1F1");
                            ok = false;
                            break;
                        case ConsoleKey.DownArrow:
                            port.WriteLine("G21G91G0X-1F1");
                            ok = false;
                            break;
                        case ConsoleKey.LeftArrow:
                            port.WriteLine("G21G91G0Y-1F1");
                            ok = false;
                            break;
                        case ConsoleKey.RightArrow:
                            port.WriteLine("G21G91G0Y1F1");
                            ok = false;
                            break;
                        case ConsoleKey.PageUp:
                            port.WriteLine("M5");
                            ok = false;
                            break;
                        case ConsoleKey.PageDown:
                            port.WriteLine("M3 S35");
                            ok = false;
                            break;
                        case ConsoleKey.Tab:
                            Console.WriteLine("Connected gamepad: " + controller.connected);
                            controller.Update();
                            float LX = (float)Math.Round(controller.leftThumbX / 100, 2);
                            float LY = (float)Math.Round(controller.leftThumbY / 100, 2);
                            if (LX > 0.05)
                            {
                                port.WriteLine("G21 G91 G0 Y" + LX + " F1");

                            }
                            else if (LX < -0.05)
                            {
                                port.WriteLine("G21G91G0Y" + LX + "F1");
                            }
                            else
                            {

                            }

                            if (LY < -0.05)
                            {
                                port.WriteLine("G21G91G0X" + LY + "F1");

                            }
                            else if (LY > 0.05)
                            {
                                port.WriteLine("G21G91G0X" + LY + "F1");
                            }
                            Console.WriteLine("LX: " + LX);
                            Console.WriteLine("LY: " + LY);
                            Console.WriteLine("RX: " + controller.rightThumbX);
                            Console.WriteLine("RY: " + controller.rightThumbY);
                            break;
                    }
                }
            }
            while (keyinfo.Key != ConsoleKey.X);
        }

        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            // получаем список доступных портов 
            string[] ports = SerialPort.GetPortNames();

            Console.WriteLine("Select COM port:");

            // выводим список портов
            for (int i = 0; i < ports.Length; i++)
            {
                Console.WriteLine("[" + i.ToString() + "] " + ports[i].ToString());
            }
            port = new SerialPort();

            // читаем номер из консоли
            string n = Console.ReadLine();

            int num = int.Parse(n);
            try
            {
                // настройки порта
                port.PortName = ports[num];
                port.BaudRate = 115200;
                port.DataBits = 8;
                port.Parity = System.IO.Ports.Parity.None;
                port.StopBits = System.IO.Ports.StopBits.One;
                port.ReadTimeout = 1000;
                port.WriteTimeout = 1000;
                port.Open();
                if (port.IsOpen)
                {
                    Console.WriteLine(port.PortName + " open success!");
                    port.WriteLine("?");
                    Thread.Sleep(200);
                    port.ReadExisting();
                    Thread.Sleep(200);
                    port.WriteLine("$$");
                    Thread.Sleep(200);
                    string msg = port.ReadExisting();
                    Console.WriteLine(msg);

                    Control();
                    //port.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: open port:" + e.ToString());
                return;
            }


            Console.ReadLine();
        }
    }

    class XInputController
    {
        Controller controller;
        Gamepad gamepad;
        public bool connected = false;
        public int deadband = 2500;
        public float leftThumbX, leftThumbY, rightThumbX, rightThumbY = 0;
        public float leftTrigger, rightTrigger;

        public XInputController()
        {
            controller = new Controller(UserIndex.One);
            connected = controller.IsConnected;
        }

        // Call this method to update all class values
        public void Update()
        {
            if (!connected)
                return;

            gamepad = controller.GetState().Gamepad;

            leftThumbX = (Math.Abs((float)gamepad.LeftThumbX) < deadband) ? 0 : (float)gamepad.LeftThumbX / short.MinValue * -100;
            leftThumbY = (Math.Abs((float)gamepad.LeftThumbY) < deadband) ? 0 : (float)gamepad.LeftThumbY / short.MaxValue * 100;
            rightThumbY = (Math.Abs((float)gamepad.RightThumbX) < deadband) ? 0 : (float)gamepad.RightThumbX / short.MaxValue * 100;
            rightThumbX = (Math.Abs((float)gamepad.RightThumbY) < deadband) ? 0 : (float)gamepad.RightThumbY / short.MaxValue * 100;
            leftTrigger = gamepad.LeftTrigger;
            rightTrigger = gamepad.RightTrigger;
        }
    }

}
