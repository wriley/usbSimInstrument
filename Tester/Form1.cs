using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.Main;
using LibUsbDotNet.Info;
using System.Diagnostics;
using usbSimInstrument;

namespace Tester
{
    public partial class Form1 : Form
    {
        public static DeviceNotifier DeviceNotifier = new DeviceNotifier();
        private Instrument MyInstrument;

        const int VENDOR_ID = 0x16C0;
        const int PRODUCT_ID = 0x05DC;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;
            FindDevices();
        }

        private void FindDevices()
        {
            lbDevices.Items.Clear();

            UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(VENDOR_ID, PRODUCT_ID);
            UsbRegDeviceList MyUsbRegDeviceList = UsbGlobals.AllDevices.FindAll(MyUsbFinder);
            foreach (UsbRegistry MyUsbRegistry in MyUsbRegDeviceList)
            {
                lbDevices.Items.Add(String.Format("{0} - {1}", MyUsbRegistry.Device.Info.SerialString.ToString(), MyUsbRegistry.Name));
            }
        }

        private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {
            if ((e.Device.IdVendor == VENDOR_ID) && (e.Device.IdProduct == PRODUCT_ID))
            {

                switch(e.EventType)
                {
                    case EventType.DeviceArrival:
                        FindDevices();
                        break;
                    case EventType.DeviceRemoveComplete:
                        FindDevices();
                        break;
                }
            }
        }

        private void lbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            String[] pieces = lbDevices.SelectedItem.ToString().Split(' ');
            MyInstrument = new Instrument(pieces[0]);
            MyInstrument.Set1(trackBar1.Value);
        }

        private static void ShowLastUsbError()
        {
            Debug.WriteLine(String.Format("Error Number: {0}", UsbGlobals.LastErrorNumber));
            Debug.WriteLine(UsbGlobals.LastErrorString);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
            if (MyInstrument != null)
            {
                MyInstrument.Set1(trackBar1.Value);
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (MyInstrument != null)
            {
                double val;
                lbData.Items.Clear();
                for (int i = 0; i < 8; i++)
                {
                    val = MyInstrument.ReadTable(i);
                    lbData.Items.Add(val.ToString());
                }
            }
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            if ((MyInstrument != null) && (tbWriteData.Text.Length >= 15))
            {
                String[] values = System.Text.RegularExpressions.Regex.Split(tbWriteData.Text, "\r\n");
                for (int i = 0; i < 8; i++)
                {
                    Debug.WriteLine(String.Format("Writing table value[{0}]: {1}", i, values[i]));
                    MyInstrument.WriteTable(i, double.Parse(values[i]));
                }
            }
        }
    }
}
