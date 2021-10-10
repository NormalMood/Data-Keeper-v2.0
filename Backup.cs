using System;
using System.Collections.Generic;
using System.IO;
using ConfigurationData;
using System.Windows.Forms;
using System.Windows;
using System.Diagnostics;
using DB_Files;

namespace DataBaseFilesHandler
{
    public static class Backup
    {
        private static void CopyDataBaseFilesFromOneToAnotherFolder(string sourceFolder, string destinationFolder)
        {
            foreach (var elem in PathsToDataFiles.listOfAllDataBaseFiles)
            {
                try
                {
                    if (File.Exists(sourceFolder + elem)) //without checking of this condition a file will be created anyway with name of a not found file
                        File.Copy(sourceFolder + elem, destinationFolder + elem);  //so there will be created a wrong empty file
                    else
                        throw new FileNotFoundException(); 
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBoxResult msgBoxResult = System.Windows.MessageBox.Show("Копирование в папку не разрешено. " +
                        "\nОткрыть папки с базой данных и с резервной копией?", "Выберите действие",
                        MessageBoxButton.YesNo);
                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", sourceFolder);
                        Process.Start("explorer.exe", destinationFolder);
                    }
                    return;
                }
                catch (FileNotFoundException)
                {
                    MessageBoxResult msgBoxResult = System.Windows.MessageBox.Show("Файлы были потеряны. Копирование невозможно." +
                        "\nОткрыть папки с базой данных и с резервной копией?", "Выберите действие", MessageBoxButton.YesNo);
                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", sourceFolder);
                        Process.Start("explorer.exe", destinationFolder);
                    }
                    return;
                }
                catch (IOException)   //there is a file in backup folder with the same name as in a source directory
                {
                    try
                    {
                        File.Delete(destinationFolder + elem);
                        File.Copy(sourceFolder + elem, destinationFolder + elem);
                    }
                    catch    //if the program cannot delete a file in backup folder with the same name as in a source directory
                    {
                        MessageBoxResult msgBoxResult = System.Windows.MessageBox.Show("Удаление резервных копий невозможно. Открыть папку с резервной копией?",
                                    "Выберите действие", MessageBoxButton.YesNo);
                        if (msgBoxResult == MessageBoxResult.Yes)
                        {
                            Process.Start("explorer.exe", destinationFolder);
                        }
                        return;
                    }
                }
            }
        }
        public static void MakeDataBaseBackup()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string pathToSelectedFolder = folderDialog.SelectedPath;
                string pathToApplicationDirectory = Directory.GetCurrentDirectory();
                pathToSelectedFolder = pathToSelectedFolder.Length == 3 ? 
                    pathToSelectedFolder.Substring(0, 2) : pathToSelectedFolder; //to get a right path to the root folder if it was selected
                pathToApplicationDirectory = pathToApplicationDirectory.Length == 3 ?
                    pathToApplicationDirectory.Substring(0, 2) : pathToApplicationDirectory;
                List<string> existingDataBaseFilesInApplicationFolder = DBFilesChecker.GetExistingFilesOfDataBase("");
                if (existingDataBaseFilesInApplicationFolder.Count < 4)
                {
                    MessageBoxResult msgBoxResult = System.Windows.MessageBox.Show("Часть файлов базы данных потеряна. Резервное копирование невозможно." +
                        "\nОткрыть папку с базой данных?", "Выберите действие", MessageBoxButton.YesNo);
                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", Directory.GetCurrentDirectory());
                    }
                    return;
                }
                List<string> existingDataBaseFilesInBackupFolder = DBFilesChecker.GetExistingFilesOfDataBase(pathToSelectedFolder + "\\");
                if (existingDataBaseFilesInBackupFolder.Count > 0)
                {
                    MessageBoxResult msgBoxResult = System.Windows.MessageBox.Show("Файлы базы данных уже существуют в выбранной папке." +
                        "\nЗаменить их новой копией?", "Выберите действие", MessageBoxButton.YesNo);
                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        CopyDataBaseFilesFromOneToAnotherFolder(pathToApplicationDirectory + "\\", pathToSelectedFolder + "\\");
                    }
                }
                else if (existingDataBaseFilesInBackupFolder.Count == 0)
                {
                    CopyDataBaseFilesFromOneToAnotherFolder(pathToApplicationDirectory + "\\", pathToSelectedFolder + "\\");
                }
            }
        }
        public static void ReplaceDataBaseFilesByBackup()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string pathToSelectedFolder = folderDialog.SelectedPath;
                string pathToApplicationDirectory = Directory.GetCurrentDirectory();
                pathToSelectedFolder = pathToSelectedFolder.Length == 3 ?
                    pathToSelectedFolder.Substring(0, 2) : pathToSelectedFolder; //to get a right path to the root folder if it was selected
                pathToApplicationDirectory = pathToApplicationDirectory.Length == 3 ?
                    pathToApplicationDirectory.Substring(0, 2) : pathToApplicationDirectory;
                List<string> existingDataBaseFileInBackupFolder = DBFilesChecker.GetExistingFilesOfDataBase(pathToSelectedFolder + "\\");
                if (existingDataBaseFileInBackupFolder.Count < 4)
                {
                    MessageBoxResult msgBoxResult = System.Windows.MessageBox.Show("Файлы были потеряны. Копирование невозможно." +
                        "\nОткрыть папки с базой данных и с резервной копией?", "Выберите действие", MessageBoxButton.YesNo);
                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", Directory.GetCurrentDirectory());
                        Process.Start("explorer.exe", pathToSelectedFolder);
                    }
                    return;
                }
                List<string> existingDataBaseFilesInApplicationFolder = DBFilesChecker.GetExistingFilesOfDataBase("");
                if (existingDataBaseFilesInApplicationFolder.Count > 0)
                {
                    MessageBoxResult msgBoxResult = System.Windows.MessageBox.Show("Вы хотите заменить существующую базу данных ее резервной копией?",
                         "Выберите действие", MessageBoxButton.YesNo);
                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        CopyDataBaseFilesFromOneToAnotherFolder(pathToSelectedFolder + "\\", pathToApplicationDirectory + "\\");
                    }
                }
                else if (existingDataBaseFilesInApplicationFolder.Count == 0)
                {
                    CopyDataBaseFilesFromOneToAnotherFolder(pathToSelectedFolder + "\\", pathToApplicationDirectory + "\\");
                }
            }
        }
    }
}
