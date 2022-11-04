using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CypressLibrary
{
    public class CypressI2c
    {
        private IntPtr mHandle;
        
        public int Open(byte dn = 0)
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;
            int deviceIndexAtSCB0 = Cypress.FindDeviceAtSCB(Cypress.CY_DEVICE_SERIAL_BLOCK.SerialBlock_SCB0, dn);

            if (deviceIndexAtSCB0 >= 0)
            {
                cyReturnStatus = Cypress.CyOpen((byte)deviceIndexAtSCB0, 0, out mHandle);
                Console.WriteLine("I2c SCB0 device index = {0}", deviceIndexAtSCB0);
            }
            else
            {
                cyReturnStatus = Cypress.CyOpen(0, 0, out mHandle);
                Console.WriteLine("Detect device index fail. I2C cyOpen with index 0.");
            }

            return (-1) * (int)cyReturnStatus;

        }

        public int Config(int frequencyHz)
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;
            Cypress.CY_I2C_CONFIG config = new Cypress.CY_I2C_CONFIG();

            config.frequency = (uint)frequencyHz;
            cyReturnStatus = Cypress.CySetI2cConfig(mHandle, ref config);

            return (-1) * (int)cyReturnStatus;
        }

        public int Close()
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;

            cyReturnStatus = Cypress.CyClose(mHandle);

            return (-1) * (int)cyReturnStatus;
        }

        //buf length must be more than len
        public int Read(byte slaveAddress7bit, ref byte[] buf, int len, bool isStopBit = true)
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;
            Cypress.CY_I2C_DATA_CONFIG dataConfig = new Cypress.CY_I2C_DATA_CONFIG();
            Cypress.CY_DATA_BUFFER dataBuf = new Cypress.CY_DATA_BUFFER();

            //////////////////////////////////////////////////////////////////////
            // Read

            //Create a IntPtr for dll API.
            //Allocate a unmanaged memory for dll API
            IntPtr pRecvBuf = Marshal.AllocHGlobal(len);

            //Config
            dataConfig.slaveAddress = slaveAddress7bit;
            dataConfig.isStopBit = isStopBit;
            dataConfig.isNakBit = true;
            dataBuf.buffer = pRecvBuf;
            dataBuf.length = (uint)len;

            //Call dll API
            cyReturnStatus = Cypress.CyI2cRead(mHandle, ref dataConfig, ref dataBuf, 5000);

            //Copy data from unmanaged memory
            Marshal.Copy(pRecvBuf, buf, 0, len);

            Marshal.FreeHGlobal(pRecvBuf);

            return (-1) * (int)cyReturnStatus;
        }

        public int Write(byte slaveAddress7bit, byte[] buf, int len, bool isStopBit = true)
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;
            Cypress.CY_I2C_DATA_CONFIG dataConfig = new Cypress.CY_I2C_DATA_CONFIG();
            Cypress.CY_DATA_BUFFER dataBuf = new Cypress.CY_DATA_BUFFER();

            ///////////////////////////////////////////////////////////////////////////////
            // Write

            //Create a IntPtr for dll API.
            //Allocate a unmanaged memory and copy buf into it.
            IntPtr pBuf = Marshal.AllocHGlobal(len);
            Marshal.Copy(buf, 0, pBuf, len);

            //Config
            dataConfig.slaveAddress = slaveAddress7bit;
            dataConfig.isStopBit = isStopBit;
            dataBuf.length = (uint)len;
            dataBuf.buffer = pBuf;

            //Call dll API
            cyReturnStatus = Cypress.CyI2cWrite(mHandle, ref dataConfig, ref dataBuf, 5000);

            //Free unmanaged memory.
            Marshal.FreeHGlobal(pBuf);

            return (-1) * (int)cyReturnStatus;
        }
    }//class CypressI2c

    public class CypressSpi
    {
        private IntPtr mHandle;

        public int Open(byte dn = 0)
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;
            int deviceIndexAtSCB1 = Cypress.FindDeviceAtSCB(Cypress.CY_DEVICE_SERIAL_BLOCK.SerialBlock_SCB1, dn);

            if (deviceIndexAtSCB1 >= 0)
            {
                cyReturnStatus = Cypress.CyOpen((byte)deviceIndexAtSCB1, 0, out mHandle);
                Console.WriteLine("Spi SCB1 device index = {0}", deviceIndexAtSCB1);
            }
            else
            {
                cyReturnStatus = Cypress.CyOpen(1, 0, out mHandle);
                Console.WriteLine("Detect device index fail. SPI cyOpen with index 1.");
            }


            return (-1) * (int)cyReturnStatus;
        }

        public int Config(UInt32 frequencyHz)
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;
            Cypress.CY_SPI_CONFIG config = new Cypress.CY_SPI_CONFIG();

            config.frequency = frequencyHz;
            cyReturnStatus = Cypress.CySetSpiConfig(mHandle, ref config);

            return (-1) * (int)cyReturnStatus;
        }

        public int Close()
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;

            cyReturnStatus = Cypress.CyClose(mHandle);

            return (-1) * (int)cyReturnStatus;
        }

        public int ReadWrite(ref byte[] rdbuf, byte[] wrbuf, int len)
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;
            //Cypress.CY_I2C_DATA_CONFIG dataConfig = new Cypress.CY_I2C_DATA_CONFIG();
            Cypress.CY_DATA_BUFFER RdBuf = new Cypress.CY_DATA_BUFFER();
            Cypress.CY_DATA_BUFFER WrBuf = new Cypress.CY_DATA_BUFFER();

            ///////////////////////////////////////////////////////////////////////////////
            IntPtr pWrBuf = Marshal.AllocHGlobal(len);
            Marshal.Copy(wrbuf, 0, pWrBuf, len);
            WrBuf.length = (UInt32)len;
            WrBuf.buffer = pWrBuf;

            IntPtr pRdBuf = Marshal.AllocHGlobal(len);
            RdBuf.buffer = pRdBuf;
            RdBuf.length = (UInt32)len;

            //Call dll API
            cyReturnStatus = Cypress.CySpiReadWrite(mHandle, ref RdBuf, ref WrBuf, 5000);

            //Copy data from unmanaged memory
            Marshal.Copy(pRdBuf, rdbuf, 0, len);

            //Free unmanaged memory.
            Marshal.FreeHGlobal(pWrBuf);
            Marshal.FreeHGlobal(pRdBuf);

            return (-1) * (int)cyReturnStatus;
        }
    } //Class CypressSpi

    public class CypressGpio
    {
        private IntPtr mHandle;

        public int Open(byte dn = 0)
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;
            int deviceIndexAtSCB0 = Cypress.FindDeviceAtSCB(Cypress.CY_DEVICE_SERIAL_BLOCK.SerialBlock_SCB0, dn);

            if (deviceIndexAtSCB0 >= 0)
            {
                cyReturnStatus = Cypress.CyOpen((byte)deviceIndexAtSCB0, 0, out mHandle);
                Console.WriteLine("Gpio SCB0 device index = {0}", deviceIndexAtSCB0);
            }
            else
            {
                cyReturnStatus = Cypress.CyOpen(0, 0, out mHandle);
                Console.WriteLine("Detect device index fail. GPIO cyOpen with index 0.");
            }

            return (-1) * (int)cyReturnStatus;
        }

        public int Close()
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;

            cyReturnStatus = Cypress.CyClose(mHandle);

            return (-1) * (int)cyReturnStatus;
        }

        //buf length must be more than len
        public int Read(byte GpioNum, ref byte Value)
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;
            //Create a IntPtr for dll API.
            //Allocate a unmanaged memory for dll API
            //IntPtr pRecvBuf = Marshal.AllocHGlobal(1);

            //Call dll API
            cyReturnStatus = Cypress.CyGetGpioValue(mHandle, GpioNum, ref Value);

            //Copy data from unmanaged memory
            //Marshal.Copy(pRecvBuf, (int)Value, 0, 1);

            // Marshal.FreeHGlobal(pRecvBuf);

            return (-1) * (int)cyReturnStatus;
        }

        public int Write(byte GpioNum, byte Value)
        {
            Cypress.CY_RETURN_STATUS cyReturnStatus;
            ///////////////////////////////////////////////////////////////////////////////
            // Write

            //Create a IntPtr for dll API.
            //Allocate a unmanaged memory and copy buf into it.
            //IntPtr pBuf = Marshal.AllocHGlobal(1);
            //Marshal.Copy(buf, 0, pBuf, 1);

            //Call dll API
            cyReturnStatus = Cypress.CySetGpioValue(mHandle, GpioNum, Value);

            //Free unmanaged memory.
            //Marshal.FreeHGlobal(pBuf);

            return (-1) * (int)cyReturnStatus;
        }
    } //Class CypressGpio
}//namespace CypressApi