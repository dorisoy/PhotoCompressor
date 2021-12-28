using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PhotoUtil
{
	/// <summary>
	/// Summary description for PreviewForm.
	/// </summary>
	public class PreviewForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox picPreview;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.CheckBox cbFit;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox cbKeepRatio;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		internal Image PreviewImage {
			get { return  picPreview.Image; }
			set {
				picPreview.Image = value;
				ResizePictureBox();
			}
		}

		public PreviewForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbFit = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbKeepRatio = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picPreview
            // 
            this.picPreview.Location = new System.Drawing.Point(48, 13);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(100, 168);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picPreview.TabIndex = 1;
            this.picPreview.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel1.Controls.Add(this.picPreview);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(506, 291);
            this.panel1.TabIndex = 2;
            // 
            // cbFit
            // 
            this.cbFit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbFit.Checked = true;
            this.cbFit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbFit.Location = new System.Drawing.Point(16, 331);
            this.cbFit.Name = "cbFit";
            this.cbFit.Size = new System.Drawing.Size(160, 26);
            this.cbFit.TabIndex = 13;
            this.cbFit.Text = "&Fit window";
            this.cbFit.CheckedChanged += new System.EventHandler(this.cbFit_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Location = new System.Drawing.Point(16, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(512, 318);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "‘§¿¿";
            this.groupBox1.SizeChanged += new System.EventHandler(this.groupBox1_SizeChanged);
            // 
            // cbKeepRatio
            // 
            this.cbKeepRatio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbKeepRatio.Checked = true;
            this.cbKeepRatio.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbKeepRatio.Enabled = false;
            this.cbKeepRatio.Location = new System.Drawing.Point(208, 331);
            this.cbKeepRatio.Name = "cbKeepRatio";
            this.cbKeepRatio.Size = new System.Drawing.Size(160, 26);
            this.cbKeepRatio.TabIndex = 13;
            this.cbKeepRatio.Text = "&Keep ratio";
            this.cbKeepRatio.CheckedChanged += new System.EventHandler(this.cbKeepRatio_CheckedChanged);
            // 
            // PreviewForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(10, 21);
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(544, 365);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cbFit);
            this.Controls.Add(this.cbKeepRatio);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(200, 129);
            this.Name = "PreviewForm";
            this.Text = "‘§¿¿ ”Õº";
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void cbFit_CheckedChanged(object sender, System.EventArgs e)
		{
			if(cbFit.Checked)
			{
				picPreview.SizeMode = PictureBoxSizeMode.StretchImage;
				TryKeepRatio();
				//picPreview.Dock = DockStyle.Fill;
			}
			else 
			{
				picPreview.Dock = DockStyle.None;
				picPreview.SizeMode = PictureBoxSizeMode.AutoSize;
				picPreview.Location = new Point(0, 0);
			}

			cbKeepRatio.Enabled = cbFit.Checked;
		}

		private void cbKeepRatio_CheckedChanged(object sender, System.EventArgs e)
		{
			TryKeepRatio();
		}

		private void TryKeepRatio()
		{
			if(cbKeepRatio.Checked)
			{
				picPreview.Dock = DockStyle.None;
				ResizePictureBox();
			} 
			else
			{
				picPreview.Dock = DockStyle.Fill;
			}
		}

		public void ResizePictureBox()
		{
			if(picPreview.SizeMode != PictureBoxSizeMode.StretchImage
				|| !cbKeepRatio.Checked
				|| null == picPreview.Image)
			{
				return;
			}

			const int Margin = 2;
			double rPic = (double)picPreview.Image.Width / picPreview.Image.Height;
			double rPanel = (double)panel1.Size.Width / panel1.Size.Height;

			panel1.SuspendLayout();
			if(rPic < rPanel) 
			{	//!! use Height
				picPreview.Height = panel1.Size.Height - 2 * Margin;
				picPreview.Width = (int)(panel1.Size.Height * rPic);
				picPreview.Location = new Point((panel1.Size.Width - picPreview.Width) / 2, Margin);
			} 
			else
			{
				picPreview.Width = panel1.Size.Width - 2 * Margin;
				picPreview.Height = (int)(panel1.Size.Width / rPic);
				picPreview.Location = new Point(Margin, (panel1.Size.Height - picPreview.Height) / 2);
			}
			panel1.ResumeLayout();
		}

		private void groupBox1_SizeChanged(object sender, System.EventArgs e)
		{
			ResizePictureBox();
		}
	}
}
