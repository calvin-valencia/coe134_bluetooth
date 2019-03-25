using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InTheHand;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Ports;
using InTheHand.Net.Sockets;
using System.IO;

namespace Bluetooth
{
    public partial class Form1 : Form
    {
        List<string> items;
        public Form1()
        {
            items = new List<string>();
            InitializeComponent();
        }

        private void bGo_Click(object sender, EventArgs e)
        {
            if (ServerStarted)
            {
                updateUI("Server already started...");
            }
            else
            {
                if (rbClient.Checked)
                {
                    startScan();
                }
                else
                {
                    ConnectAsServer();
                }
            }
        }

        private void startScan()
        {
            listBox1.DataSource = null;
            listBox1.Items.Clear();
            items.Clear();
            Thread BluetoothScanThread = new Thread(new ThreadStart(scan));
            BluetoothScanThread.Start();
        }
        BluetoothDeviceInfo[] devices;
        private void scan()
        {
            updateUI("Starting scan...");
            BluetoothClient client = new BluetoothClient();
            devices = client.DiscoverDevicesInRange();
            updateUI("Scan complete...");
            updateUI(devices.Length.ToString()+" devices discovered.");
            foreach(BluetoothDeviceInfo d in devices)
            {
                items.Add(d.DeviceName);
            }

            updateDeviceList();
        }

        private void ConnectAsServer()
        {
            Thread BluetoothServerThread = new Thread(new ThreadStart(ServerConnectThread));
            BluetoothServerThread.Start();
        }

        private void ConnectAsClient()
        {
            throw new NotImplementedException();
        }

        Guid mUUID = new Guid("00001101-0000-1000-0000-00005F9B34FB");
        bool ServerStarted = false;
        public void ServerConnectThread()
        {
            ServerStarted = true;
            updateUI("Server has started, waiting for connections...");
            BluetoothListener bluetoothListener = new BluetoothListener(mUUID);
            bluetoothListener.Start();
            BluetoothClient conn = bluetoothListener.AcceptBluetoothClient();
            updateUI("Client has connected");

            while (true)
            {

            }
        }

        private void updateDeviceList()
        {
            Func<int> del = delegate ()
            {
                listBox1.DataSource = items;
                return 0;
            };
            Invoke(del);
        }

        private void updateUI(string Message)
        {
            Func<int> del = delegate ()
            {
                tbOutput.AppendText(Message + System.Environment.NewLine);
                return 0;
            };
            Invoke(del);
        }

        private void bGo_Click_1(object sender, EventArgs e)
        {

        }
        BluetoothDeviceInfo deviceInfo;
        Boolean isPaired;
        Boolean success;
        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
           deviceInfo = devices.ElementAt(listBox1.SelectedIndex);
           updateUI(deviceInfo.DeviceName + " was selected. Attempting to connect...");
            success = true;
            if (listBox1.SelectedItem != null)
            {
                if (deviceInfo.Authenticated)
                {
                    updateUI("Device is already paired...");
                }
                else
                {
                    updateUI("Device is not yet paired. Attempting to pair...");
                    isPaired = BluetoothSecurity.PairRequest(deviceInfo.DeviceAddress, "123456");
                    if (isPaired)
                    {
                        updateUI("Device paired...");
                    }
                    else
                    {
                        updateUI("Device not paired...");
                        success = false;
                    }
                }
                if (success)
                {
                    updateUI("Starting connect thread...");
                    Thread bluetoothClientThread = new Thread(new ThreadStart(ClientConnectThread));
                    bluetoothClientThread.Start();
                }
                else
                {
                    updateUI("Can not proceed to connect since pair failed...");
                }

            }
        }

        private void ClientConnectThread()
        {
            BluetoothClient client = new BluetoothClient();
            updateUI("Attempting to connect...");
            client.BeginConnect(deviceInfo.DeviceAddress, BluetoothService.SerialPort, this.BluetoothClientConnectCallback, client);
        }

        private void BluetoothClientConnectCallback(IAsyncResult result)
        {
            updateUI("Callback...");
            return;
        }
        Boolean ready = false;
        byte[] message;
        private void tbText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                message = Encoding.ASCII.GetBytes(tbText.Text);
                ready = true;
                tbText.Clear();
            }
        }
    }
}
