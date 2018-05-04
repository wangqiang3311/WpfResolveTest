using ResourceShare.UserClient.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace WpfReovlveTest
{
    /// <summary>
    /// 数据池，专门用来存储数据（包括数据的读写以及用户界面数据显示）
    /// </summary>
    public class ResolveDataPool
    {
        #region  属性

        /// <summary>
        /// 在全文任务删除和任务状态更改同步时加锁
        /// </summary>
        public static readonly object SyncRoot = new object();

        /// <summary>
        /// 当全文任务列表页面数据源变动时，锁定
        /// </summary>
        public static readonly object UiSourceLock = new object();

        private static readonly object SingleFetcherLock = new object();


        private static List<TestCaseListViewModel> dbList = new List<TestCaseListViewModel>();


        private static ObservableCollection<TestCaseListViewModel> uiResource = new ObservableCollection<TestCaseListViewModel>();

        public static List<TestCaseListViewModel> Data
        {
            get
            {
                return dbList;
            }
        }

        public static ObservableCollection<TestCaseListViewModel> Resource
        {
            get
            {
                return uiResource;
            }
        }

        public static readonly Fetcher fetcherInstance = new Fetcher();

        /// <summary>
        /// 是否初始化完成
        /// </summary>
        public static bool IsInited = false;
        /// <summary>
        /// 是否同步过
        /// </summary>
        public static bool IsSyned = false;

        public static int ItemCount { set; get; }


        public static event DataPoolChangedHandle DataPoolChangedEvent;

        #endregion

        /// <summary>
        /// 读取测试用例
        /// </summary>
        /// <returns></returns>
        private static List<TestCase> GetTestCase(string caseDir)
        {
            List<TestCase> tests = new List<TestCase>();

            var files = Directory.GetFiles(caseDir);


            foreach (var f in files)
            {
                var filePath = f;
                var testCase = Tools.SelectSingle<TestCase>("//case", filePath);
                testCase.FilePath = filePath;
                tests.Add(testCase);
            }

            ItemCount = tests.Count;

            return tests;
        }

        public static string CaseDir { set; get; }

        public static string TestDataDir { set; get; }


        /// <summary>
        /// 数据池初始化
        /// </summary>
        public static void Init(string caseDir, string testDataDir, bool isStartGetState = true)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                CaseDir = caseDir;
                TestDataDir = testDataDir;

                var cases = GetTestCase(caseDir);
                //根据count决定数据分页方式
                PagerWay = ItemCount > Global.FullTextDBLargeData ? PagerMode.DB : PagerMode.Memory;

                InitPool(cases);
            });
        }

        public static PagerMode PagerWay = PagerMode.Memory;
        public static void ReadSource(int pageSize = 20, int index = 1)
        {
            try
            {
                if (PagerWay == PagerMode.Memory)
                {
                    var list = dbList;
                    ItemCount = list.Count;
                    DoInitUI(list);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("全文读取出错：" + ex.Message);
            }
        }


        public static void InitPool(IEnumerable<TestCase> cases)
        {
            IsInited = true;

            if (PagerWay == PagerMode.DB)
            {
                //数据库分页方式，只需要依赖数据库
                NotifyOnlyUIUpdate("初始化数据库分页");
            }

            //内存分页依赖于数据池中的数据

            Logger.Log("列表初始化开始，共有" + ItemCount + "条数据,当前线程Id：" + Thread.CurrentThread.ManagedThreadId);

            int maxNum = Global.FullTextMemoryLargeData;

            lock (SyncRoot)
            {
                ResolveDataPool.dbList.Clear();
            }

            if (ItemCount <= maxNum)
            {
                var models = ReadFullTaskRecord(cases);
                if (models != null && models.Count > 0)
                {
                    DoInit(models);

                    if (PagerWay == PagerMode.Memory)
                    {
                        ItemCount = dbList.Count;
                        NotifyOnlyUIUpdate("小数据量初始化，更新条数：" + models.Count);
                    }
                }
            }
            else
            {
                //如果数据超过maxNum条，那么分批次加载数据

                int times = ItemCount % maxNum == 0 ? ItemCount / maxNum : ItemCount / maxNum + 1;

                var task = Task.Factory.StartNew(() =>
                {
                    for (int i = 1; i <= times; i++)
                    {
                        var pagerDatas = cases.Skip((i - 1) * maxNum).Take(maxNum);

                        var models = ReadFullTaskRecord(pagerDatas);

                        if (models != null && models.Count > 0)
                        {
                            DoInit(models);

                            if (PagerWay == PagerMode.Memory)
                            {
                                ItemCount = dbList.Count;
                                NotifyOnlyUIUpdate("大数据量分批次初始化,当前批次更新条数：" + models.Count);
                            }
                        }
                    }
                    Logger.Log("任务列表初始化完成，初始化条数：" + ItemCount);
                });

            }
        }

        public static void DoInit(IEnumerable<TestCaseListViewModel> models, bool isStartGetState = true)
        {
            lock (ResolveDataPool.SyncRoot)
            {
                foreach (var item in models)
                {
                    if (!ResolveDataPool.dbList.Exists(c => c.Id == item.Id)) ResolveDataPool.dbList.Add(item);
                }
            }

            StartFetchState(false);
        }

        public static void DoInitUI(IEnumerable<TestCaseListViewModel> models)
        {
            lock (ResolveDataPool.UiSourceLock)
            {
                ResolveDataPool.uiResource.Clear();

                foreach (var item in models)
                {
                    ResolveDataPool.uiResource.Add(item);
                }
            }
        }

        public static void ClearUIResource()
        {
            lock (ResolveDataPool.UiSourceLock)
            {
                ResolveDataPool.uiResource.Clear();
            }
        }

        /// <summary>
        /// 通知UI更新
        /// </summary>
        /// <param name="isStartGetState"></param>
        public static void NotifyOnlyUIUpdate(string message = "")
        {
            if (DataPoolChangedEvent != null)
            {
                DataPoolChangedEvent(null, new DataPoolChangedEventArgs(message));
            }
        }
        //通知获取状态
        private static void StartFetchState(bool isStartGetState = true)
        {
            if (isStartGetState)
            {
                StartGetState();
            }
        }
        /// <summary>
        /// 通知ui更新和开启状态获取
        /// </summary>
        private static void NotifyAll(string message = "")
        {
            NotifyOnlyUIUpdate(message);
            StartFetchState();
        }
        /// <summary>
        /// 向数据池中批量添加
        /// </summary>
        public static void AddRange(IEnumerable<TestCase> records, string message = "")
        {
            var models = ReadFullTaskRecord(records);

            if (models != null && models.Count > 0)
            {
                lock (ResolveDataPool.SyncRoot)
                {
                    foreach (var item in models)
                    {
                        if (!ResolveDataPool.dbList.Exists(d => d.Id == item.Id)) ResolveDataPool.dbList.Add(item);
                    }
                }
                NotifyAll(message);
            }
        }
        public static void AddOne(TestCaseListViewModel model, string message = "")
        {
            lock (ResolveDataPool.SyncRoot)
            {
                if (!ResolveDataPool.dbList.Exists(d => d.Id == model.Id)) ResolveDataPool.dbList.Insert(0, model);
            }
            NotifyAll(message);
        }

        public static void RemoveOne(TestCaseListViewModel model)
        {
            lock (ResolveDataPool.SyncRoot)
            {
                ResolveDataPool.dbList.Remove(model);
            }
            NotifyOnlyUIUpdate("删除一条任务");
        }
        public static void RemoveRange(IEnumerable<TestCaseListViewModel> removes)
        {
            lock (ResolveDataPool.SyncRoot)
            {
                foreach (var model in removes)
                {
                    ResolveDataPool.dbList.Remove(model);
                }
            }
            NotifyOnlyUIUpdate("批量删除一组任务");
        }

        /// <summary>
        /// 启动任务状态线程
        /// </summary>
        public static void StartGetState()
        {
            lock (SingleFetcherLock)
            {
                if (fetcherInstance.WorkState == WorkState.Stopped)
                {
                    fetcherInstance.WorkState = WorkState.Running;
                    fetcherInstance.Start();
                }
            }
        }

        public static void Cancel()
        {
            if (fetcherInstance.WorkState == WorkState.Running)
            {
                fetcherInstance.Cancel();
                fetcherInstance.WorkState = WorkState.Stopped;
            }
        }


        #region 全文任务列表操作API

        public static bool IsEndPass(TestCaseListViewModel model)
        {
            return IsEndPass(model.TestResult);
        }
        public static bool IsEndPass(TestState state)
        {
            return state == TestState.Pass || state == TestState.Unpass;
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="taskClientId"></param>
        public static bool RemoveTask(TestCaseListViewModel model, bool isDeleteFile, Window w = null)
        {
            var flag = false;

            if (!model.CanDelete)
            {
                MessageBox.Show("当前测试不可删除");
                return false;
            }

            var result = RemoveTaskRecord(model);

            if (result == false)
            {
                Logger.Log("删除任务记录失败");
                return flag;
            }
            flag = result;

            RemoveOne(model);
            return flag;
        }

        /// <summary>
        /// 删除一组任务
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public static bool RemoveTasks(List<TestCaseListViewModel> models, bool isDeleteFile)
        {
            var flag = false;
            List<TestCaseListViewModel> removes = new List<TestCaseListViewModel>();

            foreach (var model in models)
            {
                removes.Add(model);
            }

            var result = RemoveTaskRecords(removes);

            if (result == false)
            {
                new MessageTip().Show("全文任务", "批量删除任务记录失败");
                return flag;
            }
            flag = result;

            RemoveRange(removes);

            return flag;
        }
        #endregion

        #region  全文任务记录数据库表操作

        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        public static List<TestCaseListViewModel> ReadFullTaskRecord(IEnumerable<TestCase> models)
        {
            var viewModels = new List<TestCaseListViewModel>();

            int i = 0;

            foreach (var item in models)
            {
                i++;
                var vm = new TestCaseListViewModel()
                {
                    FilePath = item.FilePath,
                    Id = i,
                    SearchAuthor = item.Author,
                    SearchTitle = item.Title,
                    SearchType = item.BibType,
                    TestResult = TestState.UnRun,
                    TestResultFilePath = TestDataDir,
                    StandardResults = item.StandardResults
                };
                viewModels.Add(vm);
            }
            return viewModels;
        }

        /// <summary>
        /// 更新任务记录
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static bool UpdateTaskRecord(TestCaseListViewModel viewModel)
        {
            bool result = false;

            try
            {
                //写入测试结果数据
                if (viewModel.TestResultData == null || viewModel.TestResultData.Count == 0)
                {
                    viewModel.TestResult = TestState.NotFound;
                }
                else
                {
                    //保存到xml中
                    viewModel.TestResult = TestState.Found;

                    TestCase t = new TestCase()
                    {
                        FilePath = viewModel.FilePath,
                        Author = viewModel.SearchAuthor,
                        Title = viewModel.SearchTitle,
                        BibType = viewModel.SearchType,
                        SearchResults = viewModel.TestResultData,
                        StandardResults = viewModel.StandardResults
                    };

                    string path = viewModel.TestResultFilePath + "\\" + viewModel.ResolveName;

                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    path += "\\" + Path.GetFileNameWithoutExtension(t.FilePath) + "_data.xml";

                    Tools.SaveXml(t, path);

                    //待匹配，根据匹配模式进行匹配

                    if (viewModel.MatchWay == MatchWay.ExactMode) viewModel.TestResult = t.IsFullMatch() ? TestState.Pass : TestState.Unpass;
                    if (viewModel.MatchWay == MatchWay.PatternMode) viewModel.TestResult = t.IsMatch() ? TestState.Pass : TestState.Unpass;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("更新全文任务记录表异常：" + ex);
            }

            return result;
        }
        /// <summary>
        /// 删除任务记录
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static bool RemoveTaskRecord(TestCaseListViewModel viewModel)
        {
            if (File.Exists(viewModel.FilePath))
                File.Delete(viewModel.FilePath);

            if (File.Exists(viewModel.TestResultFilePath))
                File.Delete(viewModel.TestResultFilePath);

            return true;
        }

        /// <summary>
        /// 批量删除任务记录
        /// </summary>
        /// <returns></returns>
        public static bool RemoveTaskRecords(List<TestCaseListViewModel> viewModels)
        {
            bool result = false;

            if (viewModels.Count > 0)
            {
                foreach (var item in viewModels)
                {
                    File.Delete(item.TestResultFilePath);
                }
            }
            return result;
        }

        #endregion

        public static void Reset(bool allowCancelSelect = true)
        {
            if (ResolveDataPool.Data != null && ResolveDataPool.Data.Count > 0)
            {
                foreach (var item in ResolveDataPool.Data)
                {
                    item.TestResult = TestState.UnRun;
                    item.ResolveName = "";
                    item.DuringTime = "";

                    if (allowCancelSelect)
                        item.IsSelected = false;
                }
            }
        }

        public static void ReStart(TestCaseListViewModel item)
        {
            item.TestResult = TestState.Ready;
            item.DuringTime = "";

            if (item.TestResultData != null)
                item.TestResultData.Clear();

            ResolveDataPool.StartGetState();

        }


        public static List<string> Resolves { get; set; }
    }

    public delegate void DataPoolChangedHandle(object sender, DataPoolChangedEventArgs e);

    public class DataPoolChangedEventArgs : EventArgs
    {
        public string Message { set; get; }
        public DataPoolChangedEventArgs(string message)
        {
            this.Message = message;
        }
    }
    public enum PagerMode
    {
        /// <summary>
        /// 内存分页
        /// </summary>
        Memory = 0,
        /// <summary>
        /// 数据库分页
        /// </summary>
        DB = 1,

        /// <summary>
        /// 文件分页
        /// </summary>
        File = 2
    }
}


