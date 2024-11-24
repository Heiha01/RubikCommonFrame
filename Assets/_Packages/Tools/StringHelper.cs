namespace Tools
{
    public static class StringHelper
    {
        //首字母大写
        public static string CapitalizeFirstLetter(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            
            if (input.Length == 1)
            {
                return input.ToUpper();
            }
            
            return char.ToUpper(input[0]) + input[1..];
        }
    }
}
