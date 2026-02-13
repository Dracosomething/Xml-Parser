using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace xml_parser
{
    partial class MenuStripCustom
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            if (disposing && (file != null))
            {
                file.Dispose();
            }
            if (disposing && (openFile != null))
            {
                openFile.Dispose();
            }
            if (disposing && (exit != null))
            {
                exit.Dispose();
            }
            if (disposing && (openFileDialogue != null))
            {
                openFileDialogue.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.file = new ToolStripMenuItem("File");
            this.openFile = new ToolStripMenuItem("Open");
            this.exit = new ToolStripMenuItem("Exit");
            this.settings = new ToolStripMenuItem("Settings");
            this.darkMode = new ToolStripMenuItem("Dark Mode");
            this.openFileDialogue = new OpenFileDialog();
            var items = new List<ToolStripMenuItem> { openFile, exit, darkMode };
            this.SuspendLayout();
            //
            // menuBar
            //
            this.Items.AddRange(new ToolStripItem[] { file, settings });
            this.BackColor = ColorScheme.colorScheme.Secondary;
            this.Size = new Size(screenWidth, barHeight);
            this.Location = new Point(0, 0);
            this.Renderer = new MenuStripCustomRenderer();

            //
            // file
            //
            this.file.DropDownItems.AddRange(new ToolStripItem[] { 
                this.openFile, 
                this.exit });
            this.file.Size = new Size(width, height * this.file.DropDownItems.Count);
            this.file.Font = this.font;
            this.file.ForeColor = ColorScheme.colorScheme.Text;
            this.file.BackColor = ColorScheme.colorScheme.Secondary;
            
            //
            // settings
            //
            this.settings.DropDownItems.AddRange(new ToolStripItem[] {
                this.darkMode });
            this.settings.Size = new Size(width, height * this.settings.DropDownItems.Count);
            this.settings.Font = this.font;
            this.settings.ForeColor = ColorScheme.colorScheme.Text;
            this.settings.BackColor = ColorScheme.colorScheme.Secondary;

            this.darkMode.MouseUp += new MouseEventHandler(this.onClickDarkItem);
            this.openFile.MouseUp += new MouseEventHandler(this.onClickOpenFile);
            this.exit.MouseUp += new MouseEventHandler(this.onClickExit);

            foreach (ToolStripMenuItem item in items)
            {
                item.Font = this.font;
                item.Size = new Size(width, height);
                item.BackColor = ColorScheme.colorScheme.Accent;
                item.ForeColor = ColorScheme.colorScheme.Text;
            }

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // items
        private ToolStripMenuItem file = null;
        private ToolStripMenuItem openFile = null;
        private ToolStripMenuItem exit = null;
        private ToolStripMenuItem settings = null;
        private ToolStripMenuItem darkMode = null;

        // dialogue
        private OpenFileDialog openFileDialogue = null;
    }
}
