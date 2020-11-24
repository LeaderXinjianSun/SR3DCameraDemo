using HalconDotNet;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using SR3DCameraDemo.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ViewROI;

namespace SR3DCameraDemo.ViewModel
{
    class MainWindowViewModel: NotificationObject
    {
        #region 属性绑定
        private string version;

        public string Version
        {
            get { return version; }
            set
            {
                version = value;
                this.RaisePropertyChanged("Version");
            }
        }
        private bool statusCamera;

        public bool StatusCamera
        {
            get { return statusCamera; }
            set
            {
                statusCamera = value;
                this.RaisePropertyChanged("StatusCamera");
            }
        }
        private bool statusPLC;

        public bool StatusPLC
        {
            get { return statusPLC; }
            set
            {
                statusPLC = value;
                this.RaisePropertyChanged("StatusPLC");
            }
        }

        private string messageStr;

        public string MessageStr
        {
            get { return messageStr; }
            set
            {
                messageStr = value;
                this.RaisePropertyChanged("MessageStr");
            }
        }
        private HImage cameraIamge;

        public HImage CameraIamge
        {
            get { return cameraIamge; }
            set
            {
                cameraIamge = value;
                this.RaisePropertyChanged("CameraIamge");
            }
        }
        private bool cameraRepaint;

        public bool CameraRepaint
        {
            get { return cameraRepaint; }
            set
            {
                cameraRepaint = value;
                this.RaisePropertyChanged("CameraRepaint");
            }
        }
        private ObservableCollection<ROI> cameraROIList;

        public ObservableCollection<ROI> CameraROIList
        {
            get { return cameraROIList; }
            set
            {
                cameraROIList = value;
                this.RaisePropertyChanged("CameraROIList");
            }
        }
        private HObject cameraAppendHObject;

        public HObject CameraAppendHObject
        {
            get { return cameraAppendHObject; }
            set
            {
                cameraAppendHObject = value;
                this.RaisePropertyChanged("CameraAppendHObject");
            }
        }
        private Tuple<string, object> cameraGCStyle;

        public Tuple<string, object> CameraGCStyle
        {
            get { return cameraGCStyle; }
            set
            {
                cameraGCStyle = value;
                this.RaisePropertyChanged("CameraGCStyle");
            }
        }
        #endregion
        #region 方法绑定
        public DelegateCommand<object> MenuActionCommand { get; set; }
        public DelegateCommand AppLoadedEventCommand { get; set; }
        public DelegateCommand<object> OperateButtonCommand { get; set; }
        #endregion
        #region 变量
        /*******接口函数返回值**********/
        int SR7IF_ERROR_NOT_FOUND = (-999);                  // Item is not found.
        int SR7IF_ERROR_COMMAND = (-998);                  // Command not recognized.
        int SR7IF_ERROR_PARAMETER = (-997);                  // Parameter is invalid.
        int SR7IF_ERROR_UNIMPLEMENTED = (-996);                  // Feature not implemented.
        int SR7IF_ERROR_HANDLE = (-995);                  // Handle is invalid.
        int SR7IF_ERROR_MEMORY = (-994);                  // Out of memory.
        int SR7IF_ERROR_TIMEOUT = (-993);                  // Action timed out.
        int SR7IF_ERROR_DATABUFFER = (-992);                  // Buffer not large enough for data.
        int SR7IF_ERROR_STREAM = (-991);                  // Error in stream.
        int SR7IF_ERROR_CLOSED = (-990);                  // Resource is no longer avaiable.
        int SR7IF_ERROR_VERSION = (-989);                  // Invalid version number.
        int SR7IF_ERROR_ABORT = (-988);                  // Operation aborted.
        int SR7IF_ERROR_ALREADY_EXISTS = (-987);                  // Conflicts with existing item.
        int SR7IF_ERROR_FRAME_LOSS = (-986);                  // Loss of frame.
        int SR7IF_ERROR_ROLL_DATA_OVERFLOW = (-985);                  // Continue mode Data overflow.
        int SR7IF_ERROR_ROLL_BUSY = (-984);                  // Read Busy.
        int SR7IF_ERROR_MODE = (-983);                  // Err mode.
        int SR7IF_ERROR_CAMERA_NOT_ONLINE = (-982);                  // Camera not online.
        int SR7IF_ERROR = (-1);                    // General error.
        int SR7IF_OK = (0);                     // Operation successful.
        int SR7IF_NORMAL_STOP = (-100);                  //A normal stop caused by external IO or other causes

