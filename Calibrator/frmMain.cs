﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using usbSimInstrument;
using LibUsbDotNet;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.Main;
using LibUsbDotNet.Info;

namespace Calibrator
{
    public partial class frmMain : Form
    {
        private Instrument Instrument;
        public static DeviceNotifier DeviceNotifier = new DeviceNotifier();

        const int VENDOR_ID = 0x16C0;
        const int PRODUCT_ID = 0x05DC;

        public frmMain()
        {
            InitializeComponent();
        }

        private void FindDevices()
        {
            cbDevices.Items.Clear();
            cbDevices.Enabled = false;
            Instrument = null;
            
            UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(VENDOR_ID, PRODUCT_ID);
            UsbRegDeviceList MyUsbRegDeviceList = UsbGlobals.AllDevices.FindAll(MyUsbFinder);

            if (MyUsbRegDeviceList.Count > 0)
            {
                foreach (UsbRegistry MyUsbRegistry in MyUsbRegDeviceList)
                {
                    cbDevices.Items.Add(String.Format("{0} - {1}", MyUsbRegistry.Device.Info.SerialString.ToString(), MyUsbRegistry.Name));
                }
                cbDevices.Enabled = true;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            DeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;
            FindDevices();
        }

        private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {
            if ((e.Device.IdVendor == VENDOR_ID) && (e.Device.IdProduct == PRODUCT_ID))
            {

                switch (e.EventType)
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

        private void cbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            String[] pieces = cbDevices.SelectedItem.ToString().Split(' ');
            Instrument = new Instrument(pieces[0]);
            LoadTableValues();
        }

        private void LoadTableValues()
        {
            if (Instrument != null)
            {
                int i;
                double[] table = Instrument.ReadTable();
                double[] val = Instrument.ReadTableRaw();
                int[] raw = new int[8];

                for (i = 0; i < 8; i++)
                {
                    if ((val[i] > hScrollBarSet1_1.Maximum) || (val[i] < hScrollBarSet1_1.Minimum) || double.IsNaN(val[i]))
                    {
                        raw[i] = 0;
                    }
                    else
                    {
                        raw[i] = int.Parse(Math.Round(val[i]).ToString());
                    }
                }

                textBoxSet1_1.Text = table[0].ToString();
                hScrollBarSet1_1.Value = raw[0];
                labelSet1_1Value.Text = raw[0].ToString();

                textBoxSet1_2.Text = table[1].ToString();
                hScrollBarSet1_2.Value = raw[1];
                labelSet1_2Value.Text = raw[1].ToString();

                textBoxSet1_3.Text = table[2].ToString();
                hScrollBarSet3.Value = raw[2];
                labelSet1_3Value.Text = raw[2].ToString();

                textBoxSet1_4.Text = table[3].ToString();
                hScrollBarSet1_4.Value = raw[3];
                labelSet1_4Value.Text = raw[3].ToString();

                textBoxSet1_5.Text = table[4].ToString();
                hScrollBarSet5.Value = raw[4];
                labelSet1_5Value.Text = raw[4].ToString();

                textBoxSet1_6.Text = table[5].ToString();
                hScrollBarSet1_6.Value = raw[5];
                labelSet1_6Value.Text = raw[5].ToString();

                textBoxSet1_7.Text = table[6].ToString();
                hScrollBarSet7.Value = raw[6];
                labelSet1_7Value.Text = raw[6].ToString();

                textBoxSet1_8.Text = table[7].ToString();
                hScrollBarSet1_8.Value = raw[7];
                labelSet1_8Value.Text = raw[7].ToString();
            }
        }

        private void hScrollBarSet1_1_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet1_1Value.Text = hScrollBarSet1_1.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet1_1.Value);
            }
        }

        private void hScrollBarSet1_2_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet1_2Value.Text = hScrollBarSet1_2.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet1_2.Value);
            }
        }

        private void hScrollBarSet1_3_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet1_3Value.Text = hScrollBarSet3.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet3.Value);
            }
        }

        private void hScrollBarSet1_4_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet1_4Value.Text = hScrollBarSet1_4.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet1_4.Value);
            }
        }
        private void hScrollBarSet1_5_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet1_5Value.Text = hScrollBarSet5.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet5.Value);
            }
        }

        private void hScrollBarSet1_6_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet1_6Value.Text = hScrollBarSet1_6.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet1_6.Value);
            }
        }

        private void hScrollBarSet1_7_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet1_7Value.Text = hScrollBarSet7.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet7.Value);
            }
        }

        private void hScrollBarSet1_8_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet1_8Value.Text = hScrollBarSet1_8.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet1_8.Value);
            }
        }

        private void hScrollBarSet2_1_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet2_1Value.Text = hScrollBarSet2_1.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet2_1.Value);
            }
        }

        private void hScrollBarSet2_2_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet2_2Value.Text = hScrollBarSet2_2.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet2_2.Value);
            }
        }

        private void hScrollBarSet2_3_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet2_3Value.Text = hScrollBarSet2_3.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet2_3.Value);
            }
        }

        private void hScrollBarSet2_4_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet2_4Value.Text = hScrollBarSet2_4.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet2_4.Value);
            }
        }
        private void hScrollBarSet2_5_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet2_5Value.Text = hScrollBarSet2_5.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet2_5.Value);
            }
        }

        private void hScrollBarSet2_6_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet2_6Value.Text = hScrollBarSet2_6.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet2_6.Value);
            }
        }

        private void hScrollBarSet2_7_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet2_7Value.Text = hScrollBarSet2_7.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet2_7.Value);
            }
        }

        private void hScrollBarSet2_8_Scroll(object sender, ScrollEventArgs e)
        {
            labelSet2_8Value.Text = hScrollBarSet2_8.Value.ToString();

            if (Instrument != null)
            {
                Instrument.Set1(hScrollBarSet2_8.Value);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (Instrument != null)
            {
                double[] table = new double[8];
                double[] raw = new double[8];

                table[0] = double.Parse(textBoxSet1_1.Text.ToString());
                raw[0] = double.Parse(hScrollBarSet1_1.Value.ToString());

                table[1] = double.Parse(textBoxSet1_2.Text.ToString());
                raw[1] = double.Parse(hScrollBarSet1_2.Value.ToString());

                table[2] = double.Parse(textBoxSet1_3.Text.ToString());
                raw[2] = double.Parse(hScrollBarSet3.Value.ToString());

                table[3] = double.Parse(textBoxSet1_4.Text.ToString());
                raw[3] = double.Parse(hScrollBarSet1_4.Value.ToString());

                table[4] = double.Parse(textBoxSet1_5.Text.ToString());
                raw[4] = double.Parse(hScrollBarSet5.Value.ToString());

                table[5] = double.Parse(textBoxSet1_6.Text.ToString());
                raw[5] = double.Parse(hScrollBarSet1_6.Value.ToString());

                table[6] = double.Parse(textBoxSet1_7.Text.ToString());
                raw[6] = double.Parse(hScrollBarSet7.Value.ToString());

                table[7] = double.Parse(textBoxSet1_8.Text.ToString());
                raw[7] = double.Parse(hScrollBarSet1_8.Value.ToString());

                Instrument.writeTable(table);
                Instrument.writeTableRaw(raw);
            }
        }
    }
}