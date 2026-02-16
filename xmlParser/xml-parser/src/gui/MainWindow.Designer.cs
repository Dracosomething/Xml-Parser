using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System;
using XmlParser.src;

namespace XmlParser
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Content = new TextBox();
            this.menuBar = new MenuStripCustom(width, this, this.Content);
            this.SuspendLayout();
            this.menuBar.TabIndex = 0;
            //this.Content.TabIndex = 1;
            this.Content.Multiline = true;
            this.Content.ReadOnly = true;
            this.Content.Location = new Point(0, this.menuBar.ClientSize.Height - this.menuBarMarginSize);
            this.Content.ClientSize = new Size(width - this.scrollbarWidth, height - this.menuBar.ClientSize.Height + this.menuBarMarginSize);
            this.Content.ForeColor = Constants.colorScheme.Text;
            this.Content.BackColor = Constants.colorScheme.Primary;
            this.Content.BorderStyle = BorderStyle.None;
            this.Content.ScrollBars = ScrollBars.Both;
            //this.Content.

            // 
            // MainWindow
            // 
            this.Controls.Add(this.menuBar);
            this.Controls.Add(this.Content);
            this.ClientSize = new Size(width, height);
            this.Name = "MainWindow";
            this.Text = "xml parser";
            this.BackColor = Constants.colorScheme.Primary;
            this.SizeChanged += new EventHandler(this.onResize);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private MenuStripCustom menuBar = null;
        private TextBox Content = null;
    }
}

