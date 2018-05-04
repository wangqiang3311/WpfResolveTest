using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WpfReovlveTest
{
    /// <summary>
    /// 元素隐藏转换器
    /// </summary>
    public class ValidVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 全文传递按钮处理
    /// </summary>
    public class cvtfulltextImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = "";

            var canExcute = (bool)value;

            int btnType = -1;
            int.TryParse(parameter.ToString(), out btnType);

            switch (btnType)
            {
                case 0:
                    //背景
                    if (canExcute)
                    {
                        str = "#3091f2";
                    }
                    else
                    {
                        str = "#e1e1e1";
                    }
                    break;


                case 1:
                    //边框
                    if (canExcute)
                    {
                        str = "#0e6dcd";
                    }
                    else
                    {
                        str = "#c9c9c9";
                    }
                    break;

                case 2:
                    //前景色

                    if (canExcute)
                    {
                        str = "#fff";
                    }
                    else
                    {
                        str = "#c1c1c1";
                    }
                    break;
            }
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// 全文传递列表按钮处理
    /// </summary>
    public class cvtfulltextOperImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = "";

            var canExcute = (bool)value;

            string btnType = parameter.ToString();

            switch (btnType)
            {
                case "00":
                    //打开全文路径
                    if (canExcute)
                    {
                        imagePath = "pack://application:,,,/Skin/Images/open.png";
                    }
                    else
                    {
                        imagePath = "pack://application:,,,/Skin/Images/open_dis.png";
                    }
                    break;
                case "01":
                    //打开全文路径
                    if (canExcute)
                    {
                        imagePath = "pack://application:,,,/Skin/Images/openhover.png";
                    }
                    else
                    {
                        imagePath = "pack://application:,,,/Skin/Images/open_dis.png";
                    }
                    break;

                case "10":
                    //查看全文
                    if (canExcute)
                    {
                        imagePath = "pack://application:,,,/Skin/Images/back.png";
                    }
                    else
                    {
                        imagePath = "pack://application:,,,/Skin/Images/back_dis.png";
                    }
                    break;
                case "11":
                    //取消全文
                    if (canExcute)
                    {
                        imagePath = "pack://application:,,,/Skin/Images/backhover.png";
                    }
                    else
                    {
                        imagePath = "pack://application:,,,/Skin/Images/back_dis.png";
                    }
                    break;

                case "20":
                    //删除

                    if (canExcute)
                    {
                        imagePath = "pack://application:,,,/Skin/Images/delete.png";
                    }
                    else
                    {
                        imagePath = "pack://application:,,,/Skin/Images/delete_dis.png";
                    }
                    break;
                case "21":
                    //删除

                    if (canExcute)
                    {
                        imagePath = "pack://application:,,,/Skin/Images/deletehover.png";
                    }
                    else
                    {
                        imagePath = "pack://application:,,,/Skin/Images/delete_dis.png";
                    }
                    break;
                case "30":
                    //再次提交

                    if (canExcute)
                    {
                        imagePath = "pack://application:,,,/Skin/Images/review.png";
                    }
                    else
                    {
                        imagePath = "pack://application:,,,/Skin/Images/review_dis.png";
                    }
                    break;
                case "31":
                    //再次提交

                    if (canExcute)
                    {
                        imagePath = "pack://application:,,,/Skin/Images/reviewHover.png";
                    }
                    else
                    {
                        imagePath = "pack://application:,,,/Skin/Images/review_dis.png";
                    }
                    break;
            }
            return imagePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class cvtImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string loveImagePath = "";

            var isLove = (bool)value;

            if (isLove)
            {
                loveImagePath = "../Skin/Images/love_hover.png";
            }
            else
            {
                loveImagePath = "../Skin/Images/love.png";

            }
            return loveImagePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class MatchWayToBoolConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MatchWay s = (MatchWay)value;
            return s == (MatchWay)int.Parse(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isChecked = (bool)value;
            if (!isChecked)
            {
                return null;
            }
            return (MatchWay)int.Parse(parameter.ToString());
        }
    }



    public class CellBackGroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string bgColor = "";
            var callsate = (TestState)value;


            switch (callsate)
            {
                case TestState.Error:

                    bgColor = "Yellow";
                    break;
                case TestState.NotFound:

                    bgColor = "Gold";
                    break;
                case TestState.Unpass:
                    bgColor = "Orange";

                    break;
                default:
                    break;
            }

            return bgColor;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
