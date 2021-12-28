using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PhotoUtil
{
	/// <summary>
	/// Summary description for ExifPropertiesForm.
	/// </summary>
	public class ExifPropertiesForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		Hashtable	_pooledFiles;
		ArrayList	_pooledOrder;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		int			_poolSize = 100;
		ListViewItemComparer	_lvicProperty;

		string _takenTime;

		internal string TakenTime {
			get { return _takenTime; }
		}

		public ExifPropertiesForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			_pooledFiles = new Hashtable(_poolSize);
			_pooledOrder = new ArrayList(_poolSize);

			_lvicProperty = new ListViewItemComparer();
			_lvicProperty.SortIndexRemoved += new EventHandler(_lvicProperty_SortIndexRemoved);
			listView1.ListViewItemSorter = _lvicProperty;
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "文件名:";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(128, 16);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(344, 21);
            this.textBox1.TabIndex = 1;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(16, 65);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(456, 144);
            this.listView1.TabIndex = 2;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 87;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Type";
            this.columnHeader2.Width = 126;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Value";
            this.columnHeader3.Width = 197;
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(170, 222);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 37);
            this.button1.TabIndex = 3;
            this.button1.Text = "确定";
            // 
            // ExifPropertiesForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleBaseSize = new System.Drawing.Size(10, 21);
            this.CancelButton = this.button1;
            this.ClientSize = new System.Drawing.Size(488, 269);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Name = "ExifPropertiesForm";
            this.Text = "ExifPropertiesForm";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		public void ShowFile(IWin32Window owner, string filename)
		{
			textBox1.Text = filename;
			listView1.Items.Clear();
			_lvicProperty.SortByColumn(-1);

			filename = filename.ToLower();
			PropertyItem[] pis = _pooledFiles[filename] as PropertyItem[];

			if(null == pis)  
			{
				try 
				{
					Image pic = Image.FromFile(filename);

					_takenTime = ExifHelper.GetImageTakenTimeString(pic, MainForm.PredefinedTimeFormat);

					pis = pic.PropertyItems;

					//!! Create an empty array, so we can pool it well.
					if(null == pis) pis = new PropertyItem[0];
					_pooledFiles[filename] = pis;

					if(_pooledOrder.Contains(filename)) 
					{
						_pooledOrder.Remove(filename);
					} 
					_pooledOrder.Add(filename);

					if(_pooledOrder.Count > _poolSize) 
					{
						_pooledFiles.Remove(_pooledOrder[0]);
						_pooledOrder.RemoveAt(0);
					}
				}
				catch(Exception ex)
				{
					_takenTime = ex.Message;
					MessageBox.Show(owner, ex.Message);
				}
			}

			foreach(PropertyItem pi in pis) 
			{
				string name, type, value;
				try 
				{
					name = ((EXIFIDCodes)pi.Id).ToString();
				} 
				catch
				{
					name = pi.Id.ToString();
				}

				try 
				{
					type = ((EXIFPropertyTypes)pi.Type).ToString();
				} 
				catch
				{
					type = pi.Type.ToString();
				}

				try 
				{
					value = GetPropertyValue(pi);
				}
				catch(Exception ex)
				{
					value = "Exception: " + ex.Message;
				}

				ListViewItem item = new ListViewItem(new string[]{name, type, value});
				listView1.Items.Add(item);
			}

			this.ShowDialog(owner);
		}

		string GetPropertyValue(PropertyItem pi)
		{
			string ret;

			switch(pi.Type) 
			{
				case (short)EXIFPropertyTypes.String:
					ret = ExifHelper.GetPropertyString(pi);
					break;
				case (short)EXIFPropertyTypes.SignedInt:
				case (short)EXIFPropertyTypes.UnsignedInt:
					ret = ExifHelper.GetPropertyInt32(pi).ToString();
					break;
				case (short)EXIFPropertyTypes.SignedRational:
				case (short)EXIFPropertyTypes.UnsignedRational:
					ret = ExifHelper.GetPropertyRational(pi).ToString();
					break;
				default:
					System.Text.StringBuilder sb = new System.Text.StringBuilder(pi.Value.Length * 3);
					foreach(byte bt in pi.Value)
					{
						sb.Append(string.Format("{0:X2} ", bt));
					}
					ret = sb.ToString();
					break;
			}

			return ret;
		}

		private void _lvicProperty_SortIndexRemoved(object sender, EventArgs e)
		{
			try 
			{
				int old = _lvicProperty.CurrentColumn;
				if(old >= 0) 
				{
					ColumnHeader ch = listView1.Columns[old];
					ch.Text = ch.Text.Substring(0, ch.Text.Length - ListViewItemComparer.SortingFlagLength);
				}
			} 
			catch{}
		}

		private void listView1_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			_lvicProperty.SortByColumn(e.Column);

			listView1.Sort();
			listView1.Columns[e.Column].Text += _lvicProperty.SortingFlag;
		}
	}
}
