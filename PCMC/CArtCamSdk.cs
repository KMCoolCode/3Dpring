//==========================================================
//	ARTRAY Camera / Capture Module Software Developer Kit
//
//						Version 1.301		2012.07.26
//						Version 1.302		2012.12.20
//									(C) 2002-2012 Artray
//==========================================================

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading; 

namespace ArtCamSdk
{
	public enum DLL_MESSAGE
	{
		WM_GRAPHNOTIFY = 0x8001,	// Not used in this version.
		WM_GRAPHPAINT,				// Sent to application when an image is updated.
		WM_ERROR,					// Sent to application when an error occurs
	}                                  

	public enum ARTCAM_CAMERATYPE
	{
		ARTCAM_CAMERATYPE_DS			= 1,	// DirectShow Camera
		ARTCAM_CAMERATYPE_DISABLE1		= 2,	// Invalid
		ARTCAM_CAMERATYPE_USTC			= 3,	// ARTUST
		ARTCAM_CAMERATYPE_CNV			= 4,	// ARTCNV
		ARTCAM_CAMERATYPE_DISABLE2		= 5,	// Invalid
		ARTCAM_CAMERATYPE_130MI			= 6,	// ARTCAM-130MI
		ARTCAM_CAMERATYPE_DISABLE3		= 7,	// Invalid
		ARTCAM_CAMERATYPE_200MI			= 8,	// ARTCAM-200MI
		ARTCAM_CAMERATYPE_300MI			= 9,	// ARTCAM-300MI
		ARTCAM_CAMERATYPE_150P			= 10,	// ARTCAM-150P
		ARTCAM_CAMERATYPE_320P			= 11,	// ARTCAM-320P
		ARTCAM_CAMERATYPE_DISABLE4		= 12,	// Invalid
		ARTCAM_CAMERATYPE_200SH			= 13,	// ARTCAM-200SH
		ARTCAM_CAMERATYPE_098			= 14,	// ARTCAM-098
		ARTCAM_CAMERATYPE_036MI			= 15,	// ARTCAM-036MI
		ARTCAM_CAMERATYPE_500P			= 16,	// ARTCAM-500P
		ARTCAM_CAMERATYPE_150P2			= 17,	// ARTCAM-150P2
		ARTCAM_CAMERATYPE_036MIST		= 18,	// ARTCAM-036MIST
		ARTCAM_CAMERATYPE_500MI			= 19,	// ARTCAM-500MI
		ARTCAM_CAMERATYPE_DISABLE5		= 20,	// Invalid
		ARTCAM_CAMERATYPE_DISABLE6		= 21,	// Invalid
		ARTCAM_CAMERATYPE_150P3			= 22,	// ARTCAM-150P3
		ARTCAM_CAMERATYPE_130MI_MOUT	= 23,	// ARTCAM-130MI MOUT
		ARTCAM_CAMERATYPE_150P3_MOUT	= 24,	// ARTCAM-150P3 MOUT
		ARTCAM_CAMERATYPE_267KY			= 25,	// ARTCAM-267KY
		ARTCAM_CAMERATYPE_274KY			= 26,	// ARTCAM-274KY
		ARTCAM_CAMERATYPE_625KY			= 27,	// ARTCAM-274KY
		ARTCAM_CAMERATYPE_V135MI		= 28,	// ARTCAM-V135MI
		ARTCAM_CAMERATYPE_445KY			= 29,	// ARTCAM-445KY
		ARTCAM_CAMERATYPE_098II			= 30,	// ARTCAM-098II
		ARTCAM_CAMERATYPE_MV413			= 31,	// ARTCAM-MV413USB
		ARTCAM_CAMERATYPE_OV210			= 32,	// ARTCAM-OV210
		ARTCAM_CAMERATYPE_850SH			= 33,	// ARTCAM-850SH
		ARTCAM_CAMERATYPE_1251SH		= 34,	// ARTCAM-1252SH
		ARTCAM_CAMERATYPE_D131			= 35,	// ARTCAM-D131
		ARTCAM_CAMERATYPE_900MI			= 36,	// ARTCAM-900MI
		ARTCAM_CAMERATYPE_1000MI		= 37,	// ARTCAM-1000MI
		ARTCAM_CAMERATYPE_500P2			= 38,	// ARTCAM-500P2
		ARTCAM_CAMERATYPE_035KY			= 39,	// ARTCAM-035KY
		ARTCAM_CAMERATYPE_1000MI_HD2	= 40,	// ARTCAM-1000MI-HD2
		ARTCAM_CAMERATYPE_006MAT		= 41,	// ARTCAM-006MAT
		ARTCAM_CAMERATYPE_150P5_HD2		= 42,	// ARTCAM-150P5-HD2
		ARTCAM_CAMERATYPE_130MI_HD2		= 43,	// ARTCAM-130MI-HD2

		ARTCAM_CAMERATYPE_008TNIR		= 126,	// ARTCAM-008TNIR
		ARTCAM_CAMERATYPE_031TNIR		= 127,	// ARTCAM-031TNIR

		// SATA Series
		ARTCAM_CAMERATYPE_SATA = 201,	// SATA CAMERA

		// USB3.0 Series
		ARTCAM_CAMERATYPE_USB3_900MI	= 301,	// ARTCAM-900MI-USB3
		ARTCAM_CAMERATYPE_USB3_500MI	= 302,	// ARTCAM-500MI-USB3
		ARTCAM_CAMERATYPE_USB3_150P3	= 303,	// ARTCAM-150P3-USB3
		ARTCAM_CAMERATYPE_USB3_445KY	= 304,	// ARTCAM-445KY2-USB3
		ARTCAM_CAMERATYPE_USB3_1400MI	= 305,	// ARTCAM-1400MI-USB3
		ARTCAM_CAMERATYPE_USB3_267KY	= 306,	// ARTCAM-267KY-USB3
		ARTCAM_CAMERATYPE_USB3_655KY	= 307,	// ARTCAM-655KY-USB3
		ARTCAM_CAMERATYPE_USB3_274KY	= 308,	// ARTCAM-274KY-USB3
		ARTCAM_CAMERATYPE_USB3_424KY	= 309,	// ARTCAM-424KY-USB3
		ARTCAM_CAMERATYPE_USB3_2900KAI	= 310,	// ARTCAM-2900KAI-USB3
		ARTCAM_CAMERATYPE_USB3_810KAI	= 311,	// ARTCAM-810KAI-USB3
		ARTCAM_CAMERATYPE_USB3_1000MI	= 312,	// ARTCAM-1000MI-USB3
		ARTCAM_CAMERATYPE_USB3_2000CMV	= 313,	// ARTCAM-2000CMV-USB3
		ARTCAM_CAMERATYPE_USB3_1600KAI	= 314,	// ARTCAM-1600KAI-USB3
		ARTCAM_CAMERATYPE_USB3_410KAI	= 315,	// ARTCAM-410KAI-USB3
		ARTCAM_CAMERATYPE_USB3_100KAI	= 316,	// ARTCAM-100KAI-USB3
		ARTCAM_CAMERATYPE_USB3_210KAI	= 317,	// ARTCAM-210KAI-USB3

		// WOM Series
		ARTCAM_CAMERATYPE_036MI2_WOM	= 400,	// ARTCAM-036MI WOM 
		ARTCAM_CAMERATYPE_130MI_WOM		= 401,	// ARTCAM-130MI WOM 
		ARTCAM_CAMERATYPE_300MI_WOM		= 402,	// ARTCAM-300MI WOM 
		ARTCAM_CAMERATYPE_500MI_WOM		= 403,	// ARTCAM-500MI WOM 
		ARTCAM_CAMERATYPE_900MI_WOM		= 404,	// ARTCAM-900MI WOM 
		ARTCAM_CAMERATYPE_1000MI_WOM	= 405,	// ARTCAM-1000MI WOM
		ARTCAM_CAMERATYPE_1400MI_WOM	= 406,	// ARTCAM-1100MI WOM
		ARTCAM_CAMERATYPE_IMX035_WOM	= 407,	// ARTCAM-IMX035 WOM
		ARTCAM_CAMERATYPE_130HP_WOM		= 408,	// ARTCAM-130HP WOM

		ARTCAM_CAMERATYPE_150P5_WOM		= 420,	// ARTCAM-150P3 WOM 
		ARTCAM_CAMERATYPE_267KY_WOM		= 421,	// ARTCAM-267KY WOM 
		ARTCAM_CAMERATYPE_274KY_WOM		= 422,	// ARTCAM-274KY WOM 
		ARTCAM_CAMERATYPE_445KY2_WOM	= 423,	// ARTCAM-445KY2 WOM
		ARTCAM_CAMERATYPE_500P2_WOM		= 424,	// ARTCAM-500P2 WOM 
		ARTCAM_CAMERATYPE_655KY_WOM		= 425,	// ARTCAM-655KY WOM 
		ARTCAM_CAMERATYPE_424KY_WOM		= 426,	// ARTCAM-424KY WOM
		ARTCAM_CAMERATYPE_445KY3_WOM	= 427,	// ARTCAM-445KY2 WOM
		ARTCAM_CAMERATYPE_285CX_WOM		= 428,	// ARTCAM-285CX WOM
		ARTCAM_CAMERATYPE_407UV_WOM		= 429,	// ARTCAM-407UV WOM
		ARTCAM_CAMERATYPE_130E2V_WOM	= 430,	// ARTCAM-130E2V WOM
		ARTCAM_CAMERATYPE_130XQE_WOM	= 431,	// ARTCAM-130XQE WOM
		ARTCAM_CAMERATYPE_0134AR_WOM	= 432,	// ARTCAM-0134AR WOM
		ARTCAM_CAMERATYPE_092XQE_WOM	= 433,	// ARTCAM-092XQE WOM
		ARTCAM_CAMERATYPE_265IMX_WOM	= 434,  // ARTCAM-265IMX WOM
		ARTCAM_CAMERATYPE_264IMX_WOM	= 435,  // ARTCAM-264IMX WOM
		ARTCAM_CAMERATYPE_130UV_WOM		= 436,	// ARTCAM-130UV WOM
		ARTCAM_CAMERATYPE_092UV_WOM		= 437,	// ARTCAM-092UV WOM

		//USB3 Type2
		ARTCAM_CAMERATYPE_500MI_USB3_T2  = 500,	// ARTCAM-500MI-USB3-T2
		ARTCAM_CAMERATYPE_1000MI_USB3_T2 = 501,	// ARTCAM-1000MI-USB3-T2
		ARTCAM_CAMERATYPE_1400MI_USB3_T2 = 502,	// ARTCAM-1400MI-USB3-T2
		ARTCAM_CAMERATYPE_034MI_USB3_T2  = 503,	// ARTCAM-034MI-USB3-T2
		ARTCAM_CAMERATYPE_178IMX_USB3_T2 = 504,	// ARTCAM-178IMX-USB3-T2
		ARTCAM_CAMERATYPE_174IMX_USB3_T2 = 505,	// ARTCAM-174IMX-USB3-T2

		ARTCAM_CAMERATYPE_410KAI_USB3_T2	= 509,	//
		ARTCAM_CAMERATYPE_810KAI_USB3_T2	= 510,	//
		ARTCAM_CAMERATYPE_1600KAI_USB3_T2	= 511,	//
		ARTCAM_CAMERATYPE_2900KAI_USB3_T2	= 512,	//


		ARTCAM_CAMERATYPE_250IMX_USB3_T2	= 522,
		ARTCAM_CAMERATYPE_252IMX_USB3_T2	= 523,
		ARTCAM_CAMERATYPE_264IMX_USB3_T2	= 524,
		ARTCAM_CAMERATYPE_265IMX_USB3_T2	= 525,
		ARTCAM_CAMERATYPE_2020UV_USB3_T2	= 526,
		ARTCAM_CAMERATYPE_226IMX_USB3_T2	= 527,
	}

	// SATA Camera type
	public enum ARTCAM_CAMERATYPE_SATA 
	{
		ARTCAM_CAMERATYPE_SATA_LVDS		= 0,
		ARTCAM_CAMERATYPE_SATA_300MI	= 1,
		ARTCAM_CAMERATYPE_SATA_500MI	= 2,
		ARTCAM_CAMERATYPE_SATA_MV413	= 3,
		ARTCAM_CAMERATYPE_SATA_800MI	= 4,
		ARTCAM_CAMERATYPE_SATA_036MI	= 5,
		ARTCAM_CAMERATYPE_SATA_150P		= 6,
		ARTCAM_CAMERATYPE_SATA_267KY	= 7,
		ARTCAM_CAMERATYPE_SATA_274KY	= 8,
		ARTCAM_CAMERATYPE_SATA_625KY	= 9,
		ARTCAM_CAMERATYPE_SATA_130MI	=10,
		ARTCAM_CAMERATYPE_SATA_200MI	=11,
	}                                    
                                         

	// Error Code
	public enum ARTCAMSDK_ERROR
	{
		ARTCAMSDK_NOERROR = 0,		// Normal
		ARTCAMSDK_NOT_INITIALIZE,	// Not initialized
		ARTCAMSDK_DISABLEDDEVICE,	// Access to unavailable device was attempted
		ARTCAMSDK_CREATETHREAD,		// Failure of thread creation for capturing
		ARTCAMSDK_CREATEWINDOW,		// Window creation failed
		ARTCAMSDK_OUTOFMEMORY,		// Not enough memory for image transfer/Failure for securing memory
		ARTCAMSDK_CAMERASET,		// Error for camera (device) settings
		ARTCAMSDK_CAMERASIZE,		// Error for camera (device) size settings
		ARTCAMSDK_CAPTURE,			// Capturing failed
		ARTCAMSDK_PARAM,			// Wrong argument
		ARTCAMSDK_DIRECTSHOW,		// Directshow initialization error
		ARTCAMSDK_UNSUPPORTED,		// Not supported
		ARTCAMSDK_UNKNOWN,			// Unknow error
		ARTCAMSDK_CAPTURELOST,		// Device lost
		ARTCAMSDK_FILENOTFOUND,		// File not found
		ARTCAMSDK_FPGASET,			// FPGA settings error
	}

	// Information for transferring
	public struct GP_INFO
	{
		public int		lSize;	// size of struct sizeof(GP_INFO)
		public int		lWidth;	// Width of image
		public int		lHeigh;	// Height of image
		public int		lBpp; 	// Byte per pixcel
		public int		lFps; 	// Frame rate (x10)
//		public byte[]	pImage;	// The pointer to the captured image
	}

	// Type of image filter
	public enum ARTCAM_FILTERTYPE
	{
		ARTCAM_FILTERTYPE_RESERVE = 0,			// Reserved
		ARTCAM_FILTERTYPE_BRIGHTNESS,			// Brightness
		ARTCAM_FILTERTYPE_CONTRAST,				// Contrast
		ARTCAM_FILTERTYPE_HUE,					// Hue
		ARTCAM_FILTERTYPE_SATURATION,			// Saturation
		ARTCAM_FILTERTYPE_SHARPNESS,			// Sharpness
		ARTCAM_FILTERTYPE_BAYER_GAIN_RGB,		// Bayer coversion color gain
		ARTCAM_FILTERTYPE_BAYER_GAIN_R,			// Color gain (red only) for bayer conversion
		ARTCAM_FILTERTYPE_BAYER_GAIN_G,			// Color gain (green only) for bayer conversion
		ARTCAM_FILTERTYPE_BAYER_GAIN_B,			// Color gain (Blue only) for bayer conversion
		ARTCAM_FILTERTYPE_BAYER_GAIN_AUTO,		// Auto white balance
		ARTCAM_FILTERTYPE_GAMMA,				// Gumma
		ARTCAM_FILTERTYPE_BAYERMODE,			// Bayer conversion mode
		ARTCAM_FILTERTYPE_GLOBAL_GAIN,			// Global gain for camera
		ARTCAM_FILTERTYPE_COLOR_GAIN_R,			// Color gain (red) for camera
		ARTCAM_FILTERTYPE_COLOR_GAIN_G1,		// Color gain (green1) for camera
		ARTCAM_FILTERTYPE_COLOR_GAIN_G2,		// Color gain (green2) for camera
		ARTCAM_FILTERTYPE_COLOR_GAIN_B,			// Color gain (blue) for camera
		ARTCAM_FILTERTYPE_EXPOSURETIME,			// Exposure time (shutter speed)
		ARTCAM_FILTERTYPE_GRAY_MODE,			// Convert mode for gray scale
		ARTCAM_FILTERTYPE_GRAY_GAIN_R,			// Color gain (red) for gray scale
		ARTCAM_FILTERTYPE_GRAY_GAIN_G1,			// Color gain (green1) for gray scale
		ARTCAM_FILTERTYPE_GRAY_GAIN_G2,			// Color gain (green2) for gray scale
		ARTCAM_FILTERTYPE_GRAY_GAIN_B,			// Color gain (blue) for gray scale
		ARTCAM_FILTERTYPE_GRAY_OFFSET_R,		// Color offset (red) for gray scale
		ARTCAM_FILTERTYPE_GRAY_OFFSET_G1,		// Color offset (green1) for gray scale
		ARTCAM_FILTERTYPE_GRAY_OFFSET_G2,		// Color offset (green2) for gray scale
		ARTCAM_FILTERTYPE_GRAY_OFFSET_B,		// Color offset (blue) for gray scale
		ARTCAM_FILTERTYPE_GRAY_GAIN_AUTO,		//

