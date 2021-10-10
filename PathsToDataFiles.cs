using System.Collections.Generic;

namespace ConfigurationData
{
    public static class PathsToDataFiles
    {
        public static string accountFilePath = "account.data";
        public static string usernameFilePath = "username.data";
        public static string accountLabelFilePath = "account_label.lbl";
        public static string usernameLabelFilePath = "username_label.lbl";
        public static string nameOfTempFileForKey = "common_file_with_entered_key.data";
        public const int amountOfFilesOfDataBase = 4;
        public static List<string> listOfAllDataBaseFiles = new List<string>()
        {
            accountFilePath,
            usernameFilePath,
            accountLabelFilePath,
            usernameLabelFilePath
        };
    }
}
