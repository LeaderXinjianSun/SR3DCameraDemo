using SXJLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR3DCameraDemo.Model
{
    public class XinjePLCModbusRTU : IPLCCommunication
    {
        public DXH.PLC.Xinjie_ModbushRTU PLC;
        public XinjePLCModbusRTU(string PortName)
        {
            PLC = new DXH.PLC.Xinjie_ModbushRTU(PortName);
            //PLC.Connect();
        }
        public int ReadD(string address)
        {
            throw new NotImplementedException();
        }

        public bool ReadM(string address)
        {
            throw new NotImplementedException();
        }

        public bool[] ReadMultiM(string address, ushort length)
        {
            throw new NotImplementedException();
        }

        public int[] ReadMultiW(string address, ushort length)
        {
            throw new NotImplementedException();
        }

        public int ReadW(string address)
        {
            throw new NotImplementedException();
        }

        public void SetM(string address, bool value)
        {
            PLC.PLCWrite("01", address, value ? "FF00" : "0000");
        }

        public void SetMultiM(string address, bool[] value)
        {
            throw new NotImplementedException();
        }

        public void WriteD(string address, short value)
        {
            throw new NotImplementedException();
        }

        public void WriteMultD(string address, short[] value)
        {
            throw new NotImplementedException();
        }

        public void WriteMultW(string address, int[] value)
        {
            throw new NotImplementedException();
        }

        public void WriteW(string address, int value)
        {
            throw new NotImplementedException();
        }
    }
}
