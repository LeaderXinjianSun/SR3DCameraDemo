using HalconDotNet;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using SR3DCameraDemo.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
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
        private bool funcButtonIsEnabled;

        public bool FuncButtonIsEnabled
        {
            get { return funcButtonIsEnabled; }
            set
            {
                funcButtonIsEnabled = value;
                this.RaisePropertyChanged("FuncButtonIsEnabled");
            }
        }
        private string halconWindowVisibility;

        public string HalconWindowVisibility
        {
            get { return halconWindowVisibility; }
            set
            {
                halconWindowVisibility = value;
                this.RaisePropertyChanged("HalconWindowVisibility");
            }
        }
        private double testPValue;

        public double TestPValue
        {
            get { return testPValue; }
            set
            {
                testPValue = value;
                this.RaisePropertyChanged("TestPValue");
            }
        }
        private double fundPValue;

        public double FundPValue
        {
            get { return fundPValue; }
            set
            {
                fundPValue = value;
                this.RaisePropertyChanged("FundPValue");
            }
        }
        private double distPValue;

        public double DistPValue
        {
            get { return distPValue; }
            set
            {
                distPValue = value;
                this.RaisePropertyChanged("DistPValue");
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
        PointCloudHead pointCloudHead = new PointCloudHead();
        private Metro metro = new Metro();
        #endregion
        #region 构造函数
        public MainWindowViewModel()
        {
            #region 参数初始化
            Version = "20201125";
            MessageStr = "";
            try
            {
                
                using (FileStream fileStream = new FileStream(Path.Combine(System.Environment.CurrentDirectory, "ROI.bin"), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter mBinFmat = new BinaryFormatter();                    
                    CameraROIList = (ObservableCollection<ROI>)mBinFmat.Deserialize(fileStream);
                    foreach (var item in CameraROIList)
                    {
                        if (item.ROIId == 0)
                        {
                            item.ROIColor = "green";
                        }
                        if (item.ROIId == 1)
                        {
                            item.ROIColor = "magenta";
                        }
                    }
                    CameraRepaint = !CameraRepaint;
                }
            }
            catch (Exception ex)
            {
                CameraROIList = new ObservableCollection<ROI>();
                AddMessage(ex.Message);
            }

            FuncButtonIsEnabled = true;
            string com = Inifile.INIGetStringValue(iniParameterPath, "PLC", "COM", "COM1");
            xinje = new XinjePLCModbusRTU(com);
            xinje.PLC.StateChanged += PLC_StateChanged;
            xinje.PLC.Connect();
            StatusPLC = true;
            HalconWindowVisibility = "Visible";
            #endregion
            MenuActionCommand = new DelegateCommand<object>(new Action<object>(this.MenuActionCommandExecute));
            AppLoadedEventCommand = new DelegateCommand(new Action(this.AppLoadedEventCommandExecute));
            OperateButtonCommand = new DelegateCommand<object>(new Action<object>(this.OperateButtonCommandExecute));

            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcessesByName("SR3DCameraDemo");//获取指定的进程名   
            if (myProcesses.Length > 1) //如果可以获取到知道的进程名则说明已经启动
            {
                System.Windows.MessageBox.Show("不允许重复打开软件", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void MenuActionCommandExecute(object obj)
        {
            
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

        private async void OperateButtonCommandExecute(object obj)
        {
            bool r;
            switch (obj.ToString())
            {
                case "0":
                    await Task.Run(() =>
                    {
                        FuncButtonIsEnabled = false;
                        int _currentDeviceId = 0;
                        IntPtr DataObject = new IntPtr();
                        xinje.SetM("M500", true);
                        AddMessage("触发，并获取图像");
                        int Rc = -1;
                        //Rc = SR7LinkFunc.SR7IF_StartMeasure(_currentDeviceId, 20000);
                        Rc = SR7LinkFunc.SR7IF_StartIOTriggerMeasure(_currentDeviceId, 20000, 0);

                        // 接收数据
                        Rc = SR7LinkFunc.SR7IF_ReceiveData(_currentDeviceId, DataObject);
                        FuncButtonIsEnabled = true;
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
                            pointCloudHead._height = BatchPointTmp;

                            // 获取宽度
                            int m_DataWidthTmp = SR7LinkFunc.SR7IF_ProfileDataWidth(_currentDeviceId, DataObject);
                            pointCloudHead._width = m_DataWidthTmp;

                            // 数据x方向间距(mm)
                            double m_XPitch = SR7LinkFunc.SR7IF_ProfileData_XPitch(_currentDeviceId, DataObject);
                            pointCloudHead._yInterval = pointCloudHead._xInterval = m_XPitch;

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
                                var img = BatchDataShow(HeightData, pointCloudHead,255, 8.4, -8.4);

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
                case "2":
                    if (pointCloudHead._width != 0)
                    {
                        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                        {
                            saveFileDialog.Filter = "文本文件(*.ecd)|*.ecd";
                            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                try
                                {
                                    AddMessage("开始保存高度数据……");
                                    await Task.Run(()=> { SSZNEcd.WriteEcd(saveFileDialog.FileName, HeightData, pointCloudHead); }); 
                                    AddMessage("保存高度数据完成");
                                }
                                catch (Exception ex)
                                {
                                    AddMessage(ex.Message);
                                }
                                
                            }
                        }
                    }
                    else
                    {
                        AddMessage("保存失败：无数据");
                    }
                    break;
                case "3":
                    using (OpenFileDialog dlg = new OpenFileDialog())
                    {
                        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        dlg.Filter = "文本文件(*.ecd)|*.ecd";  //设置文件过滤
                       
                        dlg.Multiselect = false;              //禁止多选
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            AddMessage("开始加载高度数据……");
                            string fileName = dlg.FileName;
                            await Task.Run(()=> { SSZNEcd.ReadEcd(fileName, ref HeightData, ref pointCloudHead); });                                                       
                            AddMessage("加载高度数据完成");
                            var img = BatchDataShow(HeightData, pointCloudHead, 255, 8.4, -8.4);
                            // 显示
                            CameraIamge = Bitmap2HImage_24(img);
                        }
                    }
                    break;
                case "4":
                    if (pointCloudHead._width != 0)
                    {
                        try
                        {
                            using (PinnedObject pin = new PinnedObject(HeightData))
                            {
                                SR7LinkFunc.SR_3D_EXE_Show(pin.Pointer, pointCloudHead._xInterval, pointCloudHead._yInterval, pointCloudHead._width, pointCloudHead._height, 1, 8.4, -8.4);
                            }
                            AddMessage("3D显示成功");
                        }
                        catch (Exception ex)
                        {
                            AddMessage(ex.Message);
                        }
                    }
                    else
                    {
                        AddMessage("3D显示失败：无数据");
                    }
                    break;
                case "5":
                    metro.ChangeAccent("Dark.Red");
                    HalconWindowVisibility = "Collapsed";
                    r = await metro.ShowConfirm("确认", "请确认需要重画测量点吗？");
                    if (r)
                    {
                        metro.ChangeAccent("Light.Teal");
                        HalconWindowVisibility = "Visible";
                        ROI roi = Global.CameraImageViewer.DrawROI(ROI.ROI_TYPE_RECTANGLE1);
                        roi.ROIColor = "green";
                        roi.ROIId = 0;
                        roi.ROIInfo = "测量点";
                        var roifind = CameraROIList.FirstOrDefault(roi1 => roi1.ROIId == 0);
                        if (roifind == null)
                        {
                            CameraROIList.Add(roi);
                            CameraRepaint = !CameraRepaint;
                        }
                        else
                        {
                            CameraROIList.Remove(roifind);
                            CameraROIList.Add(roi);
                            CameraRepaint = !CameraRepaint;
                        }
                        WriteToBin(CameraROIList, Path.Combine(System.Environment.CurrentDirectory, "ROI.bin"));
                    }
                    else
                    {
                        metro.ChangeAccent("Light.Teal");
                        HalconWindowVisibility = "Visible";
                    }
                    break;
                case "6":
                    metro.ChangeAccent("Dark.Red");
                    HalconWindowVisibility = "Collapsed";
                    r = await metro.ShowConfirm("确认", "请确认需要重画基准点吗？");
                    if (r)
                    {
                        metro.ChangeAccent("Light.Teal");
                        HalconWindowVisibility = "Visible";
                        ROI roi = Global.CameraImageViewer.DrawROI(ROI.ROI_TYPE_RECTANGLE1);
                        roi.ROIColor = "magenta";
                        roi.ROIId = 1;
                        roi.ROIInfo = "基准点";
                        var roifind = CameraROIList.FirstOrDefault(roi1 => roi1.ROIId == 1);
                        if (roifind == null)
                        {
                            CameraROIList.Add(roi);
                            CameraRepaint = !CameraRepaint;
                        }
                        else
                        {
                            CameraROIList.Remove(roifind);
                            CameraROIList.Add(roi);
                            CameraRepaint = !CameraRepaint;
                        }
                        WriteToBin(CameraROIList, Path.Combine(System.Environment.CurrentDirectory, "ROI.bin"));
                    }
                    else
                    {
                        metro.ChangeAccent("Light.Teal");
                        HalconWindowVisibility = "Visible";

                    }
                    break;
                case "7":
                    if (pointCloudHead._width != 0 && CameraROIList.Count >= 2)
                    {
                        TestPValue = CalcHeight((ROIRectangle1)CameraROIList.FirstOrDefault(_roi => _roi.ROIId == 0), HeightData, pointCloudHead, CameraIamge);
                        FundPValue = CalcHeight((ROIRectangle1)CameraROIList.FirstOrDefault(_roi => _roi.ROIId == 1), HeightData, pointCloudHead, CameraIamge);
                        DistPValue = TestPValue - FundPValue;
                        AddMessage($"计算完成：{DistPValue:F3}");
                    }
                    else
                    {
                        AddMessage("计算失败：无数据");
                    }
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
        private Bitmap BatchDataShow(int[] data, PointCloudHead pcHead, int _ColorMax, double max_height, double min_height)
        {            
            int imgW = 800;
            int imgH = (int)((double)imgW / pcHead._width * pcHead._height);
            int TmpX = 0;
            int Tmppx = 0;
            if (pcHead._height < imgH)
                imgH = pcHead._height;
            if (pcHead._width < imgW)
                imgW = pcHead._width;

            int _scaleH = Convert.ToInt32(Convert.ToDouble(pcHead._height) / imgH - 0.5);   //Y方向缩放倍数
            int _scaleW = Convert.ToInt32(Convert.ToDouble(pcHead._width) / imgW);        //X方向缩放倍数
            if (pcHead._height <= imgH)
                _scaleH = 1;
            if (pcHead._width <= imgW)
                _scaleW = 1;

            int TT = (imgW * 8 + 31) / 32;   //图像四字节对齐
            TT = TT * 4;

            int m_HeightDataNum = TT * imgH;
            double fscale = _ColorMax / (max_height - min_height);

            byte[] BatchImage = new byte[m_HeightDataNum];

            for (int i = 0; i < imgH; i++)
            {
                TmpX = i * _scaleH * pcHead._width;
                Tmppx = i * TT;
                for (int j = 0; j < imgW; j++)
                {
                    double Tmp = data[TmpX + j * _scaleW] * 0.00001;
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
        private void WriteToBin(object p, string path)
        {
            try
            {
                using (FileStream fs = File.Open(path, FileMode.Create))
                {
                    BinaryFormatter b = new BinaryFormatter();
                    b.Serialize(fs, p);
                }
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
        }
        private double CalcHeight(ROIRectangle1 roi, int[] data, PointCloudHead pcHead, HImage img)
        {
            HTuple width, height;
            HOperatorSet.GetImageSize(img, out width, out height);
            double wscale = (double)pcHead._width / width;
            double hscale = (double)pcHead._height / height;
            int width1 = (int)(wscale * Math.Abs(roi.col2 - roi.col1));
            int height1 = (int)(hscale * Math.Abs(roi.row2 - roi.row1));
            double[] data1 = new double[width1 * height1];
            for (int i = 0; i < height1; i++)
            {
                for (int j = 0; j < width1; j++)
                {
                    //一定要保证是整数行 × 行点数，否则小数部分会影响定位
                    double Tmp = data[((int)(roi.row1 * hscale) + i) * pcHead._width + j + (int)(roi.col1 * wscale)] * 0.00001;
                    if (Tmp < -8.4)
                        data1[i * width1 + j] = -8.4;
                    else if (Tmp > 8.4)
                        data1[i * width1 + j] = 8.4;
                    else
                    {
                        data1[i * width1 + j] = Tmp;
                    }
                    
                }
            }
            return MathNet.Numerics.Statistics.ArrayStatistics.Mean(data1);
        }
        #endregion
    }
}
