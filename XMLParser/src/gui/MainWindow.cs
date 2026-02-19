using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using xml_parser.src.xml.dtd;

namespace XmlParser
{
    public partial class MainWindow : Form
    {
        private int height = 450;
        private int width = 800;
        private int menuBarMarginSize = 13;
        private int scrollbarWidth = 5;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void onResize(object sender, EventArgs e)
        {
            Size size = this.ClientSize;
            size.Height -= this.menuBar.Height - menuBarMarginSize;
            size.Width -= this.scrollbarWidth;
            this.Content.ClientSize = size;
        }
    }
}
