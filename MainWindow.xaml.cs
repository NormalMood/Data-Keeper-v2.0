using System;
using System.Windows;
using System.Media;
using FileWithKey;
using HashAlgorithm;
using DB_Access;
using System.IO;

namespace Data_Keeper_v2._0
{
    public partial class MainWindow : Window
    {
        private static void TestWriteInFileAccess()
        {
            string testFileName = "emptyfilefortest_itsGuidIs_" + Guid.NewGuid().ToString() + ".access_test_quickly_temp_file"; //get unique name for a test file in current directory
            try
            {
                StreamWriter testOutputFile = new StreamWriter(testFileName);
                testOutputFile.Close();
                File.Delete(testFileName);
            }
            catch
            {
                MessageBox.Show("В данной папке нет разрешения на создание файлов. Вероятно, папка является системной. " +
                    "\nПереместите, пожалуйста, программу и ее компоненты (если таковые имеются в текущей папке) в другую папку");
                Application.Current.Shutdown();
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            TestWriteInFileAccess();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string password = passwordField.Password;
            if (SHA256.GenerateHash(password) == InformationForAccess.password)
            {
                KeyHandler.WriteKeyToTempFile(keyField.Password);
                WindowWithData newWindow = new WindowWithData();
                newWindow.Owner = this;
                newWindow.Show();
                this.Hide();
            }
            else
            {
                passwordField.Password = string.Empty;
                keyField.Password = string.Empty;
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Неверный пароль!");
            }
        }
    }
}
