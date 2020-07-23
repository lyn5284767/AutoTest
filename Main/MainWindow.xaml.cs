using COM.Common;
using DatabaseLib;
using DevExpress.Charts.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        List<ShowDate> showDateList = new List<ShowDate>();
        BackgroundWorker bgMeet;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            showDateList.Clear();
            this.lvList.Items.Clear();
            string sql = "Select * from AutoTest Order by ID";
            showDateList = DataHelper.Instance.ExecuteList<ShowDate>(sql);
            this.tbTips.Text = "开始检测";
            this.tbTotalNum.Text = "0/" + showDateList.Count();
            bgMeet = new BackgroundWorker();
            //能否报告进度更新
            bgMeet.WorkerReportsProgress = true;
            bgMeet.WorkerSupportsCancellation = true;
            //要执行的后台任务
            bgMeet.DoWork += new DoWorkEventHandler(bgMeet_DoWork);
            //进度报告方法
            bgMeet.ProgressChanged += new ProgressChangedEventHandler(bgMeet_ProgressChanged);
            //后台任务执行完成时调用的方法
            bgMeet.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgMeet_RunWorkerCompleted);
            bgMeet.RunWorkerAsync(); //任务启动
        }

        //执行任务
        void bgMeet_DoWork(object sender, DoWorkEventArgs e)
        {
            ////开始播放等待动画
            //this.Dispatcher.Invoke(new Action(() =>
            //{
            //    this.tbprocess.Text = "0/" + calList.Count;
            //}));
            for (int i = -1; i < showDateList.Count; i++)
            {
                if (bgMeet.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    //int now = 0;
                    //if (i == -1 || i == 0) now = 0;
                    //else now = i;
                    ShowDate showDate = showDateList[i];
                    if (showDate.Protocal != null && showDate.Protocal != string.Empty)
                    {
                        byte[] byteToSend = StrToByte(showDate.Protocal.Split(','));
                        GlobalData.Instance.da.SendBytes(byteToSend);
                    }
                    System.Threading.Thread.Sleep(200);
                    bgMeet.ReportProgress(i + 1, showDate);
                }
            }
        }

        private byte[] StrToByte(string[] strs)
        {
            byte[] bytes = new byte[strs.Count()];
            for (int i = 0; i < strs.Count(); i++)
            {
                bytes[i] = (byte)int.Parse(strs[i]);
            }
            return bytes;
        }
        //报告任务进度
        void bgMeet_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (e.ProgressPercentage > 0)
                {
                    this.tbTotalNum.Text = e.ProgressPercentage +"/" + showDateList.Count();
                    if (e.UserState is ShowDate)
                    {
                        ShowDate data = e.UserState as ShowDate;
                        if (data.Field != null && data.Field != string.Empty) // 需要从通信数据中读取
                        {
                            data.Value = GlobalData.Instance.da[data.Field].Value.Int16;
                            if (data.MinValue > 0 && data.MaxValue > 0) // 有范围值
                            {
                                if (data.Value >= data.MinValue && data.Value <= data.MaxValue)
                                {
                                    data.Result = "读取值正常";
                                }
                                else
                                {
                                    data.Result = "读取值异常";
                                }
                            }
                            else
                            {
                                data.Result = "读取值正常";
                            }
                        }
                        else
                        {
                            data.Value = showDateList[data.MaxValue].Value - showDateList[data.MinValue].Value;
                            if(Math.Abs(data.Value)<=35) data.Result = "读取值正常";
                            else data.Result = "读取值异常";
                        }
                        
                        this.lvList.Items.Add(data);
                        //string sql = string.Format("Update ShowDate Set Value='{0}' Where ID='{1}'", data.Value, data.ID);
                        //DataHelper.Instance.ExecuteNonQuery(sql);
                    }
                }
            }));
        }
        //任务执行完成后更新状态
        void bgMeet_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (this.showDateList.Where(w => w.Result == "读取值异常").Count() > 0)
                {
                    this.tbTips.Text = "读取值存在异常，请重新读取";
                    return;
                }
                else
                {
                    this.tbTips.Text = "开始复测数据";
                    this.lvList.Items.Clear();
                    bgMeet = new BackgroundWorker();
                    //能否报告进度更新
                    bgMeet.WorkerReportsProgress = true;
                    bgMeet.WorkerSupportsCancellation = true;
                    //要执行的后台任务
                    bgMeet.DoWork += new DoWorkEventHandler(bgMeet_TestDoWork);
                    //进度报告方法
                    bgMeet.ProgressChanged += new ProgressChangedEventHandler(bgMeet_TestProgressChanged);
                    //后台任务执行完成时调用的方法
                    bgMeet.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgMeet_TestRunWorkerCompleted);
                    bgMeet.RunWorkerAsync(); //任务启动
                }
            }));
        }

        //执行任务
        void bgMeet_TestDoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = -1; i < showDateList.Count; i++)
            {
                if (bgMeet.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    //int now = 0;
                    //if (i == -1 || i == 0) now = 0;
                    //else now = i;
                    ShowDate showDate = showDateList[i];
                    if (showDate.Protocal != null && showDate.Protocal != string.Empty)
                    {
                        byte[] byteToSend = StrToByte(showDate.Protocal.Split(','));
                        GlobalData.Instance.da.SendBytes(byteToSend);
                    }
                    System.Threading.Thread.Sleep(200);
                    bgMeet.ReportProgress(i + 1, showDate);
                }
            }
        }
        //报告任务进度
        void bgMeet_TestProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (e.ProgressPercentage > 0)
                {
                    this.tbTotalNum.Text = e.ProgressPercentage + "/" + showDateList.Count();
                    if (e.UserState is ShowDate)
                    {
                        ShowDate data = e.UserState as ShowDate;
                        if (data.Field != null && data.Field != string.Empty) // 需要从通信数据中读取
                        {
                            data.Value = GlobalData.Instance.da[data.Field].Value.Int16;
                            if (Math.Abs(data.Value - GlobalData.Instance.da[data.Field].Value.Int16) <=10) // 有范围值
                            {
                                data.Result = "检测正常";
                                this.tbCorrect.Text = (int.Parse(this.tbCorrect.Text) + 1).ToString();
                            }
                            else
                            {
                                data.Result = "检测异常";
                                this.tbError.Text = (int.Parse(this.tbCorrect.Text) + 1).ToString();
                            }
                        }
                        else
                        {
                            if (Math.Abs(data.Value - (showDateList[data.MaxValue].Value - showDateList[data.MinValue].Value)) <= 35)
                            {
                                data.Result = "检测正常";
                                this.tbCorrect.Text = (int.Parse(this.tbCorrect.Text) + 1).ToString();
                            }
                            else
                            {
                                data.Result = "读取值异常";
                            }
                        }

                        this.lvList.Items.Add(data);
                        string sql = string.Format("Update ShowDate Set Value='{0}' Where ID='{1}'", data.Value, data.ID);
                        DataHelper.Instance.ExecuteNonQuery(sql);
                    }
                }
            }));
        }

        void bgMeet_TestRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.tbTips.Text = "检测完毕";
            }));
        }
    }

    public class ShowDate
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 调试项目
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public int Value { get; set; }
        /// <summary>
        /// 项目父级
        /// </summary>
        public string Parent { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public int MaxValue { get; set; }
        /// <summary>
        /// 最小值
        /// </summary>
        public int MinValue { get; set; }
        /// <summary>
        /// 协议
        /// </summary>
        public string Protocal { get; set; }
        /// <summary>
        /// 检测结果
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 读取字段
        /// </summary>
        public string Field { get; set; }
    }
}
