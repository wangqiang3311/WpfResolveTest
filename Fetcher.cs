using NoteFirst.Proofread.Server.Common.Interface;
using ResourceShare.UserClient.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using serverModel = NoteFirst.Proofread.Server.Common.Model;
using s = Samson.NoteFirst.Model;

namespace WpfReovlveTest
{
    /// <summary>
    /// 抓取器，用来获取测试状态
    /// </summary>
    public class Fetcher
    {

        #region 属性和构造函数

        static Dictionary<string, string> maps = new Dictionary<string, string>();
        static Fetcher()
        {
            maps.Add("Title", "题录标题");
            maps.Add("ForeignTitle", "外文标题");
            maps.Add("Type", "题录类型");
            maps.Add("Media", "题录媒体");
            maps.Add("MediaEn", "外文题录媒体");
            maps.Add("Year", "年");
            maps.Add("Volume", "卷");
            maps.Add("Issue", "期");
            maps.Add("PageScope", "题录页面范围");
            maps.Add("CLC", "题录CLC");
            maps.Add("DOI", "题录DOI");
            maps.Add("ISSN", "题录ISSN");
            maps.Add("Press", "题录出版社");
            maps.Add("Place", "题录出版地");
            maps.Add("Url", "题录Url");
            maps.Add("PublishDate", "题录出版日期");
            maps.Add("PrintDate", "题录出印日期");
            maps.Add("Country", "题录国家");
            maps.Add("Version", "题录版本");
            maps.Add("Prites", "题录版次");
            maps.Add("Words", "题录字数");
            maps.Add("CustomString1", "用户自定义字符串1（必须有值，暂时放题录URL）");
            maps.Add("CustomString2", "用户自定义字符串2（必须有值，暂时放全文URL）");
            maps.Add("DutyPerson", "DutyPerson");
            maps.Add("Language", "Language");
            maps.Add("Abstract", "Abstract");
            maps.Add("ForeignAbstract", "ForeignAbstract");
            maps.Add("Prints", "Prints");
            maps.Add("PageCount", "PageCount");
            maps.Add("ReferenceCount", "ReferenceCount");
            maps.Add("References", "References");
            maps.Add("Citations", "Citations");
        }
        /// <summary>
        /// 数据源
        /// </summary>
        public List<TestCaseListViewModel> Source { set; get; }

        public List<TestCaseListViewModel> SourceFrom = ResolveDataPool.Data;


        /// <summary>
        /// 数据执行中的状态过滤
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private bool ExcuteStateFilter(TestCaseListViewModel d)
        {
            return d.TestResult == TestState.UnRun;
        }
        private bool ExcuteStateFilterSpecial(TestCaseListViewModel d)
        {
            return d.TestResult == TestState.Ready;
        }

        public int SourceCount
        {
            get
            {
                if (Source == null)
                {
                    return 0;
                }
                else
                {
                    var filter = SourceFrom.Where(d => ExcuteStateFilterSpecial(d));

                    if (filter.Count() == 0)

                        filter = SourceFrom.Where(d => ExcuteStateFilter(d));

                    this.Source = filter.ToList();
                    return this.Source.Count;
                }
            }
        }
        private WorkState workState = WorkState.Stopped;
        /// <summary>
        /// 工作状态,初始状态为停止状态
        /// </summary>
        public WorkState WorkState
        {
            set
            {
                workState = value;
            }
            get
            {
                return workState;
            }
        }

        static CancellationTokenSource cancel = null;


        #endregion

        public void Start()
        {
            cancel = new CancellationTokenSource();

            var task = Task.Factory.StartNew((token) =>
            {
                try
                {
                    Logger.Log("抓取器开启了工作线程,共有" + this.SourceCount + "个任务等待测试");
                    Excute(token);
                }
                catch (Exception ex)
                {
                    Logger.Debug("执行全文任务状态更新时异常：" + ex.Message + "," + ex.StackTrace);
                }
            }, cancel.Token);
        }

