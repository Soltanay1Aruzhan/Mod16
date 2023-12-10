using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace FManagerApp.Forms
{
    public partial class MoveForm : Form
    {

        private List<FileSystemInfo> FilesAndDirectories;

        private string Source;

        private string Destination;

        public MoveForm()
        {
            InitializeComponent();
        }

        public void SetParametres(List<FileSystemInfo> filesAndDirs, string source, string destination)
        {
            this.FilesAndDirectories = filesAndDirs;
            this.Source = source;
            this.Destination = destination;
        }

        private void MoveForm_Load(object sender, EventArgs e)
        {
            CancelButton.Enabled = false;
        }

        private CancellationTokenSource cts = new CancellationTokenSource();
        private async void StartButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = false;
            CancelButton.Enabled = true;
            var progressHandlerForCurrentFile = new Progress<int>(value =>
            {
                CurrentFileProgressBar.Value = value;
            });
            IProgress<int> progressCurrentFile = progressHandlerForCurrentFile as IProgress<int>;

            try
            {
                await Task.Run(() =>
                {
                    int progress = 0;
                    double count = 0;
                    string destPath;
                    foreach (FileSystemInfo fsi in FilesAndDirectories)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        DirectoryInfo dir = fsi as DirectoryInfo;
                        if (dir != null)
                        {
                            //перемещаем директорию
                            MoveDirectory(dir, Destination);
                        }
                        else
                        {
                            //перемещаем файл
                            FileInfo fileInfo = fsi as FileInfo;
                            destPath = Path.Combine(Destination, Path.GetFileName(fileInfo.Name));
                            if (File.Exists(destPath)) File.Delete(destPath);
                            fileInfo.MoveTo(destPath);
                        }
                        count++;
                        progress = (int)Math.Round(100 *count/FilesAndDirectories.Count);
                        progressCurrentFile.Report(progress);
                    }
                });

                MessageBox.Show("Перемещение завершено!");

            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Операция отменена!");
            }
            finally
            {
                this.Dispose();
                this.Close();
            }
        }


        private void MoveDirectory(DirectoryInfo sourceDir, string destination)
        {
            string path = Path.Combine(destination, sourceDir.Name);
            DirectoryInfo destDir = new DirectoryInfo(path);
            if (Directory.Exists(path))
            {
                var result = MessageBox.Show("Директория с таким именем уже существует. Выполнить слияние файлов и папок?", "", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                }
            }
            else
            {
                destDir.Create();
            }
            foreach (DirectoryInfo dir in sourceDir.GetDirectories())
            {
                cts.Token.ThrowIfCancellationRequested();
                MoveDirectory(dir, destDir.FullName);
            }

            string destPath;
            foreach (FileInfo file in sourceDir.GetFiles())
            {
                cts.Token.ThrowIfCancellationRequested();
                destPath = Path.Combine(destDir.FullName,Path.GetFileName(file.Name));
                if (File.Exists(destPath)) File.Delete(destPath);
                file.MoveTo(Path.Combine(destDir.FullName,Path.GetFileName(file.Name)));
            }

            sourceDir.Delete();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (cts != null)
                cts.Cancel();
        }

        private void MoveForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cts != null)
                cts.Cancel();
        }




    }
}
