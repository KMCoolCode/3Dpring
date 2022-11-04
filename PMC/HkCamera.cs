using MvCamCtrl.NET;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static MvCamCtrl.NET.MyCamera;

namespace HkCameraDisplay
{
    public class HkCamera
    {
        private MV_CC_DEVICE_INFO_LIST stDevList;
        private MyCamera[] cameras;
        public List<string> GetCameraList()
        {
           
            int nRet = MV_OK;
            stDevList = new MV_CC_DEVICE_INFO_LIST();
            nRet = MV_CC_EnumDevices_NET(MV_GIGE_DEVICE | MV_USB_DEVICE, ref stDevList);
            if (MV_OK != nRet)
            {
                throw new Exception(string.Format("Enum device failed:{0:x8}", nRet));
            }

            Console.WriteLine("Enum device count : " + Convert.ToString(stDevList.nDeviceNum));
            if (0 == stDevList.nDeviceNum)
            {
                return new List<string>();
            }

            MV_CC_DEVICE_INFO stDevInfo;                            // 通用设备信息
            var devices = new List<string>();
            cameras = new MyCamera[stDevList.nDeviceNum];
            // ch:打印设备信息 en:Print device info
            for (int i = 0; i < stDevList.nDeviceNum; i++)
            {
                stDevInfo = (MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[i], typeof(MV_CC_DEVICE_INFO));

                if (MV_GIGE_DEVICE == stDevInfo.nTLayerType)
                {
                    MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MV_GIGE_DEVICE_INFO)ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MV_GIGE_DEVICE_INFO));
                    uint nIp1 = (stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24;
                    uint nIp2 = (stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16;
                    uint nIp3 = (stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8;
                    uint nIp4 = stGigEDeviceInfo.nCurrentIp & 0x000000ff;
                    Console.WriteLine("[device " + i.ToString() + "]:");
                    Console.WriteLine("DevIP:" + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4);
                    Console.WriteLine("UserDefineName:" + stGigEDeviceInfo.chUserDefinedName + "\n");
                    devices.Add(nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4);
                }
                else if (MV_USB_DEVICE == stDevInfo.nTLayerType)
                {
                    MV_USB3_DEVICE_INFO stUsb3DeviceInfo = (MV_USB3_DEVICE_INFO)ByteToStruct(stDevInfo.SpecialInfo.stUsb3VInfo, typeof(MV_USB3_DEVICE_INFO));
                    Console.WriteLine("[device " + i.ToString() + "]:");
                    Console.WriteLine("SerialNumber:" + stUsb3DeviceInfo.chSerialNumber);
                    Console.WriteLine("UserDefineName:" + stUsb3DeviceInfo.chUserDefinedName + "\n");
                }
                if (cameras[i]==null)
                {
                    cameras[i] = new MyCamera();
                    MVCC_ENUMVALUE formate = new MVCC_ENUMVALUE();
                    //cameras[i].MV_CC_GetPixelFormat_NET(ref formate);
                    nRet = cameras[i].MV_CC_CreateDevice_NET(ref stDevInfo);
                    cameras[i].MV_CC_SetEnumValue_NET("PixelFormat", (uint)MvGvspPixelType.PixelType_Gvsp_RGB8_Packed);

                }
               
              
            }
           
            return devices;
        }

        internal void SetExposure(int selectedIndex, int value)
        {
            cameras[selectedIndex].MV_CC_SetFloatValue_NET("ExposureTime", value);
        }