        static int[] HeightData = null;        //高度数据缓存

        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        XinjePLCModbusRTU xinje;
        #endregion
        #region 构造函数
        public MainWindowViewModel()
        {
            #region 参数初始化
            Version = "20201118";
            MessageStr = "";
            CameraROIList = new ObservableCollection<ROI>();
            string com = Inifile.INIGetStringValue(iniParameterPath, "PLC", "COM", "COM1");
            xinje = new XinjePLCModbusRTU(com);
            xinje.PLC.StateChanged += PLC_StateChanged;
            xinje.PLC.Connect();
            StatusPLC = true;
            #endregion
            MenuActionCommand = new DelegateCommand<object>(new Action<object>(this.OperateButtonCommandExecute));
            AppLoadedEventCommand = new DelegateCommand(new Action(this.AppLoadedEventCommandExecute));
            OperateButtonCommand = new DelegateCommand<object>(new Action<object>(this.OperateButtonCommandExecute));
        }

        private void PLC_StateChanged(object sender, bool e)
        {
            StatusPLC = e;
        }

        private void AppLoadedEventCommandExecute()
        {
            #region 3D相机初始化
            SR7IF_ETHERNET_CONFIG _ethernetConfig;
            int _currentDeviceId = 0;
            _ethernetConfig.abyIpAddress = new Byte[] { 192, 168, 0, 10 };
            int Rc = SR7LinkFunc.SR7IF_EthernetOpen(_currentDeviceId, ref _ethernetConfig);
            if (Rc == 0)
            {
                AddMessage("3D相机连接成功");
                StatusCamera = true;
                //获取型号判断高度范围
                IntPtr str_Model = SR7LinkFunc.SR7IF_GetModels(_currentDeviceId);
                String s_model = Marshal.PtrToStringAnsi(str_Model);
                HeightData = new int[15000 * 6400];
                for (int i = 0; i < HeightData.Length; i++)
                {
                    HeightData[i] = -1000000;
                }
            }

                #endregion
            AddMessage("软件加载完成");
        }

