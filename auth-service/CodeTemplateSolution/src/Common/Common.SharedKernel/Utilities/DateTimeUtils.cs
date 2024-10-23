namespace Common.SharedKernel.Utilities
{
    public class DateTimeUtils
    {
        public static string GetDateTime(DateTime value) => value.ToString("yyyyMMddHHmmssffff");
        public static string GetDateTimeWithoutMiliseconds(DateTime value) => value.ToString("yyyyMMddHHmmss");


    }


}
