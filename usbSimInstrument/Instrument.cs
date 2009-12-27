using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace usbSimInstrument
{
    public class Instrument
    {
        private static UsbDevice usbInstrumentDevice;

        internal const int VENDOR_ID = 0x16C0;
        internal const int PRODUCT_ID = 0x05DC;

        internal enum USBSIM_COMMANDS
        {
            ECHO = 0,
            SET1 = 1,
            SET2 = 2,
            GET_TABLE = 3,
            SET_TABLE = 4,
            RESET = 9,
        };

        public Instrument(String serial)
        {
            UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(VENDOR_ID, PRODUCT_ID, serial);
            UsbRegistry MyUsbRegistry = UsbGlobals.AllDevices.Find(MyUsbFinder);
            if (MyUsbRegistry == null)
            {
                Debug.WriteLine(String.Format("Could not find device with serial {0}", serial));
            }
            else
            {
                if (!MyUsbRegistry.Open(out usbInstrumentDevice))
                {
                    Debug.WriteLine(string.Format("Could not open device with serial {0}", serial));
                }
            }
        }

        public bool Set1(int val)
        {
            if (usbInstrumentDevice.Open())
            {
                int lengthTransferred;
                byte[] buf = new byte[1];

                UsbSetupPacket cmd = UsbCmdSet1;
                cmd.Value = (short)val;
                bool bSuccess = usbInstrumentDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);
                return bSuccess && lengthTransferred == 1;
            }
            else
            {
                return false;
            }
        }

        public double ReadTable(int addr)
        {
            if (usbInstrumentDevice.Open())
            {
                int lengthTransferred;
                byte[] buf = new byte[8];

                UsbSetupPacket cmd = UsbCmdReadTable;
                cmd.Value = (short)addr;
                if (usbInstrumentDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred))
                {
                    return BitConverter.ToDouble(buf, 0);
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public bool WriteTable(int addr, double val)
        {
            if (usbInstrumentDevice.Open())
            {
                int lengthTransferred;
                byte[] buf = new byte[1];

                UsbSetupPacket cmd = UsbCmdWriteTable;
                byte[] addrBits = new byte[2];
                addrBits[0] = (byte)addr;
                bool allOK = false;
                byte[] valBits = BitConverter.GetBytes(val);
                for (byte i = 0; i < 8; i++)
                {
                    addrBits[1] = i;
                    cmd.Value = (short)BitConverter.ToInt16(addrBits, 0);
                    cmd.Index = (short)valBits[i];
                    bool bSuccess = usbInstrumentDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);
                    allOK = bSuccess && lengthTransferred == 1;
                }
                return allOK;
            }
            else
            {
                return false;
            }
        }

        internal static readonly UsbSetupPacket UsbCmdSet1 =
            new UsbSetupPacket(UsbRequestType.EndpointIn | UsbRequestType.RecipDevice | UsbRequestType.TypeVendor,
                               (DeviceRequestType)USBSIM_COMMANDS.SET1,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdWriteTable =
            new UsbSetupPacket(UsbRequestType.EndpointIn | UsbRequestType.RecipDevice | UsbRequestType.TypeVendor,
                               (DeviceRequestType)USBSIM_COMMANDS.SET_TABLE,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdReadTable =
            new UsbSetupPacket(UsbRequestType.EndpointIn | UsbRequestType.RecipDevice | UsbRequestType.TypeVendor,
                               (DeviceRequestType)USBSIM_COMMANDS.GET_TABLE,
                               0,
                               0,
                               1);
    }
}
