using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Chat.TcpClient
{
	/// <summary>
	/// Summary description for SendPrivateMessageDialog.
	/// </summary>
	public class SendPrivateMessageDialog : System.Windows.Forms.Form
	{
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.ComboBox clientComboBox;
        private System.Windows.Forms.TextBox textBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SendPrivateMessageDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

            updateEnabled();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.clientComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox = new System.Windows.Forms.TextBox();
            this.CancelButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // clientComboBox
            // 
            this.clientComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.clientComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.clientComboBox.Location = new System.Drawing.Point(8, 24);
            this.clientComboBox.Name = "clientComboBox";
            this.clientComboBox.Size = new System.Drawing.Size(408, 21);
            this.clientComboBox.TabIndex = 0;
            this.clientComboBox.SelectedIndexChanged += new System.EventHandler(this.changed);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Send message to";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 56);
            this.label2.Name = "label2";
            this.label2.TabIndex = 2;
            this.label2.Text = "Message";
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.Location = new System.Drawing.Point(8, 72);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(408, 20);
            this.textBox.TabIndex = 3;
            this.textBox.Text = "";
            this.textBox.TextChanged += new System.EventHandler(this.changed);
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(344, 104);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.TabIndex = 4;
            this.CancelButton.Text = "Cancel";
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(264, 104);
            this.OKButton.Name = "OKButton";
            this.OKButton.TabIndex = 5;
            this.OKButton.Text = "OK";
            // 
            // SendPrivateMessageDialog
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            //this.CancelButton = this.CancelButton;
            this.ClientSize = new System.Drawing.Size(424, 136);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.clientComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "SendPrivateMessageDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SendPrivateMessageDialog";
            this.ResumeLayout(false);

        }
		#endregion

        private void changed(object sender, System.EventArgs e)
        {
            updateEnabled();
        }

        public string[] ClientNames 
        {
            set 
            {
                this.clientComboBox.Items.Clear();
                foreach (string s in value)
                    this.clientComboBox.Items.Add(s);
            }
        }

        public string ClientName 
        {
            get 
            {
                return (string)this.clientComboBox.SelectedItem;
            }
        }

        public string MessageText 
        {
            get 
            {
                return this.textBox.Text;
            }
        }

        private void updateEnabled() 
        {
            this.OKButton.Enabled = 
                this.clientComboBox.SelectedIndex != -1 &&
                this.textBox.Text.Length > 0;
        }
	}
}
