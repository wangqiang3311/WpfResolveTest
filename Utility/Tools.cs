using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace WpfReovlveTest
{
    public class Tools
    {
        private static XmlNodeList SelectNodes(string xpath, string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            return doc.SelectNodes(xpath);
        }
        private static XmlNode SelectSingleNode(string xpath, string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            return doc.SelectSingleNode(xpath);
        }

        public static T SelectSingle<T>(string selectNode, string path) where T : IxmlToObject<T>, new()
        {
            T item = new T();

            var node = SelectSingleNode(selectNode, path);

            if (node != null)
            {
                T obj = item.XmlToObject(node);

            }
            return item;
        }
        public static void SaveXml(IxmlFormat obj, string path)
        {
            XmlDocument doc = new XmlDocument();
            var xml = obj.ToXml();
            doc.LoadXml(xml);
            doc.Save(path);
        }

        public static List<T> SelectList<T>(string selectNode,string path) where T:new()
        {
            List<T> items = new List<T>();

            var nodes = SelectNodes(selectNode,path);

            if (nodes != null)
            {
                var type = typeof(T);
                var properties = type.GetProperties().ToList();

                foreach (XmlNode node in nodes)
                {
                    T config = new T();

                    foreach (XmlAttribute a in node.Attributes)
                    {
                        string name = a.Name;
                        string value = a.Value;

                        var p = properties.FirstOrDefault(t => t.Name.ToLower() == name.ToLower());

                        if (p != null)
                        {
                           p.SetValue(config, value, null);
                        }
                    }
                    items.Add(config);
                }
            }
            return items;
        }

        /// <summary>  
        /// 获取目录path下所有子文件名  
        /// </summary>  
        public static List<string> GetAllFiles(String path)
        {
            List<string> fileNames = new List<string>();
            if (System.IO.Directory.Exists(path))
            {
                //所有子文件名  
                string[] files = System.IO.Directory.GetFiles(path);
                foreach (string file in files)
                {
                    fileNames.Add(file);
                }
                //所有子目录名  
                string[] Dirs = System.IO.Directory.GetDirectories(path);
                foreach (string dir in Dirs)
                {
                    var tmp = GetAllFiles(dir);  //子目录下所有子文件名  
                    if (tmp.Count > 0)
                    {
                        fileNames.AddRange(tmp);
                    }
                }
            }
            return fileNames;
        }


        public static string ReplaceSpecialChars(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";

            string[] specialChars = { "&", "<", ">", "\"", "'" };
            string[] entities = { "&amp;", "&lt;", "&gt;", "&quot;", "&apos;" };

            int i = 0;
            foreach (var item in specialChars)
            {
                content = content.Replace(item, entities[i]);
                i++;
            }
            return content;
        }

        /// <summary>
        /// WPF中查找元素的父元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i_dp"></param>
        /// <returns></returns>
        public static T FindParent<T>(DependencyObject i_dp) where T : DependencyObject
        {
            DependencyObject dobj = (DependencyObject)VisualTreeHelper.GetParent(i_dp);
            if (dobj != null)
            {
                if (dobj is T)
                {
                    return (T)dobj;
                }
                else
                {
                    dobj = FindParent<T>(dobj);
                    if (dobj != null && dobj is T)
                    {
                        return (T)dobj;
                    }
                }
            }
            return null;
        }



        /// <summary>
        ///WPF查找元素的子元素
        /// </summary>
        /// <typeparam name="childItem"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static childItem FindVisualChild<childItem>(DependencyObject obj)
    where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

    }
}
