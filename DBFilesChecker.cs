using System.Collections.Generic;
using System.IO;
using ConfigurationData;

namespace DB_Files
{
    static class DBFilesChecker
    {
        public static List<string> GetExistingFilesOfDataBase(string pathToFolder)
        {
            List<string> pathsToExistingFiles = new List<string>() { };
            if (File.Exists(pathToFolder + PathsToDataFiles.accountFilePath))
            {
                pathsToExistingFiles.Add(pathToFolder + PathsToDataFiles.accountFilePath);
            }
            if (File.Exists(pathToFolder + PathsToDataFiles.accountLabelFilePath))
            {
                pathsToExistingFiles.Add(pathToFolder + PathsToDataFiles.accountLabelFilePath);
            }
            if (File.Exists(pathToFolder + PathsToDataFiles.usernameFilePath))
            {
                pathsToExistingFiles.Add(pathToFolder + PathsToDataFiles.usernameFilePath);
            }
            if (File.Exists(pathToFolder + PathsToDataFiles.usernameLabelFilePath))
            {
                pathsToExistingFiles.Add(pathToFolder + PathsToDataFiles.usernameLabelFilePath);
            }
            return pathsToExistingFiles;
        }
        public static List<string> GetDistinctNameFiles(string pathToFolder, List<string> existingFiles)
        {
            List<string> namesOfDistinctFiles = new List<string>();
            foreach (var elem in PathsToDataFiles.listOfAllDataBaseFiles)
            {
                if (!existingFiles.Contains(pathToFolder + elem))
                {
                    namesOfDistinctFiles.Add(elem);
                }
            }
            return namesOfDistinctFiles;
        }
    }
}