        private void OperateButtonCommandExecute(object obj)
        {
            switch (obj.ToString())
            {
                case "0":
                    Task.Run(() =>
                    {
                        int _currentDeviceId = 0;
                        IntPtr DataObject = new IntPtr();
                        xinje.SetM("M500", true);
                        AddMessage("触发，并获取图像");
                        int Rc = -1;
                        //Rc = SR7LinkFunc.SR7IF_StartMeasure(_currentDeviceId, 20000);
                        Rc = SR7LinkFunc.SR7IF_StartIOTriggerMeasure(_currentDeviceId, 20000, 0);

                        // 接收数据
                        Rc = SR7LinkFunc.SR7IF_ReceiveData(_currentDeviceId, DataObject);

                        if (Rc == SR7IF_ERROR_MODE)
                        {
                            AddMessage("当前为循环模式!");
                            SR7LinkFunc.SR7IF_StopMeasure(_currentDeviceId);
                            return;
                        }
                        if (Rc < 0)
                        {
                            AddMessage("数据接收失败,返回值：" + Rc.ToString());
                            return;
                        }
                        else
                        {
                            // 获取批处理行数
                            int BatchPointTmp = SR7LinkFunc.SR7IF_ProfilePointCount(_currentDeviceId, DataObject);

                            // 获取宽度
                            int m_DataWidthTmp = SR7LinkFunc.SR7IF_ProfileDataWidth(_currentDeviceId, DataObject);

                            // 数据x方向间距(mm)
                            double m_XPitch = SR7LinkFunc.SR7IF_ProfileData_XPitch(_currentDeviceId, DataObject);

                            int BatchPoint = BatchPointTmp;
                            int m_DataWidth = m_DataWidthTmp;

                            int Tmpys = Convert.ToInt32(Convert.ToDouble(BatchPoint) / 1875 - 0.5);   //Y方向缩放倍数
                            int Tmpxs = Convert.ToInt32(Convert.ToDouble(m_DataWidth) / 800);        //X方向缩放倍数
                            if (BatchPoint < 1875)
                                Tmpys = 1;
                            if (m_DataWidth < 800)
                                Tmpxs = 1;

                            // 获取高度数据
                            using (PinnedObject pin = new PinnedObject(HeightData))       //内存自动释放接口
                            {
                                Rc = SR7LinkFunc.SR7IF_GetProfileData(_currentDeviceId, DataObject, pin.Pointer);
                                if (Rc < 0)
                                {
                                    AddMessage("高度数据获取失败");

                                    for (int i = 0; i < HeightData.Length; i++)
                                    {
                                        HeightData[i] = -1000000;
                                    }
                                }// pin.Pointer 获取高度数据缓存地址
                                var img = BatchDataShow(HeightData, 8.4, -8.4, 255, m_DataWidth, BatchPoint, Tmpxs, Tmpys);

                                // 显示
                                CameraIamge = Bitmap2HImage_24(img);
                            }
                        }
                    });                    
                    break;
                case "1":
                    xinje.SetM("M500", true);
                    AddMessage("仅触发");

                    break;
                default:
                    break;
            }
        }
        #endregion
        #region 自定义函数
        private void AddMessage(string str)
        {
            string[] s = MessageStr.Split('\n');
            if (s.Length > 1000)
            {
                MessageStr = "";
            }
            if (MessageStr != "")
            {
                MessageStr += "\n";
            }
            MessageStr += System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + str;
        }
        HImage Bitmap2HImage_24(Bitmap bImage)
        {
            Bitmap bImage24;
            BitmapData bmData = null;
            Rectangle rect;
            IntPtr pBitmap;
            IntPtr pPixels;
            HImage hImage = new HImage();


            rect = new Rectangle(0, 0, bImage.Width, bImage.Height);
            bImage24 = new Bitmap(bImage.Width, bImage.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bImage24);
            g.DrawImage(bImage, rect);
            g.Dispose();


            bmData = bImage24.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            pBitmap = bmData.Scan0;
            pPixels = pBitmap;


            hImage.GenImageInterleaved(pPixels, "bgr", bImage.Width, bImage.Height, -1, "byte", 0, 0, 0, 0, -1, 0);


            bImage24.UnlockBits(bmData);


            return hImage;
        }
        /// <summary>
        /// 高度图像显示--非无限循环方式
        /// </summary>
        /// <param name="_BatchData"></param>
        /// <param name="max_height"></param>
        /// <param name="min_height"></param>
        /// <param name="_ColorMax"></param>
        /// <param name="img_w"></param>
        /// <param name="img_h"></param>
        /// <param name="_scaleW"></param>
        /// <param name="_scaleH"></param>
        private Bitmap BatchDataShow(int[] _BatchData, double max_height, double min_height, int _ColorMax, int img_w, int img_h, int _scaleW, int _scaleH)
        {
            //颜色区间与高度区间比例
            //抽帧抽点显示
            int imgH = 1875;
            int imgW = 800;
            int TmpX = 0;
            int Tmppx = 0;
            if (img_h < imgH)
                imgH = img_h;
            if (img_w < imgW)
                imgW = img_w;

            int TT = (imgW * 8 + 31) / 32;   //图像四字节对齐
            TT = TT * 4;

            int m_HeightDataNum = TT * imgH;
            double fscale = _ColorMax / (max_height - min_height);

            byte[] BatchImage = new byte[m_HeightDataNum];

            for (int i = 0; i < imgH; i++)
            {
                TmpX = i * _scaleH * img_w;
                Tmppx = i * TT;
                for (int j = 0; j < imgW; j++)
                {
                    double Tmp = _BatchData[TmpX + j * _scaleW] * 0.00001;
                    if (Tmp < min_height)
                        BatchImage[Tmppx + j] = 0;
                    else if (Tmp > max_height)
                        BatchImage[Tmppx + j] = 255;
                    else
                    {
                        byte tmpt = Convert.ToByte((Tmp - min_height) * fscale);
                        BatchImage[Tmppx + j] = tmpt;
                    }
                }
            }

            Bitmap TmpBitmap = new Bitmap(imgW, imgH, PixelFormat.Format8bppIndexed);

            // 256 调色板
            ColorPalette monoPalette = TmpBitmap.Palette;
            Color[] entries = monoPalette.Entries;
            for (int i = 0; i < 256; i++)
                entries[i] = Color.FromArgb(i, i, i);

            TmpBitmap.Palette = monoPalette;

            Rectangle rect = new Rectangle(0, 0, TmpBitmap.Width, TmpBitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData = TmpBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            int bytes = TT * TmpBitmap.Height;  //每行实际字节数
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(BatchImage, 0, ptr, bytes);
            TmpBitmap.UnlockBits(bmpData);

            return TmpBitmap;
        }
        private int PointToRealData(int[] data, int width, int height, ref float[] realData)
        {
            if (data.Length < 1)
            {
                return -1;
            }

            if (height < 1 || width < 1)
            {
                return -1;
            }

            int upper = 0;
            int lower = 0;
            calUpperAndLower(data, height, width, ref upper, ref lower);

            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    if (data[i * width + j] >= lower && data[i * width + j] <= upper)
                    {
                        realData[i * width + j] = Convert.ToSingle(data[i * width + j]) / 100000;
                    }
                    else
                    {
                        realData[i * width + j] = -10000;
                    }
                }
            }
            return 0;
        }
        /// <summary>
		/// 自动计算数据最大值最小值
		/// </summary>
		/// <param name="data"></param>
		/// <param name="height"></param>
		/// <param name="width"></param>
		/// <param name="upper"></param>
		/// <param name="lower"></param>
		private void calUpperAndLower(int[] data, int height, int width, ref int upper, ref int lower)
        {
            int radio = 100000;
            lower = 100 * radio;
            upper = -100 * radio;
            for (int i = 0; i < height * width; ++i)
            {
                if (data[i] > -99 * radio && data[i] < 99 * radio)
                {
                    if (data[i] < lower)
                    {
                        lower = data[i];
                    }
                    if (data[i] > upper)
                    {
                        upper = data[i];
                    }
                }
            }
        }
        #endregion
    }
}
