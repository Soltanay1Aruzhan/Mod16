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
using FManagerApp.Forms;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace FManagerApp
{
    public partial class Form1 : Form
    {
        private List<Button> LeftLogicalDrives = new List<Button>();
        private List<Button> RightLogicalDrives = new List<Button>();

        private string LeftViewRootPath = "C:\\";
        private string RightViewRootPath = "C:\\";

        private FileSystemWatcher LeftViewObserver = new FileSystemWatcher();
        private FileSystemWatcher RightViewObserver = new FileSystemWatcher();

        private bool isLeftActive = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            LoadFontSettings();
            UpdateDrivesPanel();
            ConfigurateObservers();
            UpdateListView(LeftListView, LeftViewRootPath);
            LeftPathTextBox.Text = LeftViewRootPath;
            RightPathTextBox.Text = RightViewRootPath;
            UpdateListView(RightListView, RightViewRootPath);
        }

        //обновляет информацию на панели вывода существующих дисков
        private void UpdateDrivesPanel()
        {
            //чистим предыдущее состояние
            foreach (Button btn in LeftLogicalDrives)
                LeftDrivesPanel.Controls.Remove(btn);
            foreach (Button btn in RightLogicalDrives)
                RightDrivesPanel.Controls.Remove(btn);
            LeftLogicalDrives.Clear();
            RightLogicalDrives.Clear();

            //загружаем новое
            DriveInfo[] drivesInfo = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drivesInfo)
            {
                if (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable) continue;
                Button driveBtnLeft = new Button();
                Button driveBtnRight = new Button();
                LeftDrivesPanel.Controls.Add(driveBtnLeft);
                RightDrivesPanel.Controls.Add(driveBtnRight);

                driveBtnLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                driveBtnLeft.Size = new System.Drawing.Size(37, 28);
                driveBtnLeft.Location = new System.Drawing.Point(3 + LeftLogicalDrives.Count * (driveBtnLeft.Size.Width + 5), 3);
                driveBtnLeft.Text = drive.Name[0].ToString();
                driveBtnLeft.Name = driveBtnLeft.Text + "button";
                driveBtnLeft.TabIndex = LeftLogicalDrives.Count;
                driveBtnLeft.UseVisualStyleBackColor = true;
                driveBtnLeft.Click += new System.EventHandler(ChangeDriveButton_Click);

                driveBtnRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                driveBtnRight.Size = new System.Drawing.Size(37, 28);
                driveBtnRight.Location = new System.Drawing.Point(3 + RightLogicalDrives.Count * (driveBtnRight.Size.Width + 5), 3);
                driveBtnRight.Text = drive.Name[0].ToString();
                driveBtnRight.Name = driveBtnRight.Text + "button";
                driveBtnRight.TabIndex = RightLogicalDrives.Count;
                driveBtnRight.UseVisualStyleBackColor = true;
                driveBtnRight.Click += new System.EventHandler(ChangeDriveButton_Click);

                LeftLogicalDrives.Add(driveBtnLeft);
                RightLogicalDrives.Add(driveBtnRight);
            }

        }

        private void UpdateListView(ListView listView, string root)
        {
            listView.Items.Clear();
            DirectoryInfo dirInfo = new DirectoryInfo(root);
            try
            {
                var files = dirInfo.GetFiles();
                var folders = dirInfo.GetDirectories();

                if (dirInfo.Parent != null)
                    listView.Items.Add(BuildListViewItem(
                        "...",
                        "",
                        "",
                        ""
                    ));

                foreach (DirectoryInfo folder in folders)
                {
                    ListViewItem item = BuildListViewItem(
                        System.IO.Path.GetFileName(folder.FullName),
                        "",
                        "<папка>",
                        folder.CreationTime.ToShortDateString() + " " + folder.CreationTime.ToShortTimeString()
                    );
                    item.ImageIndex = 0;
                    listView.Items.Add(item);
                }

                foreach (FileInfo file in files)
                {
                    string ext = (file.Extension.Length > 0) ? file.Extension.Substring(1) : "";
                    listView.Items.Add(BuildListViewItem(
                        System.IO.Path.GetFileNameWithoutExtension(file.FullName),
                        ext,
                        GetSize(file.Length),
                        file.CreationTime.ToShortDateString() + " " + file.CreationTime.ToShortTimeString()
                    ));
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Устройство не готово!");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Отказано в доступе!");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
        }

        //Построение объекта ListViewItem для ListView в программе
        private ListViewItem BuildListViewItem(string itemName, string type, string size, string date)
        {
            ListViewItem item = new ListViewItem(itemName);
            item.SubItems.Add(type);
            item.SubItems.Add(size);
            item.SubItems.Add(date);
            return item;
        }

        // Форматируем размер файла из числа байт в необходимые единицы измерения
        private string GetSize(long size)
        {
            double bytes = size;
            double kBytes = Math.Round(bytes/1024,1);
            double mBytes = Math.Round(kBytes/1024,1);
            double gBytes = Math.Round(mBytes/1024,1);

            if (gBytes >= 1)
                return String.Format("{0} Гб", gBytes);
            else if (mBytes >= 1)
                return String.Format("{0} Мб", mBytes);
            else if (kBytes >= 1)
                return String.Format("{0} Кб", kBytes);
            else return String.Format("{0} байт", bytes);
        }

        private delegate void Del(object source, FileSystemEventArgs e);
        // инициализируем обозреватели
        private void ConfigurateObservers()
        {
            Del leftOnChanded = delegate(object source, FileSystemEventArgs e){UpdateListView(LeftListView, LeftViewRootPath);};
            Del rightOnChanded = delegate(object source, FileSystemEventArgs e) { UpdateListView(RightListView, RightViewRootPath); };

            // для левой панели
            LeftViewObserver.Path = LeftViewRootPath;
            LeftViewObserver.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            LeftViewObserver.Changed += new FileSystemEventHandler(leftOnChanded);
            LeftViewObserver.Created += new FileSystemEventHandler(leftOnChanded);
            LeftViewObserver.Deleted += new FileSystemEventHandler(leftOnChanded);
            LeftViewObserver.Renamed += new RenamedEventHandler(leftOnChanded);
            LeftViewObserver.SynchronizingObject = LeftListView;
            LeftViewObserver.EnableRaisingEvents = true;


            // для правой панели
            RightViewObserver.Path = RightViewRootPath;
            RightViewObserver.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            RightViewObserver.Changed += new FileSystemEventHandler(rightOnChanded);
            RightViewObserver.Created += new FileSystemEventHandler(rightOnChanded);
            RightViewObserver.Deleted += new FileSystemEventHandler(rightOnChanded);
            RightViewObserver.Renamed += new RenamedEventHandler(rightOnChanded);
            RightViewObserver.SynchronizingObject = RightListView;
            RightViewObserver.EnableRaisingEvents = true;
        }

        private void ChangeDriveButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (LeftPanel.Contains(button))
            {
                //данная кнопка принадлежит левой панели
                LeftViewRootPath = button.Text + ":\\";
                LeftPathTextBox.Text = LeftViewRootPath;
                LeftViewObserver.Path = LeftViewRootPath;
                UpdateListView(LeftListView, LeftViewRootPath);
            }
            else
            {
                //данная кнопка принадлежит правой панели
                RightViewRootPath = button.Text + ":\\";
                RightPathTextBox.Text = RightViewRootPath;
                RightViewObserver.Path = RightViewRootPath;
                UpdateListView(RightListView, RightViewRootPath);
            }
        }

        private void LeftListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //если выбранный элемент не является папкой или переходом к родительскому каталогу, выходим
            if (!LeftListView.SelectedItems[0].SubItems[2].Text.Equals("<папка>") && !LeftListView.SelectedItems[0].SubItems[0].Text.Equals("...")) return;

            //если выбранный элемент является переходом к родительскому каталогу
            if (LeftListView.SelectedItems[0].SubItems[0].Text.Equals("..."))
            {
                try
                {
                    string parent = Directory.GetParent(LeftViewRootPath).FullName;
                    LeftViewObserver.Path = parent;
                    LeftViewRootPath = parent;
                    LeftPathTextBox.Text = parent;
                    UpdateListView(LeftListView, LeftViewRootPath);
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("Отказано в доступе!");
                }
                return;
            }

            try
            {
                string child = LeftViewRootPath;
                if (LeftViewRootPath[LeftViewRootPath.Length - 1] != '\\') child += "\\";
                child += LeftListView.SelectedItems[0].SubItems[0].Text;
                LeftViewObserver.Path = child;
                LeftViewRootPath = child;
                LeftPathTextBox.Text = child;
                UpdateListView(LeftListView, LeftViewRootPath);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Отказано в доступе!");
            }
        }

        private void RightListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //если выбранный элемент не является папкой или переходом к родительскому каталогу, выходим
            if (!RightListView.SelectedItems[0].SubItems[2].Text.Equals("<папка>") && !RightListView.SelectedItems[0].SubItems[0].Text.Equals("...")) return;

            //если выбранный элемент является переходом к родительскому каталогу
            if (RightListView.SelectedItems[0].SubItems[0].Text.Equals("..."))
            {
                try
                {
                    string parent = Directory.GetParent(RightViewRootPath).FullName;
                    RightViewObserver.Path = parent;
                    RightViewRootPath = parent;
                    RightPathTextBox.Text = parent;
                    UpdateListView(RightListView, RightViewRootPath);
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("Отказано в доступе!");
                }
                return;
            }
            try
            {
                string child = RightViewRootPath;
                if (RightViewRootPath[RightViewRootPath.Length - 1] != '\\') child += "\\";
                child += RightListView.SelectedItems[0].SubItems[0].Text;
                RightViewObserver.Path = child;
                RightViewRootPath = child;
                RightPathTextBox.Text = child;
                UpdateListView(RightListView, RightViewRootPath);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Отказано в доступе!");
            }
        }

        private void LeftPathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = sender as TextBox;
                if (Directory.Exists(textBox.Text))
                {
                    try
                    {
                        LeftViewObserver.Path = LeftViewRootPath;
                        LeftViewRootPath = textBox.Text;
                        UpdateListView(LeftListView, LeftViewRootPath);
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("Отказано в доступе!");
                    }
                }
                else
                {
                    MessageBox.Show("Неверный путь!");
                    textBox.Text = LeftViewRootPath;
                }
            }
        }

        private void RightPathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = sender as TextBox;
                if (Directory.Exists(textBox.Text))
                {
                    try
                    {
                        RightViewObserver.Path = RightViewRootPath;
                        RightViewRootPath = textBox.Text;
                        UpdateListView(RightListView, RightViewRootPath);
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("Отказано в доступе!");
                    }
                }
                else
                {
                    MessageBox.Show("Неверный путь!");
                    textBox.Text = RightViewRootPath;
                }
            }
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            try
            {

                CopyForm copyForm = new CopyForm();
                List<FileSystemInfo> checkedfilesAndDirectories = new List<FileSystemInfo>();

                if (isLeftActive)
                {
                    foreach (ListViewItem item in LeftListView.SelectedItems)
                    {
                        if (item.SubItems[2].Text.Equals("<папка>"))
                        {
                            checkedfilesAndDirectories.Add(new DirectoryInfo(Path.Combine(LeftViewRootPath, item.SubItems[0].Text)));
                        }
                        else if (!item.SubItems[0].Text.Equals("..."))
                        {
                            string filename = item.SubItems[0].Text;
                            if (item.SubItems[1].Text != "") filename = filename + "." + item.SubItems[1].Text;
                            checkedfilesAndDirectories.Add(new FileInfo(Path.Combine(LeftViewRootPath, filename)));
                        }
                    }
                    if (checkedfilesAndDirectories.Count == 0)
                    {
                        copyForm.Dispose();
                        return;
                    }
                    copyForm.SetParametres(checkedfilesAndDirectories, LeftViewRootPath, RightViewRootPath);
                }
                else
                {
                    foreach (ListViewItem item in RightListView.SelectedItems)
                    {
                        if (item.SubItems[2].Text.Equals("<папка>"))
                        {
                            checkedfilesAndDirectories.Add(new DirectoryInfo(Path.Combine(RightViewRootPath, item.SubItems[0].Text)));
                        }
                        else
                        {
                            string filename = item.SubItems[0].Text;
                            if (item.SubItems[1].Text != "") filename = filename + "." + item.SubItems[1].Text;
                            checkedfilesAndDirectories.Add(new FileInfo(Path.Combine(RightViewRootPath, filename)));
                        }
                    }
                    if (checkedfilesAndDirectories.Count == 0)
                    {
                        copyForm.Dispose();
                        return;
                    }
                    copyForm.SetParametres(checkedfilesAndDirectories, RightViewRootPath, LeftViewRootPath);                  
                }
                copyForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FATAL: " + ex.Message + "\n" + ex.StackTrace);
            }

        }

        private void LeftListView_Enter(object sender, EventArgs e)
        {
            isLeftActive = true;
        }

        private void RightListView_Enter(object sender, EventArgs e)
        {
            isLeftActive = false;
        }

        private void MoveButton_Click(object sender, EventArgs e)
        {
            try
            {

                MoveForm moveForm = new MoveForm();
                List<FileSystemInfo> checkedfilesAndDirectories = new List<FileSystemInfo>();

                if (isLeftActive)
                {
                    foreach (ListViewItem item in LeftListView.SelectedItems)
                    {
                        if (item.SubItems[2].Text.Equals("<папка>"))
                        {
                            checkedfilesAndDirectories.Add(new DirectoryInfo(Path.Combine(LeftViewRootPath, item.SubItems[0].Text)));
                        }
                        else if (!item.SubItems[0].Text.Equals("..."))
                        {
                            string filename = item.SubItems[0].Text;
                            if (item.SubItems[1].Text != "") filename = filename + "." + item.SubItems[1].Text;
                            checkedfilesAndDirectories.Add(new FileInfo(Path.Combine(LeftViewRootPath, filename)));
                        }
                    }
                    if (checkedfilesAndDirectories.Count == 0)
                    {
                        moveForm.Dispose();
                        return;
                    }
                    moveForm.SetParametres(checkedfilesAndDirectories, LeftViewRootPath, RightViewRootPath);
                }
                else
                {
                    foreach (ListViewItem item in RightListView.SelectedItems)
                    {
                        if (item.SubItems[2].Text.Equals("<папка>"))
                        {
                            checkedfilesAndDirectories.Add(new DirectoryInfo(Path.Combine(RightViewRootPath, item.SubItems[0].Text)));
                        }
                        else
                        {
                            string filename = item.SubItems[0].Text;
                            if (item.SubItems[1].Text != "") filename = filename + "." + item.SubItems[1].Text;
                            checkedfilesAndDirectories.Add(new FileInfo(Path.Combine(RightViewRootPath, filename)));
                        }
                    }
                    if (checkedfilesAndDirectories.Count == 0)
                    {
                        moveForm.Dispose();
                        return;
                    }
                    moveForm.SetParametres(checkedfilesAndDirectories, RightViewRootPath, LeftViewRootPath);
                }
                moveForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FATAL: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            try
            {

                DeleteForm deleteForm = new DeleteForm();
                List<FileSystemInfo> checkedfilesAndDirectories = new List<FileSystemInfo>();

                if (isLeftActive)
                {
                    foreach (ListViewItem item in LeftListView.SelectedItems)
                    {
                        if (item.SubItems[2].Text.Equals("<папка>"))
                        {
                            checkedfilesAndDirectories.Add(new DirectoryInfo(Path.Combine(LeftViewRootPath, item.SubItems[0].Text)));
                        }
                        else if (!item.SubItems[0].Text.Equals("..."))
                        {
                            string filename = item.SubItems[0].Text;
                            if (item.SubItems[1].Text != "") filename = filename + "." + item.SubItems[1].Text;
                            checkedfilesAndDirectories.Add(new FileInfo(Path.Combine(LeftViewRootPath, filename)));
                        }
                    }
                    if (checkedfilesAndDirectories.Count == 0)
                    {
                        deleteForm.Dispose();
                        return;
                    }
                    deleteForm.SetParametres(checkedfilesAndDirectories);
                }
                else
                {
                    foreach (ListViewItem item in RightListView.SelectedItems)
                    {
                        if (item.SubItems[2].Text.Equals("<папка>"))
                        {
                            checkedfilesAndDirectories.Add(new DirectoryInfo(Path.Combine(RightViewRootPath, item.SubItems[0].Text)));
                        }
                        else if (!item.SubItems[0].Text.Equals("..."))
                        {
                            string filename = item.SubItems[0].Text;
                            if (item.SubItems[1].Text != "") filename = filename + "." + item.SubItems[1].Text;
                            checkedfilesAndDirectories.Add(new FileInfo(Path.Combine(RightViewRootPath, filename)));
                        }
                    }
                    if (checkedfilesAndDirectories.Count == 0)
                    {
                        deleteForm.Dispose();
                        return;
                    }
                    deleteForm.SetParametres(checkedfilesAndDirectories);
                }
                deleteForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FATAL: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void RenameButton_Click(object sender, EventArgs e)
        {
            RenameForm renameForm = new RenameForm();
            if (isLeftActive && LeftListView.SelectedItems.Count !=0)
            {
                if (LeftListView.SelectedItems.Count > 1)
                {
                    MessageBox.Show("Выбрано более одного элемента!");
                    renameForm.Dispose();
                    return;
                }

                if (LeftListView.SelectedItems[0].SubItems[2].Text.Equals("<папка>"))
                    renameForm.fsi = new DirectoryInfo(Path.Combine(LeftViewRootPath, LeftListView.SelectedItems[0].SubItems[0].Text));
                else if (!LeftListView.SelectedItems[0].Text.Equals("..."))
                {
                    string filename = LeftListView.SelectedItems[0].SubItems[0].Text;
                    if (LeftListView.SelectedItems[0].SubItems[1].Text != "") filename = filename + "." + LeftListView.SelectedItems[0].SubItems[1].Text;
                    renameForm.fsi = new FileInfo(Path.Combine(LeftViewRootPath, LeftListView.SelectedItems[0].SubItems[0].Text));
                }
                else
                {
                    renameForm.Dispose();
                    return;
                }
            }
            else if (!isLeftActive && RightListView.SelectedItems.Count !=0)
            {
                if (RightListView.SelectedItems.Count != 1)
                {
                    MessageBox.Show("Выбрано более одного элемента!");
                    renameForm.Dispose();
                    return;
                }
                if (RightListView.SelectedItems[0].SubItems[2].Text.Equals("<папка>"))
                    renameForm.fsi = new DirectoryInfo(Path.Combine(RightViewRootPath, RightListView.SelectedItems[0].SubItems[0].Text));
                else if (!RightListView.SelectedItems[0].Text.Equals("..."))
                {
                    string filename = RightListView.SelectedItems[0].SubItems[0].Text;
                    if (RightListView.SelectedItems[0].SubItems[1].Text != "") filename = filename + "." + RightListView.SelectedItems[0].SubItems[1].Text;
                    renameForm.fsi = new FileInfo(Path.Combine(RightViewRootPath, filename));
                }
                else
                {
                    renameForm.Dispose();
                    return;
                }
            }
            renameForm.Show();
        }

        private void ShowButton_Click(object sender, EventArgs e)
        {
            try
            {
                ShowAssemblyForm showAssemblyForm = new ShowAssemblyForm();
                if (isLeftActive && LeftListView.SelectedItems.Count != 0)
                {
                    if (LeftListView.SelectedItems.Count > 1)
                    {
                        MessageBox.Show("Выбрано более одного элемента!");
                        return;
                    }
                    string filename = (LeftListView.SelectedItems[0].SubItems[1].Text != "") ? LeftListView.SelectedItems[0].SubItems[0].Text + "." + LeftListView.SelectedItems[0].SubItems[1].Text : LeftListView.SelectedItems[0].SubItems[0].Text;
                    string path = Path.Combine(LeftViewRootPath, filename);
                    Assembly assembly = Assembly.LoadFrom(path);
                    showAssemblyForm.Text = filename;
                    showAssemblyForm.assembly = assembly;
                }
                else if (!isLeftActive && RightListView.SelectedItems.Count != 0)
                {
                    if (RightListView.SelectedItems.Count != 1)
                    {
                        MessageBox.Show("Выбрано более одного элемента!");
                        return;
                    }

                    string filename = (RightListView.SelectedItems[0].SubItems[1].Text != "") ? RightListView.SelectedItems[0].SubItems[0].Text + "." + RightListView.SelectedItems[0].SubItems[1].Text : RightListView.SelectedItems[0].SubItems[0].Text;
                    string path = Path.Combine(RightViewRootPath, filename);
                    Assembly assembly = Assembly.LoadFrom(path);
                    showAssemblyForm.Text = filename;
                    showAssemblyForm.assembly = assembly;
                }
                showAssemblyForm.Show();
            }
            catch (BadImageFormatException)
            {
                MessageBox.Show("Выбранный файл не является допустимой сборкой.");
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Значение не найдено, либо модуль, который пытаются загрузить, не указывает расширение имени файла.");
            }
            catch (FileLoadException)
            {
                MessageBox.Show("Не удалось загрузить указанный объект.");
            }
        }

        private void FontSettingsMenuItem_Click(object sender, EventArgs e)
        {
            FontSettingsForm fontSettingForm = new FontSettingsForm();
            fontSettingForm.ShowDialog();
            LoadFontSettings();
        }

        private void LoadFontSettings()
        {
            FileStream fs = null;
            XmlDictionaryReader reader = null;
            try
            {
                fs = new FileStream("settings.xml", FileMode.Open);
                XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
                reader = XmlDictionaryReader.CreateTextReader(fs, quotas);
                DataContractSerializer ser = new DataContractSerializer(typeof(Settings));

                Settings fontSettings =
                    (Settings)ser.ReadObject(reader, true);

                LeftListView.Font = new Font(FontFamily.GenericSansSerif, fontSettings.FontSize);
                LeftListView.ForeColor = fontSettings.FontColor;
                RightListView.Font = new Font(FontFamily.GenericSansSerif, fontSettings.FontSize);
                RightListView.ForeColor = fontSettings.FontColor;
            }
            catch (System.Xml.XmlException)
            {
                MessageBox.Show("Ошибка при десериализации!");
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("Файл не найден!");
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
                MessageBox.Show("Файл поврежден и не может быть открыт!");
            }
            finally
            {
                if (reader != null) reader.Dispose();
                fs.Close();
            }
        }

    }
}