using System.Globalization;

namespace Common.SharedKernel.Extensions
{
    public static class DateTimeExtension
    {
        public static string FormatTime(this DateTimeOffset dataTime, string format = "")
        {
            if (string.IsNullOrEmpty(format))
            {
                return dataTime.ToString(TimeFormat.Default);
            }
            return dataTime.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string FormatDate(this DateTimeOffset dataTime, string format = "")
        {
            if (string.IsNullOrEmpty(format))
            {
                return dataTime.ToString(DateFormat.Default);
            }
            return dataTime.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string FormatDateTime(this DateTimeOffset dataTime, string format = "")
        {
            if (string.IsNullOrEmpty(format))
            {
                return dataTime.ToString(DateTimeFormat.Default);
            }
            return dataTime.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string FormatDateTimeWithSecond(this DateTimeOffset dataTime, string format = "")
        {
            if (string.IsNullOrEmpty(format))
            {
                return dataTime.ToString(DateTimeFormat.DefaultWithSecond);
            }
            return dataTime.ToString(format, CultureInfo.InvariantCulture);
        }

        public static DateTimeOffset ConvertToDateTime(string date, string format = "")
        {
            format = string.IsNullOrEmpty(format) ? DateTimeFormat.Default : format;
            return DateTimeOffset.ParseExact(date, format, CultureInfo.CurrentCulture);
        }
        public static bool TryConvertDateTime(string data, out DateTimeOffset dateTime, string format = "")
        {
            format = string.IsNullOrEmpty(format) ? DateTimeFormat.Default : format;
            return DateTimeOffset.TryParseExact(data, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime);
        }
    }

    public static class DateFormat
    {
        public static readonly string Default = "dd/MM/yyyy";
    }

    public static class TimeFormat
    {
        public static readonly string Default = "HH:mm";
        public static readonly string DefaultWithSecond = "HH:mm:ss";
    }

    public static class DateTimeFormat
    {
        public static readonly string Default = "dd/MM/yyyy HH:mm";
        public static readonly string DefaultWithSecond = "dd/MM/yyyy HH:mm:ss";
        public static readonly string DefaultDate = "dd/MM/yyyy";

    }
}
