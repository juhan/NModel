namespace WinFormImpl
{
    partial class WinFormImpl
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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbPlayer1 = new System.Windows.Forms.TextBox();
            this.tbPlayer2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Submit1 = new System.Windows.Forms.Button();
            this.Submit2 = new System.Windows.Forms.Button();
            this.tbResults = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbPlayer1
            // 
            this.tbPlayer1.Location = new System.Drawing.Point(14, 83);
            this.tbPlayer1.Name = "tbPlayer1";
            this.tbPlayer1.Size = new System.Drawing.Size(80, 20);
            this.tbPlayer1.TabIndex = 0;
            this.tbPlayer1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbPlayer1_KeyDown);
            // 
            // tbPlayer2
            // 
            this.tbPlayer2.Location = new System.Drawing.Point(159, 83);
            this.tbPlayer2.Name = "tbPlayer2";
            this.tbPlayer2.Size = new System.Drawing.Size(80, 20);
            this.tbPlayer2.TabIndex = 1;
            this.tbPlayer2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbPlayer2_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Player1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(159, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Player2";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 173);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Round results:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(257, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // Submit1
            // 
            this.Submit1.Location = new System.Drawing.Point(14, 111);
            this.Submit1.Name = "Submit1";
            this.Submit1.Size = new System.Drawing.Size(80, 27);
            this.Submit1.TabIndex = 9;
            this.Submit1.Text = "Submit1";
            this.Submit1.UseVisualStyleBackColor = true;
            this.Submit1.Click += new System.EventHandler(this.Submit1_Click);
            // 
            // Submit2
            // 
            this.Submit2.Location = new System.Drawing.Point(159, 111);
            this.Submit2.Name = "Submit2";
            this.Submit2.Size = new System.Drawing.Size(80, 27);
            this.Submit2.TabIndex = 10;
            this.Submit2.Text = "Submit2";
            this.Submit2.UseVisualStyleBackColor = true;
            this.Submit2.Click += new System.EventHandler(this.Submit2_Click);
            // 
            // tbResults
            // 
            this.tbResults.Location = new System.Drawing.Point(95, 170);
            this.tbResults.Name = "tbResults";
            this.tbResults.Size = new System.Drawing.Size(144, 20);
            this.tbResults.TabIndex = 11;
            // 
            // WinFormImpl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 215);
            this.Controls.Add(this.tbResults);
            this.Controls.Add(this.Submit2);
            this.Controls.Add(this.Submit1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbPlayer2);
            this.Controls.Add(this.tbPlayer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "WinFormImpl";
            this.Text = "WinFormImpl";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbPlayer1;
        private System.Windows.Forms.TextBox tbPlayer2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button Submit1;
        private System.Windows.Forms.Button Submit2;
        private System.Windows.Forms.TextBox tbResults;
    }
}

