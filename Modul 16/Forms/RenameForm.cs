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

namespace FManagerApp.Forms
{
    public partial class RenameForm : Form
    {

        public FileSystemInfo fsi
        { get; set; }

        public RenameForm()
        {
            InitializeComponent();
        }

        private void NewNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = sender as TextBox;
                if (!textBox.Text.Equals("..."))
                {
                    DirectoryInfo dir = fsi as DirectoryInfo;
                    if (dir != null)
                    {
                        dir.MoveTo(Path.Combine(dir.Parent.FullName, textBox.Text));
                    }
                    else
                    {
                        FileInfo file = fsi as FileInfo;
                        string str = Path.Combine(file.DirectoryName, textBox.Text + file.Extension);
                        file.MoveTo(Path.Combine(file.DirectoryName, textBox.Text + file.Extension));
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверное имя!");
                    textBox.Text = "";
                }
            }
        }
    }
}
