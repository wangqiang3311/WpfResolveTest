using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfReovlveTest
{
    public class StaViewModel : INotifyPropertyChanged
    {
        private DateTime startTime;

        public DateTime StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                if (startTime == value) return;

                startTime = value;
                StartTimeFormat = startTime.ToString("yyyy-MM-dd HH:mm:ss");

                Notify("StartTime");
            }
        }

        private int totalCount;

        public int TotalCount
        {
            get
            {
                return totalCount;
            }
            set
            {
                if (totalCount == value) return;

                totalCount = value;
                Notify("TotalCount");
            }
        }



        private string startTimeFormat;

        public string StartTimeFormat
        {
            get
            {
                return startTimeFormat;
            }
            set
            {
                if (startTimeFormat == value) return;

                startTimeFormat = value;
                Notify("StartTimeFormat");
            }
        }

        private string endTimeFormat;

        public string EndTimeFormat
        {
            get
            {
                return endTimeFormat;
            }
            set
            {
                if (endTimeFormat == value) return;

                endTimeFormat = value;
                Notify("EndTimeFormat");
            }
        }
        private MatchWay matchWay= MatchWay.PatternMode;
        public MatchWay MatchWay
        {
            get { return matchWay; }

            set
            {
                matchWay = value;
                Notify("MatchWay");
            }

        }

        private DateTime endTime;

        public DateTime EndTime
        {
            get
            {
                return endTime;
            }
            set
            {
                if (endTime == value) return;

                endTime = value;

                EndTimeFormat = endTime.ToString("yyyy-MM-dd HH:mm:ss");


                Notify("EndTime");
            }
        }


        /// <summary>
        /// 当前测试持续时间
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

        /// <summary>
        /// 当前测试用例Id
        /// </summary>
        private int currentTestCaseId;

        public int CurrentTestCaseId
        {
            get
            {
                return currentTestCaseId;
            }
            set
            {
                if (currentTestCaseId == value) return;

                currentTestCaseId = value;
                Notify("CurrentTestCaseId");
            }
        }

        /// <summary>
        /// 当前解析器名称
        /// </summary>
        private string currentResolveName;

        public string CurrentResolveName
        {
            get
            {
                return currentResolveName;
            }
            set
            {
                if (currentResolveName == value) return;

                currentResolveName = value;
                Notify("CurrentResolveName");
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
}