        internal Mat Grab(int id)
        {
            MV_FRAME_OUT stFrameInfo = new MV_FRAME_OUT();
            MV_DISPLAY_FRAME_INFO stDisplayInfo = new MV_DISPLAY_FRAME_INFO();
            cameras[id].MV_CC_StartGrabbing_NET();
            cameras[id].MV_CC_SetEnumValue_NET("TriggerSource", (uint)MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            cameras[id].MV_CC_SetCommandValue_NET("TriggerSoftware");
            var nRet = cameras[id].MV_CC_GetImageBuffer_NET(ref stFrameInfo, 1000);
            cameras[id].MV_CC_StopGrabbing_NET();


            if (nRet==0)
            {
                stDisplayInfo.pData = stFrameInfo.pBufAddr;
                stDisplayInfo.nDataLen = stFrameInfo.stFrameInfo.nFrameLen;
                stDisplayInfo.nWidth = stFrameInfo.stFrameInfo.nWidth;
                stDisplayInfo.nHeight = stFrameInfo.stFrameInfo.nHeight;
                stDisplayInfo.enPixelType = stFrameInfo.stFrameInfo.enPixelType;
                if (stDisplayInfo.enPixelType== MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    var srcImage = new Mat(stDisplayInfo.nHeight, stDisplayInfo.nWidth, MatType.CV_8UC1, stDisplayInfo.pData);
                    return srcImage;
                }
                else if (stDisplayInfo.enPixelType == MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                {
                    var srcImage = new Mat(stDisplayInfo.nHeight, stDisplayInfo.nWidth, MatType.CV_8UC3, stDisplayInfo.pData);
                    var bgrImage = new Mat();
                    Cv2.CvtColor(srcImage, bgrImage, ColorConversionCodes.RGB2BGR);     //将图像转换为CV_RGB2BGR
                    return bgrImage;
                }
                else
                {
                    int nChannelNum = 0;
                    MyCamera.MvGvspPixelType enType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined;
                    if (IsColorPixelFormat(stFrameInfo.stFrameInfo.enPixelType))
                    {
                        enType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
                        nChannelNum = 3;
                    }
                    else if (IsMonoPixelFormat(stFrameInfo.stFrameInfo.enPixelType))
                    {
                        enType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                        nChannelNum = 1;
                    }
                    else
                    {
                        Console.WriteLine("Don't need to convert!");
                    }

                    MyCamera.MV_PIXEL_CONVERT_PARAM stConvertPixelParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
                    stConvertPixelParam.nWidth = stFrameInfo.stFrameInfo.nWidth;
                    stConvertPixelParam.nHeight = stFrameInfo.stFrameInfo.nHeight;
                    stConvertPixelParam.pSrcData = stFrameInfo.pBufAddr;
                    stConvertPixelParam.nSrcDataLen = stFrameInfo.stFrameInfo.nFrameLen;

                    stConvertPixelParam.enSrcPixelType = stFrameInfo.stFrameInfo.enPixelType;
                    stConvertPixelParam.enDstPixelType = enType;
                    IntPtr pBufForConvert = IntPtr.Zero;
                    if (pBufForConvert == IntPtr.Zero)
                    {
                        pBufForConvert = Marshal.AllocHGlobal((int)(stFrameInfo.stFrameInfo.nWidth * stFrameInfo.stFrameInfo.nHeight * nChannelNum));
                    }

                    stConvertPixelParam.pDstBuffer = pBufForConvert;

                    stConvertPixelParam.nDstBufferSize = (uint)(stFrameInfo.stFrameInfo.nWidth * stFrameInfo.stFrameInfo.nHeight * nChannelNum);


                    nRet = cameras[id].MV_CC_ConvertPixelType_NET(ref stConvertPixelParam);

                    var srcImage = new Mat(stDisplayInfo.nHeight, stDisplayInfo.nWidth, MatType.CV_8UC3, stConvertPixelParam.pDstBuffer);
                    var bgrImage = new Mat();
                    Cv2.CvtColor(srcImage, bgrImage, ColorConversionCodes.RGB2BGR);     //将图像转换为CV_RGB2BGR
                    return bgrImage;
                }
                
            }
            return null;
           
        }
        static bool IsMonoPixelFormat(MyCamera.MvGvspPixelType enType)
        {
            switch (enType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;
                default:
                    return false;
            }
        }

        static bool IsColorPixelFormat(MyCamera.MvGvspPixelType enType)
        {
            switch (enType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                    return true;
                default:
                    return false;
            }
        }

        internal bool OpenDevice(int index)
        {
            MV_CC_DEVICE_INFO device =
                 (MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[index], typeof(MV_CC_DEVICE_INFO));

            int nRet = cameras[index].MV_CC_CreateDevice_NET(ref device);
            if (MV_OK != nRet)
            {
                return false;
            }
            nRet = cameras[index].MV_CC_OpenDevice_NET();
            if (MV_OK != nRet)
            {
                MV_GIGE_DEVICE_INFO stGigEDeviceInfo = 
                    (MV_GIGE_DEVICE_INFO)ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MV_GIGE_DEVICE_INFO));
                uint nIp1 = (stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24;
                uint nIp2 = (stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16;
                uint nIp3 = (stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8;
                uint nIp4 = stGigEDeviceInfo.nCurrentIp & 0x000000ff;
               
                //throw new Exception(string.Format("Open Device[{0}] failed! nRet=0x{1}\r\n", "DevIP:" + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4, nRet.ToString("X")));
                return false;
            }
            return true;
        }
        public void Dispose()
        {
            foreach (var camera in cameras)
            {
                var nRet = camera.MV_CC_CloseDevice_NET();
                nRet = camera.MV_CC_DestroyDevice_NET();
            }
           
        }
    }
}
