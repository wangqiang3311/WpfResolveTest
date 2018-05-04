using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace WpfReovlveTest
{
    public interface IxmlFormat
    {
        string ToXml();
    }
    public interface IxmlToObject<T>
    {
        T XmlToObject(XmlNode node);
    }

    public interface IResult
    {
        bool IsMatch();

        bool IsFullMatch();
    }

    public class TestCase : IxmlFormat, IxmlToObject<TestCase>, IResult
    {
        public string Title { set; get; }
        public string Author { set; get; }
        public string BibType { set; get; }
        /// <summary>
        /// 测试用例路径
        /// </summary>
        public string FilePath { set; get; }

        public List<SearchResult> StandardResults { set; get; }

        public List<SearchResult> SearchResults { set; get; }

        public string ToXml()
        {
            var title = Tools.ReplaceSpecialChars(this.Title);
            var author = Tools.ReplaceSpecialChars(this.Author);
            var bibType = Tools.ReplaceSpecialChars(this.BibType);

            StringBuilder sb = new StringBuilder();

            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");

            if (SearchResults != null && SearchResults.Count > 0)
            {
                sb.AppendFormat("<case  title=\"{0}\"  author=\"{1}\"  bibType=\"{2}\" >", title, author, bibType);

                foreach (SearchResult item in SearchResults)
                {
                    sb.AppendFormat("<item title=\"{0}\">", Tools.ReplaceSpecialChars(item.Title));

                    foreach (var fitem in item.Fields)
                    {
                        sb.AppendFormat("<field content=\"{0}\"/>", Tools.ReplaceSpecialChars(fitem));
                    }
                    sb.AppendFormat("</item>");
                }

                sb.Append("</case>");
            }
            return sb.ToString();
        }

        public TestCase XmlToObject(XmlNode node)
        {
            string title = node.Attributes["title"].Value;
            string author = node.Attributes["author"].Value;
            string bibType = node.Attributes["bibType"].Value;

            this.Title = title;
            this.Author = author;
            this.BibType = bibType;


            this.StandardResults = new List<SearchResult>();
            XmlNodeList itemNodes = node.SelectNodes("//item");
            foreach (XmlNode n in itemNodes)
            {
                var sr = new SearchResult()
                {
                    Title = n.Attributes["title"].Value,
                    Fields = new List<string>()
                };

                var fileds = n.ChildNodes;

                foreach (XmlNode fn in fileds)
                {
                    sr.Fields.Add(fn.Attributes["content"].Value);
                }
                this.StandardResults.Add(sr);
            }

            return this;
        }

        public bool IsMatch()
        {
            if (SearchResults != null && SearchResults.Count > 0) return true;
            return false;
        }
        public bool IsFullMatch()
        {
            bool isPass = false;

            int j = 0;

            if (StandardResults != null && SearchResults != null)
            {
                if (StandardResults.Count == SearchResults.Count)
                {
                    for (int i = 0; i < StandardResults.Count; i++)
                    {
                        if (SearchResults[i].Title == StandardResults[i].Title)
                        {
                            j++;
                        }
                        else
                        {
                            j = 0;
                            break;
                        }
                    }

                }
            }

            if (j > 0) isPass = true;

            return isPass;
        }
    }

    public class SearchResult
    {
        public string Title { set; get; }

        public List<string> Fields { set; get; }

    }
}
