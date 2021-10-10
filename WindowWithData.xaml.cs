using System;
using System.Windows;
using System.Media;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using EncipherData;
using ConfigurationData;
using FileWithKey;
using MyExtensions;
using DB_Access;
using DB_Files;
using DataBaseFilesHandler;

namespace Data_Keeper_v2._0
{
    static class CollectedRecords
    {
        public static ObservableCollection<UserData> DataCollection = new ObservableCollection<UserData>();
        public static Stack<DeletedRecord> DeletedRecords = new Stack<DeletedRecord>();
        private static DeletedRecord deletedRecord;
        public static Visibility[] VisibilityPropertiesForData = { Visibility.Visible, Visibility.Collapsed };
        public static void RestoreLastDeletedRecord()
        {
            if (DeletedRecords.Count > 0)
            {
                deletedRecord = new DeletedRecord();
                deletedRecord = DeletedRecords.Pop();
                deletedRecord.Record.IsSelected = false;
                if (deletedRecord.Index == DataCollection.Count)
                {
                    DataCollection.Add(deletedRecord.Record);
                }
                else
                {
                    DataCollection.Insert(deletedRecord.Index, deletedRecord.Record);
                }
            }
        }
        public static void RestoreAllDeletedRecords()
        {
            int DeletedRecordsLength = DeletedRecords.Count;
            for (int i = 0; i < DeletedRecordsLength; i++)
            {
                RestoreLastDeletedRecord();
            }
        }
    }
    public struct DeletedRecord
    {
        public UserData Record { get; set; }
        public int Index { get; set; }
    }
    public class UserData
    {
        public string Website { get; set; }
        public string User { get; set; }
        public string FakeUser { get { return String.Join("", User.Select(elem => '●')); } set { } }
        public string Email { get; set; }
        public string FakeEmail { get { return String.Join("", Email.Select(elem => '●')); } set { } }
        public string Password { get; set; }
        public string FakePassword { get { return String.Join("", Password.Select(elem => '●')); } set { } }
        public bool IsSelected { get; set; } = false;
    }
    static class UserDataBase
    {
        public static void HandleFileNotFoundInDataBaseExceptions()
        {
            List<string> listOfExistingFiles = DBFilesChecker.GetExistingFilesOfDataBase("");
            if (listOfExistingFiles.Count == 0)
            {
                MessageBox.Show("База данных будет создана заново, так как она отсутствует");
                CreateDBFiles();
            }
            else
            {
                List<string> namesOfNotFoundFiles = DBFilesChecker.GetDistinctNameFiles("", listOfExistingFiles);
                string namesOfNotFoundFilesForMessage = "\n";
                foreach (var elem in namesOfNotFoundFiles)
                {
                    namesOfNotFoundFilesForMessage += (elem + '\n');
                }
                MessageBoxResult msgBoxResult = MessageBox.Show($"В базе данных отсутствуют следующие файлы:{namesOfNotFoundFilesForMessage}Создать новую базу данных? (нажмите \"Нет\" для выхода из программы)",
                        "Выберите действие", MessageBoxButton.YesNo);
                if (msgBoxResult == MessageBoxResult.Yes)
                {
                    CreateDBFiles();
                }
                else if (msgBoxResult == MessageBoxResult.No)
                {
                    Application.Current.Shutdown();
                }
            }
        }
        private static void CreateDBFiles()  //FilesNotFoundException
        {
            StreamWriter accountFile = new StreamWriter(PathsToDataFiles.accountFilePath, append: false);
            accountFile.Close();
            StreamWriter accountLabelFile = new StreamWriter(PathsToDataFiles.accountLabelFilePath, append: false);
            accountLabelFile.Close();
            StreamWriter usernameFile = new StreamWriter(PathsToDataFiles.usernameFilePath, append: false);
            usernameFile.Close();
            StreamWriter usernameLabelFile = new StreamWriter(PathsToDataFiles.usernameLabelFilePath, append: false);
            usernameLabelFile.Close();
        }
        private static void IsWordConsistsOfSpaces(in string str, out bool checkingSpaces)
        {
            checkingSpaces = str.Contains(" ");
        }
        public static void splitDecipheredDataField(in string data_field, out string web, out string email, out string password)
        {
            web = "";
            email = "";
            password = "";
            string[] str_array = data_field.Split(' ');
            web = str_array[0];
            email = str_array[1];
            password = str_array[2];
        }
        public static void AddRecord(string website, string user, string email, string password, WindowWithData window)
        {
            UserData userRecord = new UserData();
            userRecord.Website = website;
            userRecord.User = user;
            userRecord.Email = email;
            userRecord.Password = password;
            IsWordConsistsOfSpaces(userRecord.Website, out bool checkingSpaces_website);
            IsWordConsistsOfSpaces(userRecord.Email, out bool checkingSpaces_email);
            IsWordConsistsOfSpaces(userRecord.Password, out bool checkingSpaces_password);
            if (userRecord.Website == "" || userRecord.Email == "" || userRecord.Password == "" ||
                checkingSpaces_website || checkingSpaces_email || checkingSpaces_password)
                SystemSounds.Exclamation.Play();
            if (userRecord.Website == "")
                MessageBox.Show("Название сайта не может содержать пустую строку!");
            if (userRecord.Email == "")
                MessageBox.Show("Логин не может содержать пустую строку!");
            if (userRecord.Password == "")
                MessageBox.Show("Пароль не может содержать пустую строку!");
            if (checkingSpaces_website)
                MessageBox.Show("Удалите пробелы в названии сайта");
            if (checkingSpaces_email)
                MessageBox.Show("Удалите пробелы в логине");
            if (checkingSpaces_password)
                MessageBox.Show("Удалите пробелы в пароле");
            if ((userRecord.Website != "" && userRecord.Email != "" && userRecord.Password != "") &&
                    (!checkingSpaces_website && !checkingSpaces_email && !checkingSpaces_password))
            {
                CollectedRecords.DataCollection.Add(userRecord);

                Cipher rc4 = new Cipher(InformationForAccess.key);
                //account file
                EncipheredStreamWriter encAccountFile = new EncipheredStreamWriter(PathsToDataFiles.accountFilePath,
                    PathsToDataFiles.accountLabelFilePath, isAppend: true);
                rc4.SourceText = $"{userRecord.Website} {userRecord.Email} {userRecord.Password}";
                rc4.Encipher();
                encAccountFile.WriteEncipheredLine(rc4.ConvertedText);
                encAccountFile.Close();
                //
                //username file
                EncipheredStreamWriter encUsernameFile = new EncipheredStreamWriter(PathsToDataFiles.usernameFilePath,
                    PathsToDataFiles.usernameLabelFilePath, isAppend: true);
                rc4.SourceText = userRecord.User;
                rc4.Encipher();
                encUsernameFile.WriteEncipheredLine(rc4.ConvertedText);
                encUsernameFile.Close();
                //
            }
        }
        public static void GetAndShowDataFromDB(WindowWithData window)
        {
            EncipheredStreamReader encAccountFile = null;
            EncipheredStreamReader encUsernameFile = null;
            try
            {
                Cipher rc4 = new Cipher(InformationForAccess.key);
                //init enciphered stream readers
                encAccountFile = new EncipheredStreamReader(PathsToDataFiles.accountFilePath,
                    PathsToDataFiles.accountLabelFilePath);
                encAccountFile.ReadLabelsFile();
                encUsernameFile = new EncipheredStreamReader(PathsToDataFiles.usernameFilePath,
                    PathsToDataFiles.usernameLabelFilePath);
                encUsernameFile.ReadLabelsFile();
                //
                string encipheredDataString;
                string encipheredUsernameString;
                string decipheredDataString;
                string decipheredUsernameString;
                string website, email, password;
                while (((encipheredDataString = encAccountFile.ReadEncipheredLine()) != null) &&
                    ((encipheredUsernameString = encUsernameFile.ReadEncipheredLine()) != null))
                {
                    //deciphering account data
                    rc4.SourceText = encipheredDataString;
                    rc4.Decipher();
                    decipheredDataString = rc4.ConvertedText;
                    splitDecipheredDataField(in decipheredDataString, out website, out email, out password);
                    //

                    //deciphering user's name
                    rc4.SourceText = encipheredUsernameString;
                    rc4.Decipher();
                    decipheredUsernameString = rc4.ConvertedText;
                    //

                    UserData userRecord = new UserData();
                    userRecord.Website = website;
                    userRecord.User = decipheredUsernameString;
                    userRecord.Email = email;
                    userRecord.Password = password;
                    CollectedRecords.DataCollection.Add(userRecord);
                }
                encAccountFile.Close();
                encUsernameFile.Close();
            }
            catch (FileNotFoundException)
            {
                encAccountFile?.Close();     //one should close stream readers. Without closing the program will crash with IO.Exception while
                encUsernameFile?.Close();    //creating these files again in CreateDBFiles method
                HandleFileNotFoundInDataBaseExceptions();
            }
            catch
            {
                MessageBoxResult msgBoxResult = MessageBox.Show("Все файлы базы данных на месте. Однако их не удается открыть." +
                    "\nПредложить способы решения проблемы? (нажмите \"Нет\" для выхода из программы)", "Выберите действие", MessageBoxButton.YesNo);
                if (msgBoxResult == MessageBoxResult.Yes)
                {
                    MessageBox.Show("1) Переместить программу и ее файлы в другую папку (в ту папку, в которой есть разрешение на чтение файлов и запись в них данных)" +
                        "\n2) Закрыть файлы базы данных, если они были открыты (account.data, account_label.lbl, username.data, username_label.lbl)" +
                        "\n3) Попробовать запустить программу еще раз" +
                        "\n4) Убедитесь, что ключ шифрования введен вверно" +
                        "\n5) Перезагрузить компьютер и попробовать запустить программу еще раз" +
                        "\n6) Удалить лишние записи из файлов базы данных, если содержимое файлов было изменено вручную или программно (файлы можно открыть в любом текстовом редакторе)" +
                        "\n7) Добавить удаленные символы в файлы базы данных, если содержимое файлов было изменено вручную или программно (файлы можно открыть в любом текстовом редакторе)" +
                        "\n8) Удалить базу данных (крайний вариант, если ничего больше не помогает, и потеря данных не критична)", "Попробуйте сделать следующие действия в любом порядке");
                    Application.Current.Shutdown();
                }
                else if (msgBoxResult == MessageBoxResult.No)
                {
                    Application.Current.Shutdown();
                }
            }
        }
        public static void RefreshDataInDB()
        {
            EncipheredStreamWriter encAccountFile = new EncipheredStreamWriter(PathsToDataFiles.accountFilePath,
                PathsToDataFiles.accountLabelFilePath, isAppend: false);
            EncipheredStreamWriter encUsernameFile = new EncipheredStreamWriter(PathsToDataFiles.usernameFilePath,
                PathsToDataFiles.usernameLabelFilePath, isAppend: false);
            Cipher rc4 = new Cipher(InformationForAccess.key);
            foreach (var elem in CollectedRecords.DataCollection)
            {
                rc4.SourceText = $"{elem.Website} {elem.Email} {elem.Password}";
                rc4.Encipher();
                encAccountFile.WriteEncipheredLine(rc4.ConvertedText);
                rc4.SourceText = elem.User;
                rc4.Encipher();
                encUsernameFile.WriteEncipheredLine(rc4.ConvertedText);
            }
            encAccountFile.Close();
            encUsernameFile.Close();
        }
    }
    public partial class WindowWithData : Window
    {
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
        public WindowWithData()
        {
            InitializeComponent();
            try
            {
                InformationForAccess.key = KeyHandler.ReadKeyFromTempFileAndDeleteFile();
            }
            catch
            {
                MessageBox.Show("В данной папке нет разрешения на чтение файлов. Вероятно, папка является системной или в ней не разрешено чтение файлов. " +
                    "\nПереместите, пожалуйста, программу и ее компоненты (если таковые имеются в текущей папке) в другую папку");
                Application.Current.Shutdown();
            }
            UserDataBase.GetAndShowDataFromDB(this);
            DataGridForUsers.ItemsSource = CollectedRecords.DataCollection;
        }
        //Add new user button clicked
        private void AddNewUserBN_Clicked(object sender, RoutedEventArgs e)
        {
            UserDataBase.AddRecord(WEBSITE.Text, USERNAME.Text, EMAIL.Text, PASSWORD.Text, this);
        }
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            MinHeight = ActualHeight;
            MinWidth = ActualWidth;
            MaxWidth = ActualWidth;
        }
        private void ClearInputFields_Clicked(object sender, RoutedEventArgs e)
        {
            WEBSITE.Text = "";
            USERNAME.Text = "";
            EMAIL.Text = "";
            PASSWORD.Text = "";
        }