        public void Cancel()
        {
            if (cancel != null)
            {
                cancel.Cancel();
            }
            Logger.Log("取消状态线程的执行");
        }

        IDatabaseProcess resolveIDatabaseProcess = null;
        IDatabaseProcessBiblioraphyUpdateEx resolveIDatabaseProcessBiblioraphyUpdateEx = null;
        IDatabaseProcessSearch resolveIDatabaseProcessSearch = null;

        Dictionary<string, object> ResolveCache = new Dictionary<string, object>();

        private void ExcuteCore(TestCaseListViewModel model)
        {
            //调用解析器获取结果

            var assemblyName = string.Format("NoteFirst.Proofread.SubServer.{0}", model.ResolveName);
            var className = string.Format("NoteFirst.Proofread.SubServer.{0}.{0}Process", model.ResolveName);

            object resolve = null;

            if (ResolveCache.ContainsKey(model.ResolveName))
            {
                resolve = ResolveCache[model.ResolveName];
            }
            else
            {
                var resolvePath = MainWindow.appDir + @"\dll\NoteFirst.Proofread.SubServer." + model.resolveName + ".dll";

                if (!File.Exists(resolvePath))
                {
                    model.TestResult = TestState.Error;
                    Logger.Debug("解析器文件不存在");

                    return;
                }


                Assembly ass = Assembly.LoadFrom(resolvePath);

                Type type = ass.GetType(className);

                if (type == null)
                {
                    model.TestResult = TestState.Error;
                    return;
                }
                // 实例化该类  

                resolve = Activator.CreateInstance(type);

                ResolveCache.Add(model.ResolveName, resolve);

            }

            if (resolve is IDatabaseProcess)
            {
                resolveIDatabaseProcess = (IDatabaseProcess)resolve;
            }
            if (resolve is IDatabaseProcessBiblioraphyUpdateEx)
            {
                resolveIDatabaseProcessBiblioraphyUpdateEx = (IDatabaseProcessBiblioraphyUpdateEx)resolve;
            }
            if (resolve is IDatabaseProcessSearch)
            {
                resolveIDatabaseProcessSearch = (IDatabaseProcessSearch)resolve;
            }

            if (resolveIDatabaseProcessSearch == null)
            {
                if (resolveIDatabaseProcessBiblioraphyUpdateEx == null)
                {
                    MessageBox.Show("此在线数据库未实现题录更新接口IDatabaseProcessBiblioraphyUpdateEx", "提示信息");
                    return;
                }

                if (resolveIDatabaseProcess.Search(model.SearchTitle, model.SearchAuthor, model.SearchType))
                {
                    var searchResults = resolveIDatabaseProcessBiblioraphyUpdateEx.GetSearchResult();

                    model.TestResultData = GetSearchResult(searchResults, sr => resolveIDatabaseProcessBiblioraphyUpdateEx.GetBibliographyBiblioraphyUpdate(sr));

                }
            }
            else
            {
                if (!string.IsNullOrEmpty(model.SearchAuthor))
                {
                    resolveIDatabaseProcessSearch.AdvSearch(new s.AdvSeacherParam()
                    {
                        Auther = model.SearchAuthor,
                        Title = model.SearchTitle
                    });
                }
                else
                {
                    resolveIDatabaseProcessSearch.SimpleSearch(model.SearchTitle);
                }

                if (resolveIDatabaseProcess == null)
                {
                    MessageBox.Show("此在线数据库未实现题录更新接口IDatabaseProcess", "提示信息");
                    return;
                }

                var searchResults = resolveIDatabaseProcess.GetResult();
                model.TestResultData = GetSearchResult(searchResults, sr => resolveIDatabaseProcess.GetBibliography(sr));
            }
        }