		ARTCAM_FILTERTYPE_CHANNEL_GAIN1,		//
		ARTCAM_FILTERTYPE_CHANNEL_GAIN2,		//
		ARTCAM_FILTERTYPE_CHANNEL_GAIN3,		//
		ARTCAM_FILTERTYPE_CHANNEL_GAIN4,		//
		ARTCAM_FILTERTYPE_CHANNEL_OFFSET1,		//
		ARTCAM_FILTERTYPE_CHANNEL_OFFSET2,		//
		ARTCAM_FILTERTYPE_CHANNEL_OFFSET3,		//
		ARTCAM_FILTERTYPE_CHANNEL_OFFSET4,		//

		// For TNIR Cameras
		ARTCAM_FILTERTYPE_PELTIER = 100,		// Peltier control
		ARTCAM_FILTERTYPE_TEMPERATURE,			// sensor temperature (only received),
		ARTCAM_FILTERTYPE_DOTFILTER,			// pixel correction filter
		ARTCAM_FILTERTYPE_MASKFILTER,			// mask correction filter
	}

	// Pixel skipping transfer mode
	public enum SUBSAMPLE
	{
		SUBSAMPLE_1 = 0,	// Full size
		SUBSAMPLE_2,		// 1/2
		SUBSAMPLE_4,		// 1/4
		SUBSAMPLE_8,		// 1/8
		BINNING_2   = 0x11,
		BINNING_4	= 0x12,
	}

	// Auto iris mode
	public enum AI_TYPE
	{
		AI_NONE = 0,		// Invalid
		AI_EXPOSURE,		// Exposure time
		AI_GAIN,			// Gain
		AI_BOTH,			// Exposure time + Gain
	};

	// Convert mode for gray scale
	public enum GRAY_TYPE 
	{
		GRAY_NONE = 0,		// Disable (without convertion)
		GRAY_BAYERCONVERT,	// Use Gray gain and offset on Bayer image
		GRAY_GRAYSCALE,		// Use Brightness of color-converted image
	};


	// Video format
	public enum VIDEOFORMAT
	{
		VIDEOFORMAT_NTSC = 0,	// NTSC
		VIDEOFORMAT_PAL,		// PAL
		VIDEOFORMAT_PALM,		// PALM
		VIDEOFORMAT_SECAM,		// SECAM
	};

	// Sampling rate
	public enum SAMPLING_RATE
	{
		WIDE_HISPEED = 0,	// 
		WIDE_LOWSPEED,		// 
		NORMAL_HISPEED,		// 
		NORMAL_LOWSPEED,	// 
	};


	// Format for image saving
	public enum FILETYPE
	{
		FILETYPE_BITMAP = 0,	// Bitmap saving (large size).
		FILETYPE_RAW,       	// Only data is saved (large size)
		FILETYPE_JPEG_HIGH, 	// Save in high-quality JPEG format (medium size). Characters and lines are distorted.
		FILETYPE_JPEG_NOMAL,	// Save in standard JPEG format (small size). Small patterns are distorted.
		FILETYPE_JPEG_LOW,  	// Save in low-quality JPEG (small size). Block noise are seen.
		FILETYPE_PNG,       	// Save as PNG. 16 bits image can be saved correctly. Size=Medium
		FILETYPE_TIFF,      	// Save as TIFF. 16 bits image can be saved correctly. Size=Large
	};                             

	// Camera information
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public struct CAMERAINFO 
	{
		public int	lSize;				// Size of this struct
		public int	lWidth;				// H-Effective
		public int	lHeight;			// V-Effective
		public int	lGlobalGainMin;		// Min value of Global Gain (Return -1 if disabled)
		public int	lGlobalGainMax;		// Max value of Global Gain(Return -1 if disabled)
		public int	lColorGainMin;		// Min value of Color Gain(Return -1 if disabled)
		public int	lColorGainMax;		// Max value of Colof Gain(Return -1 if disabled)
		public int	lExposureMin;		// Min value of Shutter Width(Return -1 if disabled)
		public int	lExposureMax;		// Max value of Shutter Width(Return -1 if disabled)
	}

/*
 *Remark*
 16 bit image cannot be read in every image-processing software or in every environment.
 16 bit PNG and TIFF saved by this SDK can be read by our Veiwer Software Version2 or later and Adobe Photoshop6.
 For all other software, deterioration of image or an error may be experienced.
 Since it saves as a 16-bit gray scale picture when saved by 16-bit bitmap   The color palette etc. is not saved. 
 Since it saves as a 16-bit gray scale picture when saved by 16-bit bitmap   The color palette etc. is not saved.
 
 In case it saves by 16 bits which needs to prepare a 16-bit palette by the software side, 
 or needs to prepare the device context of gray scale, we recommend you to save by TIFF or PNG. 
 In JPEG, it cannot save other than a color picture. 
 Especially about a 16-bit picture, data of 8 bits of low ranks is omitted and saved. 
*/

	// For TNIR Cameras
	public enum MASKTYPE
	{
		MASKTYPE_LOW = 0,
		MASKTYPE_HIGH,
	};