        private void DeleteSelectedRecord_Clicked(object sender, RoutedEventArgs e)
        {
            Queue<UserData> queueForDeletedElements = new Queue<UserData>();
            foreach (var elem in CollectedRecords.DataCollection)
            {
                if (elem.IsSelected)
                {
                    queueForDeletedElements.Enqueue(elem);
                }
            }
            int indexOfDeletedRecord;
            DeletedRecord record;
            while (queueForDeletedElements.Count > 0)
            {
                indexOfDeletedRecord = CollectedRecords.DataCollection.IndexOf(queueForDeletedElements.Peek());
                record = new DeletedRecord();
                record.Record = queueForDeletedElements.Dequeue();
                record.Index = indexOfDeletedRecord;
                CollectedRecords.DeletedRecords.Push(record);
                CollectedRecords.DataCollection.RemoveAt(indexOfDeletedRecord);
            }
            UserDataBase.RefreshDataInDB();
        }

        private void CancelSelection_Clicked(object sender, RoutedEventArgs e)
        {
            int dataCollectionLength = CollectedRecords.DataCollection.Count;
            for (int i = 0; i < dataCollectionLength; i++)
            {
                CollectedRecords.DataCollection[i].IsSelected = false;
            }
            CollectionViewSource.GetDefaultView(CollectedRecords.DataCollection).Refresh();
        }

