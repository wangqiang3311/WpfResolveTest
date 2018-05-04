using ResourceShare.UserClient.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace WpfReovlveTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, ITestCaseView
    {
        public static StaViewModel StaModel { set; get; }


        public static int pageSize = 10;

        public static string appDir;

        public MainWindow()
        {
            InitializeComponent();

            StaModel = new StaViewModel();

            this.DataContext = StaModel;

            //获取项目根目录
            appDir = System.Environment.CurrentDirectory;

            if (appDir.Contains("\\bin\\"))
            {
                appDir = appDir.Substring(0, appDir.LastIndexOf("\\"));
                appDir = appDir.Substring(0, appDir.LastIndexOf("\\"));
            }
            dropdownResolveList.SelectedIndex = 0;

            ResolveDataPool.DataPoolChangedEvent += ResolveDataPool_DataPoolChangedEvent;
        }
        void ResolveDataPool_DataPoolChangedEvent(object sender, DataPoolChangedEventArgs e)
        {
            //手动刷新界面
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                 (ThreadStart)delegate()
                                 {
                                     ResolveDataPool.ReadSource(pageSize);

                                     bool isMemoryPager = ResolveDataPool.PagerWay == PagerMode.Memory ? true : false;

                                     testCaseUserControl.SetSource(ResolveDataPool.Resource, ResolveDataPool.ItemCount, isMemoryPager);

                                 });

        }

        /// <summary>
        /// 重新测试
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void ReAddTask(TestCaseListViewModel model)
        {
            try
            {
                if (model.CanReSummit)
                {
                    if (this.dropdownResolveList.SelectedIndex == 0)
                    {
                        MessageBox.Show("请选择解析器");
                        return;
                    }

                    StaModel.DuringTime = "";
                    StaModel.StartTime = DateTime.Now;

                    ResolveDataPool.ReStart(model);
                }
                else
                {
                    MessageBox.Show("当前测试正在运行，不能重新测试");
                }


            }
            catch (Exception ex)
            {

            }
        }
        public void OpenAddTask()
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ResolveDataPool.IsInited == false)
            {
                testCaseUserControl.DeleteFullTask = ResolveDataPool.RemoveTask;
                testCaseUserControl.DeleteFullTasks = ResolveDataPool.RemoveTasks;

                testCaseUserControl.ReSummitFullTask = ReAddTask;
                testCaseUserControl.OpenAddFullTask = OpenAddTask;

                testCaseUserControl.PagerFullTask = ResolveDataPool.ReadSource;

                testCaseUserControl.OpenWaitingWindow = OpenWaitingWindow;
                testCaseUserControl.CloseWaitingWindow = CloseWaitingWindow;

            }

            string caseDir = appDir + "\\testcase\\xml";
            string testDataDir = appDir + "\\testData\\xml";

            ResolveDataPool.Init(caseDir, testDataDir);
        }

        TaskAdding adding;
        private void OpenWaitingWindow()
        {
            adding = new TaskAdding();
            adding.lbTip.Content = "正在加载数据...";
            adding.Owner = this;
            adding.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            adding.ShowDialog();
        }

        private void CloseWaitingWindow()
        {
            if (adding != null)
            {
                adding.Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ResolveDataPool.DataPoolChangedEvent -= ResolveDataPool_DataPoolChangedEvent;
            ResolveDataPool.Cancel();
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            if (this.dropdownResolveList.SelectedIndex == 0)
            {
                MessageBox.Show("请选择解析器");
                return;
            }

            StaModel.StartTime = DateTime.Now;
            ResolveDataPool.Reset(false);
            List<string> resolves = new List<string>()
            {
                (this.dropdownResolveList.SelectedItem as XmlNode).Attributes["Name"].Value
            };
            ResolveDataPool.Resolves = resolves;

            var selectedItems = ResolveDataPool.Data.Where(r => r.IsSelected);

            if (selectedItems.Count() > 0)
            {
                foreach (var item in selectedItems)
                {
                    item.TestResult = TestState.Ready;
                }
            }

            foreach (var item in ResolveDataPool.Data)
            {
                item.ResolveName = ResolveDataPool.Resolves.First();
                item.MatchWay = StaModel.MatchWay;

                if (item.TestResultData != null)
                    item.TestResultData.Clear();
            }

            ResolveDataPool.StartGetState();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            StaModel.DuringTime = "";
            ResolveDataPool.Reset();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            ResolveDataPool.Cancel();
        }

        public void Init()
        {
            throw new NotImplementedException();
        }
    }
}