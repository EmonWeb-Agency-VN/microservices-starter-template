using System.Text;

namespace Common.SharedKernel.Extensions
{
    public static class StringExtension
    {
        public static string RemoveDiacritics(this string input)
        {
            Dictionary<char, char> vietnameseCharMap = new Dictionary<char, char>
            {
                {'á', 'a'}, {'à', 'a'}, {'ả', 'a'}, {'ã', 'a'}, {'ạ', 'a'},
                {'ă', 'a'}, {'ắ', 'a'}, {'ằ', 'a'}, {'ẳ', 'a'}, {'ẵ', 'a'}, {'ặ', 'a'},
                {'â', 'a'}, {'ấ', 'a'}, {'ầ', 'a'}, {'ẩ', 'a'}, {'ẫ', 'a'}, {'ậ', 'a'},
                {'đ', 'd'},
                {'é', 'e'}, {'è', 'e'}, {'ẻ', 'e'}, {'ẽ', 'e'}, {'ẹ', 'e'},
                {'ê', 'e'}, {'ế', 'e'}, {'ề', 'e'}, {'ể', 'e'}, {'ễ', 'e'}, {'ệ', 'e'},
                {'í', 'i'}, {'ì', 'i'}, {'ỉ', 'i'}, {'ĩ', 'i'}, {'ị', 'i'},
                {'ó', 'o'}, {'ò', 'o'}, {'ỏ', 'o'}, {'õ', 'o'}, {'ọ', 'o'},
                {'ô', 'o'}, {'ố', 'o'}, {'ồ', 'o'}, {'ổ', 'o'}, {'ỗ', 'o'}, {'ộ', 'o'},
                {'ơ', 'o'}, {'ớ', 'o'}, {'ờ', 'o'}, {'ở', 'o'}, {'ỡ', 'o'}, {'ợ', 'o'},
                {'ú', 'u'}, {'ù', 'u'}, {'ủ', 'u'}, {'ũ', 'u'}, {'ụ', 'u'},
                {'ư', 'u'}, {'ứ', 'u'}, {'ừ', 'u'}, {'ử', 'u'}, {'ữ', 'u'}, {'ự', 'u'},
                {'ý', 'y'}, {'ỳ', 'y'}, {'ỷ', 'y'}, {'ỹ', 'y'}, {'ỵ', 'y'}
            };

            StringBuilder result = new StringBuilder();

            foreach (char c in input.ToLower())
            {
                if (vietnameseCharMap.TryGetValue(c, out char value))
                {
                    result.Append(value);
                }
                else if (char.IsLetterOrDigit(c))
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
        public static bool EqualsIgnoreCase(this string source, string target)
        {
            if (source == null && target == null)
            {
                return true;
            }
            if (source == null || target == null)
            {
                return false;
            }
            return source.Equals(target, StringComparison.OrdinalIgnoreCase);
        }
        public static bool ContainsIgnoreCase(this string source, string target)
        {
            if (source == null && target == null)
            {
                return true;
            }
            if (source == null || target == null)
            {
                return false;
            }
            return source.Contains(target, StringComparison.OrdinalIgnoreCase);
        }
    }
}
