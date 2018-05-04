using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfReovlveTest
{
    /// <summary>
    /// 测试用例列表
    /// </summary>
    public class TestCaseListViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }

        public Guid TestId { set; get; }

        /// <summary>
        /// 按标题搜索
        /// </summary>
        public string SearchTitle { set; get; }

        /// <summary>
        /// 按作者搜索
        /// </summary>
        public string SearchAuthor { set; get; }


        /// <summary>
        /// 按bib类型搜索
        /// </summary>
        public string SearchType { set; get; }

        /// <summary>
        ///测试用例路径
        /// </summary>
        public string FilePath { set; get; }


        /// <summary>
        /// 测试结果保存路径
        /// </summary>
        private string testResultFilePath;

        public string TestResultFilePath
        {
            get
            {
                return testResultFilePath;
            }
            set
            {
                if (testResultFilePath == value) return;

                testResultFilePath = value;
                Notify("TestResultFilePath");
            }
        }

        public string resolveName;
        /// <summary>
        /// 解析器名称
        /// </summary>
        public string ResolveName
        {

            get
            {
                return resolveName;
            }
            set
            {
                if (resolveName == value) return;

                resolveName = value;
                Notify("ResolveName");
            }
        }


        /// <summary>
        /// 每条用例花费的时间
        /// </summary>
        private string duringTime;

        public string DuringTime
        {
            get
            {
                return duringTime;
            }
            set
            {
                if (duringTime == value) return;

                duringTime = value;
                Notify("DuringTime");
            }
        }
        public List<SearchResult> StandardResults { set; get; }
        /// <summary>
        /// 测试结果数据
        /// </summary>
        private List<SearchResult> testResultData;
        public List<SearchResult> TestResultData
        {
            get
            {
                return testResultData;
            }
            set
            {
                if (testResultData == value) return;

                testResultData = value;
                Notify("TestResultData");
            }
        }

        /// <summary>
        /// 测试结果
        /// </summary>
        private TestState testResult;
        public TestState TestResult
        {
            get
            {
                return testResult;
            }
            set
            {

                if (testResult == value) return;

                testResult = value;

                if (testResult == TestState.Running || testResult == TestState.Found)
                {
                    this.CanDelete = false;
                    this.CanReSummit = false;
                }
                else
                {
                    this.CanDelete = true;
                    this.CanReSummit = true;
                }
                Notify("TestResult");
            }
        }

        private bool isSelected;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }

            set
            {
                isSelected = value;
                Notify("IsSelected");
            }
        }

        private bool canLookDetail = true;
        /// <summary>
        /// 是否可以查看详情
        /// </summary>
        public bool CanLookDetail
        {
            get { return canLookDetail; }

            set
            {
                canLookDetail = value;
                Notify("CanLookDetail");
            }
        }


        private bool canOpenFilePath = true;
        /// <summary>
        /// 是否可以打开测试用例路径
        /// </summary>
        public bool CanOpenFilePath
        {
            get { return canOpenFilePath; }

            set
            {
                canOpenFilePath = value;
                Notify("CanOpenFilePath");
            }
        }

        private bool canDelete = true;
        /// <summary>
        /// 是否可以删除
        /// </summary>
        public bool CanDelete
        {
            get { return canDelete; }

            set
            {
                canDelete = value;
                Notify("CanDelete");
            }
        }
        private MatchWay matchWay;
        public MatchWay MatchWay
        {
            get { return matchWay; }

            set
            {
                matchWay = value;
                Notify("MatchWay");
            }

        }

        private bool canReSummit = true;
        /// <summary>
        /// 是否可以重提
        /// </summary>
        public bool CanReSummit
        {
            get { return canReSummit; }

            set
            {
                canReSummit = value;
                Notify("CanReSummit");
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propName)
        {

            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }

    public enum MatchWay
    {
        ExactMode = 1,
        PatternMode = 2
    }
    public enum TestState
    {
        /// <summary>
        /// 未通过
        /// </summary>
        Unpass = 0,
        /// <summary>
        /// 通过
        /// </summary>
        Pass = 1,
        /// <summary>
        /// 未执行
        /// </summary>
        UnRun = -1,

        /// <summary>
        ///未找到
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// 搜索出错
        /// </summary>
        Error = 3,

        /// <summary>
        /// 其它
        /// </summary>
        Other = 4,
        /// <summary>
        /// 正在运行
        /// </summary>
        Running = 5,
        /// <summary>
        /// 已经找到，等待匹配
        /// </summary>
        Found = 6,
        /// <summary>
        /// 准备运行，如选中一个或者多个测试用例，然后执行
        /// </summary>
        Ready = 8,

        /// <summary>
        /// 取消
        /// </summary>
        Cancel=9
    }

    public class PDFTestListViewModelComparer : IEqualityComparer<TestCaseListViewModel>
    {
        public bool Equals(TestCaseListViewModel x, TestCaseListViewModel y)
        {
            return x.Id == y.Id;
        }
        public int GetHashCode(TestCaseListViewModel obj)
        {
            return base.GetHashCode();
        }
    }

}