        private void SelectAll_Clicked(object sender, RoutedEventArgs e)
        {
            int dataCollectionLength = CollectedRecords.DataCollection.Count;
            for (int i = 0; i < dataCollectionLength; i++)
            {
                CollectedRecords.DataCollection[i].IsSelected = true;
            }
            CollectionViewSource.GetDefaultView(CollectedRecords.DataCollection).Refresh();
        }

        private void RestoreLastDeletedRecord_Clicked(object sender, RoutedEventArgs e)
        {
            CollectedRecords.RestoreLastDeletedRecord();
            CollectionViewSource.GetDefaultView(CollectedRecords.DataCollection).Refresh();
            UserDataBase.RefreshDataInDB();
        }

        private void RestoreAllDeletedRecords_Clicked(object sender, RoutedEventArgs e)
        {
            CollectedRecords.RestoreAllDeletedRecords();
            CollectionViewSource.GetDefaultView(CollectedRecords.DataCollection).Refresh();
            UserDataBase.RefreshDataInDB();
        }

        private void InsertSelectedRecordInTextBoxes_Clicked(object sender, RoutedEventArgs e)
        {
            UserData record = new UserData();
            bool isItemSelected = false;
            foreach (var elem in CollectedRecords.DataCollection)
            {
                if (elem.IsSelected)
                {
                    record = elem;
                    isItemSelected = true;
                    break;
                }
            }
            if (isItemSelected)
            {
                WEBSITE.Text = record.Website;
                USERNAME.Text = record.User;
                EMAIL.Text = record.Email;
                PASSWORD.Text = record.Password;
            }
            else
            {
                MessageBox.Show("Выберите запись");
            }
        }

