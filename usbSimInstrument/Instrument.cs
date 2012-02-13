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
            GET_TABLE = 10,
            SET_TABLE = 11,
            GET_TABLE_RAW = 20,
            SET_TABLE_RAW = 21,
            GET_TABLE2 = 30,
            SET_TABLE2 = 31,
            GET_TABLE_RAW2 = 40,
            SET_TABLE_RAW2 = 41,
            SET_SERIAL = 90,
            RESET = 99,
        };

        internal enum USBSIM_TABLE_TYPES
        {
            TABLE,
            TABLE_RAW,
            TABLE2,
            TABLE_RAW2,
        };

        public Instrument(String serial)
        {
            UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(VENDOR_ID, PRODUCT_ID);
            UsbRegDeviceList MyUsbRegDeviceList = UsbDevice.AllDevices.FindAll(MyUsbFinder);

            if (MyUsbRegDeviceList.Count > 0)
            {
                foreach (UsbRegistry MyUsbRegistry in MyUsbRegDeviceList)
                {
                    if (MyUsbRegistry.Device.Info.SerialString == serial)
                    {
                        if (MyUsbRegistry.Open(out usbInstrumentDevice))
                        {
                            break;
                        }
                        else
                        {
                            Debug.WriteLine(string.Format("Could not open device with serial {0}", serial));
                        }
                    }
                }

                Debug.WriteLine(string.Format("Could not find device with serial {0}", serial));
            }
            else
            {
                Debug.WriteLine("No devices found");
            }
        }

        private bool SetHelper(byte pointer, int val)
        {
            if ((usbInstrumentDevice != null) && usbInstrumentDevice.Open())
            {
                int lengthTransferred;
                byte[] buf = new byte[1];
                UsbSetupPacket cmd = new UsbSetupPacket();

                switch (pointer)
                {
                    case 1:
                        cmd = UsbCmdSet1;
                        break;
                    case 2:
                        cmd = UsbCmdSet2;
                        break;
                    default:
                        cmd.RequestType = (Byte)UsbRequestType.TypeReserved;
                        break;
                }

                if (cmd.RequestType != (Byte)UsbRequestType.TypeReserved)
                {

                    cmd.Value = (short)val;
                    return usbInstrumentDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool Set1(int val)
        {
            return SetHelper(1, val);
        }

        public bool Set2(int val)
        {
            return SetHelper(2, val);
        }

        private double[] ReadTableHelper(USBSIM_TABLE_TYPES type)
        {
            double[] table = new double[8];

            if ((usbInstrumentDevice != null) && usbInstrumentDevice.Open())
            {
                int lengthTransferred;
                byte[] buf = new byte[8];
                UsbSetupPacket cmd = new UsbSetupPacket();

                switch (type)
                {
                    case USBSIM_TABLE_TYPES.TABLE:
                        cmd = UsbCmdReadTable;
                        break;
                    case USBSIM_TABLE_TYPES.TABLE2:
                        cmd = UsbCmdReadTable2;
                        break;
                    case USBSIM_TABLE_TYPES.TABLE_RAW:
                        cmd = UsbCmdReadTableRaw;
                        break;
                    case USBSIM_TABLE_TYPES.TABLE_RAW2:
                        cmd = UsbCmdReadTableRaw2;
                        break;
                    default:
                        cmd.RequestType = (Byte)UsbRequestType.TypeReserved;
                        break;
                }

                if (cmd.RequestType != (Byte)UsbRequestType.TypeReserved)
                {

                    for (short i = 0; i < 8; i++)
                    {
                        cmd.Value = i;
                        if (usbInstrumentDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred))
                        {
                            table[i] = BitConverter.ToDouble(buf, 0);
                            Debug.WriteLine(String.Format("ReadTable lengthTransferred = {0}", lengthTransferred));
                        }
                        else
                        {
                            table[i] = 0;
                        }
                    }
                }
            }

            return table;
        }

        public double[] ReadTable()
        {
            return ReadTableHelper(USBSIM_TABLE_TYPES.TABLE);
        }

        public double[] ReadTable2()
        {
            return ReadTableHelper(USBSIM_TABLE_TYPES.TABLE2);
        }

        public double[] ReadTableRaw()
        {
            return ReadTableHelper(USBSIM_TABLE_TYPES.TABLE_RAW);
        }

        public double[] ReadTableRaw2()
        {
            return ReadTableHelper(USBSIM_TABLE_TYPES.TABLE_RAW2);
        }

        private bool WriteTableHelper(USBSIM_TABLE_TYPES type, double[] values)
        {
            if ((usbInstrumentDevice != null) && usbInstrumentDevice.Open())
            {
                int lengthTransferred;
                byte[] buf = new byte[1];

                UsbSetupPacket cmd = new UsbSetupPacket();

                switch (type)
                {
                    case USBSIM_TABLE_TYPES.TABLE:
                        cmd = UsbCmdWriteTable;
                        break;
                    case USBSIM_TABLE_TYPES.TABLE2:
                        cmd = UsbCmdWriteTable2;
                        break;
                    case USBSIM_TABLE_TYPES.TABLE_RAW:
                        cmd = UsbCmdWriteTableRaw;
                        break;
                    case USBSIM_TABLE_TYPES.TABLE_RAW2:
                        cmd = UsbCmdWriteTableRaw2;
                        break;
                    default:
                        cmd.RequestType = (Byte)UsbRequestType.TypeReserved;
                        break;
                }

                if (cmd.RequestType != (Byte)UsbRequestType.TypeReserved)
                {

                    byte[] addrBits = new byte[2];
                    bool allOK = false;

                    for (byte j = 0; j < 8; j++)
                    {
                        addrBits[0] = j;
                        byte[] valBits = BitConverter.GetBytes(values[j]);
                        for (byte i = 0; i < 8; i++)
                        {
                            addrBits[1] = i;
                            cmd.Value = (short)BitConverter.ToInt16(addrBits, 0);
                            cmd.Index = (short)valBits[i];
                            bool bSuccess = usbInstrumentDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);
                            Debug.WriteLine(String.Format("WriteTable lengthTransferred = {0}", lengthTransferred));
                            allOK = bSuccess && lengthTransferred == 1;
                        }
                    }
                    return allOK;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool writeTable(double[] vals)
        {
            return WriteTableHelper(USBSIM_TABLE_TYPES.TABLE, vals);
        }

        public bool writeTable2(double[] vals)
        {
            return WriteTableHelper(USBSIM_TABLE_TYPES.TABLE2, vals);
        }

        public bool writeTableRaw(double[] vals)
        {
            return WriteTableHelper(USBSIM_TABLE_TYPES.TABLE_RAW, vals);
        }

        public bool writeTableRaw2(double[] vals)
        {
            return WriteTableHelper(USBSIM_TABLE_TYPES.TABLE_RAW2, vals);
        }

        private bool SetSerialHelper(Int16 val)
        {
            if ((usbInstrumentDevice != null) && usbInstrumentDevice.Open())
            {
                int lengthTransferred;
                byte[] buf = new byte[3];
                UsbSetupPacket cmd = UsbCmdSetSerial;
                cmd.Value = (short)val;
                
                bool bSuccess = usbInstrumentDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);
                Debug.WriteLine(String.Format("buf[0] => {0}\nbuf[1] => {1}\nbuf[2] => {2}", buf[0], buf[1], buf[2]));
                return bSuccess;
            }
            else
            {
                return false;
            }
        }

        public bool SetSerial(Int16 val)
        {
            return SetSerialHelper(val);
        }

        private bool ResetHelper()
        {
            if ((usbInstrumentDevice != null) && usbInstrumentDevice.Open())
            {
                int lengthTransferred;
                byte[] buf = new byte[1];
                UsbSetupPacket cmd = UsbCmdReset;
                return usbInstrumentDevice.ControlTransfer(ref cmd, buf, buf.Length, out lengthTransferred);
            }
            else
            {
                return false;
            }
        }

        public bool Reset()
        {
            return ResetHelper();
        }

        /*
        int transferred;
                        byte[] ctrlData=new byte[1];
                        UsbSetupPacket setTestTypePacket = 
                            new UsbSetupPacket((byte) (UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                                0x0E,0x01,usbInterfaceInfo.Descriptor.InterfaceID,1);
                        MyUsbDevice.ControlTransfer(ref setTestTypePacket,ctrlData, 1, out transferred);
        */

        internal static readonly UsbSetupPacket UsbCmdSet1 =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.SET1,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdSet2 =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.SET2,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdReadTable =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.GET_TABLE,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdWriteTable =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.SET_TABLE,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdReadTable2 =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.GET_TABLE2,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdWriteTable2 =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.SET_TABLE2,
                               0,
                               0,
                               1);

        internal static readonly UsbSetupPacket UsbCmdReadTableRaw =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.GET_TABLE_RAW,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdWriteTableRaw =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.SET_TABLE_RAW,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdReadTableRaw2 =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.GET_TABLE_RAW2,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdWriteTableRaw2 =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.SET_TABLE_RAW2,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdSetSerial =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.SET_SERIAL,
                               0,
                               0,
                               1);
        internal static readonly UsbSetupPacket UsbCmdReset =
            new UsbSetupPacket((byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.RequestType_Vendor),
                               (byte)USBSIM_COMMANDS.RESET,
                               0,
                               0,
                               1);
    }
}
