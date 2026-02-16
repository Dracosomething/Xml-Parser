using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XmlParser.src;
using XmlParser.src.xml;

namespace XmlParser
{
    public partial class MenuStripCustom : MenuStrip
    {
        private MainWindow main;
        private TextBox fileContent;

        private int screenWidth;
        private int barHeight = 40;
        // Item properties
        private int width = 50;
        private int height = 10;
        private Font font = new Font("Arial", 8);
        // color
        private int lightenAmount = 10;

        public MenuStripCustom(int screenWidth, MainWindow main, TextBox content)
        {
            this.screenWidth = screenWidth;
            this.main = main;
            this.fileContent = content;
            InitializeComponent();
        }

        private void onClickExit(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                main.Close();
            }
        }
        
        private void onClickOpenFile(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.openFileDialogue.Title = "Open File";
                this.openFileDialogue.Filter = "xml files (*.xml)|*.xml";
                this.openFileDialogue.InitialDirectory = "~";

                if (this.openFileDialogue.ShowDialog() == DialogResult.OK)
                {
                    var parser = new XMLParser(this.openFileDialogue.FileName);
                    this.fileContent.Text = parser.Content;
                }
            }
        }

        private void onClickDarkItem(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Constants.colorScheme.ChangeDarkMode();
                main.BackColor = Constants.colorScheme.Primary;
                this.BackColor = Constants.colorScheme.Secondary;
                this.file.ForeColor = Constants.colorScheme.Text;
                this.file.BackColor = Constants.colorScheme.Secondary;
                this.settings.ForeColor = Constants.colorScheme.Text;
                this.settings.BackColor = Constants.colorScheme.Secondary;
                this.fileContent.ForeColor = Constants.colorScheme.Text;
                this.fileContent.BackColor = Constants.colorScheme.Primary;
                var items = new List<ToolStripMenuItem> { openFile, exit, darkMode };
                foreach (ToolStripMenuItem item in items)
                {
                    item.BackColor = Constants.colorScheme.Accent;
                    item.ForeColor = Constants.colorScheme.Text;
                }
            }
        }

    }
}