        private List<SearchResult> GetSearchResult(serverModel.SearchResults searchResults, Func<serverModel.SearchResult, Samson.NoteFirst.Model.Bibliography> GetBib)
        {
            List<SearchResult> results = new List<SearchResult>();


            foreach (serverModel.SearchResult sr in searchResults)
            {

                Samson.NoteFirst.Model.Bibliography bib = null;

                if (GetBib != null)
                {
                    bib = GetBib(sr);
                }

                if (bib != null)
                {

                    SearchResult result = new SearchResult();

                    results.Add(result);

                    result.Title = sr.Title;

                    List<string> fields = new List<string>();

                    result.Fields = fields;

                    foreach (System.Reflection.PropertyInfo p in typeof(Samson.NoteFirst.Model.Bibliography).GetProperties())
                    {
                        #region  反射字段

                        if (maps.Keys.Contains(p.Name))
                        {
                            Object value = p.GetValue(bib, null);
                            if (value != null)
                            {
                                if (value is DateTime)
                                {
                                    fields.Add(string.Format("{0}：{1}", maps[p.Name], ((DateTime)value).ToShortDateString()));
                                }
                                else
                                {
                                    fields.Add(string.Format("{0}：{1}", maps[p.Name], value.ToString()));
                                }
                            }
                        }
                        else
                        {

                            if (p.Name.Equals("Authors"))
                            {
                                Object value = p.GetValue(bib, null);
                                if (value != null)
                                {
                                    s.Author[] authors = value as s.Author[];
                                    foreach (s.Author author1 in authors)
                                    {
                                        foreach (PropertyInfo propertyInfo in typeof(s.Author).GetProperties())
                                        {
                                            if (propertyInfo.Name.Equals("FullName"))
                                            {
                                                fields.Add(string.Format("作者全名：{0}", author1.FullName));
                                            }
                                            if (propertyInfo.Name.Equals("Organization"))
                                            {
                                                if (!string.IsNullOrEmpty(author1.Organization))
                                                {
                                                    fields.Add(string.Format("作者机构：{0}", author1.Organization));
                                                }
                                            }
                                            if (propertyInfo.Name.Equals("FullNameEn"))
                                            {
                                                if (!string.IsNullOrEmpty(author1.FullNameEn))
                                                {
                                                    fields.Add(string.Format("作者外文名称：{0}", author1.FullNameEn));
                                                }
                                            }
                                            if (propertyInfo.Name.Equals("IsOrganization"))
                                            {
                                                fields.Add(string.Format("是否为作者机构：{0}", author1.IsOrganization));

                                            }
                                        }
                                    }
                                }
                            }


                            if (p.Name.Equals("Keywords"))
                            {
                                Object value = p.GetValue(bib, null);
                                if (value != null)
                                {
                                    string tempStr = string.Empty;
                                    string[] strArray = (string[])value;
                                    if (strArray != null && strArray.Length > 0)
                                    {
                                        for (int i = 0; i < strArray.Length; i++)
                                        {
                                            tempStr += strArray[i] + ";";
                                        }
                                    }

                                    fields.Add(string.Format("Keywords：{0}", tempStr.Trim(';')));
                                }
                            }
                            if (p.Name.Equals("ForeignKeywords"))
                            {
                                Object value = p.GetValue(bib, null);
                                if (value != null)
                                {
                                    string tempStr = string.Empty;
                                    string[] strArray = (string[])value;
                                    if (strArray != null && strArray.Length > 0)
                                    {
                                        for (int i = 0; i < strArray.Length; i++)
                                        {
                                            tempStr += strArray[i] + ";";
                                        }
                                    }
                                    fields.Add(string.Format("ForeignKeywords：{0}", tempStr.Trim(';')));
                                }
                            }
                        }

                        #endregion
                    }

                }
            }
            return results;
        }
        private void ExcutePiece(IEnumerable<TestCaseListViewModel> source, CancellationToken t)
        {
            foreach (var item in source)
            {
                var start = DateTime.Now;

                if (t.IsCancellationRequested)
                {
                    item.TestResult = TestState.Cancel;
                    return;
                }

                item.TestResult = TestState.Running;

                try
                {
                    //业务执行核心
                    ExcuteCore(item);

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    item.TestResult = TestState.Error;
                    Logger.Debug("执行ExcuteCore时报错：" + ex.Message);
                    continue;
                }

                var end = DateTime.Now;

                item.DuringTime = GetCostTime(start, end);

                MainWindow.StaModel.CurrentResolveName = item.ResolveName;
                MainWindow.StaModel.CurrentTestCaseId = item.Id;
                MainWindow.StaModel.TotalCount = item.TestResultData.Count;

                UpdateModel(item, t);
                MainWindow.StaModel.EndTime = DateTime.Now;

                MainWindow.StaModel.DuringTime = GetCostTime(MainWindow.StaModel.StartTime, MainWindow.StaModel.EndTime);
            }
        }
        private string GetCostTime(DateTime start, DateTime end)
        {
            var totalSeconds = end.Subtract(start).TotalSeconds;

            var totalMinutes = totalSeconds / 60;

            var second = (int)totalSeconds % 60;

            var hours = (int)totalMinutes / 60;

            var minutes = (int)totalMinutes % 60;

            return string.Format("{0} 时：{1}分：{2}秒", hours, minutes, second);
        }

