using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR3DCameraDemo.Model
{
    public static class SSZNEcd
    {
        public static void ReadEcd(string file, ref int[] data, ref PointCloudHead pcHead)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (!File.Exists(file))
            {
                return;
            }
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    BATCH_INFO head = new BATCH_INFO();
                    head.version = br.ReadUInt32();
                    head.width = br.ReadInt32();
                    head.height = br.ReadInt32();
                    br.ReadInt32();  //无效位，为了数据对齐
                    head.xInterval = Math.Round(br.ReadDouble(), 3);
                    head.yInterval = Math.Round(br.ReadDouble(), 3);
                    br.ReadBytes(10208);   //文件头共10240字节
                    data = new int[head.width * head.height];
                    for (int i = 0; i < head.width * head.height; i++)
                    {
                        data[i] = br.ReadInt32();
                    }
                    pcHead._width = head.width;
                    pcHead._height = head.height;
                    pcHead._xInterval = head.xInterval;
                    pcHead._yInterval = head.yInterval;
                    sw.Stop();
                    Console.WriteLine("图像数据加载耗时 :" + sw.ElapsedMilliseconds.ToString());
                    sw.Restart();
                }
            }
        }

        public static void WriteEcd(string file, int[] data, PointCloudHead pcHead)
        {
            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.ReadWrite))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    BATCH_INFO head = new BATCH_INFO();
                    head.width = pcHead._width;
                    head.height = pcHead._height;
                    head.xInterval = pcHead._xInterval;
                    head.yInterval = pcHead._yInterval;
                    bw.Write(head.version);
                    bw.Write(head.width);
                    bw.Write(head.height);
                    bw.Write((int)0);
                    bw.Write(head.xInterval);
                    bw.Write(head.yInterval);
                    byte[] buff = new byte[10208];
                    bw.Write(buff, 0, buff.Length);
                    for (int i = 0; i < data.Length; i++)
                    {
                        bw.Write(data[i]);
                    }
                }
            }
        }
    }
    public struct BATCH_INFO
    {
        public uint version;
        public int width;
        public int height;
        public double xInterval;
        public double yInterval;
        public int[] reserve;
    };

    public struct PointCloudHead
    {
        public int _height; //点云行数
        public int _width; //点云列数
        public double _xInterval; //点云列间距
        public double _yInterval; //点云行间距
    };
}
