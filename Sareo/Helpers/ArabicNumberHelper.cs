namespace Sareoo.Helpers
{
    public static class ArabicNumberHelper
    {
        public static string ToArabicNumerals(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Replace('0', '٠')
                        .Replace('1', '١')
                        .Replace('2', '٢')
                        .Replace('3', '٣')
                        .Replace('4', '٤')
                        .Replace('5', '٥')
                        .Replace('6', '٦')
                        .Replace('7', '٧')
                        .Replace('8', '٨')
                        .Replace('9', '٩');
        }

        public static string ToArabicNumerals(int input)
        {
            return ToArabicNumerals(input.ToString());
        }

        public static string ToArabicNumerals(double input)
        {
            return ToArabicNumerals(input.ToString());
        }
    }
}