        /// <summary>
        /// 每次接口请求，执行多个全文任务
        /// </summary>
        /// <param name="item"></param>
        private void Excute(object cancelToken)
        {
            CancellationToken t = (CancellationToken)cancelToken;
            FetchSource();

            if (this.SourceCount > 0)
            {
                WorkState = WorkState.Running;
            }
            else
            {
                Logger.Log("当前抓取器可执行数为0，准备退出");
                WorkState = WorkState.Stopped;
                return;
            }

            int maxNum = Global.FullTextStateGetLargeData;//5万数据预估，向服务器请求一次得5分钟

            while (this.SourceCount > 0 && !t.IsCancellationRequested)
            {
                int count = this.SourceCount;

                //分解大数据为小数据，然后执行
                if (count > maxNum)
                {
                    int threadCount = count % maxNum == 0 ? count / maxNum : count / maxNum + 1;

                    for (int i = 1; i <= threadCount; i++)
                    {
                        var pagerDatas = this.Source.Skip((i - 1) * maxNum).Take(maxNum);

                        ExcutePiece(pagerDatas, t);
                    }
                }
                else
                {
                    ExcutePiece(this.Source, t);
                }

                //每次不管当前的数据有没有消费掉，都会去拿新的数据
                FetchSource();

                if (SourceCount == 0)
                {
                    WorkState = WorkState.Stopped;
                    Logger.Log(string.Format("当前抓取全文状态的线程Id：{0},已完成任务，准备退出", Thread.CurrentThread.ManagedThreadId));
                    break;
                }
            }
            WorkState = WorkState.Stopped;
            Logger.Log(string.Format("当前抓取全文状态的线程Id：{0},已完成任务，准备退出", Thread.CurrentThread.ManagedThreadId));
        }

        private void UpdateModel(TestCaseListViewModel model, CancellationToken t)
        {
            if (t.IsCancellationRequested)
            {
                model.TestResult = TestState.Cancel;
            }

            ////更新数据
            ResolveDataPool.UpdateTaskRecord(model);
        }

        /// <summary>
        /// 每次都全盘扫描数据池中的数据
        /// </summary>
        private void FetchSource()
        {
            int count = 0;

            lock (ResolveDataPool.SyncRoot)
            {
                if (ResolveDataPool.Data.Count > 0)
                {

                    var filter = ResolveDataPool.Data.Where(d => ExcuteStateFilterSpecial(d));

                    if (filter.Count() == 0)

                        filter = ResolveDataPool.Data.Where(d => ExcuteStateFilter(d));

                    this.Source = filter.ToList();
                    count = this.Source.Count;
                }
            }

            if (count == 0)
            {
                if (this.Source != null)
                {
                    this.Source.Clear();
                }
            }
        }
    }

    /// <summary>
    /// 自定义抓取器工作状态
    /// </summary>
    public enum WorkState
    {
        /// <summary>
        /// 运行
        /// </summary>
        Running,
        /// <summary>
        /// 停止
        /// </summary>
        Stopped
    }
}