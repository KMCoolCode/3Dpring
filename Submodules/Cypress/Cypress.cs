using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CypressLibrary
{
    public class Cypress
    {
        public enum CY_DEVICE_SERIAL_BLOCK
        {
            SerialBlock_SCB0 = 0,               /*Serial Block Number 0*/
            SerialBlock_SCB1,                   /*Serial Block Number 1*/
            SerialBlock_MFG                     /*Serial Block Manufacturing Interface.*/
        }

        private enum CY_DEVICE_CLASS
        {
            CY_CLASS_DISABLED = 0,              /*None or the interface is disabled */
            CY_CLASS_CDC = 0x02,                /*CDC ACM class*/
            CY_CLASS_PHDC = 0x0F,               /*PHDC class */
            CY_CLASS_VENDOR = 0xFF              /*VENDOR specific class*/
        }

        public enum CY_DEVICE_TYPE
        {
            CY_TYPE_DISABLED = 0,               /*Invalid device type or interface is not CY_CLASS_VENDOR*/
            CY_TYPE_UART,                       /*Interface of device is of type UART*/
            CY_TYPE_SPI,                        /*Interface of device is of type SPI */
            CY_TYPE_I2C,                        /*Interface of device is of type I2C */
            CY_TYPE_JTAG,                       /*Interface of device is of type JTAG*/
            CY_TYPE_MFG                         /*Interface of device is in Manufacturing mode*/
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CY_VID_PID
        {
            public ushort vid;         /*Holds the VID of the device*/
            public ushort pid;         /*Holds the PID of the device*/
        }

        public enum CY_RETURN_STATUS
        {
            CY_SUCCESS = 0,                         /*API returned successfully without any errors.*/
            CY_ERROR_ACCESS_DENIED,                 /*Access of the API is denied for the application */
            CY_ERROR_DRIVER_INIT_FAILED,            /*Driver initialisation failed*/
            CY_ERROR_DEVICE_INFO_FETCH_FAILED,      /*Device information fetch failed */
            CY_ERROR_DRIVER_OPEN_FAILED,            /*Failed to open a device in the library */
            CY_ERROR_INVALID_PARAMETER,             /*One or more parameters sent to the API was invalid*/
            CY_ERROR_REQUEST_FAILED,                /*Request sent to USB Serial device failed */
            CY_ERROR_DOWNLOAD_FAILED,               /*Firmware download to the device failed */
            CY_ERROR_FIRMWARE_INVALID_SIGNATURE,    /*Invalid Firmware signature in firmware file*/
            CY_ERROR_INVALID_FIRMWARE,              /*Invalid firmware */
            CY_ERROR_DEVICE_NOT_FOUND,              /*Device disconnected */
            CY_ERROR_IO_TIMEOUT,                    /*Timed out while processing a user request*/
            CY_ERROR_PIPE_HALTED,                   /*Pipe halted while trying to transfer data*/
            CY_ERROR_BUFFER_OVERFLOW,               /*OverFlow of buffer while trying to read/write data */
            CY_ERROR_INVALID_HANDLE,                /*Device handle is invalid */
            CY_ERROR_ALLOCATION_FAILED,             /*Error in Allocation of the resource inside the library*/
            CY_ERROR_I2C_DEVICE_BUSY,               /*I2C device busy*/
            CY_ERROR_I2C_NAK_ERROR,                 /*I2C device NAK*/
            CY_ERROR_I2C_ARBITRATION_ERROR,         /*I2C bus arbitration error*/
            CY_ERROR_I2C_BUS_ERROR,                 /*I2C bus error*/
            CY_ERROR_I2C_BUS_BUSY,                  /*I2C bus is busy*/
            CY_ERROR_I2C_STOP_BIT_SET,              /*I2C master has sent a stop bit during a transaction*/
            CY_ERROR_STATUS_MONITOR_EXIST           /*API Failed because the SPI/UART status monitor thread already exists*/
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CY_I2C_CONFIG
        {
            public uint frequency;
            public byte slaveAddress;
            public bool isMaster;
            public bool isClockStretch;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CY_DEVICE_INFO
        {
            private CY_VID_PID vidPid;                                      /*VID and PID*/
            private byte numInterfaces;                                    /*Number of interfaces supported*/

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            private byte[] manufacturerName;     /*Manufacturer name*/

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            private byte[] productName;          /*Product name*/

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            private byte[] serialNum;            /*Serial number*/

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            private byte[] deviceFriendlyName;   /*Device friendly name : Windows only*/

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            private CY_DEVICE_TYPE[] deviceType;    /*Type of the device each interface has(Valid only

                                                            for USB Serial Device) and interface in vendor class*/

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            private CY_DEVICE_CLASS[] deviceClass;  /*Interface class of each interface*/

            public CY_DEVICE_SERIAL_BLOCK deviceBlock; /* On Windows, each USB Serial device interface is associated with a
                                            separate driver instance. This variable represents the present driver
                                            interface instance that is associated with a serial block. */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CY_I2C_DATA_CONFIG
        {
            public byte slaveAddress;     /*Slave address the master will communicate with*/
            public bool isStopBit;         /*Set when stop bit is used*/
            public bool isNakBit;          /*Set when I2C master wants to NAK the slave after read
                              Applicable only when doing I2C read*/
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CY_DATA_BUFFER
        {
            public IntPtr buffer;
            public UInt32 length;                      /*Length of the buffer */
            public UInt32 transferCount;               /*Number of bytes actually read/written*/
        }

        //
        // API
        //
        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CyI2cRead(
            IntPtr handle,                           /*Valid device handle*/
            ref CY_I2C_DATA_CONFIG dataConfig,	            /*I2C data config*/
            ref CY_DATA_BUFFER readBuffer,                 /*Read buffer details*/
            UInt32 timeout                              /*API timeout value*/
        );

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CyI2cWrite(
            IntPtr handle,                           /*Valid device handle*/
            ref CY_I2C_DATA_CONFIG dataConfig,	            /*I2C Slave address */
            ref CY_DATA_BUFFER writeBuffer,                /*Write buffer details*/
            UInt32 timeout                              /*API timeout value*/
        );

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CyGetI2cConfig(
            IntPtr handle,                         /*Valid device handle*/
            ref CY_I2C_CONFIG i2cConfig                  /*I2C configuration value read back*/
        );

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CySetI2cConfig(
            IntPtr handle,                         /*Valid device handle*/
            ref CY_I2C_CONFIG i2cConfig                   /*I2C configuration value*/
        );

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CyGetDeviceInfoVidPid(
            CY_VID_PID vidPid,                          /*VID and PID of device of interest*/
            ref byte deviceIdList,                        /*Array of device ID's returned*/
            ref CY_DEVICE_INFO deviceInfoList,             /*Array of pointers to device info list*/
            ref byte deviceCount,                         /*Count of devices with specified VID PID*/
            byte infoListLength                                /*Total length of the deviceInfoList allocated
                                                 (Size of deviceInfoList array)*/
        );

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CyOpen(byte deviceNumber, byte interfaceNum, out IntPtr handle);

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CyClose(IntPtr handle);

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CyGetListofDevices(ref byte numDevices);

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CyGetDeviceInfo(byte deviceNumber, ref CY_DEVICE_INFO deviceInfo);

        //[StructLayout(LayoutKind.Sequential)]
        public enum CY_SPI_PROTOCOL
        {
            CY_SPI_MOTOROLA = 0,  /**< In master mode, when not transmitting data (SELECT is inactive), SCLK is stable at CPOL.
                                   *   In slave mode, when not selected, SCLK is ignored; i.e. it can be either stable or clocking.
                                   *   In master mode, when there is no data to transmit (TX FIFO is empty), SELECT is inactive.
                                   */
            CY_SPI_TI,            /**< In master mode, when not transmitting data, SCLK is stable at '0'.
                                   *   In slave mode, when not selected, SCLK is ignored - i.e. it can be either stable or clocking.
                                   *   In master mode, when there is no data to transmit (TX FIFO is empty), SELECT is inactive -
                                   *   i.e. no pulse is generated.
                                   *   *** It supports only mode 1 whose polarity values are
                                   *   CPOL = 0
                                   *   CPHA = 1
                                   */
            CY_SPI_NS             /**< In master mode, when not transmitting data, SCLK is stable at '0'. In slave mode,
                                   *   when not selected, SCLK is ignored; i.e. it can be either stable or clocking.
                                   *   In master mode, when there is no data to transmit (TX FIFO is empty), SELECT is inactive.
                                   *   *** It supports only mode 0 whose polarity values are
                                   *   CPOL = 0
                                   *   CPHA = 0
                                   */
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct CY_SPI_CONFIG
        {
            public UInt32 frequency;                               /**< SPI clock frequency.
                                                             *   ** IMPORTANT: The frequency range supported by SPI module is
                                                             *   1000(1KHz) to 3000000(3MHz)
                                                             */
            public byte dataWidth;                                /**< Data width in bits. The valid values are from 4 to 16. */
            public CY_SPI_PROTOCOL protocol;                      /**< SPI Protocols to be used as defined in CY_SPI_PROTOCOL */
            public bool isMsbFirst;                                /**< false -> least significant bit is sent out first
                                                                 true -> most significant bit is sent out first */
            public bool isMaster;                                  /**< false --> Slave mode selected:
                                                                 true --> Master mode selected*/
            public bool isContinuousMode;                          /**< true - Slave select line is not asserted i.e
                                                             *   de-asserted for every word.
                                                             *   false- Slave select line is always asserted
                                                             */
            public bool isSelectPrecede;                           /**< Valid only in TI mode.
                                                             *   true - The start pulse precedes the first data
                                                             *   false - The start pulse is in sync with first data.
                                                             */
            public bool isCpha;                                    /**< false - Clock phase is 0; true - Clock phase is 1. */
            public bool isCpol;                                    /**< false - Clock polarity is 0;true - Clock polarity is 1. */
        };

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CyGetSpiConfig(
        IntPtr handle,                         /**< Valid device handle */
        ref CY_SPI_CONFIG spiConfig                  /**< SPI configuration structure value read back */
        );

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CySetSpiConfig(
        IntPtr handle,
        ref CY_SPI_CONFIG spiConfig
        );

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CySpiReadWrite(
        IntPtr handle,               /**< Valid device handle */
        ref CY_DATA_BUFFER readBuffer,     /**< Read data buffer */
        ref CY_DATA_BUFFER writeBuffer,    /**< Write data buffer */
        UInt32 timeout                  /**< Time out value of the API */
        );

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CySetGpioValue(
        IntPtr handle,                               /*Valid device handle*/
        byte gpioNumber,                         /*GPIO configuration value*/
        byte value                              /*Value that needs to be set*/
        );

        [DllImport("cyusbserial.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CY_RETURN_STATUS CyGetGpioValue(
        IntPtr handle,                               /*Valid device handle*/
        byte gpioNumber,                         /*GPIO configuration value*/
        ref byte value                              /*Value that needs to be set*/
        );
        //private static object lockDevice = new object();
        public static int FindDeviceAtSCB(CY_DEVICE_SERIAL_BLOCK Serial_block, int device_number)
        {
            CY_VID_PID cyVidPid;

            cyVidPid.vid = 0x4B4;
            cyVidPid.pid = 0xA;
            CY_RETURN_STATUS cyReturnStatus;
            byte cyNumDevices = new byte();
            CY_DEVICE_INFO cyDeviceInfoList = new CY_DEVICE_INFO();
            cyReturnStatus = CyGetListofDevices(ref cyNumDevices);
            int device_count = -1;
            int deviceIndex = -1;
            for (int index = 0; index < cyNumDevices; index++)
            {
                cyReturnStatus = CyGetDeviceInfo((byte)index, ref cyDeviceInfoList);
                //printf ("\nNumber of interfaces: %d\n \
                //        Vid: 0x%X \n\
                //        Pid: 0x%X \n\
                //        Serial name is: %s\n \
                //        Manufacturer name: %s\n \
                //        Product Name: %s\n \
                //        SCB Number: 0x%X \n\
                //        Device Type: %d \n \
                //        Device Class: %d\n\n\n",
                //        cyDeviceInfoList[index].numInterfaces,                  
                //        cyDeviceInfoList[index].vidPid.vid,
                //        cyDeviceInfoList[index].vidPid.pid,
                //        cyDeviceInfoList[index].serialNum,
                //        cyDeviceInfoList[index].manufacturerName,
                //        cyDeviceInfoList[index].productName,
                //        cyDeviceInfoList[index].deviceBlock,
                //        cyDeviceInfoList[index].deviceType[0],
                //        cyDeviceInfoList[index].deviceClass[0]);

                // Find the device at device index at SCB0
                //Console.WriteLine("index={0}, deviceBlock={1}", index, cyDeviceInfoList.deviceBlock);
                if (cyDeviceInfoList.deviceBlock == Serial_block)
                {
                    device_count++;
                    if (device_count == device_number)
                    {
                        deviceIndex = index;
                        break;
                    }
                    continue;
                }
            }
            return deviceIndex;

        }


        /// <summary>
        /// find CY_DEVICE_SERIAL_BLOCK type count
        /// by liwenqiang
        /// </summary>
        /// <param name="Serial_block"></param>
        /// <returns></returns>
        public static int FindDeviceAtSCBCount(CY_DEVICE_SERIAL_BLOCK Serial_block)
        {
            CY_VID_PID cyVidPid;

            cyVidPid.vid = 0x4B4;
            cyVidPid.pid = 0xA;
            CY_RETURN_STATUS cyReturnStatus;
            byte cyNumDevices = new byte();
            CY_DEVICE_INFO cyDeviceInfoList = new CY_DEVICE_INFO();
            cyReturnStatus = CyGetListofDevices(ref cyNumDevices);
            int device_count = 0;
            for (int index = 0; index < cyNumDevices; index++)
            {
                cyReturnStatus = CyGetDeviceInfo((byte)index, ref cyDeviceInfoList);
                //printf ("\nNumber of interfaces: %d\n \
                //        Vid: 0x%X \n\
                //        Pid: 0x%X \n\
                //        Serial name is: %s\n \
                //        Manufacturer name: %s\n \
                //        Product Name: %s\n \
                //        SCB Number: 0x%X \n\
                //        Device Type: %d \n \
                //        Device Class: %d\n\n\n",
                //        cyDeviceInfoList[index].numInterfaces,                  
                //        cyDeviceInfoList[index].vidPid.vid,
                //        cyDeviceInfoList[index].vidPid.pid,
                //        cyDeviceInfoList[index].serialNum,
                //        cyDeviceInfoList[index].manufacturerName,
                //        cyDeviceInfoList[index].productName,
                //        cyDeviceInfoList[index].deviceBlock,
                //        cyDeviceInfoList[index].deviceType[0],
                //        cyDeviceInfoList[index].deviceClass[0]);

                // Find the device at device index at SCB0
                //Console.WriteLine("index={0}, deviceBlock={1}", index, cyDeviceInfoList.deviceBlock);
                if (cyDeviceInfoList.deviceBlock == Serial_block)
                {
                    device_count++;
                }
            }
            return device_count;
        }

    }
}