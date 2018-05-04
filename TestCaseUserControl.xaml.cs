using ResourceShare.UserClient.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfReovlveTest
{
    /// <summary>
    /// TestCaseUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class TestCaseUserControl : UserControl
    {
        public TestCaseUserControl()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 分页管理
        /// </summary>
        public PageDataManager<TestCaseListViewModel> Data;

        /// <summary>
        /// 打开等待窗口
        /// </summary>
        public Action OpenWaitingWindow { set; get; }
        /// <summary>
        /// 关闭等待窗口
        /// </summary>
        public Action CloseWaitingWindow { set; get; }

        public Action<string> Search { set; get; }

        private int PageSize = 10; //默认10条

        public void SetSource(ObservableCollection<TestCaseListViewModel> models, int itemCount, bool isMemoryPager, int pageIndex = 1)
        {
            this.FullTextList = models;
            this.Data = new PageDataManager<TestCaseListViewModel>(FullTextList, itemCount, isMemoryPager, this.PagerFullTask, PageSize, pageIndex);

            this.Data.OpenWaitingWindow = OpenWaitingWindow;
            this.Data.CloseWaitingWindow = CloseWaitingWindow;
            this.Data.Owner = this;

            this.DataContext = Data;
            this.TestCaseDataGrid.DataContext = Data.PagerSource;

            fulltextPager.Visibility = itemCount == 0 ? Visibility.Collapsed : Visibility.Visible;

            prePage.Content = "<<";
            btnGoLastPage.Visibility = Data.Total == 1 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

        }

        private int pageIndex = 1;
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndex
        {
            set
            {
                pageIndex = value;
            }
            get
            {
                return pageIndex;
            }
        }

        public ObservableCollection<TestCaseListViewModel> FullTextList { set; get; }


        #region  全文列表按钮操作

        /// <summary>
        /// 取消全文任务
        /// </summary>
        public Func<TestCaseListViewModel, bool> CancelFullTask { set; get; }

        /// <summary>
        /// 取消任务（向服务器端提出取消请求）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var item = this.TestCaseDataGrid.SelectedItem as TestCaseListViewModel;

            if (item != null)
            {
                if (CancelFullTask != null)
                {
                    CancelFullTask(item);
                }
            }
        }
        /// <summary>
        /// 重新提交测试
        /// </summary>
        public Action<TestCaseListViewModel> ReSummitFullTask { set; get; }

        private void btnReSummit_Click(object sender, RoutedEventArgs e)
        {
            var item = this.TestCaseDataGrid.SelectedItem as TestCaseListViewModel;

            if (item != null)
            {
                if (ReSummitFullTask != null)
                {
                    ReSummitFullTask(item);
                }
            }
        }
        /// <summary>
        /// 打开测试用例所在路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenFullTextPath_Click(object sender, RoutedEventArgs e)
        {
            var item = this.TestCaseDataGrid.SelectedItem as TestCaseListViewModel;
            if (item != null)
            {
                OpenFolderAndSelectFile(item.FilePath);
            }
        }

        private void OpenFolderAndSelectFile(String fileFullName)
        {
            if (!string.IsNullOrEmpty(fileFullName))
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                psi.Arguments = "/e,/select," + fileFullName;
                System.Diagnostics.Process.Start(psi);
            }
        }
        private void OpenFile(String fileFullName)
        {
            if (!string.IsNullOrEmpty(fileFullName))
            {
                if (File.Exists(fileFullName))
                {
                    System.Diagnostics.Process.Start(fileFullName);
                }
                else
                {
                    new MessageTip().Show("全文文件", "全文文件不存在");
                }
            }
        }

        /// <summary>
        /// 删除全文任务
        /// </summary>
        public Func<TestCaseListViewModel, bool, Window, bool> DeleteFullTask { set; get; }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //删除任务
            var item = this.TestCaseDataGrid.SelectedItem as TestCaseListViewModel;

            if (item != null)
            {
                if (DeleteFullTask != null)
                {
                    StackPanel container;
                    var tip = CreateTipInstance(out container);
                    tip.Show("全文任务", "确定要删除吗？", null, () =>
                    {
                        bool isDeleteFile = false;
                        foreach (var c in container.Children)
                        {
                            var u = c as CheckBox;

                            if (u != null)
                            {
                                isDeleteFile = u.IsChecked.Value;
                                break;
                            }
                        }
                        DeleteFullTask(item, isDeleteFile, null);
                    }, null, true);
                }
            }
        }
        #endregion


        /// <summary>
        /// 全选\取消全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSelected_OnClick(object sender, RoutedEventArgs e)
        {
            CheckBox chkSelected = e.OriginalSource as CheckBox;
            if (chkSelected == null)
            {
                return;
            }

            bool isChecked = chkSelected.IsChecked.HasValue ? chkSelected.IsChecked.Value : true;

            FrameworkElement templateParent = chkSelected.TemplatedParent is FrameworkElement
                                                  ? (chkSelected.TemplatedParent as FrameworkElement).TemplatedParent as FrameworkElement
                                                  : null;

            if (templateParent is DataGridColumnHeader && this.Data != null)
            {
                foreach (var item in this.Data.DataSource)
                {
                    item.IsSelected = isChecked;
                }
            }
        }

        /// <summary>
        /// 批量删除全文任务
        /// </summary>
        public Func<List<TestCaseListViewModel>, bool, bool> DeleteFullTasks { set; get; }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBachDelete_Click(object sender, RoutedEventArgs e)
        {
            var models = this.TestCaseDataGrid.DataContext as ObservableCollection<TestCaseListViewModel>;

            if (models != null && models.Count > 0)
            {
                var filters = models.Where(d => d.IsSelected == true).ToList();
                if (filters.Count > 0)
                {
                    if (DeleteFullTasks != null)
                    {
                        StackPanel container;
                        var tip = CreateTipInstance(out container);

                        tip.Show("全文任务", "确定要删除吗？", null, () =>
                        {
                            bool isDeleteFile = false;
                            foreach (var c in container.Children)
                            {
                                var u = c as CheckBox;

                                if (u != null)
                                {
                                    isDeleteFile = u.IsChecked.Value;
                                    break;
                                }
                            }
                            DeleteFullTasks(filters, isDeleteFile);
                        }, null, true);
                    }
                }
            }
        }

        private static MessageTip CreateTipInstance(out StackPanel container)
        {
            var tip = new MessageTip();
            container = tip.FindName("ContentContainer") as StackPanel;

            if (container != null)
            {
                CheckBox box = new CheckBox();
                box.Content = "同时删除对应文件";
                box.Margin = new Thickness(0, 8, 0, 0);
                box.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                box.IsChecked = true;
                container.Children.Add(box);
            }
            return tip;
        }
        /// <summary>
        /// 添加全文任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddTask_Click(object sender, RoutedEventArgs e)
        {
            if (OpenAddFullTask != null)
            {
                OpenAddFullTask();
            }
        }

        public Action OpenAddFullTask;

        public Action<int, int> PagerFullTask;

        private void TestCaseDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point aP = e.GetPosition(this.TestCaseDataGrid);
            IInputElement obj = this.TestCaseDataGrid.InputHitTest(aP);
            DependencyObject target = obj as DependencyObject;

            try
            {
                while (target != null)
                {
                    if (target is DataGridCell)
                    {
                        var dg = target as DataGridCell;

                        if (dg != null)
                        {
                            if (dg.Column.DisplayIndex == 1)
                            {
                                var item = this.TestCaseDataGrid.SelectedItem as TestCaseListViewModel;
                                if (item != null)
                                {
                                    OpenFile(item.FilePath);
                                }
                            }
                        }
                        break;
                    }
                    target = VisualTreeHelper.GetParent(target);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("打开全文文件出错：" + ex.Message);
            }
        }

        private void btn_GotoPage(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button != null)
            {
                var pages = button.DataContext as Pages;
                if (pages != null)
                {
                    this.PageIndex = pages.PageIndex;
                    Data.Pager(PageIndex);
                }
            }
        }

        private void goPage_Click(object sender, RoutedEventArgs e)
        {
            int pageIndex = 1;
            int.TryParse(this.wantToGo.Text, out pageIndex);

            this.PageIndex = pageIndex;
            Data.Pager(pageIndex);
        }

        private void prePage_Click(object sender, RoutedEventArgs e)
        {
            if (this.PageIndex > 1)
            {
                this.PageIndex--;
                Data.Pager(this.PageIndex);
            }
        }

        private void nextPage_Click(object sender, RoutedEventArgs e)
        {
            if (this.PageIndex < this.Data.Total)
            {
                this.PageIndex++;
                Data.Pager(this.PageIndex);
            }
        }

        private void bntGoFirstPage_Click(object sender, RoutedEventArgs e)
        {
            this.PageIndex = 1;
            Data.Pager(1);
        }

        private void btnGoLastPage_Click(object sender, RoutedEventArgs e)
        {
            int pageIndex = int.Parse((sender as Button).Content.ToString());
            this.PageIndex = pageIndex;
            Data.Pager(pageIndex);
        }

        private void btnSearchDetail_Click(object sender, RoutedEventArgs e)
        {
            //给listbox设置数据源

            var btn = sender as Button;

            var data = btn.DataContext;

            var listbox = this.ItemTitle;

            if (listbox != null)
            {
                var model = data as TestCaseListViewModel;

                if (model != null)
                {
                    listbox.ItemsSource = model.TestResultData;
                    listbox.SelectedIndex = 0;
                }
            }
        }

        private Paragraph paragraph;

        private void ItemTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listbox = sender as ListBox;

            if (listbox != null)
            {
                this.ItemContent.Document.Blocks.Clear();

                if (listbox.SelectedItem != null)
                {

                    this.paragraph = new Paragraph();
                    this.ItemContent.Document = new FlowDocument(paragraph);

                    foreach (var item in ((WpfReovlveTest.SearchResult)(listbox.SelectedItem)).Fields)
                    {
                        var s = item.Split('：');

                        if (s.Length > 0)
                        {
                            paragraph.Inlines.Add(new Bold(new Run(s[0] + "："))
                            {
                                Foreground = Brushes.Black
                            });

                        }
                        if (s.Length > 1)
                        {
                            string key = s[0];

                            if (key == "题录标题" || key == "作者全名")
                            {
                                paragraph.Inlines.Add(new Run()
                                {
                                    FontSize = 16,
                                    Text = s[1],
                                    Foreground=Brushes.RoyalBlue,
                                });
                            }
                            else
                            {
                                paragraph.Inlines.Add(s[1]);
                            }
                        }
                        paragraph.Inlines.Add(new LineBreak());
                    }
                }
            }
        }
        /// <summary>
        /// 行变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestCaseDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //给listbox设置数据源

            var data = this.TestCaseDataGrid.SelectedItem;

            var listbox = this.ItemTitle;

            if (listbox != null)
            {
                var model = data as TestCaseListViewModel;

                if (model != null)
                {
                    MainWindow.StaModel.TotalCount = model.TestResultData == null ? 0 : model.TestResultData.Count;
                    listbox.ItemsSource = model.TestResultData;
                    listbox.SelectedIndex = 0;
                }
            }
        }
    }
}
