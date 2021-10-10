using System.IO;
using ConfigurationData;

namespace FileWithKey
{
    public static class KeyHandler
    {
        public static void WriteKeyToTempFile(string key)
        {
            StreamWriter tempFile = new StreamWriter(PathsToDataFiles.nameOfTempFileForKey);
            tempFile.WriteLine(key);
            tempFile.Close();
        }
        public static string ReadKeyFromTempFileAndDeleteFile()
        {
            StreamReader tempFile = new StreamReader(PathsToDataFiles.nameOfTempFileForKey);
            string key = tempFile.ReadLine();
            tempFile.Close();
            File.Delete(PathsToDataFiles.nameOfTempFileForKey);
            return key;
        }
    }
}