        private void MakeDataBaseBackup_Clicked(object sender, RoutedEventArgs e)
        {
            Backup.MakeDataBaseBackup();
        }

        private void ReplaceDataBaseByBackup_Clicked(object sender, RoutedEventArgs e)
        {
            Backup.ReplaceDataBaseFilesByBackup();
            CollectedRecords.DataCollection.Clear();
            CollectedRecords.DeletedRecords.Clear();
            UserDataBase.GetAndShowDataFromDB(this);
        }

        private void ShowOrHideData_Clicked(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 4; i++)
            {
                DataGridForUsers.Columns[i].Visibility = CollectedRecords.VisibilityPropertiesForData[0];
            }
            for (int i = 4; i < DataGridForUsers.Columns.Count - 1; i++)
            {
                DataGridForUsers.Columns[i].Visibility = CollectedRecords.VisibilityPropertiesForData[1];
            }
            var temp = CollectedRecords.VisibilityPropertiesForData[0];
            CollectedRecords.VisibilityPropertiesForData[0] = CollectedRecords.VisibilityPropertiesForData[1];
            CollectedRecords.VisibilityPropertiesForData[1] = temp;
        }

        private void GetDataAboutPasswordsAndTheirKeeping_Clicked(object sender, RoutedEventArgs e)
        {
            WindowWithDataAboutPasswords usefulWindow = new WindowWithDataAboutPasswords();
            usefulWindow.Show();
        }
    }
}