	public class CArtCam
	{
		[DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
		static extern IntPtr Win32LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);
		[DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
		static extern IntPtr Win32GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
		[DllImport("kernel32.dll", SetLastError=true, EntryPoint="GetLastError")]
		static extern uint Win32GetLastError();
		[DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
		static extern bool Win32FreeLibrary(IntPtr hModule);


		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETDLLVERSION					();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETLASTERROR					(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int INITIALIZE						(IntPtr hWnd);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int RELEASE						(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int PREVIEW						(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int RECORD							(int hACam, string lpAviName, int RecTime, int fShow);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int CALLBACKPREVIEWA				(int hACam, IntPtr hWnd, byte[] lpImage, int Size, int TopDown);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int CALLBACKPREVIEWP				(int hACam, IntPtr hWnd, IntPtr lpImage, int Size, int TopDown);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SNAPSHOTA						(int hACam, byte[] lpImage, int Size, int TopDown);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SNAPSHOTP						(int hACam, IntPtr lpImage, int Size, int TopDown);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int CLOSE							(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int CAPTURE						(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int TRIGGERA						(int hACam, IntPtr hWnd, byte[] lpImage, int Size, int TopDown);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int TRIGGERP						(int hACam, IntPtr hWnd, IntPtr lpImage, int Size, int TopDown);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SAVEIMAGE						(int hACam, string  lpSaveName, FILETYPE FileType);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETPREVIEWWINDOW				(int hACam, IntPtr hWnd, int Left, int Top, int Right, int Bottom);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCAPTUREWINDOW				(int hACam, int Width, int Height, int Frame);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCAPTUREWINDOWEX				(int hACam, int HTotal, int HStart, int HEffective, int VTotal, int VStart, int VEffective);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETCAPTUREWINDOWEX				(int hACam, out int HTotal, out int HStart, out int HEffective, out int VTotal, out int VStart, out int VEffective);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCOLORMODE					(int hACam, int ColorMode);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETCOLORMODE					(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCROSSBAR					(int hACam, int Output, int Input);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETDEVICENUMBER				(int hACam, int Number);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int ENUMDEVICE						(int hACam, StringBuilder[] szDeviceName);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETDEVICENAME					(int hACam, int index, StringBuilder szDeviceName, int nSize);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int STARTPREVIEW					(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int STOPPREVIEW					(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETIMAGEA						(int hACam, byte[] lpImage, int Size, int TopDown);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETIMAGEP						(int hACam, IntPtr lpImage, int Size, int TopDown);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int WIDTH							(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int HEIGHT							(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int FPS							(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCAMERADLG					(int hACam, IntPtr hWnd);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETIMAGEDLG					(int hACam, IntPtr hWnd);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETANALOGDLG					(int hACam, IntPtr hWnd);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETIOPORT						(int hACam, byte byteData, int longData, int Reserve);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETIOPORT						(int hACam, out byte byteData, out int longData, int Reserve);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETFILTERVALUE					(int hACam, ARTCAM_FILTERTYPE FilterType, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETFILTERVALUE					(int hACam, ARTCAM_FILTERTYPE FilterType, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETSUBSAMPLE					(int hACam, SUBSAMPLE SubSampleMode);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETSUBSAMPLE					(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETWAITTIME					(int hACam, int WaitTime);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETWAITTIME					(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETMIRRORV						(int hACam, int Flg);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETMIRRORV						(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETMIRRORH						(int hACam, int Flg);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETMIRRORH						(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETBRIGHTNESS					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETBRIGHTNESS					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCONTRAST					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETCONTRAST					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETHUE							(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETHUE							(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETSATURATION					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETSATURATION					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETSHARPNESS					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETSHARPNESS					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETBAYERGAINRGB				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETBAYERGAINRGB				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETBAYERGAINRED				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETBAYERGAINRED				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETBAYERGAINGREEN				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETBAYERGAINGREEN				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETBAYERGAINBLUE				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETBAYERGAINBLUE				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETBAYERGAINAUTO				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETBAYERGAINAUTO				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGAMMA						(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGAMMA						(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETBAYERMODE					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETBAYERMODE					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGLOBALGAIN					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGLOBALGAIN					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCOLORGAINRED				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETCOLORGAINRED				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCOLORGAINGREEN1				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETCOLORGAINGREEN1				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCOLORGAINGREEN2				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETCOLORGAINGREEN2				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCOLORGAINBLUE				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETCOLORGAINBLUE				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETEXPOSURETIME				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETEXPOSURETIME				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETHALFCLOCK					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETHALFCLOCK					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETAUTOIRIS					(int hACam, AI_TYPE Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETAUTOIRIS					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETSAMPLINGRATE				(int hACam, SAMPLING_RATE Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETSAMPLINGRATE				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETVIDEOFORMAT					(int hACam, out int outError);
		// 1276      
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int WRITESROMID					(int hACam, int Address, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int READSROMID						(int hACam, int Address, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETCAMERAINFO					(int hACam, ref CAMERAINFO pInfo);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETSTATUS						(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int WRITEREGISTER					(int hACam, int Address, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int READREGISTER					(int hACam, int Address, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate bool	WRITEREGISTEREX				(int hACam, uint Address, uint Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate uint	READREGISTEREX				(int hACam, uint Address, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGRAYMODE					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGRAYMODE					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGRAYGAINR					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGRAYGAINR					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGRAYGAING1					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGRAYGAING1					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGRAYGAING2					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGRAYGAING2					(int hACam, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGRAYGAINB					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGRAYGAINB					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGRAYOFFSETR					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGRAYOFFSETR					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGRAYOFFSETG1				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGRAYOFFSETG1				(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGRAYOFFSETG2				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGRAYOFFSETG2				(int hACam, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETGRAYOFFSETB					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETGRAYOFFSETB					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCAMERATYPE					(int hACam, int Flg);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETCAMERATYPE					(int hACam, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate bool	SETSYNCV					(int hACam, int Flg);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int	GETSYNCV					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate bool	SETSYNCH					(int hACam, int Flg);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int	GETSYNCH					(int hACam, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int FPGA_WRITEREGISTER				(int hACam, int Address, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int FPGA_READREGISTER				(int hACam, int Address, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETPREVIEWMODE			(int hACam, int Preview);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETPREVIEWMODE			(int hACam, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETPREVIEWSIZE			(int hACam, int Width, int Height);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETPREVIEWSIZE			(int hACam, out int Width, out int Height);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETCOLORMODE			(int hACam, int ColorMode);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETCOLORMODE			(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETCAMERACLOCK			(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETCAMERACLOCK			(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETBAYERGAINAUTO		(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETBAYERGAINAUTO		(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETBAYERGAINLOCK		(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETBAYERGAINLOCK		(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETBAYERGAINRED		(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETBAYERGAINRED		(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETBAYERGAINGREEN		(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETBAYERGAINGREEN		(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETBAYERGAINBLUE		(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETBAYERGAINBLUE		(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SAVECURRENTSETTINGS	(int hACam);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_INITREGISTERSETTINGS	(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETCROSSCURSORMODE		(int hACam, int CursorNum, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETCROSSCURSORMODE		(int hACam, int CursorNum, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETCROSSCURSORCOLORR	(int hACam, int CursorNum, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETCROSSCURSORCOLORR	(int hACam, int CursorNum, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETCROSSCURSORCOLORG	(int hACam, int CursorNum, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETCROSSCURSORCOLORG	(int hACam, int CursorNum, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETCROSSCURSORCOLORB	(int hACam, int CursorNum, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETCROSSCURSORCOLORB	(int hACam, int CursorNum, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETCROSSCURSORCOLORRGB	(int hACam, int CursorNum, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETCROSSCURSORCOLORRGB	(int hACam, int CursorNum, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETCROSSCURSORPOS		(int hACam, int CursorNum, int PosX, int PosY);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETCROSSCURSORPOS		(int hACam, int CursorNum, out int PosX, out int PoxY);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETCROSSCURSORSIZE		(int hACam, int CursorNum, int SizeX, int SizeY);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETCROSSCURSORSIZE		(int hACam, int CursorNum, out int SizeX, out int SizeY);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETAUTOIRISMODE		(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETAUTOIRISMODE		(int hACam, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_SETAUTOIRISRANGE		(int hACam, int Min, int Max);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_GETAUTOIRISRANGE		(int hACam, out int Min, out int Max);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int MONITOR_LOADFIRMWARE			(int hACam, StringBuilder szFileName);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETREALEXPOSURETIME			(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETREALEXPOSURETIME			(int hACam, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SAVEMASKFILE					(int hACam, StringBuilder szFileName); 
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int LOADMASKFILE					(int hACam, StringBuilder szFileName);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int UPDATEMASKDATA					(int hACam, MASKTYPE Flg);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETPELTIER						(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETPELTIER						(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETTEMPERATURE					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETDOTFILTER					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETDOTFILTER					(int hACam, out int outError);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETMASKFILTER					(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETMASKFILTER					(int hACam, out int outError);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int LOADCONFIGFILE					(int hACam, StringBuilder szFileName);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int SETCONFIGFILTER				(int hACam, int Value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GETCONFIGFILTER				(int hACam);


		private IntPtr	m_hArtCamSdk = IntPtr.Zero;
		private int		m_hACam = 0;
		public	int		m_Error = 1;

		private	GETDLLVERSION		m_GetDllVersion;
		private	GETLASTERROR		m_GetLastError;

		private	INITIALIZE			m_Initialize;
		private	RELEASE				m_Release;

		private	PREVIEW				m_Preview;
		private	RECORD				m_Record;
		private	CALLBACKPREVIEWA	m_CallBackPreviewA;
		private	CALLBACKPREVIEWP	m_CallBackPreviewP;
		private	SNAPSHOTA			m_SnapShotA;
		private	SNAPSHOTP			m_SnapShotP;
		private	CLOSE				m_Close;
		private	CAPTURE				m_Capture;
		private	TRIGGERA			m_TriggerA;
		private	TRIGGERP			m_TriggerP;
		private	SAVEIMAGE			m_SaveImage;

		private	SETPREVIEWWINDOW	m_SetPreviewWindow;
		private	SETCAPTUREWINDOW	m_SetCaptureWindow;
		private	SETCAPTUREWINDOWEX	m_SetCaptureWindowEx;
		private	GETCAPTUREWINDOWEX	m_GetCaptureWindowEx;

		private	SETCOLORMODE		m_SetColorMode;
		private	GETCOLORMODE		m_GetColorMode;

		private	SETCROSSBAR			m_SetCrossbar;
		private	SETDEVICENUMBER		m_SetDeviceNumber;
		private	ENUMDEVICE			m_EnumDevice;
		private	GETDEVICENAME		m_GetDeviceName;

		private	STARTPREVIEW		m_StartPreview;
		private	STOPPREVIEW			m_StopPreview;
		private	GETIMAGEA			m_GetImageA;
		private	GETIMAGEP			m_GetImageP;

		private	WIDTH				m_Width;
		private	HEIGHT				m_Height;
		private	FPS					m_Fps;

		private	SETCAMERADLG		m_SetCameraDlg;
		private	SETIMAGEDLG			m_SetImageDlg;
		private	SETANALOGDLG		m_SetAnalogDlg;

		private	SETIOPORT			m_SetIOPort;
		private	GETIOPORT			m_GetIOPort;
		private	SETFILTERVALUE		m_SetFilterValue;
		private	GETFILTERVALUE		m_GetFilterValue;

		private	SETSUBSAMPLE		m_SetSubSample;
		private	GETSUBSAMPLE		m_GetSubSample;
		private	SETWAITTIME			m_SetWaitTime;
		private	GETWAITTIME			m_GetWaitTime;

		private	SETMIRRORV			m_SetMirrorV;
		private	GETMIRRORV			m_GetMirrorV;
		private	SETMIRRORH			m_SetMirrorH;
		private	GETMIRRORH			m_GetMirrorH;

		private	SETBRIGHTNESS		m_SetBrightness;
		private	GETBRIGHTNESS		m_GetBrightness;
		private	SETCONTRAST			m_SetContrast;
		private	GETCONTRAST			m_GetContrast;
		private	SETHUE				m_SetHue;
		private	GETHUE				m_GetHue;
		private	SETSATURATION		m_SetSaturation;
		private	GETSATURATION		m_GetSaturation;
		private	SETSHARPNESS		m_SetSharpness;
		private	GETSHARPNESS		m_GetSharpness;
		private	SETBAYERGAINRGB		m_SetBayerGainRGB;
		private	GETBAYERGAINRGB		m_GetBayerGainRGB;
		private	SETBAYERGAINRED		m_SetBayerGainRed;
		private	GETBAYERGAINRED		m_GetBayerGainRed;
		private	SETBAYERGAINGREEN	m_SetBayerGainGreen;
		private	GETBAYERGAINGREEN	m_GetBayerGainGreen;
		private	SETBAYERGAINBLUE	m_SetBayerGainBlue;
		private	GETBAYERGAINBLUE	m_GetBayerGainBlue;
		private	SETBAYERGAINAUTO	m_SetBayerGainAuto;
		private	GETBAYERGAINAUTO	m_GetBayerGainAuto;
		private	SETGAMMA			m_SetGamma;
		private	GETGAMMA			m_GetGamma;
		private	SETBAYERMODE		m_SetBayerMode;
		private	GETBAYERMODE		m_GetBayerMode;

		private	SETGLOBALGAIN		m_SetGlobalGain;
		private	GETGLOBALGAIN		m_GetGlobalGain;
		private	SETCOLORGAINRED		m_SetColorGainRed;
		private	GETCOLORGAINRED		m_GetColorGainRed;
		private	SETCOLORGAINGREEN1	m_SetColorGainGreen1;
		private	GETCOLORGAINGREEN1	m_GetColorGainGreen1;
		private	SETCOLORGAINGREEN2	m_SetColorGainGreen2;
		private	GETCOLORGAINGREEN2	m_GetColorGainGreen2;
		private	SETCOLORGAINBLUE	m_SetColorGainBlue;
		private	GETCOLORGAINBLUE	m_GetColorGainBlue;
		private	SETEXPOSURETIME		m_SetExposureTime;
		private	GETEXPOSURETIME		m_GetExposureTime;

		private	SETHALFCLOCK		m_SetHalfClock;
		private	GETHALFCLOCK		m_GetHalfClock;
		private	SETAUTOIRIS			m_SetAutoIris;
		private	GETAUTOIRIS			m_GetAutoIris;

		private	SETSAMPLINGRATE		m_SetSamplingRate;
		private	GETSAMPLINGRATE		m_GetSamplingRate;
		private	GETVIDEOFORMAT		m_GetVideoFormat;

		private	WRITESROMID			m_WriteSromID;
		private	READSROMID			m_ReadSromID;

		private	GETCAMERAINFO		m_GetCameraInfo;
		private	GETSTATUS			m_GetStatus;

		private	WRITEREGISTER		m_WriteRegister;
		private	READREGISTER		m_ReadRegister;

		private	WRITEREGISTEREX		m_WriteRegisterEx;
		private	READREGISTEREX		m_ReadRegisterEx;

		private	SETGRAYMODE			m_SetGrayMode;
		private	GETGRAYMODE			m_GetGrayMode;
		private	SETGRAYGAINR		m_SetGrayGainR;
		private	GETGRAYGAINR		m_GetGrayGainR;
		private	SETGRAYGAING1		m_SetGrayGainG1;
		private	GETGRAYGAING1		m_GetGrayGainG1;
		private	SETGRAYGAING2		m_SetGrayGainG2;
		private	GETGRAYGAING2		m_GetGrayGainG2;
		private	SETGRAYGAINB		m_SetGrayGainB;
		private	GETGRAYGAINB		m_GetGrayGainB;
		private	SETGRAYOFFSETR		m_SetGrayOffsetR;
		private	GETGRAYOFFSETR		m_GetGrayOffsetR;
		private	SETGRAYOFFSETG1		m_SetGrayOffsetG1;
		private	GETGRAYOFFSETG1		m_GetGrayOffsetG1;
		private	SETGRAYOFFSETG2		m_SetGrayOffsetG2;
		private	GETGRAYOFFSETG2		m_GetGrayOffsetG2;
		private	SETGRAYOFFSETB		m_SetGrayOffsetB;
		private	GETGRAYOFFSETB		m_GetGrayOffsetB;

		private	SETCAMERATYPE		m_SetCameraType;
		private	GETCAMERATYPE		m_GetCameraType;

		private	SETSYNCV			m_SetSyncV;
		private	GETSYNCV			m_GetSyncV;
		private	SETSYNCH			m_SetSyncH;
		private	GETSYNCH			m_GetSyncH;

		private	FPGA_WRITEREGISTER	m_Fpga_WriteRegister;
		private	FPGA_READREGISTER	m_Fpga_ReadRegister;

		private	MONITOR_SETPREVIEWMODE		m_Monitor_SetPreviewMode;
		private	MONITOR_GETPREVIEWMODE		m_Monitor_GetPreviewMode;
		private	MONITOR_SETPREVIEWSIZE		m_Monitor_SetPreviewSize;
		private	MONITOR_GETPREVIEWSIZE		m_Monitor_GetPreviewSize;
		private	MONITOR_SETCOLORMODE		m_Monitor_SetColorMode;
		private	MONITOR_GETCOLORMODE		m_Monitor_GetColorMode;
		private	MONITOR_SETCAMERACLOCK		m_Monitor_SetCameraClock;
		private	MONITOR_GETCAMERACLOCK		m_Monitor_GetCameraClock;
		private	MONITOR_SETBAYERGAINAUTO	m_Monitor_SetBayerGainAuto;
		private	MONITOR_GETBAYERGAINAUTO	m_Monitor_GetBayerGainAuto;
		private	MONITOR_SETBAYERGAINLOCK	m_Monitor_SetBayerGainLock;
		private	MONITOR_GETBAYERGAINLOCK	m_Monitor_GetBayerGainLock;
		private	MONITOR_SETBAYERGAINRED		m_Monitor_SetBayerGainRed;
		private	MONITOR_GETBAYERGAINRED		m_Monitor_GetBayerGainRed;
		private	MONITOR_SETBAYERGAINGREEN	m_Monitor_SetBayerGainGreen;
		private	MONITOR_GETBAYERGAINGREEN	m_Monitor_GetBayerGainGreen;
		private	MONITOR_SETBAYERGAINBLUE	m_Monitor_SetBayerGainBlue;
		private	MONITOR_GETBAYERGAINBLUE	m_Monitor_GetBayerGainBlue;

		private	MONITOR_SAVECURRENTSETTINGS		m_Monitor_SaveCurrentSettings;
		private	MONITOR_INITREGISTERSETTINGS	m_Monitor_InitRegisterSettings;
		private	MONITOR_SETCROSSCURSORMODE		m_Monitor_SetCrossCursorMode;
		private	MONITOR_GETCROSSCURSORMODE		m_Monitor_GetCrossCursorMode;
		private	MONITOR_SETCROSSCURSORCOLORR	m_Monitor_SetCrossCursorColorR;
		private	MONITOR_GETCROSSCURSORCOLORR	m_Monitor_GetCrossCursorColorR;
		private	MONITOR_SETCROSSCURSORCOLORG	m_Monitor_SetCrossCursorColorG;
		private	MONITOR_GETCROSSCURSORCOLORG	m_Monitor_GetCrossCursorColorG;
		private	MONITOR_SETCROSSCURSORCOLORB	m_Monitor_SetCrossCursorColorB;
		private	MONITOR_GETCROSSCURSORCOLORB	m_Monitor_GetCrossCursorColorB;
		private	MONITOR_SETCROSSCURSORCOLORRGB	m_Monitor_SetCrossCursorColorRGB;
		private	MONITOR_GETCROSSCURSORCOLORRGB	m_Monitor_GetCrossCursorColorRGB;
		private	MONITOR_SETCROSSCURSORPOS		m_Monitor_SetCrossCursorPos;
		private	MONITOR_GETCROSSCURSORPOS		m_Monitor_GetCrossCursorPos;
		private	MONITOR_SETCROSSCURSORSIZE		m_Monitor_SetCrossCursorSize;
		private	MONITOR_GETCROSSCURSORSIZE		m_Monitor_GetCrossCursorSize;	

		private	MONITOR_SETAUTOIRISMODE			m_Monitor_SetAutoIrisMode;		
		private	MONITOR_GETAUTOIRISMODE			m_Monitor_GetAutoIrisMode;		
		private	MONITOR_SETAUTOIRISRANGE		m_Monitor_SetAutoIrisRange;		
		private	MONITOR_GETAUTOIRISRANGE		m_Monitor_GetAutoIrisRange;
		private	MONITOR_LOADFIRMWARE			m_Monitor_LoadFirmware;

		private	GETREALEXPOSURETIME				m_GetRealExposureTime;
		private SETREALEXPOSURETIME				m_SetRealExposureTime;

		private	SAVEMASKFILE					m_SaveMaskFile;
		private	LOADMASKFILE					m_LoadMaskFile;
		private	UPDATEMASKDATA					m_UpdateMaskData;
		private	SETPELTIER						m_SetPeltier;		
		private	GETPELTIER						m_GetPeltier;
		private	GETTEMPERATURE					m_GetTemperature;
		private	SETDOTFILTER					m_SetDotFilter;
		private	GETDOTFILTER					m_GetDotFilter;
		private	SETMASKFILTER					m_SetMaskFilter;
		private	GETMASKFILTER					m_GetMaskFilter;

		private	LOADCONFIGFILE					m_LoadConfigFile;
		private	SETCONFIGFILTER					m_SetConfigFilter;
		private	GETCONFIGFILTER					m_GetConfigFilter;

		public CArtCam()
		{
			NullSet();
		}

		~CArtCam()
		{
			FreeLibrary();
		}

		public bool IsInit() { return IsValid(m_hArtCamSdk); }

		protected bool IsValid(IntPtr p)
		{
			return(IntPtr.Zero != p) ? true : false;
		}
		
		protected void NullSet()
		{
			m_hArtCamSdk				= IntPtr.Zero;
			m_hACam						= 0;
			m_Error = 1;

			m_GetDllVersion				= null;
			m_GetLastError				= null;
			m_Initialize				= null;
			m_Release					= null;
			m_Preview					= null;
			m_Record					= null;
			m_CallBackPreviewA			= null;
			m_CallBackPreviewP			= null;
			m_SnapShotA					= null;
			m_SnapShotP					= null;
			m_Close						= null;
			m_Capture					= null;
			m_TriggerA					= null;
			m_TriggerP					= null;
			m_SaveImage					= null;
			m_SetPreviewWindow			= null;
			m_SetCaptureWindow			= null;
			m_SetCaptureWindowEx		= null;
			m_GetCaptureWindowEx		= null;
			m_SetColorMode				= null;
			m_GetColorMode				= null;
			m_SetCrossbar				= null;
			m_SetDeviceNumber			= null;
			m_EnumDevice				= null;
			m_GetDeviceName				= null;
			m_StartPreview				= null;
			m_StopPreview				= null;
			m_GetImageA					= null;
			m_GetImageP					= null;
			m_Width						= null;
			m_Height					= null;
			m_Fps						= null;
			m_SetCameraDlg				= null;
			m_SetImageDlg				= null;
			m_SetAnalogDlg				= null;
			m_SetIOPort					= null;
			m_GetIOPort					= null;
			m_SetFilterValue			= null;
			m_GetFilterValue			= null;
			m_SetSubSample				= null;
			m_GetSubSample				= null;
			m_SetWaitTime				= null;
			m_GetWaitTime				= null;
			m_SetMirrorV				= null;
			m_GetMirrorV				= null;
			m_SetMirrorH				= null;
			m_GetMirrorH				= null;
			m_SetBrightness				= null;
			m_GetBrightness				= null;
			m_SetContrast				= null;
			m_GetContrast				= null;
			m_SetHue					= null;
			m_GetHue					= null;
			m_SetSaturation				= null;
			m_GetSaturation				= null;
			m_SetSharpness				= null;
			m_GetSharpness				= null;
			m_SetBayerGainRGB			= null;
			m_GetBayerGainRGB			= null;
			m_SetBayerGainRed			= null;
			m_GetBayerGainRed			= null;
			m_SetBayerGainGreen			= null;
			m_GetBayerGainGreen			= null;
			m_SetBayerGainBlue			= null;
			m_GetBayerGainBlue			= null;
			m_SetBayerGainAuto			= null;
			m_GetBayerGainAuto			= null;
			m_SetGamma					= null;
			m_GetGamma					= null;
			m_SetBayerMode				= null;
			m_GetBayerMode				= null;
			m_SetGlobalGain				= null;
			m_GetGlobalGain				= null;
			m_SetColorGainRed			= null;
			m_GetColorGainRed			= null;
			m_SetColorGainGreen1		= null;
			m_GetColorGainGreen1		= null;
			m_SetColorGainGreen2		= null;
			m_GetColorGainGreen2		= null;
			m_SetColorGainBlue			= null;
			m_GetColorGainBlue			= null;
			m_SetExposureTime			= null;
			m_GetExposureTime			= null;
			m_SetHalfClock				= null;
			m_GetHalfClock				= null;
			m_SetAutoIris				= null;
			m_GetAutoIris				= null;
			m_SetSamplingRate			= null;
			m_GetSamplingRate			= null;
			m_GetVideoFormat			= null;
			m_WriteSromID				= null;
			m_ReadSromID				= null;
			m_GetCameraInfo				= null;
			m_GetStatus					= null;
			m_WriteRegister				= null;
			m_ReadRegister				= null;
			m_WriteRegisterEx			= null;
			m_ReadRegisterEx			= null;
			m_SetGrayMode				= null;
			m_GetGrayMode				= null;
			m_SetGrayGainR				= null;
			m_GetGrayGainR				= null;
			m_SetGrayGainG1				= null;
			m_GetGrayGainG1				= null;
			m_SetGrayGainG2				= null;
			m_GetGrayGainG2				= null;
			m_SetGrayGainB				= null;
			m_GetGrayGainB				= null;
			m_SetGrayOffsetR			= null;
			m_GetGrayOffsetR			= null;
			m_SetGrayOffsetG1			= null;
			m_GetGrayOffsetG1			= null;
			m_SetGrayOffsetG2			= null;
			m_GetGrayOffsetG2			= null;
			m_SetGrayOffsetB			= null;
			m_GetGrayOffsetB			= null;
			m_SetCameraType				= null;
			m_GetCameraType				= null;
			m_SetSyncV					= null;
			m_GetSyncV					= null;
			m_SetSyncH					= null;
			m_GetSyncH					= null;
			m_Fpga_WriteRegister		= null;
			m_Fpga_ReadRegister			= null;
			m_Monitor_SetPreviewMode	= null;
			m_Monitor_GetPreviewMode	= null;
			m_Monitor_SetPreviewSize	= null;
			m_Monitor_GetPreviewSize	= null;
			m_Monitor_SetColorMode		= null;
			m_Monitor_GetColorMode		= null;
			m_Monitor_SetCameraClock	= null;
			m_Monitor_GetCameraClock	= null;
			m_Monitor_SetBayerGainAuto	= null;
			m_Monitor_GetBayerGainAuto	= null;
			m_Monitor_SetBayerGainLock	= null;
			m_Monitor_GetBayerGainLock	= null;
			m_Monitor_SetBayerGainRed	= null;
			m_Monitor_GetBayerGainRed	= null;
			m_Monitor_SetBayerGainGreen	= null;
			m_Monitor_GetBayerGainGreen	= null;
			m_Monitor_SetBayerGainBlue	= null;
			m_Monitor_GetBayerGainBlue	= null;
			m_Monitor_SaveCurrentSettings		= null;
			m_Monitor_InitRegisterSettings		= null;
			m_Monitor_SetCrossCursorMode		= null;
			m_Monitor_GetCrossCursorMode		= null;
			m_Monitor_SetCrossCursorColorR		= null;
			m_Monitor_GetCrossCursorColorR		= null;
			m_Monitor_SetCrossCursorColorG		= null;
			m_Monitor_GetCrossCursorColorG		= null;
			m_Monitor_SetCrossCursorColorB		= null;
			m_Monitor_GetCrossCursorColorB		= null;
			m_Monitor_SetCrossCursorColorRGB	= null;
			m_Monitor_GetCrossCursorColorRGB	= null;
			m_Monitor_SetCrossCursorPos			= null;
			m_Monitor_GetCrossCursorPos			= null;
			m_Monitor_SetCrossCursorSize		= null;
			m_Monitor_GetCrossCursorSize		= null;
			m_Monitor_SetAutoIrisMode			= null;
			m_Monitor_GetAutoIrisMode			= null;
			m_Monitor_SetAutoIrisRange			= null;
			m_Monitor_GetAutoIrisRange			= null;
			m_Monitor_LoadFirmware				= null;

			m_GetRealExposureTime				= null;
			m_SetRealExposureTime				= null;
			
			m_SaveMaskFile						= null;
			m_LoadMaskFile						= null;
			m_UpdateMaskData					= null;
			m_SetPeltier						= null;
			m_GetPeltier						= null;
			m_GetTemperature					= null;
			m_SetDotFilter						= null;
			m_GetDotFilter						= null;
			m_SetMaskFilter						= null;
			m_GetMaskFilter						= null;

			m_LoadConfigFile					= null;
			m_SetConfigFilter					= null;
			m_GetConfigFilter					= null;
		}
		
		protected Delegate GetDelegate(string szProcName, Type DelegateType)
		{
			IntPtr p = Win32GetProcAddress(m_hArtCamSdk, szProcName);
			if (p == IntPtr.Zero)
			{
				int hResult = Marshal.GetHRForLastWin32Error();
				Marshal.ThrowExceptionForHR(hResult);
			}
			return Marshal.GetDelegateForFunctionPointer(p, DelegateType);
		}
                                     
		public bool LoadLibrary(string szDllName)
		{
			if(IsValid(m_hArtCamSdk)){
				return true;
			}

			m_hArtCamSdk = Win32LoadLibrary(szDllName);
			uint error = Win32GetLastError();

			if(!IsValid(m_hArtCamSdk)){
				return false;
			}

			// DLL version check
			m_GetDllVersion = (GETDLLVERSION)GetDelegate("ArtCam_GetDllVersion", typeof(GETDLLVERSION));
			if(null == m_GetDllVersion){
				FreeLibrary();
				return false;
			}

			// Obtain version
			long Version = GetDllVersion() & 0xFFFF;
			long DllType = GetDllVersion() >> 16;

			// Standard function
			if(1000 <= Version){
				m_GetLastError				= (GETLASTERROR)		GetDelegate("ArtCam_GetLastError",	typeof(GETLASTERROR));
				m_Initialize				= (INITIALIZE)			GetDelegate("ArtCam_Initialize", 	typeof(INITIALIZE));
				m_Release					= (RELEASE)				GetDelegate("ArtCam_Release", 		typeof(RELEASE));
				m_Preview					= (PREVIEW)				GetDelegate("ArtCam_Preview", 		typeof(PREVIEW));
		#if UNICODE
				m_Record					= (RECORD)				GetDelegate("ArtCam_RecordW",		typeof(RECORD));
		#else
				m_Record					= (RECORD)				GetDelegate("ArtCam_Record",		typeof(RECORD));
		#endif
				m_CallBackPreviewA			= (CALLBACKPREVIEWA)	GetDelegate("ArtCam_CallBackPreview",	typeof(CALLBACKPREVIEWA)	);
				m_CallBackPreviewP			= (CALLBACKPREVIEWP)	GetDelegate("ArtCam_CallBackPreview",	typeof(CALLBACKPREVIEWP)	);
				m_Close						= (CLOSE)				GetDelegate("ArtCam_Close",				typeof(CLOSE)				);
				m_SetPreviewWindow			= (SETPREVIEWWINDOW)	GetDelegate("ArtCam_SetPreviewWindow", 	typeof(SETPREVIEWWINDOW)	);
				m_SetCaptureWindow			= (SETCAPTUREWINDOW)	GetDelegate("ArtCam_SetCaptureWindow", 	typeof(SETCAPTUREWINDOW)	);
				m_SetCaptureWindowEx		= (SETCAPTUREWINDOWEX)	GetDelegate("ArtCam_SetCaptureWindowEx",typeof(SETCAPTUREWINDOWEX)	);
				m_GetCaptureWindowEx		= (GETCAPTUREWINDOWEX)	GetDelegate("ArtCam_GetCaptureWindowEx",typeof(GETCAPTUREWINDOWEX)	);
				m_SetColorMode				= (SETCOLORMODE)		GetDelegate("ArtCam_SetColorMode",		typeof(SETCOLORMODE)		);
				m_GetColorMode				= (GETCOLORMODE)		GetDelegate("ArtCam_GetColorMode",		typeof(GETCOLORMODE)		);
				m_SetCrossbar				= (SETCROSSBAR)			GetDelegate("ArtCam_SetCrossbar",		typeof(SETCROSSBAR)			);
				m_SetDeviceNumber			= (SETDEVICENUMBER)		GetDelegate("ArtCam_SetDeviceNumber",	typeof(SETDEVICENUMBER)		);

		#if UNICODE
				m_EnumDevice				= (ENUMDEVICE)			GetDelegate("ArtCam_EnumDeviceW",	typeof(ENUMDEVICE));
				m_GetDeviceName				= (GETDEVICENAME)		GetDelegate("ArtCam_GetDeviceNameW",typeof(GETDEVICENAME));
		#else
				m_EnumDevice				= (ENUMDEVICE)			GetDelegate("ArtCam_EnumDevice",	typeof(ENUMDEVICE));
				m_GetDeviceName				= (GETDEVICENAME)		GetDelegate("ArtCam_GetDeviceName",	typeof(GETDEVICENAME));
		#endif
				m_StartPreview				= (STARTPREVIEW)		GetDelegate("ArtCam_StartPreview",	typeof(STARTPREVIEW));
				m_StopPreview				= (STOPPREVIEW)			GetDelegate("ArtCam_StopPreview",	typeof(STOPPREVIEW)	);
				m_GetImageA					= (GETIMAGEA)			GetDelegate("ArtCam_GetImage",		typeof(GETIMAGEA)	);
				m_GetImageP					= (GETIMAGEP)			GetDelegate("ArtCam_GetImage",		typeof(GETIMAGEP)	);
				m_Width						= (WIDTH)				GetDelegate("ArtCam_Width",			typeof(WIDTH)		);
				m_Height					= (HEIGHT)				GetDelegate("ArtCam_Height",		typeof(HEIGHT)		);
				m_Fps						= (FPS)					GetDelegate("ArtCam_Fps",			typeof(FPS)			);
				m_SetCameraDlg				= (SETCAMERADLG)		GetDelegate("ArtCam_SetCameraDlg",	typeof(SETCAMERADLG));
				m_SetImageDlg				= (SETIMAGEDLG)			GetDelegate("ArtCam_SetImageDlg",	typeof(SETIMAGEDLG)	);
				m_SetAnalogDlg				= (SETANALOGDLG)		GetDelegate("ArtCam_SetAnalogDlg",	typeof(SETANALOGDLG));


				if(	null == m_GetLastError || null == m_Initialize || null == m_Release || null == m_Preview || null == m_Record || null == m_CallBackPreviewA || null == m_CallBackPreviewP || null == m_Close || 
					null == m_SetPreviewWindow || null == m_SetCaptureWindow || null == m_SetCaptureWindowEx || null == m_GetCaptureWindowEx || null == m_SetColorMode || null == m_GetColorMode || 
					null == m_SetCrossbar || null == m_SetDeviceNumber || null == m_EnumDevice || null == m_GetDeviceName || null == m_StartPreview || null == m_StopPreview || null == m_GetImageA || null == m_GetImageP ||
					null == m_Width || null == m_Height || null == m_Fps || null == m_SetCameraDlg || null == m_SetImageDlg || null == m_SetAnalogDlg)
				{
					FreeLibrary();
					return false;
				}
			}


			// Functions available with V1.23 or later
			if(1230 <= Version){
				m_SetIOPort			= (SETIOPORT)		GetDelegate("ArtCam_SetIOPort",		typeof(SETIOPORT)		);
				m_GetIOPort			= (GETIOPORT)		GetDelegate("ArtCam_GetIOPort",		typeof(GETIOPORT)		);
				m_SetFilterValue	= (SETFILTERVALUE)	GetDelegate("ArtCam_SetFilterValue",typeof(SETFILTERVALUE)	);
				m_GetFilterValue	= (GETFILTERVALUE)	GetDelegate("ArtCam_GetFilterValue",typeof(GETFILTERVALUE)	);
				m_SnapShotA			= (SNAPSHOTA)		GetDelegate("ArtCam_SnapShot",		typeof(SNAPSHOTA)		);
				m_SnapShotP			= (SNAPSHOTP)		GetDelegate("ArtCam_SnapShot",		typeof(SNAPSHOTP)		);
				m_SetSubSample		= (SETSUBSAMPLE)	GetDelegate("ArtCam_SetSubSample",	typeof(SETSUBSAMPLE)	);
				m_GetSubSample		= (GETSUBSAMPLE)	GetDelegate("ArtCam_GetSubSample",	typeof(GETSUBSAMPLE)	);
				m_SetWaitTime		= (SETWAITTIME)		GetDelegate("ArtCam_SetWaitTime",	typeof(SETWAITTIME)		);
				m_GetWaitTime		= (GETWAITTIME)		GetDelegate("ArtCam_GetWaitTime",	typeof(GETWAITTIME)		);

				if(	null == m_SetIOPort || null == m_GetIOPort || null == m_SetFilterValue || null == m_GetFilterValue || null == m_SnapShotA || null == m_SnapShotP ||
					null == m_SetSubSample || null == m_GetSubSample || null == m_SetWaitTime || null == m_GetWaitTime	)
				{
					FreeLibrary();
					return false;
				}
			}


			// Functions available with V1.24 or later
			if(1240 <= Version){
				m_SetMirrorV			= (SETMIRRORV)			GetDelegate("ArtCam_SetMirrorV",		typeof(SETMIRRORV)			);
				m_GetMirrorV			= (GETMIRRORV)			GetDelegate("ArtCam_GetMirrorV",		typeof(GETMIRRORV)			);
				m_SetMirrorH			= (SETMIRRORH)			GetDelegate("ArtCam_SetMirrorH",		typeof(SETMIRRORH)			);
				m_GetMirrorH			= (GETMIRRORH)			GetDelegate("ArtCam_GetMirrorH",		typeof(GETMIRRORH)			);
				m_SetBrightness			= (SETBRIGHTNESS)		GetDelegate("ArtCam_SetBrightness",		typeof(SETBRIGHTNESS)		);
				m_GetBrightness			= (GETBRIGHTNESS)		GetDelegate("ArtCam_GetBrightness",		typeof(GETBRIGHTNESS)		);
				m_SetContrast			= (SETCONTRAST)			GetDelegate("ArtCam_SetContrast",		typeof(SETCONTRAST)			);
				m_GetContrast			= (GETCONTRAST)			GetDelegate("ArtCam_GetContrast",		typeof(GETCONTRAST)			);
				m_SetHue				= (SETHUE)				GetDelegate("ArtCam_SetHue",			typeof(SETHUE)				);
				m_GetHue				= (GETHUE)				GetDelegate("ArtCam_GetHue",			typeof(GETHUE)				);
				m_SetSaturation			= (SETSATURATION)		GetDelegate("ArtCam_SetSaturation",		typeof(SETSATURATION)		);
				m_GetSaturation			= (GETSATURATION)		GetDelegate("ArtCam_GetSaturation",		typeof(GETSATURATION)		);
				m_SetSharpness			= (SETSHARPNESS)		GetDelegate("ArtCam_SetSharpness",		typeof(SETSHARPNESS)		);
				m_GetSharpness			= (GETSHARPNESS)		GetDelegate("ArtCam_GetSharpness",		typeof(GETSHARPNESS)		);
				m_SetBayerGainRGB		= (SETBAYERGAINRGB)		GetDelegate("ArtCam_SetBayerGainRGB",	typeof(SETBAYERGAINRGB)		);
				m_GetBayerGainRGB		= (GETBAYERGAINRGB)		GetDelegate("ArtCam_GetBayerGainRGB",	typeof(GETBAYERGAINRGB)		);
				m_SetBayerGainRed		= (SETBAYERGAINRED)		GetDelegate("ArtCam_SetBayerGainRed",	typeof(SETBAYERGAINRED)		);
				m_GetBayerGainRed		= (GETBAYERGAINRED)		GetDelegate("ArtCam_GetBayerGainRed",	typeof(GETBAYERGAINRED)		);
				m_SetBayerGainGreen		= (SETBAYERGAINGREEN)	GetDelegate("ArtCam_SetBayerGainGreen",	typeof(SETBAYERGAINGREEN)	);
				m_GetBayerGainGreen		= (GETBAYERGAINGREEN)	GetDelegate("ArtCam_GetBayerGainGreen",	typeof(GETBAYERGAINGREEN)	);
				m_SetBayerGainBlue		= (SETBAYERGAINBLUE)	GetDelegate("ArtCam_SetBayerGainBlue",	typeof(SETBAYERGAINBLUE)	);
				m_GetBayerGainBlue		= (GETBAYERGAINBLUE)	GetDelegate("ArtCam_GetBayerGainBlue",	typeof(GETBAYERGAINBLUE)	);
				m_SetBayerGainAuto		= (SETBAYERGAINAUTO)	GetDelegate("ArtCam_SetBayerGainAuto",	typeof(SETBAYERGAINAUTO)	);
				m_GetBayerGainAuto		= (GETBAYERGAINAUTO)	GetDelegate("ArtCam_GetBayerGainAuto",	typeof(GETBAYERGAINAUTO)	);
				m_SetGamma				= (SETGAMMA)			GetDelegate("ArtCam_SetGamma",			typeof(SETGAMMA)			);
				m_GetGamma				= (GETGAMMA)			GetDelegate("ArtCam_GetGamma",			typeof(GETGAMMA)			);
				m_SetBayerMode			= (SETBAYERMODE)		GetDelegate("ArtCam_SetBayerMode",		typeof(SETBAYERMODE)		);
				m_GetBayerMode			= (GETBAYERMODE)		GetDelegate("ArtCam_GetBayerMode",		typeof(GETBAYERMODE)		);
				m_SetGlobalGain			= (SETGLOBALGAIN)		GetDelegate("ArtCam_SetGlobalGain",		typeof(SETGLOBALGAIN)		);
				m_GetGlobalGain			= (GETGLOBALGAIN)		GetDelegate("ArtCam_GetGlobalGain",		typeof(GETGLOBALGAIN)		);
				m_SetColorGainRed		= (SETCOLORGAINRED)		GetDelegate("ArtCam_SetColorGainRed",	typeof(SETCOLORGAINRED)		);
				m_GetColorGainRed		= (GETCOLORGAINRED)		GetDelegate("ArtCam_GetColorGainRed",	typeof(GETCOLORGAINRED)		);
				m_SetColorGainGreen1	= (SETCOLORGAINGREEN1)	GetDelegate("ArtCam_SetColorGainGreen1",typeof(SETCOLORGAINGREEN1)	);
				m_GetColorGainGreen1	= (GETCOLORGAINGREEN1)	GetDelegate("ArtCam_GetColorGainGreen1",typeof(GETCOLORGAINGREEN1)	);
				m_SetColorGainGreen2	= (SETCOLORGAINGREEN2)	GetDelegate("ArtCam_SetColorGainGreen2",typeof(SETCOLORGAINGREEN2)	);
				m_GetColorGainGreen2	= (GETCOLORGAINGREEN2)	GetDelegate("ArtCam_GetColorGainGreen2",typeof(GETCOLORGAINGREEN2)	);
				m_SetColorGainBlue		= (SETCOLORGAINBLUE)	GetDelegate("ArtCam_SetColorGainBlue",	typeof(SETCOLORGAINBLUE)	);
				m_GetColorGainBlue		= (GETCOLORGAINBLUE)	GetDelegate("ArtCam_GetColorGainBlue",	typeof(GETCOLORGAINBLUE)	);
				m_SetExposureTime		= (SETEXPOSURETIME)		GetDelegate("ArtCam_SetExposureTime",	typeof(SETEXPOSURETIME)		);
				m_GetExposureTime		= (GETEXPOSURETIME)		GetDelegate("ArtCam_GetExposureTime",	typeof(GETEXPOSURETIME)		);

				if(	null == m_SetMirrorV || null == m_GetMirrorV || null == m_SetMirrorH || null == m_GetMirrorH || 
					null == m_SetBrightness || null == m_GetBrightness || null == m_SetContrast || null == m_GetContrast || 
					null == m_SetHue || null == m_GetHue || null == m_SetSaturation || null == m_GetSaturation || 
					null == m_SetSharpness || null == m_GetSharpness || null == m_SetBayerGainRGB || null == m_GetBayerGainRGB || 
					null == m_SetBayerGainRed || null == m_GetBayerGainRed || null == m_SetBayerGainGreen || null == m_GetBayerGainGreen || 
					null == m_SetBayerGainBlue || null == m_GetBayerGainBlue || null == m_SetBayerGainAuto || null == m_GetBayerGainAuto || 
					null == m_SetGamma || null == m_GetGamma || null == m_SetBayerMode || null == m_GetBayerMode || 
					null == m_SetGlobalGain || null == m_GetGlobalGain || null == m_SetColorGainRed || null == m_GetColorGainRed || 
					null == m_SetColorGainGreen1 || null == m_GetColorGainGreen1 || null == m_SetColorGainGreen2 || null == m_GetColorGainGreen2 || 
					null == m_SetColorGainBlue || null == m_GetColorGainBlue || null == m_SetExposureTime || null == m_GetExposureTime)
				{
					FreeLibrary();
					return false;
				}
			}


			// Functions available with V1.25 or later
			if(1250 <= Version){
				m_Capture = (CAPTURE) GetDelegate("ArtCam_Capture",	typeof(CAPTURE));
				if(null == m_Capture){
					FreeLibrary();
					return false;
				}
			}



			// Functions available with V1.26 or later
			if(1260 <= Version){
				m_TriggerA		= (TRIGGERA)		GetDelegate("ArtCam_Trigger",		typeof(TRIGGERA)	);
				m_TriggerP		= (TRIGGERP)		GetDelegate("ArtCam_Trigger",		typeof(TRIGGERP)	);
				m_SaveImage		= (SAVEIMAGE)		GetDelegate("ArtCam_SaveImage",		typeof(SAVEIMAGE)	);
				m_SetHalfClock	= (SETHALFCLOCK)	GetDelegate("ArtCam_SetHalfClock",	typeof(SETHALFCLOCK));
				m_GetHalfClock	= (GETHALFCLOCK)	GetDelegate("ArtCam_GetHalfClock",	typeof(GETHALFCLOCK));
				m_SetAutoIris	= (SETAUTOIRIS)		GetDelegate("ArtCam_SetAutoIris",	typeof(SETAUTOIRIS)	);
				m_GetAutoIris	= (GETAUTOIRIS)		GetDelegate("ArtCam_GetAutoIris",	typeof(GETAUTOIRIS)	);

				if(null == m_TriggerA || null == m_TriggerP || null == m_SaveImage || null == m_SetHalfClock || null == m_GetHalfClock || null == m_SetAutoIris || null == m_GetAutoIris){
					FreeLibrary();
					return false;
				}
			}


			// Functions available with V1.275 or later
			if(1275 <= Version){
				m_SetSamplingRate	= (SETSAMPLINGRATE)	GetDelegate("ArtCam_SetSamplingRate",	typeof(SETSAMPLINGRATE)	);
				m_GetSamplingRate	= (GETSAMPLINGRATE)	GetDelegate("ArtCam_GetSamplingRate",	typeof(GETSAMPLINGRATE)	);
				m_GetVideoFormat	= (GETVIDEOFORMAT)	GetDelegate("ArtCam_GetVideoFormat",	typeof(GETVIDEOFORMAT)	);

				if(null == m_SetSamplingRate || null == m_GetSamplingRate || null == m_GetVideoFormat){
					FreeLibrary();
					return false;
				}
			}


			// Functions available with V1.276 or later
			if(1276 <= Version){
				m_WriteSromID	= (WRITESROMID)	GetDelegate("ArtCam_WriteSromID",	typeof(WRITESROMID)	);
				m_ReadSromID	= (READSROMID)	GetDelegate("ArtCam_ReadSromID",	typeof(READSROMID)	);

				if(null == m_WriteSromID || null == m_ReadSromID){
					FreeLibrary();
					return false;
				}
			}


			// Functions available with V1.280 or later
			if(1280 <= Version){
				m_GetCameraInfo		= (GETCAMERAINFO)	GetDelegate("ArtCam_GetCameraInfo",		typeof(GETCAMERAINFO)	);
				m_GetStatus			= (GETSTATUS)		GetDelegate("ArtCam_GetStatus",			typeof(GETSTATUS)		);
                                                                                                       
				m_WriteRegister		= (WRITEREGISTER)	GetDelegate("ArtCam_WriteRegister",		typeof(WRITEREGISTER)	);
				m_ReadRegister		= (READREGISTER)	GetDelegate("ArtCam_ReadRegister",		typeof(READREGISTER)	);
                                                                                                       
				m_SetGrayMode		= (SETGRAYMODE)		GetDelegate("ArtCam_SetGrayMode",		typeof(SETGRAYMODE)		);
				m_GetGrayMode		= (GETGRAYMODE)		GetDelegate("ArtCam_GetGrayMode",		typeof(GETGRAYMODE)		);
				m_SetGrayGainR		= (SETGRAYGAINR)	GetDelegate("ArtCam_SetGrayGainR",		typeof(SETGRAYGAINR)	);
				m_GetGrayGainR		= (GETGRAYGAINR)	GetDelegate("ArtCam_GetGrayGainR",		typeof(GETGRAYGAINR)	);
				m_SetGrayGainG1		= (SETGRAYGAING1)	GetDelegate("ArtCam_SetGrayGainG1",		typeof(SETGRAYGAING1)	);
				m_GetGrayGainG1		= (GETGRAYGAING1)	GetDelegate("ArtCam_GetGrayGainG1",		typeof(GETGRAYGAING1)	);
				m_SetGrayGainG2		= (SETGRAYGAING2)	GetDelegate("ArtCam_SetGrayGainG2",		typeof(SETGRAYGAING2)	);
				m_GetGrayGainG2		= (GETGRAYGAING2)	GetDelegate("ArtCam_GetGrayGainG2",		typeof(GETGRAYGAING2)	);
				m_SetGrayGainB		= (SETGRAYGAINB)	GetDelegate("ArtCam_SetGrayGainB",		typeof(SETGRAYGAINB)	);
				m_GetGrayGainB		= (GETGRAYGAINB)	GetDelegate("ArtCam_GetGrayGainB",		typeof(GETGRAYGAINB)	);
				m_SetGrayOffsetR	= (SETGRAYOFFSETR)	GetDelegate("ArtCam_SetGrayOffsetR",	typeof(SETGRAYOFFSETR)	);
				m_GetGrayOffsetR	= (GETGRAYOFFSETR)	GetDelegate("ArtCam_GetGrayOffsetR",	typeof(GETGRAYOFFSETR)	);
				m_SetGrayOffsetG1	= (SETGRAYOFFSETG1)	GetDelegate("ArtCam_SetGrayOffsetG1",	typeof(SETGRAYOFFSETG1)	);
				m_GetGrayOffsetG1	= (GETGRAYOFFSETG1)	GetDelegate("ArtCam_GetGrayOffsetG1",	typeof(GETGRAYOFFSETG1)	);
				m_SetGrayOffsetG2	= (SETGRAYOFFSETG2)	GetDelegate("ArtCam_SetGrayOffsetG2",	typeof(SETGRAYOFFSETG2)	);
				m_GetGrayOffsetG2	= (GETGRAYOFFSETG2)	GetDelegate("ArtCam_GetGrayOffsetG2",	typeof(GETGRAYOFFSETG2)	);
				m_SetGrayOffsetB	= (SETGRAYOFFSETB)	GetDelegate("ArtCam_SetGrayOffsetB",	typeof(SETGRAYOFFSETB)	);
				m_GetGrayOffsetB	= (GETGRAYOFFSETB)	GetDelegate("ArtCam_GetGrayOffsetB",	typeof(GETGRAYOFFSETB)	);

				if(	null == m_GetCameraInfo || null == m_GetStatus || null == m_WriteRegister || null == m_ReadRegister || null == m_SetGrayMode || null == m_GetGrayMode || 
					null == m_SetGrayGainR || null == m_GetGrayGainR || null == m_SetGrayGainG1 || null == m_GetGrayGainG1 || 
					null == m_SetGrayGainG2 || null == m_GetGrayGainG2 || null == m_SetGrayGainB || null == m_GetGrayGainB || 
					null == m_SetGrayOffsetR || null == m_GetGrayOffsetR || null == m_SetGrayOffsetG1 || null == m_GetGrayOffsetG1 || 
					null == m_SetGrayOffsetG2 || null == m_GetGrayOffsetG2 || null == m_SetGrayOffsetB || null == m_GetGrayOffsetB)
				{
					FreeLibrary();
					return false;
				}


				if (ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_900MI == (ARTCAM_CAMERATYPE)DllType || ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_1000MI == (ARTCAM_CAMERATYPE)DllType)
				{
					m_WriteRegisterEx		= (WRITEREGISTEREX)		GetDelegate("ArtCam_WriteRegisterEx",	typeof(WRITEREGISTEREX)	);
					m_ReadRegisterEx		= (READREGISTEREX)		GetDelegate("ArtCam_ReadRegisterEx",	typeof(READREGISTEREX)	);

					if(	null == m_WriteRegisterEx || null == m_ReadRegisterEx){
						FreeLibrary();
						return false;
					}
				}

				// SATA
				if (ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_SATA == (ARTCAM_CAMERATYPE)DllType)
				{
					m_SetCameraType		= (SETCAMERATYPE)	GetDelegate("ArtCam_SetCameraType",	typeof(SETCAMERATYPE));
					m_GetCameraType		= (GETCAMERATYPE)	GetDelegate("ArtCam_GetCameraType",	typeof(GETCAMERATYPE));

					if(	null == m_SetCameraType || null == m_GetCameraType){
						FreeLibrary();
						return false;
					}
				}

				// MOUT
				if (ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_130MI_MOUT == (ARTCAM_CAMERATYPE)DllType || ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_150P3_MOUT == (ARTCAM_CAMERATYPE)DllType ||
					ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_1000MI_HD2 == (ARTCAM_CAMERATYPE)DllType || ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_150P5_HD2 == (ARTCAM_CAMERATYPE)DllType || ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_130MI_HD2 == (ARTCAM_CAMERATYPE)DllType)
				{
					m_Fpga_WriteRegister		= (FPGA_WRITEREGISTER)	GetDelegate("ArtCam_Fpga_WriteRegister",	typeof(FPGA_WRITEREGISTER)	);
					m_Fpga_ReadRegister			= (FPGA_READREGISTER)	GetDelegate("ArtCam_Fpga_ReadRegister",		typeof(FPGA_READREGISTER)	);

					m_Monitor_SetPreviewMode	= (MONITOR_SETPREVIEWMODE)		GetDelegate("ArtCam_Monitor_SetPreviewMode",	typeof(MONITOR_SETPREVIEWMODE)		);
					m_Monitor_GetPreviewMode	= (MONITOR_GETPREVIEWMODE)		GetDelegate("ArtCam_Monitor_GetPreviewMode",	typeof(MONITOR_GETPREVIEWMODE)		);
					m_Monitor_SetPreviewSize	= (MONITOR_SETPREVIEWSIZE)		GetDelegate("ArtCam_Monitor_SetPreviewSize",	typeof(MONITOR_SETPREVIEWSIZE)		);
					m_Monitor_GetPreviewSize	= (MONITOR_GETPREVIEWSIZE)		GetDelegate("ArtCam_Monitor_GetPreviewSize",	typeof(MONITOR_GETPREVIEWSIZE)		);
					m_Monitor_SetColorMode		= (MONITOR_SETCOLORMODE)		GetDelegate("ArtCam_Monitor_SetColorMode",		typeof(MONITOR_SETCOLORMODE)		);
					m_Monitor_GetColorMode		= (MONITOR_GETCOLORMODE)		GetDelegate("ArtCam_Monitor_GetColorMode",		typeof(MONITOR_GETCOLORMODE)		);
					m_Monitor_SetCameraClock	= (MONITOR_SETCAMERACLOCK)		GetDelegate("ArtCam_Monitor_SetCameraClock",	typeof(MONITOR_SETCAMERACLOCK)		);
					m_Monitor_GetCameraClock	= (MONITOR_GETCAMERACLOCK)		GetDelegate("ArtCam_Monitor_GetCameraClock",	typeof(MONITOR_GETCAMERACLOCK)		);
					m_Monitor_SetBayerGainAuto	= (MONITOR_SETBAYERGAINAUTO)	GetDelegate("ArtCam_Monitor_SetBayerGainAuto",	typeof(MONITOR_SETBAYERGAINAUTO)	);
					m_Monitor_GetBayerGainAuto	= (MONITOR_GETBAYERGAINAUTO)	GetDelegate("ArtCam_Monitor_GetBayerGainAuto",	typeof(MONITOR_GETBAYERGAINAUTO)	);
					m_Monitor_SetBayerGainLock	= (MONITOR_SETBAYERGAINLOCK)	GetDelegate("ArtCam_Monitor_SetBayerGainLock",	typeof(MONITOR_SETBAYERGAINLOCK)	);
					m_Monitor_GetBayerGainLock	= (MONITOR_GETBAYERGAINLOCK)	GetDelegate("ArtCam_Monitor_GetBayerGainLock",	typeof(MONITOR_GETBAYERGAINLOCK)	);
					m_Monitor_SetBayerGainRed	= (MONITOR_SETBAYERGAINRED)		GetDelegate("ArtCam_Monitor_SetBayerGainRed",	typeof(MONITOR_SETBAYERGAINRED)		);
					m_Monitor_GetBayerGainRed	= (MONITOR_GETBAYERGAINRED)		GetDelegate("ArtCam_Monitor_GetBayerGainRed",	typeof(MONITOR_GETBAYERGAINRED)		);
					m_Monitor_SetBayerGainGreen	= (MONITOR_SETBAYERGAINGREEN)	GetDelegate("ArtCam_Monitor_SetBayerGainGreen",	typeof(MONITOR_SETBAYERGAINGREEN)	);
					m_Monitor_GetBayerGainGreen	= (MONITOR_GETBAYERGAINGREEN)	GetDelegate("ArtCam_Monitor_GetBayerGainGreen",	typeof(MONITOR_GETBAYERGAINGREEN)	);
					m_Monitor_SetBayerGainBlue	= (MONITOR_SETBAYERGAINBLUE)	GetDelegate("ArtCam_Monitor_SetBayerGainBlue",	typeof(MONITOR_SETBAYERGAINBLUE)	);
					m_Monitor_GetBayerGainBlue	= (MONITOR_GETBAYERGAINBLUE)	GetDelegate("ArtCam_Monitor_GetBayerGainBlue",	typeof(MONITOR_GETBAYERGAINBLUE)	);


					if(	null == m_Fpga_WriteRegister || null == m_Fpga_ReadRegister || 
						null == m_Monitor_SetPreviewMode || null == m_Monitor_GetPreviewMode || null == m_Monitor_SetPreviewSize || null == m_Monitor_GetPreviewSize || 
						null == m_Monitor_SetColorMode || null == m_Monitor_GetColorMode || null == m_Monitor_SetCameraClock || null == m_Monitor_GetCameraClock || 
						null == m_Monitor_SetBayerGainAuto || null == m_Monitor_GetBayerGainAuto || null == m_Monitor_SetBayerGainRed || null == m_Monitor_GetBayerGainRed || 
						null == m_Monitor_SetBayerGainGreen || null == m_Monitor_GetBayerGainGreen || null == m_Monitor_SetBayerGainBlue || null == m_Monitor_GetBayerGainBlue
						)
					{
						FreeLibrary();
						return false;
					}
				}

			}
			
			if(1282 <= Version){
				// SATA
				if (ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_SATA == (ARTCAM_CAMERATYPE)DllType)
				{
					m_SetSyncV			= (SETSYNCV)		GetDelegate("ArtCam_SetSyncV",	typeof(SETSYNCV));
					m_GetSyncV			= (GETSYNCV)		GetDelegate("ArtCam_GetSyncV",	typeof(GETSYNCV));
					m_SetSyncH			= (SETSYNCH)		GetDelegate("ArtCam_SetSyncH",	typeof(SETSYNCH));
					m_GetSyncH			= (GETSYNCH)		GetDelegate("ArtCam_GetSyncH",	typeof(GETSYNCH));

					if(	null == m_SetSyncV || null == m_GetSyncV || null == m_SetSyncH || null == m_GetSyncH){
						FreeLibrary();
						return false;
					}
				}

				// MOUT
				if (ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_130MI_MOUT == (ARTCAM_CAMERATYPE)DllType || ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_150P3_MOUT == (ARTCAM_CAMERATYPE)DllType || 
					ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_1000MI_HD2 == (ARTCAM_CAMERATYPE)DllType || ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_150P5_HD2 == (ARTCAM_CAMERATYPE)DllType || ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_130MI_HD2 == (ARTCAM_CAMERATYPE)DllType)
				{
					m_Monitor_SaveCurrentSettings	 =	(MONITOR_SAVECURRENTSETTINGS)		GetDelegate("ArtCam_Monitor_SaveCurrentSettings",	typeof(MONITOR_SAVECURRENTSETTINGS)	);
					m_Monitor_InitRegisterSettings	 =	(MONITOR_INITREGISTERSETTINGS)		GetDelegate("ArtCam_Monitor_InitRegisterSettings",	typeof(MONITOR_INITREGISTERSETTINGS)	);
					m_Monitor_SetCrossCursorMode	 = 	(MONITOR_SETCROSSCURSORMODE)		GetDelegate("ArtCam_Monitor_SetCrossCursorMode",	typeof(MONITOR_SETCROSSCURSORMODE)	);
					m_Monitor_GetCrossCursorMode	 = 	(MONITOR_GETCROSSCURSORMODE)		GetDelegate("ArtCam_Monitor_GetCrossCursorMode",	typeof(MONITOR_GETCROSSCURSORMODE)	);
					m_Monitor_SetCrossCursorColorR	 = 	(MONITOR_SETCROSSCURSORCOLORR)		GetDelegate("ArtCam_Monitor_SetCrossCursorColorR",	typeof(MONITOR_SETCROSSCURSORCOLORR)	);
					m_Monitor_GetCrossCursorColorR	 = 	(MONITOR_GETCROSSCURSORCOLORR)		GetDelegate("ArtCam_Monitor_GetCrossCursorColorR",	typeof(MONITOR_GETCROSSCURSORCOLORR)	);
					m_Monitor_SetCrossCursorColorG	 = 	(MONITOR_SETCROSSCURSORCOLORG)		GetDelegate("ArtCam_Monitor_SetCrossCursorColorG",	typeof(MONITOR_SETCROSSCURSORCOLORG)	);
					m_Monitor_GetCrossCursorColorG	 = 	(MONITOR_GETCROSSCURSORCOLORG)		GetDelegate("ArtCam_Monitor_GetCrossCursorColorG",	typeof(MONITOR_GETCROSSCURSORCOLORG)	);
					m_Monitor_SetCrossCursorColorB	 = 	(MONITOR_SETCROSSCURSORCOLORB)		GetDelegate("ArtCam_Monitor_SetCrossCursorColorB",	typeof(MONITOR_SETCROSSCURSORCOLORB)	);
					m_Monitor_GetCrossCursorColorB	 = 	(MONITOR_GETCROSSCURSORCOLORB)		GetDelegate("ArtCam_Monitor_GetCrossCursorColorB",	typeof(MONITOR_GETCROSSCURSORCOLORB)	);
					m_Monitor_SetCrossCursorColorRGB = 	(MONITOR_SETCROSSCURSORCOLORRGB)	GetDelegate("ArtCam_Monitor_SetCrossCursorColorRGB",typeof(MONITOR_SETCROSSCURSORCOLORRGB));
					m_Monitor_GetCrossCursorColorRGB = 	(MONITOR_GETCROSSCURSORCOLORRGB)	GetDelegate("ArtCam_Monitor_GetCrossCursorColorRGB",typeof(MONITOR_GETCROSSCURSORCOLORRGB));
					m_Monitor_SetCrossCursorPos		 = 	(MONITOR_SETCROSSCURSORPOS)			GetDelegate("ArtCam_Monitor_SetCrossCursorPos",		typeof(MONITOR_SETCROSSCURSORPOS)		);
					m_Monitor_GetCrossCursorPos		 = 	(MONITOR_GETCROSSCURSORPOS)			GetDelegate("ArtCam_Monitor_GetCrossCursorPos",		typeof(MONITOR_GETCROSSCURSORPOS)		);
					m_Monitor_SetCrossCursorSize	 = 	(MONITOR_SETCROSSCURSORSIZE)		GetDelegate("ArtCam_Monitor_SetCrossCursorSize",	typeof(MONITOR_SETCROSSCURSORSIZE)	);	
					m_Monitor_GetCrossCursorSize	 = 	(MONITOR_GETCROSSCURSORSIZE)		GetDelegate("ArtCam_Monitor_GetCrossCursorSize",	typeof(MONITOR_GETCROSSCURSORSIZE)	);	
					m_Monitor_SetAutoIrisMode		 = 	(MONITOR_SETAUTOIRISMODE)			GetDelegate("ArtCam_Monitor_SetAutoIrisMode",		typeof(MONITOR_SETAUTOIRISMODE)		);	
					m_Monitor_GetAutoIrisMode		 = 	(MONITOR_GETAUTOIRISMODE)			GetDelegate("ArtCam_Monitor_GetAutoIrisMode",		typeof(MONITOR_GETAUTOIRISMODE)		);	
					m_Monitor_SetAutoIrisRange		 = 	(MONITOR_SETAUTOIRISRANGE)			GetDelegate("ArtCam_Monitor_SetAutoIrisRange",		typeof(MONITOR_SETAUTOIRISRANGE)		);	
					m_Monitor_GetAutoIrisRange		 = 	(MONITOR_GETAUTOIRISRANGE)			GetDelegate("ArtCam_Monitor_GetAutoIrisRange",		typeof(MONITOR_GETAUTOIRISRANGE)		);	
					m_Monitor_LoadFirmware			 =  (MONITOR_LOADFIRMWARE)				GetDelegate("ArtCam_Monitor_LoadFirmware",			typeof(MONITOR_LOADFIRMWARE)			);


					if(	null == m_Monitor_SaveCurrentSettings   || null == m_Monitor_InitRegisterSettings  || null == m_Monitor_SetCrossCursorMode  || null == m_Monitor_GetCrossCursorMode || 
						null == m_Monitor_SetCrossCursorColorR  || null == m_Monitor_GetCrossCursorColorR  || null == m_Monitor_SetCrossCursorColorG|| 
						null == m_Monitor_GetCrossCursorColorG  || null == m_Monitor_SetCrossCursorColorB  || null == m_Monitor_GetCrossCursorColorB|| 
						null == m_Monitor_SetCrossCursorColorRGB|| null == m_Monitor_GetCrossCursorColorRGB|| null == m_Monitor_SetCrossCursorPos   || 
						null == m_Monitor_GetCrossCursorPos	 || null == m_Monitor_SetCrossCursorSize    || null == m_Monitor_GetCrossCursorSize  || 
						null == m_Monitor_SetAutoIrisMode		 || null == m_Monitor_GetAutoIrisMode	     || 
						null == m_Monitor_SetAutoIrisRange		 || null == m_Monitor_GetAutoIrisRange		 || null == m_Monitor_LoadFirmware
						)
					{
						FreeLibrary();
						return false;
					}
				}

			}
			
			
			// Functions available with V1.300 or later
			if(1300 <= Version){
				if (ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_CNV != (ARTCAM_CAMERATYPE)DllType)
				{
					m_GetRealExposureTime	= (GETREALEXPOSURETIME)	GetDelegate("ArtCam_GetRealExposureTime", typeof(GETREALEXPOSURETIME));
					if(null == m_GetRealExposureTime){
						FreeLibrary();
						return false;
					}
				}
				if(ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_031TNIR == (ARTCAM_CAMERATYPE)DllType ||
					 ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_008TNIR == (ARTCAM_CAMERATYPE)DllType)
				{
					m_SaveMaskFile			= (SAVEMASKFILE)	GetDelegate("ArtCam_SaveMaskFile",		typeof(SAVEMASKFILE));
					m_LoadMaskFile			= (LOADMASKFILE)	GetDelegate("ArtCam_LoadMaskFile",		typeof(LOADMASKFILE));
					m_UpdateMaskData		= (UPDATEMASKDATA)	GetDelegate("ArtCam_UpdateMaskData",	typeof(UPDATEMASKDATA));
                                                                                                               
					m_SetPeltier			= (SETPELTIER)		GetDelegate("ArtCam_SetPeltier",		typeof(SETPELTIER));
					m_GetPeltier			= (GETPELTIER)		GetDelegate("ArtCam_GetPeltier",		typeof(GETPELTIER));
					m_GetTemperature		= (GETTEMPERATURE)	GetDelegate("ArtCam_GetTemperature",	typeof(GETTEMPERATURE));
					m_SetDotFilter			= (SETDOTFILTER)	GetDelegate("ArtCam_SetDotFilter",		typeof(SETDOTFILTER));
					m_GetDotFilter			= (GETDOTFILTER)	GetDelegate("ArtCam_GetDotFilter",		typeof(GETDOTFILTER));
					m_SetMaskFilter			= (SETMASKFILTER)	GetDelegate("ArtCam_SetMaskFilter",		typeof(SETMASKFILTER));
					m_GetMaskFilter			= (GETMASKFILTER)	GetDelegate("ArtCam_GetMaskFilter",		typeof(GETMASKFILTER));


					if(null == m_SaveMaskFile || null == m_LoadMaskFile   || null == m_UpdateMaskData || null == m_SetPeltier  || 
						null == m_GetPeltier  || null == m_GetTemperature || null == m_SetDotFilter   || null == m_GetDotFilter||
						null == m_SetMaskFilter || null == m_GetMaskFilter){
						FreeLibrary();
						return false;
					}
				}

			}
			
			if(1310 <= Version){
				if (ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_CNV != (ARTCAM_CAMERATYPE)DllType)
				{
					m_SetRealExposureTime	= (SETREALEXPOSURETIME)	GetDelegate("ArtCam_SetRealExposureTime", typeof(SETREALEXPOSURETIME));
					if(null == m_SetRealExposureTime){
						FreeLibrary();
						return false;
					}
				}
			}
			if(1311 <= Version){
				if (ARTCAM_CAMERATYPE.ARTCAM_CAMERATYPE_130XQE_WOM == (ARTCAM_CAMERATYPE)DllType){
#if UNICODE
					m_LoadConfigFile		= (LOADCONFIGFILE)GetDelegate("ArtCam_LoadConfigFileW", typeof(LOADCONFIGFILE));
#else
					m_LoadConfigFile = (LOADCONFIGFILE)GetDelegate("ArtCam_LoadConfigFileA", typeof(LOADCONFIGFILE));
#endif
					m_SetConfigFilter = (SETCONFIGFILTER)GetDelegate("ArtCam_SetConfigFilter", typeof(SETCONFIGFILTER));
					m_GetConfigFilter = (GETCONFIGFILTER)GetDelegate("ArtCam_GetConfigFilter", typeof(GETCONFIGFILTER));

					if (null == m_LoadConfigFile || null == m_SetConfigFilter || null == m_GetConfigFilter)
					{
						FreeLibrary();
						return false;
					}
				}
			}
			return true;
		}
		
		public void FreeLibrary()
		{
			if((0!=m_hACam) && (null!=m_Release))
				m_Release(m_hACam);

			if(IsValid(m_hArtCamSdk))
				Win32FreeLibrary(m_hArtCamSdk);

			NullSet();
		}

		public long GetDllVersion()
		{
			if(null!=m_GetDllVersion){
				return m_GetDllVersion();
			}
			return 0;
		}

		public ARTCAM_CAMERATYPE GetDllType ()
		{
			return (ARTCAM_CAMERATYPE)(GetDllVersion() >> 16);
		}

		public int GetLastError()
		{
			if(null != m_GetLastError){
				return m_GetLastError(m_hACam);
			}
			return 0;
		}

		public bool Initialize(IntPtr hWnd)
		{
			if(null!=m_Initialize){
				m_hACam = m_Initialize(hWnd);
				if(0 != m_hACam)
					return true;
			}
			return false;
		}

		public int Release()
		{
			if(null!=m_Release){
				return m_Release(m_hACam);
			}
			return 0;
		}

		public int Preview()
		{
			if(null!=m_Preview){
				return m_Preview(m_hACam);
			}
			return 0;
		}

		public int Record(string lpAviName, int RecTime, int fShow)
		{
			if(null!=m_Record){
				return m_Record(m_hACam, lpAviName, RecTime, fShow);
			}
			return 0;
		}

		public int CallBackPreview(IntPtr hWnd, byte[] lpImage, int Size, int TopDown)
		{
			if(null!=m_CallBackPreviewA){
				return m_CallBackPreviewA(m_hACam, hWnd, lpImage, Size, TopDown);
			}
			return 0;
		}

		public int CallBackPreview(IntPtr hWnd, IntPtr lpImage, int Size, int TopDown)
		{
			if(null!=m_CallBackPreviewP){
				return m_CallBackPreviewP(m_hACam, hWnd, lpImage, Size, TopDown);
			}
			return 0;
		}

		public int SnapShot(byte[] lpImage, int Size, int TopDown)
		{
			if(null!=m_SnapShotA){
				return m_SnapShotA(m_hACam, lpImage, Size, TopDown);
			}
			return 0;
		}

		public int SnapShot(IntPtr lpImage, int Size, int TopDown)
		{
			if(null!=m_SnapShotP){
				return m_SnapShotP(m_hACam, lpImage, Size, TopDown);
			}
			return 0;
		}

		public int Close()
		{
			if(null!=m_Close){
				return m_Close(m_hACam);
			}
			return 0;
		}


		// 1250
		public int Capture()
		{
			if(null!=m_Capture){
				return m_Capture(m_hACam);
			}
			return 0;
		}


		// 1260
		public int Trigger(IntPtr hWnd, byte[] lpImage, int Size, int TopDown)
		{
			if(null!=m_TriggerA){
				return m_TriggerA(m_hACam, hWnd, lpImage, Size, TopDown);
			}
			return 0;
		}

		public int Trigger(IntPtr hWnd, IntPtr lpImage, int Size, int TopDown)
		{
			if(null!=m_TriggerP){
				return m_TriggerP(m_hACam, hWnd, lpImage, Size, TopDown);
			}
			return 0;
		}

		public int SaveImage(string  lpSaveName, FILETYPE FileType)
		{
			if(null!=m_SaveImage){
				return m_SaveImage(m_hACam, lpSaveName, FileType);
			}
			return 0;
		}


		public int SetPreviewWindow (IntPtr hWnd, int Left, int Top, int Right, int Bottom)
		{
			if(null!=m_SetPreviewWindow){
				return m_SetPreviewWindow(m_hACam, hWnd, Left, Top, Right, Bottom);
			}
			return 0;
		}

		public int SetCaptureWindow (int Width, int Height, int Frame)
		{
			if(null!=m_SetCaptureWindow){
				return m_SetCaptureWindow(m_hACam, Width, Height, Frame);
			}
			return 0;
		}

		public int SetCaptureWindowEx(int HTotal, int HStart, int HEffective, int VTotal, int VStart, int VEffective)
		{
			if(null!=m_SetCaptureWindowEx){
				return m_SetCaptureWindowEx(m_hACam, HTotal, HStart, HEffective, VTotal, VStart, VEffective);
			}
			return 0;
		}

		public int GetCaptureWindowEx(out int HTotal, out int HStart, out int HEffective, out int VTotal, out int VStart, out int VEffective)
		{
			if(null!=m_GetCaptureWindowEx){
				return m_GetCaptureWindowEx(m_hACam, out HTotal, out HStart, out HEffective, out VTotal, out VStart, out VEffective);
			}
			HTotal = 0;	HStart = 0;	HEffective = 0;
			VTotal = 0; VStart = 0; VEffective = 0;
			return 0;
		}


		public int SetColorMode (int ColorMode)
		{
			if(null!=m_SetColorMode){
				return m_SetColorMode(m_hACam, ColorMode);
			}
			return 0;
		}

		public int GetColorMode ()
		{
			if(null!=m_GetColorMode){
				return m_GetColorMode(m_hACam);
			}
			return 0;
		}


		public int SetCrossbar (int Output, int Input)
		{
			if(null!=m_SetCrossbar){
				return m_SetCrossbar(m_hACam, Output, Input);
			}
			return 0;
		}

		public int SetDeviceNumber (int Number)
		{
			if(null!=m_SetDeviceNumber){
				return m_SetDeviceNumber(m_hACam, Number);
			}
			return 0;
		}

		public int EnumDevice(StringBuilder[] szDeviceName)
		{
			if (null != m_EnumDevice)
			{
				return m_EnumDevice(m_hACam, szDeviceName);
			}
			return 0;
		}

		public int GetDeviceName (int index, StringBuilder szDeviceName, int nSize)
		{
			if(null!=m_GetDeviceName){
				return m_GetDeviceName(m_hACam, index, szDeviceName, nSize);
			}
			return 0;
		}


		public int StartPreview ()
		{
			if(null!=m_StartPreview){
				return m_StartPreview(m_hACam);
			}
			return 0;
		}

		public int StopPreview ()
		{
			if(null!=m_StopPreview){
				return m_StopPreview(m_hACam);
			}
			return 0;
		}

		public int GetImage (byte[] lpImage, int Size, int TopDown)
		{
			if(null!=m_GetImageA){
				return m_GetImageA(m_hACam, lpImage, Size, TopDown);
			}
			return 0;
		}

		public int GetImage (IntPtr lpImage, int Size, int TopDown)
		{
			if(null!=m_GetImageP){
				return m_GetImageP(m_hACam, lpImage, Size, TopDown);
			}
			return 0;
		}

		public int Width ()
		{
			if(null!=m_Width){
				return m_Width(m_hACam);
			}
			return 0;
		}

		public int Height ()
		{
			if(null!=m_Height){
				return m_Height(m_hACam);
			}
			return 0;
		}

		public int Fps ()
		{
			if(null!=m_Fps){
				return m_Fps(m_hACam);
			}
			return 0;
		}

		public int SetCameraDlg (IntPtr hWnd)
		{
			if(null!=m_SetCameraDlg){
				return m_SetCameraDlg(m_hACam, hWnd);
			}
			return 0;
		}

		public int SetImageDlg (IntPtr hWnd)
		{
			if(null!=m_SetImageDlg){
				return m_SetImageDlg(m_hACam, hWnd);
			}
			return 0;
		}

		public int SetAnalogDlg (IntPtr hWnd)
		{
			if(null!=m_SetAnalogDlg){
				return m_SetAnalogDlg(m_hACam, hWnd);
			}
			return 0;
		}


		// 1230
		public int SetIOPort (byte byteData, int longData, int Reserve)
		{
			if(null!=m_SetIOPort){
				return m_SetIOPort(m_hACam, byteData, longData, Reserve);
			}
			return 0;
		}

		public bool GetIOPort(out byte byteData, out int longData, int Reserve)
		{
			if(null!=m_GetIOPort){
				if(0 != m_GetIOPort(m_hACam, out byteData, out longData, Reserve)){
					return true;
				}
			}
			byteData = 0;
			longData = 0;
			return false;
		}


		public int SetFilterValue (ARTCAM_FILTERTYPE FilterType, int Value)
		{
			if(null!=m_SetFilterValue){
				return m_SetFilterValue(m_hACam, FilterType, Value);
			}
			return 0;
		}

		public int GetFilterValue (ARTCAM_FILTERTYPE FilterType)
		{
			if(null!=m_GetFilterValue){
				return m_GetFilterValue(m_hACam, FilterType, out m_Error);
			}
			return 0;
		}


		public int SetSubSample (SUBSAMPLE SubSampleMode)
		{
			if(null!=m_SetSubSample){
				return m_SetSubSample(m_hACam, SubSampleMode);
			}
			return 0;
		}

		public SUBSAMPLE GetSubSample ()
		{
			if(null!=m_GetSubSample){
				return (SUBSAMPLE)m_GetSubSample(m_hACam);
			}
			return 0;
		}


		public int SetWaitTime (int WaitTime)
		{
			if(null!=m_SetWaitTime){
				return m_SetWaitTime(m_hACam, WaitTime);
			}
			return 0;
		}

		public int GetWaitTime ()
		{
			if(null!=m_GetWaitTime){
				return m_GetWaitTime(m_hACam);
			}
			return 0;
		}


		// 1240
		public int SetMirrorV (bool Flg)
		{
			if(null!=m_SetMirrorV){
				if(Flg){
					return m_SetMirrorV(m_hACam, 1);
				}else{
					return m_SetMirrorV(m_hACam, 0);
				}
			}
			return 0;
		}

		public bool GetMirrorV ()
		{
			if(null!=m_GetMirrorV){
				if(0 != m_GetMirrorV(m_hACam)){
					return true;
				}
			}
			return false;
		}


		public int SetMirrorH(bool Flg)
		{
			if(null!=m_SetMirrorH){
				if(Flg){
					return m_SetMirrorH(m_hACam, 1);
				}else{
					return m_SetMirrorH(m_hACam, 0);
				}
			}
			return 0;
		}

		public bool GetMirrorH()
		{
			if(null!=m_GetMirrorH){
				if(0 != m_GetMirrorH(m_hACam)){
					return true;
				}
			}
			return false;
		}


		public int SetBrightness (int Value)
		{
			if(null!=m_SetBrightness){
				return m_SetBrightness(m_hACam, Value);
			}
			return 0;
		}

		public int GetBrightness ()
		{
			if(null!=m_GetBrightness){
				return m_GetBrightness(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetContrast (int Value)
		{
			if(null!=m_SetContrast){
				return m_SetContrast(m_hACam, Value);
			}
			return 0;
		}

		public int GetContrast ()
		{
			if(null!=m_GetContrast){
				return m_GetContrast(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetHue (int Value)
		{
			if(null!=m_SetHue){
				return m_SetHue(m_hACam, Value);
			}
			return 0;
		}

		public int GetHue ()
		{
			if(null!=m_GetHue){
				return m_GetHue(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetSaturation (int Value)
		{
			if(null!=m_SetSaturation){
				return m_SetSaturation(m_hACam, Value);
			}
			return 0;
		}

		public int GetSaturation ()
		{
			if(null!=m_GetSaturation){
				return m_GetSaturation(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetSharpness (int Value)
		{
			if(null!=m_SetSharpness){
				return m_SetSharpness(m_hACam, Value);
			}
			return 0;
		}

		public int GetSharpness ()
		{
			if(null!=m_GetSharpness){
				return m_GetSharpness(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetBayerGainRGB (int Value)
		{
			if(null!=m_SetBayerGainRGB){
				return m_SetBayerGainRGB(m_hACam, Value);
			}
			return 0;
		}

		public int GetBayerGainRGB ()
		{
			if(null!=m_GetBayerGainRGB){
				return m_GetBayerGainRGB(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetBayerGainRed (int Value)
		{
			if(null!=m_SetBayerGainRed){
				return m_SetBayerGainRed(m_hACam, Value);
			}
			return 0;
		}

		public int GetBayerGainRed ()
		{
			if(null!=m_GetBayerGainRed){
				return m_GetBayerGainRed(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetBayerGainGreen (int Value)
		{
			if(null!=m_SetBayerGainGreen){
				return m_SetBayerGainGreen(m_hACam, Value);
			}
			return 0;
		}

		public int GetBayerGainGreen ()
		{
			if(null!=m_GetBayerGainGreen){
				return m_GetBayerGainGreen(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetBayerGainBlue (int Value)
		{
			if(null!=m_SetBayerGainBlue){
				return m_SetBayerGainBlue(m_hACam, Value);
			}
			return 0;
		}

		public int GetBayerGainBlue ()
		{
			if(null!=m_GetBayerGainBlue){
				return m_GetBayerGainBlue(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetBayerGainAuto (bool Flg)
		{
			if(null!= m_SetBayerGainAuto){
				if(Flg){
					return m_SetBayerGainAuto(m_hACam, 1);
				}else{
					return m_SetBayerGainAuto(m_hACam, 0);
				}
			}
			return 0;
		}

		public int GetBayerGainAuto ()
		{
			if(null!= m_GetBayerGainAuto){
				return m_GetBayerGainAuto(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetGamma (int Value)
		{
			if(null!= m_SetGamma){
				return m_SetGamma(m_hACam, Value);
			}
			return 0;
		}

		public int GetGamma()
		{
			if(null!= m_GetGamma){
				return m_GetGamma(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetBayerMode(int Value)
		{
			if(null!= m_SetBayerMode){
				return m_SetBayerMode(m_hACam, Value);
			}
			return 0;
		}

		public int GetBayerMode()
		{
			if(null!= m_GetBayerMode){
				return m_GetBayerMode(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetGlobalGain(int Value)
		{
			if(null!= m_SetGlobalGain){
				return m_SetGlobalGain(m_hACam, Value);
			}
			return 0;
		}

		public int GetGlobalGain()
		{
			if(null!= m_GetGlobalGain){
				return m_GetGlobalGain(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetColorGainRed(int Value)
		{
			if(null!= m_SetColorGainRed){
				return m_SetColorGainRed(m_hACam, Value);
			}
			return 0;
		}

		public int GetColorGainRed()
		{
			if(null!= m_GetColorGainRed){
				return m_GetColorGainRed(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetColorGainGreen1(int Value)
		{
			if(null!= m_SetColorGainGreen1){
				return m_SetColorGainGreen1(m_hACam, Value);
			}
			return 0;
		}

		public int GetColorGainGreen1()
		{
			if(null!= m_GetColorGainGreen1){
				return m_GetColorGainGreen1(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetColorGainGreen2(int Value)
		{
			if(null!= m_SetColorGainGreen2){
				return m_SetColorGainGreen2(m_hACam, Value);
			}
			return 0;
		}

		public int GetColorGainGreen2()
		{
			if(null!= m_GetColorGainGreen2){
				return m_GetColorGainGreen2(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetColorGainBlue(int Value)
		{
			if(null!= m_SetColorGainBlue){
				return m_SetColorGainBlue(m_hACam, Value);
			}
			return 0;
		}

		public int GetColorGainBlue()
		{
			if(null!= m_GetColorGainBlue){
				return m_GetColorGainBlue(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetExposureTime(int Value)
		{
			if(null!= m_SetExposureTime){
				return m_SetExposureTime(m_hACam, Value);
			}
			return 0;
		}

		public int GetExposureTime()
		{
			if(null!= m_GetExposureTime){
				return m_GetExposureTime(m_hACam, out m_Error);
			}
			return 0;
		}


		// 1260
		public int SetHalfClock(int Value)
		{
			if(null!= m_SetHalfClock){
				return m_SetHalfClock(m_hACam, Value);
			}
			return 0;
		}

		public int GetHalfClock()
		{
			if(null!= m_GetHalfClock){
				return m_GetHalfClock(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SetAutoIris(AI_TYPE Value)
		{
			if(null!= m_SetAutoIris){
				return m_SetAutoIris(m_hACam, Value);
			}
			return 0;
		}

		public AI_TYPE GetAutoIris()
		{
			if(null!= m_GetAutoIris){
				return(AI_TYPE)m_GetAutoIris(m_hACam, out m_Error);
			}
			return 0;
		}


		// 1275
		public int SetSamplingRate(SAMPLING_RATE Value)
		{
			if(null!= m_SetSamplingRate){
				return m_SetSamplingRate(m_hACam, Value);
			}
			return 0;
		}

		public SAMPLING_RATE GetSamplingRate()
		{
			if(null!= m_GetSamplingRate){
				return(SAMPLING_RATE)m_GetSamplingRate(m_hACam, out m_Error);
			}
			return 0;
		}

		public VIDEOFORMAT GetVideoFormat()
		{
			if(null!= m_GetVideoFormat){
				return(VIDEOFORMAT)m_GetVideoFormat(m_hACam, out m_Error);
			}
			return 0;
		}


		// 1276
		public int WriteSromID(int Address, int Value)
		{
			if(null!= m_WriteSromID){
				return m_WriteSromID(m_hACam, Address, Value);
			}
			return 0;
		}

		public int ReadSromID(int Address)
		{
			if(null!= m_ReadSromID){
				return m_ReadSromID(m_hACam, Address, out m_Error);
			}
			return 0;
		}

		
		public int GetCameraInfo(ref CAMERAINFO Info)
		{
			if(null!= m_GetCameraInfo){
				return m_GetCameraInfo(m_hACam, ref Info);
			}
			return 0;
		}


		public int GetStatus()
		{
			if(null!= m_GetStatus){
				return m_GetStatus(m_hACam, out m_Error);
			}
			return 0;
		}


		public int WriteRegister(int Address, int Value)
		{
			if(null!= m_WriteRegister){
				return m_WriteRegister(m_hACam, Address, Value);
			}
			return 0;
		}

		public int ReadRegister(int Address)
		{
			if(null!= m_ReadRegister){
				return m_ReadRegister(m_hACam, Address, out m_Error);
			}
			return 0;
		}		


		
		public int SetGrayMode(int Value)
		{
			if(null!= m_SetGrayMode){
				return m_SetGrayMode(m_hACam,Value);
			}
			return 0;
		}
		
		public int GetGrayMode()
		{
			if(null!= m_GetGrayMode){
				return m_GetGrayMode(m_hACam,out m_Error);
			}
			return 0;
		}

		public int SetGrayGainR(int Value)
		{
			if(null!= m_SetGrayGainR){
				return m_SetGrayGainR(m_hACam,Value);
			}
			return 0;
		}
		
		public int GetGrayGainR()
		{
			if(null!= m_GetGrayGainR){
				return m_GetGrayGainR(m_hACam,out m_Error);
			}
			return 0;
		}

		public int SetGrayGainG1(int Value)
		{
			if(null!= m_SetGrayGainG1){
				return m_SetGrayGainG1(m_hACam,Value);
			}
			return 0;
		}
		
		public int GetGrayGainG1()
		{
			if(null!= m_GetGrayGainG1){
				return m_GetGrayGainG1(m_hACam,out m_Error);
			}
			return 0;
		}

		public int SetGrayGainG2(int Value)
		{
			if(null!= m_SetGrayGainG2){
				return m_SetGrayGainG2(m_hACam,Value);
			}
			return 0;
		}
		
		public int GetGrayGainG2()
		{
			if(null!= m_GetGrayGainG2){
				return m_GetGrayGainG2(m_hACam,out m_Error);
			}
			return 0;
		}

		public int SetGrayGainB(int Value)
		{
			if(null!= m_SetGrayGainB){
				return m_SetGrayGainB(m_hACam,Value);
			}
			return 0;
		}
		
		public int GetGrayGainB()
		{
			if(null!= m_GetGrayGainB){
				return m_GetGrayGainB(m_hACam,out m_Error);
			}
			return 0;
		}

		public int SetGrayOffsetR(int Value)
		{
			if(null!= m_SetGrayOffsetR){
				return m_SetGrayOffsetR(m_hACam,Value);
			}
			return 0;
		}
		
		public int GetGrayOffsetR()
		{
			if(null!= m_GetGrayOffsetR){
				return m_GetGrayOffsetR(m_hACam,out m_Error);
			}
			return 0;
		}

		public int SetGrayOffsetG1(int Value)
		{
			if(null!= m_SetGrayOffsetG1){
				return m_SetGrayOffsetG1(m_hACam,Value);
			}
			return 0;
		}
		
		public int GetGrayOffsetG1()
		{
			if(null!= m_GetGrayOffsetG1){
				return m_GetGrayOffsetG1(m_hACam,out m_Error);
			}
			return 0;
		}

		public int SetGrayOffsetG2(int Value)
		{
			if(null!= m_SetGrayOffsetG2){
				return m_SetGrayOffsetG2(m_hACam,Value);
			}
			return 0;
		}
		
		public int GetGrayOffsetG2()
		{
			if(null!= m_GetGrayOffsetG2){
				return m_GetGrayOffsetG2(m_hACam,out m_Error);
			}
			return 0;
		}

		public int SetGrayOffsetB(int Value)
		{
			if(null!= m_SetGrayOffsetB){
				return m_SetGrayOffsetB(m_hACam,Value);
			}
			return 0;
		}
		
		public int GetGrayOffsetB()
		{
			if(null!= m_GetGrayOffsetB){
				return m_GetGrayOffsetB(m_hACam,out m_Error);
			}
			return 0;
		}


		public int SetCameraType(int Flg)
		{
			if(null!= m_SetCameraType){
				return m_SetCameraType(m_hACam , Flg);
			}
			return 0;
		}

		public int GetCameraType()
		{
			if(null!= m_GetCameraType){
				return m_GetCameraType(m_hACam , out m_Error);
			}
			return 0;
		}
	
		public int Fpga_WriteRegister(int Address, int Value)
		{
			if(null!= m_Fpga_WriteRegister){
				return m_Fpga_WriteRegister(m_hACam, Address, Value);
			}
			return 0;
		}

		public int Fpga_ReadRegister(int Address)
		{
			if(null!= m_Fpga_ReadRegister){
				return m_Fpga_ReadRegister(m_hACam , Address,  out m_Error);
			}
			return 0;
		}
		
		public int Monitor_SetPreviewMode(int Preview)
		{
			if(null!= m_Monitor_SetPreviewMode){
				return m_Monitor_SetPreviewMode(m_hACam , Preview);
			}
			return 0;
		}

		public int Monitor_GetPreviewMode()
		{
			if(null!= m_Monitor_GetPreviewMode){
				return m_Monitor_GetPreviewMode(m_hACam,  out m_Error);
			}
			return 0;
		}

		
		public int Monitor_SetPreviewSize(int Width, int Height)
		{
			if(null!= m_Monitor_SetPreviewSize){
				return m_Monitor_SetPreviewSize(m_hACam,Width, Height);
			}
			return 0;
		}
        
		public int Monitor_GetPreviewSize(out int Width, out int Height)
		{
			if(null!= m_Monitor_GetPreviewSize){
				return m_Monitor_GetPreviewSize(m_hACam, out Width, out Height);
			}
			Width = 0;
			Height = 0;
			return 0;
		}
		
		public int Monitor_SetColorMode(int ColorMode)
		{
			if(null!= m_Monitor_SetColorMode){
				return m_Monitor_SetColorMode(m_hACam, ColorMode);
			}
			return 0;
		}

		public int Monitor_GetColorMode()
		{
			if(null!= m_Monitor_GetColorMode){
				return m_Monitor_GetColorMode(m_hACam, out m_Error);
			}
			return 0;
		}

		public int Monitor_SetCameraClock(int Value)
		{
			if(null!= m_Monitor_SetCameraClock){
				return m_Monitor_SetCameraClock(m_hACam, Value);
			}
			return 0;
		}

		public int Monitor_GetCameraClock()
		{
			if(null!= m_Monitor_GetCameraClock){
				return m_Monitor_GetCameraClock(m_hACam, out m_Error);
			}
			return 0;
		}

		public int Monitor_SetBayerGainAuto(int Value)
		{
			if(null!= m_Monitor_SetBayerGainAuto){
				return m_Monitor_SetBayerGainAuto(m_hACam,Value);
			}
			return 0;
		}

		public int Monitor_GetBayerGainAuto()
		{
			if(null!= m_Monitor_GetBayerGainAuto){
				return m_Monitor_GetBayerGainAuto(m_hACam, out m_Error);
			}
			return 0;
		}

		public int Monitor_SetBayerGainLock(int Value)
		{
			if(null!= m_Monitor_SetBayerGainLock){
				return m_Monitor_SetBayerGainLock(m_hACam,Value);
			}
			return 0;
		}

		public int Monitor_GetBayerGainLock()
		{
			if(null!= m_Monitor_GetBayerGainLock){
				return m_Monitor_GetBayerGainLock(m_hACam, out m_Error);
			}
			return 0;
		}
		
		public int Monitor_SetBayerGainRed(int Value)
		{
			if(null!= m_Monitor_SetBayerGainRed){
				return m_Monitor_SetBayerGainRed(m_hACam, Value);
			}
			return 0;
		}
		
		public int Monitor_GetBayerGainRed()
		{
			if(null!= m_Monitor_GetBayerGainRed){
				return m_Monitor_GetBayerGainRed(m_hACam, out m_Error);
			}
			return 0;
		}


		public int Monitor_SetBayerGainGreen(int Value)
		{
			if(null!= m_Monitor_SetBayerGainGreen){
				return m_Monitor_SetBayerGainGreen(m_hACam, Value);
			}
			return 0;
		}
		
		public int Monitor_GetBayerGainGreen()
		{
			if(null!= m_Monitor_GetBayerGainGreen){
				return m_Monitor_GetBayerGainGreen(m_hACam, out m_Error);
			}
			return 0;
		}

		
		public int Monitor_SetBayerGainBlue(int Value)
		{
			if(null!= m_Monitor_SetBayerGainBlue){
				return m_Monitor_SetBayerGainBlue(m_hACam, Value);
			}
			return 0;
		}
		
		public int Monitor_GetBayerGainBlue()
		{
			if(null!= m_Monitor_GetBayerGainBlue){
				return m_Monitor_GetBayerGainBlue(m_hACam, out m_Error);
			}
			return 0;
		}

		public int Monitor_SaveCurrentSettings()
		{	
			if(null!= m_Monitor_SaveCurrentSettings){
				return m_Monitor_SaveCurrentSettings(m_hACam);
			}
			return 0;
		}

		public int Monitor_InitRegisterSettings(int Value)
		{	
			if(null!= m_Monitor_InitRegisterSettings){
				return m_Monitor_InitRegisterSettings(m_hACam, Value);
			}
			return 0;
		}

		public int Monitor_SetCrossCursorMode(int CursorNum, int Value)		
		{	
			if(null!= m_Monitor_SetCrossCursorMode){
				return m_Monitor_SetCrossCursorMode(m_hACam, CursorNum, Value);
			}
			return 0;
		}

		public int Monitor_GetCrossCursorMode(int CursorNum)		
		{
			if(null!= m_Monitor_GetCrossCursorMode){
				return m_Monitor_GetCrossCursorMode(m_hACam, CursorNum, out m_Error);
			}
			return 0;
		}

		public int Monitor_SetCrossCursorColorR(int CursorNum, int Value)		
		{	
			if(null!= m_Monitor_SetCrossCursorColorR){
				return m_Monitor_SetCrossCursorColorR(m_hACam, CursorNum, Value);
			}
			return 0;
		}

		public int Monitor_GetCrossCursorColorR(int CursorNum)		
		{
			if(null!= m_Monitor_GetCrossCursorColorR){
				return m_Monitor_GetCrossCursorColorR(m_hACam, CursorNum, out m_Error);
			}
			return 0;
		}

		public int Monitor_SetCrossCursorColorG	(int CursorNum, int Value)		
		{	
			if(null!= m_Monitor_SetCrossCursorColorG){
				return m_Monitor_SetCrossCursorColorG(m_hACam, CursorNum, Value);
			}
			return 0;
		}

		public int Monitor_GetCrossCursorColorG(int CursorNum)
		{
			if(null!= m_Monitor_GetCrossCursorColorG){
				return m_Monitor_GetCrossCursorColorG(m_hACam, CursorNum, out m_Error);
			}
			return 0;
		}

		public int Monitor_SetCrossCursorColorB(int CursorNum, int Value)
		{	
			if(null!= m_Monitor_SetCrossCursorColorB){
				return m_Monitor_SetCrossCursorColorB(m_hACam, CursorNum, Value);
			}
			return 0;
		}

		public int Monitor_GetCrossCursorColorB(int CursorNum)
		{	
			if(null!= m_Monitor_GetCrossCursorColorB){
				return m_Monitor_GetCrossCursorColorB(m_hACam, CursorNum, out m_Error);
			}
			return 0;
		}
		public int Monitor_SetCrossCursorColorRGB(int CursorNum, int Value)
		{
			if(null!= m_Monitor_SetCrossCursorColorRGB){
				return m_Monitor_SetCrossCursorColorRGB(m_hACam, CursorNum, Value);
			}
			return 0;
		}

		public int Monitor_GetCrossCursorColorRGB(int CursorNum)
		{	
			if(null!= m_Monitor_GetCrossCursorColorRGB){
				return m_Monitor_GetCrossCursorColorRGB(m_hACam, CursorNum, out m_Error);
			}
			return 0;
		}

		public int Monitor_SetCrossCursorPos(int CursorNum, int PosX, int PosY)
		{	
			if(null!= m_Monitor_SetCrossCursorPos){
				return m_Monitor_SetCrossCursorPos(m_hACam, CursorNum, PosX, PosY);
			}
			return 0;
		}

		public int Monitor_GetCrossCursorPos(int CursorNum, out int PosX, out int PoxY)
		{	
			if(null!= m_Monitor_GetCrossCursorPos){
				return m_Monitor_GetCrossCursorPos(m_hACam, CursorNum, out PosX, out PoxY);
			}
			PosX = 0;
			PoxY = 0;
			return 0;
		}

		public int Monitor_SetCrossCursorSize(int CursorNum, int SizeX, int SizeY)	
		{	
			if(null!= m_Monitor_SetCrossCursorSize){
				return m_Monitor_SetCrossCursorSize(m_hACam, CursorNum, SizeX, SizeY);
			}
			return 0;
		}

		public int Monitor_GetCrossCursorSize(int CursorNum, out int SizeX, out int SizeY)	
		{
			if(null!= m_Monitor_GetCrossCursorSize){
				return m_Monitor_GetCrossCursorSize(m_hACam, CursorNum, out SizeX, out SizeY);
			}
			SizeX = 0;
			SizeY = 0;
			return 0;
		}

		public int Monitor_SetAutoIrisMode(int Value)
		{	
			if(null!= m_Monitor_SetAutoIrisMode){
				return m_Monitor_SetAutoIrisMode(m_hACam, Value);
			}
			return 0;
		}

		public int Monitor_GetAutoIrisMode()
		{	
			if(null!= m_Monitor_GetAutoIrisMode){
				return m_Monitor_GetAutoIrisMode(m_hACam, out m_Error);
			}
			return 0;
		}

		public int Monitor_SetAutoIrisRange(int Min, int Max)	
		{	
			if(null!= m_Monitor_SetAutoIrisRange){
				return m_Monitor_SetAutoIrisRange(m_hACam, Min, Max);
			}
			return 0;
		}

		public int Monitor_GetAutoIrisRange(out int Min, out int Max)
		{	
			if(null!= m_Monitor_GetAutoIrisRange){
				return m_Monitor_GetAutoIrisRange(m_hACam, out Min, out Max);
			}
			Min = 0;
			Max = 0;
			return 0;
		}

		public int Monitor_LoadFirmware(StringBuilder szFileName)
		{	
			if(null!= m_Monitor_LoadFirmware){
				return m_Monitor_LoadFirmware(m_hACam, szFileName);
			}
			return 0;
		}

		public int SetRealExposureTime(int Value)
		{	
			if(null!= m_SetRealExposureTime){
				return m_SetRealExposureTime(m_hACam, Value);
			}
			return 0;
		}

		public int GetRealExposureTime()
		{	
			if(null!= m_GetRealExposureTime){
				return m_GetRealExposureTime(m_hACam, out m_Error);
			}
			return 0;
		}


		public int SaveMaskFile(StringBuilder szFileName)
		{
			if(null!= m_SaveMaskFile){		
				return m_SaveMaskFile(m_hACam, szFileName);	
			}	
			return 0;
		}

		public int LoadMaskFile(StringBuilder szFileName)
		{
			if(null!= m_LoadMaskFile){
				return m_LoadMaskFile(m_hACam, szFileName);
			}
			return 0;
		}

		public int UpdateMaskData(MASKTYPE Flg)
		{
			if(null!= m_UpdateMaskData){
				return m_UpdateMaskData(m_hACam, Flg);
			}
			return 0;
		}

		public int SetPeltier(int Value)
		{
			if(null!= m_SetPeltier){
				return m_SetPeltier(m_hACam, Value);	
			} 
			return 0;
		}

		public int GetPeltier()
		{
			if(null!= m_GetPeltier){	
				return m_GetPeltier(m_hACam, out m_Error);	
			} 
			return 0;
		}

		public int GetTemperature()
		{		
			if(null!= m_GetTemperature){	
				return m_GetTemperature(m_hACam, out m_Error);	
			} 
			return 0;
		}

		public int SetDotFilter(int Value)
		{
			if(null!= m_SetDotFilter){	
				return m_SetDotFilter(m_hACam, Value);	
			}
			return 0;	
		}

		public int GetDotFilter()
		{		
			if(null!= m_GetDotFilter){	
				return m_GetDotFilter(m_hACam, out m_Error);	
			} 
			return 0;	
		}

		public int SetMaskFilter(int Value){		
			if(null!= m_SetMaskFilter){	
				return m_SetMaskFilter(m_hACam, Value);	
			} 
			return 0;	
		}

		public int GetMaskFilter()
		{		
			if(null!= m_GetMaskFilter){	
				return m_GetMaskFilter(m_hACam, out m_Error);	
			} 
			return 0;	
		}

		public int LoadConfigFile(StringBuilder szFileName)
		{		
			if(null!= m_LoadConfigFile){	
				return m_LoadConfigFile(m_hACam, szFileName);	
			} 
			return 0;	
		}

		public int SetConfigFilter(int Value)
		{		
			if(null!= m_SetConfigFilter){	
				return m_SetConfigFilter(m_hACam, Value);	
			} 
			return 0;	
		}

		public int GetConfigFilter()
		{		
			if(null!= m_GetConfigFilter){	
				return m_GetConfigFilter(m_hACam);	
			} 
			return 0;	
		}
	}
	
}
