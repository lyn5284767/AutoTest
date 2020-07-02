using COM.Common;
using DevExpress.Charts.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Main
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalData.Instance.da = new DemoDriver.DAService();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<ShowDate> dateList = new List<ShowDate>();
            int InitValue = GlobalData.Instance.da["103N23GripMotorSampleValue"].Value.Int16;
            int id = 1;
            /// 3.5寸钻杆
            byte[] byteToSend = GlobalData.Instance.SendByte(new List<byte> { 3, 35 });
            GlobalData.Instance.da.SendBytes(byteToSend);
            Thread.Sleep(200);
            byteToSend = GlobalData.Instance.SendByte(new List<byte> { 12, 97 });
            GlobalData.Instance.da.SendBytes(byteToSend);
            Thread.Sleep(200);

            ShowDate date = new ShowDate();
            date.ID = 1;
            date.PipeName = "3.5寸钻杆";
            date.SampleValue = GlobalData.Instance.da["103N23GripMotorSampleValue"].Value.Int16;
            date.SettingValue = GlobalData.Instance.da["108N23PositionCalibrationValue"].Value.Int16;
            dateList.Add(date);
            /// 5寸钻杆
            byteToSend = GlobalData.Instance.SendByte(new List<byte> { 3, 50 });
            GlobalData.Instance.da.SendBytes(byteToSend);
            Thread.Sleep(200);
            byteToSend = GlobalData.Instance.SendByte(new List<byte> { 12, 98 });
            GlobalData.Instance.da.SendBytes(byteToSend);
            Thread.Sleep(200);

            ShowDate date2 = new ShowDate();
            date2.ID = 2;
            date2.PipeName = "5寸钻杆";
            date2.SampleValue = GlobalData.Instance.da["103N23GripMotorSampleValue"].Value.Int16;
            date2.SettingValue = GlobalData.Instance.da["108N23PositionCalibrationValue"].Value.Int16;
            dateList.Add(date2);
            /// 7寸钻铤
            byteToSend = GlobalData.Instance.SendByte(new List<byte> { 3, 70 });
            GlobalData.Instance.da.SendBytes(byteToSend);
            Thread.Sleep(200);
            byteToSend = GlobalData.Instance.SendByte(new List<byte> { 12, 101 });
            GlobalData.Instance.da.SendBytes(byteToSend);
            Thread.Sleep(200);

            ShowDate date3 = new ShowDate();
            date3.ID = 3;
            date3.PipeName = "5寸钻杆";
            date3.SampleValue = GlobalData.Instance.da["103N23GripMotorSampleValue"].Value.Int16;
            date3.SettingValue = GlobalData.Instance.da["108N23PositionCalibrationValue"].Value.Int16;
            dateList.Add(date3);

            this.testList.ItemsSource = dateList;
        }
    }

    public class ShowDate
    {
        public int ID { get; set; }
        public string PipeName { get; set; }
        public int SampleValue { get; set; }
        public int SettingValue { get; set; }
    }
}
