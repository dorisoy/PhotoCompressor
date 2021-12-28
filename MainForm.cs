/// PhotoUtil, an util to print date-stamp on photo.
/// Refer to http://www.cnblogs.com/shensr/ for latest version.

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace PhotoUtil
{
	using System.Drawing.Imaging;

	/// <summary>
	/// Summary description for MainForm.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		#region Sub-controls...

		private System.Windows.Forms.GroupBox gbFiles;
		private System.Windows.Forms.ListView lvFiles;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Button btnRemove;
		private System.Windows.Forms.Button btnFont;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnSaveAll;
		private System.Windows.Forms.FontDialog dlgFont;
		private System.Windows.Forms.OpenFileDialog dlgFile;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.TextBox tbPrefix;
		private System.Windows.Forms.TextBox tbLocation;
		private System.Windows.Forms.CheckBox cbPreview;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbFormat;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox gbResults;
		private System.Windows.Forms.ListView lvResults;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ComboBox cmbLocation;
		private System.Windows.Forms.Button btnCheckAll;
		private System.Windows.Forms.Button btnUncheckAll;
		private System.Windows.Forms.Button btnCheckRev;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cmbTargetType;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ComboBox cmbWhichTime;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.CheckBox cbAsSource;
		private System.Windows.Forms.ContextMenu cmProperty;
		private System.Windows.Forms.Button btnTakenTime;
		private System.Windows.Forms.FolderBrowserDialog dlgFolder;
		private System.Windows.Forms.Button btnAddFolder;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.CheckBox cbIgnore;
		private System.Windows.Forms.Button btnAbout;
		private System.Windows.Forms.TextBox tbRatio;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox cbSaveOptions;
		private System.Windows.Forms.GroupBox gbDate;
		private System.Windows.Forms.GroupBox gbTarget;
		private System.Windows.Forms.MenuItem miSelAll;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		#region Private fields...
		internal const string PredefinedTimeFormat = "yyyy-MM-dd hh:mm:ss";
		Font	_font		= new Font("Arial", 8, FontStyle.Regular);
		Color	_color		= Color.Red;
		IDictionary	_picFormatTable	= null;
		PreviewForm	_previewForm = null;
		ExifPropertiesForm	_exifForm = null;
		bool	_stopOperating	= true;
		string	_picFileExts;
		ArrayList	_preAddedFiles;
		string	_cfgFilePath;
		string	_lastFolder		= string.Empty;
		string	_lastBaseDir	= string.Empty;
		static ImageCodecInfo	_jpegCI;
		EncoderParameters		_jpegEP;
		ListViewItemComparer	_lvicFiles;
		#endregion

		#region Properties...
		static ImageCodecInfo JpegCodeInfo 
		{
			get {
				if(null == _jpegCI) {
					_jpegCI = GetEncoderInfo("image/jpeg");
				}

				return _jpegCI;
			}
		}

		EncoderParameters JpegEncoderParameters {
			get { return _jpegEP; }
		}

		string ConfigFilePath 
		{
			get {
				if(null == _cfgFilePath) 
				{
					_cfgFilePath = Application.ExecutablePath;
					int pos = _cfgFilePath.LastIndexOf('.');

					if(pos >= 0)
						_cfgFilePath = string.Concat(_cfgFilePath.Substring(0, ++pos), "cfg");
				}

				return _cfgFilePath;
			}
		}

		PreviewForm	InnerPreviewForm
		{
			get {
				if( null == _previewForm ) 
				{
					_previewForm = new PreviewForm();
					_previewForm.Closing += new CancelEventHandler(_previewForm_Closing);
				}

				return _previewForm;
			}
		}

		ExifPropertiesForm	InnerExifForm
		{
			get {
				if( null == _exifForm ) 
				{
					_exifForm = new ExifPropertiesForm();
				}

				return _exifForm;
			}
		}
		#endregion

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			_lvicFiles = new ListViewItemComparer();
			_lvicFiles.SortIndexRemoved += new EventHandler(_lvicFiles_SortIndexRemoved);
			lvFiles.ListViewItemSorter = _lvicFiles;

			_picFormatTable	= new Hashtable();
			_picFormatTable.Add("jpg", ImageFormat.Jpeg);
			_picFormatTable.Add("jpeg", ImageFormat.Jpeg);
			_picFormatTable.Add("gif", ImageFormat.Gif);
			_picFormatTable.Add("bmp", ImageFormat.Bmp);
			_picFormatTable.Add("png", ImageFormat.Png);
			_picFormatTable.Add("tiff", ImageFormat.Tiff);
			_picFormatTable.Add("exif", ImageFormat.Exif);

			cmbLocation.Items.AddRange(Enum.GetNames(typeof(LocationType)));
			cmbWhichTime.Items.AddRange(Enum.GetNames(typeof(PictureTime)));
			cmbTargetType.Items.AddRange(Enum.GetNames(typeof(PictureType)));

			cmbLocation.SelectedIndex = 3;
			cmbWhichTime.SelectedIndex = 0;
			cmbTargetType.SelectedIndex = 0;

			BuildFileFilter();

			cbAsSource.Checked = true;
			EnableControls(true);

			tbFormat.Text = PredefinedTimeFormat;

			UpdateConfiguration(false);
		}

		protected override void OnClosed(EventArgs e)
		{
			if(cbSaveOptions.Checked) UpdateConfiguration(true);

			base.OnClosed (e);
		}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(null != _previewForm) _previewForm.Closing -= new CancelEventHandler(_previewForm_Closing);

			if( disposing )
			{
				if (components != null) 
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.gbFiles = new System.Windows.Forms.GroupBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lvFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmProperty = new System.Windows.Forms.ContextMenu();
            this.miSelAll = new System.Windows.Forms.MenuItem();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnCheckAll = new System.Windows.Forms.Button();
            this.btnUncheckAll = new System.Windows.Forms.Button();
            this.btnCheckRev = new System.Windows.Forms.Button();
            this.btnTakenTime = new System.Windows.Forms.Button();
            this.btnAddFolder = new System.Windows.Forms.Button();
            this.cbPreview = new System.Windows.Forms.CheckBox();
            this.tbPrefix = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnFont = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnSaveAll = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbLocation = new System.Windows.Forms.TextBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.dlgFont = new System.Windows.Forms.FontDialog();
            this.dlgFile = new System.Windows.Forms.OpenFileDialog();
            this.label3 = new System.Windows.Forms.Label();
            this.tbFormat = new System.Windows.Forms.TextBox();
            this.gbDate = new System.Windows.Forms.GroupBox();
            this.cmbWhichTime = new System.Windows.Forms.ComboBox();
            this.cmbLocation = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.gbResults = new System.Windows.Forms.GroupBox();
            this.lvResults = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cbAsSource = new System.Windows.Forms.CheckBox();
            this.cmbTargetType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbIgnore = new System.Windows.Forms.CheckBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.dlgFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.btnAbout = new System.Windows.Forms.Button();
            this.tbRatio = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.gbTarget = new System.Windows.Forms.GroupBox();
            this.cbSaveOptions = new System.Windows.Forms.CheckBox();
            this.gbFiles.SuspendLayout();
            this.gbDate.SuspendLayout();
            this.gbResults.SuspendLayout();
            this.gbTarget.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbFiles
            // 
            this.gbFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFiles.Controls.Add(this.btnAdd);
            this.gbFiles.Controls.Add(this.lvFiles);
            this.gbFiles.Controls.Add(this.btnRemove);
            this.gbFiles.Controls.Add(this.btnClear);
            this.gbFiles.Controls.Add(this.btnCheckAll);
            this.gbFiles.Controls.Add(this.btnUncheckAll);
            this.gbFiles.Controls.Add(this.btnCheckRev);
            this.gbFiles.Controls.Add(this.btnTakenTime);
            this.gbFiles.Controls.Add(this.btnAddFolder);
            this.gbFiles.Location = new System.Drawing.Point(16, 106);
            this.gbFiles.Name = "gbFiles";
            this.gbFiles.Size = new System.Drawing.Size(1740, 349);
            this.gbFiles.TabIndex = 1;
            this.gbFiles.TabStop = false;
            this.gbFiles.Text = "文件选项";
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(1548, 65);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(176, 37);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "添加文件(s)...";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // lvFiles
            // 
            this.lvFiles.AllowColumnReorder = true;
            this.lvFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvFiles.CheckBoxes = true;
            this.lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader6,
            this.columnHeader2,
            this.columnHeader3});
            this.lvFiles.ContextMenu = this.cmProperty;
            this.lvFiles.FullRowSelect = true;
            this.lvFiles.GridLines = true;
            this.lvFiles.HideSelection = false;
            this.lvFiles.Location = new System.Drawing.Point(16, 26);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new System.Drawing.Size(1516, 310);
            this.lvFiles.TabIndex = 0;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            this.lvFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvFiles_ColumnClick);
            this.lvFiles.SelectedIndexChanged += new System.EventHandler(this.lvFiles_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "FileName";
            this.columnHeader1.Width = 283;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Taken Time";
            this.columnHeader6.Width = 120;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Create Time";
            this.columnHeader2.Width = 120;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Modified Time";
            this.columnHeader3.Width = 120;
            // 
            // cmProperty
            // 
            this.cmProperty.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miSelAll});
            this.cmProperty.Popup += new System.EventHandler(this.cmProperty_Popup);
            // 
            // miSelAll
            // 
            this.miSelAll.Index = 0;
            this.miSelAll.Text = "Select&All";
            this.miSelAll.Click += new System.EventHandler(this.miSelAll_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemove.Location = new System.Drawing.Point(1548, 103);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(176, 38);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "移除选择";
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(1548, 142);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(176, 37);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "清除全部";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCheckAll
            // 
            this.btnCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckAll.Location = new System.Drawing.Point(1548, 181);
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.Size = new System.Drawing.Size(176, 37);
            this.btnCheckAll.TabIndex = 3;
            this.btnCheckAll.Text = "选择全部";
            this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
            // 
            // btnUncheckAll
            // 
            this.btnUncheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUncheckAll.Location = new System.Drawing.Point(1548, 220);
            this.btnUncheckAll.Name = "btnUncheckAll";
            this.btnUncheckAll.Size = new System.Drawing.Size(176, 37);
            this.btnUncheckAll.TabIndex = 3;
            this.btnUncheckAll.Text = "取消选择";
            this.btnUncheckAll.Click += new System.EventHandler(this.btnUncheckAll_Click);
            // 
            // btnCheckRev
            // 
            this.btnCheckRev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckRev.Location = new System.Drawing.Point(1548, 258);
            this.btnCheckRev.Name = "btnCheckRev";
            this.btnCheckRev.Size = new System.Drawing.Size(176, 38);
            this.btnCheckRev.TabIndex = 3;
            this.btnCheckRev.Text = "选择";
            this.btnCheckRev.Click += new System.EventHandler(this.btnCheckRev_Click);
            // 
            // btnTakenTime
            // 
            this.btnTakenTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTakenTime.Location = new System.Drawing.Point(1548, 297);
            this.btnTakenTime.Name = "btnTakenTime";
            this.btnTakenTime.Size = new System.Drawing.Size(176, 37);
            this.btnTakenTime.TabIndex = 3;
            this.btnTakenTime.Text = "添加时间";
            this.btnTakenTime.Click += new System.EventHandler(this.btnTakenTime_Click);
            // 
            // btnAddFolder
            // 
            this.btnAddFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddFolder.Location = new System.Drawing.Point(1548, 26);
            this.btnAddFolder.Name = "btnAddFolder";
            this.btnAddFolder.Size = new System.Drawing.Size(176, 37);
            this.btnAddFolder.TabIndex = 1;
            this.btnAddFolder.Text = "添加文件夹...";
            this.btnAddFolder.Click += new System.EventHandler(this.btnAddFolder_Click);
            // 
            // cbPreview
            // 
            this.cbPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbPreview.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbPreview.Location = new System.Drawing.Point(1548, 26);
            this.cbPreview.Name = "cbPreview";
            this.cbPreview.Size = new System.Drawing.Size(176, 39);
            this.cbPreview.TabIndex = 12;
            this.cbPreview.Text = "预览";
            this.cbPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbPreview.CheckedChanged += new System.EventHandler(this.cbPreview_CheckedChanged);
            // 
            // tbPrefix
            // 
            this.tbPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPrefix.Location = new System.Drawing.Point(208, 63);
            this.tbPrefix.Name = "tbPrefix";
            this.tbPrefix.Size = new System.Drawing.Size(572, 28);
            this.tbPrefix.TabIndex = 9;
            this.tbPrefix.Text = "Dated-";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Location = new System.Drawing.Point(16, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(192, 26);
            this.label1.TabIndex = 8;
            this.label1.Text = "前缀:";
            // 
            // btnFont
            // 
            this.btnFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFont.Location = new System.Drawing.Point(1340, 27);
            this.btnFont.Name = "btnFont";
            this.btnFont.Size = new System.Drawing.Size(192, 38);
            this.btnFont.TabIndex = 4;
            this.btnFont.Text = "字体 && 颜色...";
            this.btnFont.Click += new System.EventHandler(this.btnFont_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(632, 908);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(208, 37);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "选择保存";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSaveAll
            // 
            this.btnSaveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveAll.Location = new System.Drawing.Point(857, 908);
            this.btnSaveAll.Name = "btnSaveAll";
            this.btnSaveAll.Size = new System.Drawing.Size(208, 37);
            this.btnSaveAll.TabIndex = 6;
            this.btnSaveAll.Text = "保存全部";
            this.btnSaveAll.Click += new System.EventHandler(this.btnSaveAll_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(876, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 26);
            this.label2.TabIndex = 10;
            this.label2.Text = "坐标:";
            // 
            // tbLocation
            // 
            this.tbLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLocation.Location = new System.Drawing.Point(972, 29);
            this.tbLocation.Name = "tbLocation";
            this.tbLocation.Size = new System.Drawing.Size(96, 28);
            this.tbLocation.TabIndex = 11;
            this.tbLocation.Text = "30, 20";
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Location = new System.Drawing.Point(1532, 908);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(208, 37);
            this.btnExit.TabIndex = 7;
            this.btnExit.Text = "退出";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // dlgFont
            // 
            this.dlgFont.ShowColor = true;
            // 
            // dlgFile
            // 
            this.dlgFile.Multiselect = true;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(272, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 26);
            this.label3.TabIndex = 10;
            this.label3.Text = "格式:";
            // 
            // tbFormat
            // 
            this.tbFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFormat.Location = new System.Drawing.Point(352, 29);
            this.tbFormat.Name = "tbFormat";
            this.tbFormat.Size = new System.Drawing.Size(512, 28);
            this.tbFormat.TabIndex = 11;
            // 
            // gbDate
            // 
            this.gbDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDate.Controls.Add(this.cmbWhichTime);
            this.gbDate.Controls.Add(this.cmbLocation);
            this.gbDate.Controls.Add(this.tbLocation);
            this.gbDate.Controls.Add(this.tbFormat);
            this.gbDate.Controls.Add(this.label3);
            this.gbDate.Controls.Add(this.cbPreview);
            this.gbDate.Controls.Add(this.label2);
            this.gbDate.Controls.Add(this.btnFont);
            this.gbDate.Controls.Add(this.label4);
            this.gbDate.Controls.Add(this.label6);
            this.gbDate.Location = new System.Drawing.Point(16, 13);
            this.gbDate.Name = "gbDate";
            this.gbDate.Size = new System.Drawing.Size(1740, 77);
            this.gbDate.TabIndex = 2;
            this.gbDate.TabStop = false;
            this.gbDate.Text = "数据选项";
            // 
            // cmbWhichTime
            // 
            this.cmbWhichTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWhichTime.Location = new System.Drawing.Point(80, 26);
            this.cmbWhichTime.Name = "cmbWhichTime";
            this.cmbWhichTime.Size = new System.Drawing.Size(192, 26);
            this.cmbWhichTime.TabIndex = 13;
            // 
            // cmbLocation
            // 
            this.cmbLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLocation.Location = new System.Drawing.Point(1132, 29);
            this.cmbLocation.Name = "cmbLocation";
            this.cmbLocation.Size = new System.Drawing.Size(192, 26);
            this.cmbLocation.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Location = new System.Drawing.Point(1068, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 26);
            this.label4.TabIndex = 10;
            this.label4.Text = "位置";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(16, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 26);
            this.label6.TabIndex = 10;
            this.label6.Text = "使用";
            // 
            // gbResults
            // 
            this.gbResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbResults.Controls.Add(this.lvResults);
            this.gbResults.Location = new System.Drawing.Point(16, 662);
            this.gbResults.Name = "gbResults";
            this.gbResults.Size = new System.Drawing.Size(1740, 232);
            this.gbResults.TabIndex = 3;
            this.gbResults.TabStop = false;
            this.gbResults.Text = "操作结果";
            // 
            // lvResults
            // 
            this.lvResults.AllowColumnReorder = true;
            this.lvResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5});
            this.lvResults.FullRowSelect = true;
            this.lvResults.GridLines = true;
            this.lvResults.HideSelection = false;
            this.lvResults.Location = new System.Drawing.Point(16, 26);
            this.lvResults.MultiSelect = false;
            this.lvResults.Name = "lvResults";
            this.lvResults.Size = new System.Drawing.Size(1708, 194);
            this.lvResults.TabIndex = 0;
            this.lvResults.UseCompatibleStateImageBehavior = false;
            this.lvResults.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Operation";
            this.columnHeader4.Width = 685;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Result";
            this.columnHeader5.Width = 48;
            // 
            // cbAsSource
            // 
            this.cbAsSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAsSource.Location = new System.Drawing.Point(620, 102);
            this.cbAsSource.Name = "cbAsSource";
            this.cbAsSource.Size = new System.Drawing.Size(160, 25);
            this.cbAsSource.TabIndex = 14;
            this.cbAsSource.Text = "作为来源";
            this.cbAsSource.CheckedChanged += new System.EventHandler(this.cbAsSource_CheckedChanged);
            // 
            // cmbTargetType
            // 
            this.cmbTargetType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTargetType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTargetType.Location = new System.Drawing.Point(428, 102);
            this.cmbTargetType.Name = "cmbTargetType";
            this.cmbTargetType.Size = new System.Drawing.Size(112, 26);
            this.cmbTargetType.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.Location = new System.Drawing.Point(236, 102);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(192, 25);
            this.label5.TabIndex = 10;
            this.label5.Text = "文件类型:";
            // 
            // cbIgnore
            // 
            this.cbIgnore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbIgnore.Checked = true;
            this.cbIgnore.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIgnore.Location = new System.Drawing.Point(860, 63);
            this.cbIgnore.Name = "cbIgnore";
            this.cbIgnore.Size = new System.Drawing.Size(512, 26);
            this.cbIgnore.TabIndex = 14;
            this.cbIgnore.Text = "添加文件夹时忽略带有前缀的文件";
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(1082, 908);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(208, 37);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "停止";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // dlgFolder
            // 
            this.dlgFolder.Description = "Please select a folder!";
            this.dlgFolder.ShowNewFolderButton = false;
            // 
            // btnAbout
            // 
            this.btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbout.Location = new System.Drawing.Point(1307, 908);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(208, 37);
            this.btnAbout.TabIndex = 7;
            this.btnAbout.Text = "关于";
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // tbRatio
            // 
            this.tbRatio.Location = new System.Drawing.Point(1100, 95);
            this.tbRatio.Name = "tbRatio";
            this.tbRatio.Size = new System.Drawing.Size(64, 28);
            this.tbRatio.TabIndex = 15;
            this.tbRatio.Text = "90";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.Location = new System.Drawing.Point(860, 102);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(304, 25);
            this.label7.TabIndex = 10;
            this.label7.Text = "Jpeg 图片质量 [0 - 100]:";
            // 
            // gbTarget
            // 
            this.gbTarget.Controls.Add(this.tbRatio);
            this.gbTarget.Controls.Add(this.label7);
            this.gbTarget.Controls.Add(this.cbAsSource);
            this.gbTarget.Controls.Add(this.cmbTargetType);
            this.gbTarget.Controls.Add(this.label5);
            this.gbTarget.Controls.Add(this.cbIgnore);
            this.gbTarget.Controls.Add(this.tbPrefix);
            this.gbTarget.Controls.Add(this.label1);
            this.gbTarget.Controls.Add(this.cbSaveOptions);
            this.gbTarget.Location = new System.Drawing.Point(16, 476);
            this.gbTarget.Name = "gbTarget";
            this.gbTarget.Size = new System.Drawing.Size(1740, 154);
            this.gbTarget.TabIndex = 8;
            this.gbTarget.TabStop = false;
            this.gbTarget.Text = "目标选项";
            // 
            // cbSaveOptions
            // 
            this.cbSaveOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSaveOptions.Checked = true;
            this.cbSaveOptions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSaveOptions.Location = new System.Drawing.Point(1436, 102);
            this.cbSaveOptions.Name = "cbSaveOptions";
            this.cbSaveOptions.Size = new System.Drawing.Size(288, 25);
            this.cbSaveOptions.TabIndex = 14;
            this.cbSaveOptions.Text = "退出时保存选项";
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(10, 21);
            this.ClientSize = new System.Drawing.Size(1772, 957);
            this.Controls.Add(this.gbTarget);
            this.Controls.Add(this.gbResults);
            this.Controls.Add(this.gbDate);
            this.Controls.Add(this.gbFiles);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnSaveAll);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnAbout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "PhotoCompressor";
            this.gbFiles.ResumeLayout(false);
            this.gbDate.ResumeLayout(false);
            this.gbDate.PerformLayout();
            this.gbResults.ResumeLayout(false);
            this.gbTarget.ResumeLayout(false);
            this.gbTarget.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		#region Private methodes...

		private static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			int j;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			for(j = 0; j < encoders.Length; ++j)
			{
				if(encoders[j].MimeType == mimeType)
					return encoders[j];
			}
			return null;
		}

		XmlNode XmlAddChildNode(XmlNode node, string name)
		{
			XmlNode xn = node.OwnerDocument.CreateElement(name);
			node.AppendChild(xn);
			return xn;
		}

		XmlAttribute XmlAddAttribute(XmlNode node, string name, string value)
		{
			XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
			attr.Value = value;
			node.Attributes.Append(attr);
			return attr;
		}

		void UpdateConfiguration(bool updated)
		{
			XmlDocument doc = new XmlDocument();

			try 
			{
				if(updated) 
				{
					doc.LoadXml(@"<Configuration/>");

					XmlNode node = XmlAddChildNode(doc.DocumentElement, "DateOptions");
					XmlAddAttribute(node, "use", cmbWhichTime.Text);
					XmlAddAttribute(node, "format", tbFormat.Text);
					XmlAddAttribute(node, "location", tbLocation.Text);
					XmlAddAttribute(node, "from", cmbLocation.Text);

					node = XmlAddChildNode(node, "Font");
					XmlAddAttribute(node, "family", this._font.FontFamily.Name);
					XmlAddAttribute(node, "size", this._font.Size.ToString());
					XmlAddAttribute(node, "style", this._font.Style.ToString());
					XmlAddAttribute(node, "color", this._color.ToArgb().ToString());

					node = XmlAddChildNode(doc.DocumentElement, "TargetOptions");
					XmlAddAttribute(node, "prefix", tbPrefix.Text);
					XmlAddAttribute(node, "ignore", cbIgnore.Checked.ToString());
					XmlAddAttribute(node, "type", cmbTargetType.Text);
					XmlAddAttribute(node, "as-source", cbAsSource.Checked.ToString());
					XmlAddAttribute(node, "jpeg-compression-ratio", tbRatio.Text);
					XmlAddAttribute(node, "save-onexit", cbSaveOptions.Checked.ToString());

					node = XmlAddChildNode(doc.DocumentElement, "LastVisited");
					XmlAddAttribute(node, "add-folder", _lastFolder);
					XmlAddAttribute(node, "add-files", _lastBaseDir);

					doc.Save(this.ConfigFilePath);
				}
				else
				{
					doc.Load(this.ConfigFilePath);

					XmlNode node = doc.DocumentElement.SelectSingleNode("DateOptions");
					cmbWhichTime.SelectedItem	= node.Attributes["use"].Value;
					tbFormat.Text				= node.Attributes["format"].Value;
					tbLocation.Text				= node.Attributes["location"].Value;
					cmbLocation.SelectedItem	= node.Attributes["from"].Value;

					node = node.SelectSingleNode("Font");
					this._font					=
						new Font(node.Attributes["family"].Value,
							float.Parse(node.Attributes["size"].Value),
							(FontStyle)Enum.Parse(typeof(FontStyle), node.Attributes["style"].Value)
						);
					this._color = Color.FromArgb(int.Parse(node.Attributes["color"].Value));

					node = doc.DocumentElement.SelectSingleNode("TargetOptions");
					tbPrefix.Text		= node.Attributes["prefix"].Value;
					cbIgnore.Checked	= bool.Parse(node.Attributes["ignore"].Value);
					cmbTargetType.SelectedItem	= node.Attributes["type"].Value;
					cbAsSource.Checked	= bool.Parse(node.Attributes["as-source"].Value);
					tbRatio.Text		= node.Attributes["jpeg-compression-ratio"].Value;
					cbSaveOptions.Checked = bool.Parse(node.Attributes["save-onexit"].Value);

					node = doc.DocumentElement.SelectSingleNode("LastVisited");
					_lastFolder = node.Attributes["add-folder"].Value;
					_lastBaseDir = node.Attributes["add-files"].Value;
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Trace.WriteLine(ex);
			}
		}

		void BuildJpegEncoderParameters(long ratio)
		{
			_jpegEP = new EncoderParameters(1);
			_jpegEP.Param[0] = new EncoderParameter(Encoder.Quality, ratio);
		}

		void BuildFileFilter()
		{
			StringBuilder filter = new StringBuilder(256);
			foreach(string picType in Enum.GetNames(typeof(PictureType))) 
			{
				filter.Append("*.");
				filter.Append(picType.ToLower());
				filter.Append("; ");
			}
			string exts = filter.ToString(0, filter.Length-2);
			_picFileExts = "*.jpg; " + exts;

			this.dlgFile.Filter = string.Concat("All Graphics Files(", exts, ")|", _picFileExts, "|All Files(*.*)|*.*");
		}

		void EnableControls(bool enabled)
		{
			_stopOperating			= enabled;

			gbDate.Enabled		= enabled;
			gbFiles.Enabled		= enabled;
			gbTarget.Enabled	= enabled;
			btnSave.Enabled		= enabled;
			btnSaveAll.Enabled	= enabled;

			btnStop.Enabled			= !enabled;
		}

		void ShowErrorMsg(string msg)
		{
			MessageBox.Show(this, msg, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		bool CheckInput()
		{
			string text;

			try 
			{
				tbFormat.Text = tbFormat.Text.Trim();
				text = DateTime.MinValue.ToString(tbFormat.Text);
			} 
			catch(Exception ex)
			{
				tbFormat.SelectAll();
				tbFormat.Focus();
				ShowErrorMsg(string.Format("Invalid time format string({0})!", ex.Message));
				return false;
			}

			try 
			{
				tbLocation.Text = tbLocation.Text.Trim();
				Text2Point(tbLocation.Text);
			} 
			catch(Exception ex)
			{
				tbLocation.SelectAll();
				tbLocation.Focus();
				ShowErrorMsg(ex.Message);
				return false;
			}

			try 
			{
				tbRatio.Text = tbRatio.Text.Trim();
				int n = int.Parse(tbRatio.Text);

				if(n < 0 || n > 100) throw new Exception("The compression ratio should between 0 and 100!");
			} 
			catch(Exception ex)
			{
				tbRatio.SelectAll();
				tbRatio.Focus();
				ShowErrorMsg(ex.Message);
				return false;
			}

			return true;
		}

		Image GenerateImage(string source, PictureTime picTime, string timeFormat, Point location, LocationType lt, Brush brush)
		{
			Graphics g = null;
			try 
			{
				Image pic = Bitmap.FromFile(source);

				DateTime time = DateTime.MinValue;
				switch(picTime) 
				{
					case PictureTime.CreatedTime:
						time = new FileInfo(source).CreationTime;
						break;
					case PictureTime.ModifiedTime:
						time = new FileInfo(source).LastWriteTime;
						break;
					case PictureTime.TakenTime:
						time = ExifHelper.GetPropertyDateTime(pic.GetPropertyItem((int)EXIFIDCodes.DateTimeOriginal));
						break;
					default:
						throw new Exception(string.Format("Unknow type - {0}!", picTime));
				}

				string text = time.ToString(timeFormat);


				g = Graphics.FromImage(pic);

				SizeF szText = g.MeasureString(text, _font);
				float x, y;
				switch(lt)
				{
					case LocationType.TopLeft:
						x = (float)location.X;
						y = (float)location.Y;
						break;
					case LocationType.TopRight:
						x = (float)pic.Size.Width - szText.Width - (float)location.X;
						y = (float)location.Y;
						break;
					case LocationType.BottomLeft:
						x = (float)location.X;
						y = (float)pic.Size.Height - szText.Height - (float)location.Y;
						break;
					//case LocationType.BottomRight:
					default:
						x = (float)pic.Size.Width - szText.Width - (float)location.X;
						y = (float)pic.Size.Height - szText.Height - (float)location.Y;
						break;
				}

				g.DrawString(text, _font, brush, x, y);	//StringFormat.GenericDefault;

				return pic;
			} 
			finally
			{
				if(null != g) g.Dispose();
			}
		}

		private void TryPreviewSelectedFile()
		{
			if(!cbPreview.Checked || null == lvFiles.FocusedItem) return;

			if(!CheckInput()) return;

			string filename = lvFiles.FocusedItem.Text;
			try 
			{
				PictureTime timeType = (PictureTime)Enum.Parse(typeof(PictureTime), cmbWhichTime.Text);
				LocationType lt = (LocationType)Enum.Parse(typeof(LocationType), cmbLocation.Text);

				InnerPreviewForm.PreviewImage = GenerateImage(filename, timeType, tbFormat.Text, Text2Point(tbLocation.Text), lt, new SolidBrush(_color));
				InnerPreviewForm.Show();
				//InnerPreviewForm.Activate();
			}
			catch(Exception ex)
			{
				ShowErrorMsg(ex.Message);
			}
		}

		Point Text2Point(string text)
		{
			string[] ar = tbLocation.Text.Split(',');
			if(ar.Length == 2)
			{
				try 
				{
					int x = int.Parse(ar[0].Trim());
					int y = int.Parse(ar[1].Trim());

					return new Point(x, y);
				} 
				catch
				{
				}
			}

			throw new Exception("The format should be \"x, y\"");
		}
		#endregion Private methodes...

		#region Save picture...
		private void btnSaveAll_Click(object sender, System.EventArgs e)
		{
			SaveFiles(lvFiles.Items);
		}

		private void btnSave_Click(object sender, System.EventArgs e)
		{
			SaveFiles(lvFiles.CheckedItems);
		}

		private void btnStop_Click(object sender, System.EventArgs e)
		{
			_stopOperating = true;
		}

		void SaveFile(string target, Image pic)
		{
			int pos = target.LastIndexOf('.');
			string ext = (pos < 0) ? "" : target.Substring(++pos).ToLower();

			ImageFormat format = _picFormatTable[ext] as ImageFormat;
			if(format == null) format = ImageFormat.Bmp;

			if(format == ImageFormat.Jpeg) 
			{
				pic.Save(target, JpegCodeInfo, JpegEncoderParameters);
			}
			else 
			{
				pic.Save(target, format);
			}
		}

		private void SaveFiles(ICollection items)
		{
			if(items.Count <= 0) return;

			string prefix = tbPrefix.Text;
			if(prefix.Length <= 0) {
				if(DialogResult.Yes != MessageBox.Show(this, "Do you really want to replace the original file(s).", "Warning...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning ))
				{
					tbPrefix.Focus();
					return;
				}
			}

			if(!CheckInput()) return;

			this.Cursor = Cursors.WaitCursor;

			lvResults.Items.Clear();
			EnableControls(false);

			try 
			{
				string timeFormat = tbFormat.Text;
				Point location = Text2Point(tbLocation.Text);
				LocationType lt = (LocationType)Enum.Parse(typeof(LocationType), cmbLocation.Text);
				PictureTime timeType = (PictureTime)Enum.Parse(typeof(PictureTime), cmbWhichTime.Text);
				BuildJpegEncoderParameters(long.Parse(tbRatio.Text));

				int count = 0;
				foreach(ListViewItem item in items) 
				{
					Application.DoEvents();
					if(_stopOperating) break;

					string source = item.Text;
					Brush brush = new SolidBrush(_color);

					//!! Build target
					string target;
					int pos = source.LastIndexOf('\\');
					if(pos < 0) target = prefix + source;
					else target = string.Concat(
							 source.Substring(0, ++pos),
							 prefix,
							 source.Substring(pos)
							 );

					if(!cbAsSource.Checked) 
					{
						pos = target.LastIndexOf('.');
						if(pos >= 0) target = target.Substring(0, pos);
						target = string.Concat(target, ".", cmbTargetType.Text);
					}

					ListViewItem lviResult = lvResults.Items.Add(string.Concat(source, " => ", target));
					try 
					{
						Image pic = GenerateImage(source, timeType, timeFormat, location, lt, brush);
						SaveFile(target, pic);

						lviResult.SubItems.Add( "OK" );
						count++;
					}
					catch(Exception ex)
					{
						lviResult.SubItems.Add( "Failed: " + ex.Message );
					}
				}

				this.Cursor = Cursors.Default;

				MessageBox.Show(this, string.Format("Successfully processed {0} files!", count));
			} 
			catch(Exception ex)
			{
				this.Cursor = Cursors.Default;
				ShowErrorMsg(ex.Message);
			}
			finally
			{
				EnableControls(true);
			}
		}
		#endregion

		#region File options...
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			lvFiles.Items.Clear();
		}

		private void btnRemove_Click(object sender, System.EventArgs e)
		{
			int n = lvFiles.SelectedIndices.Count;
			while(n-- >0) lvFiles.Items.RemoveAt(lvFiles.SelectedIndices[n]);
		}

		private void btnAddFolder_Click(object sender, System.EventArgs e)
		{
			dlgFolder.SelectedPath = _lastFolder;
			if(DialogResult.OK != dlgFolder.ShowDialog(this)) return;
			_lastFolder = dlgFolder.SelectedPath;

			this.Cursor = Cursors.WaitCursor;
			EnableControls(false);

			BuildPreAddedFiles();
			_lvicFiles.SortByColumn(-1);

			try 
			{
				AddFilesInFolder(dlgFolder.SelectedPath, this._picFileExts, true, cbIgnore.Checked);
			}
			catch
			{
			}
			EnableControls(true);
			this.Cursor = Cursors.Default;
		}

		private void AddFilesInFolder(string path, string pattern, bool incSub, bool ignorePrefixed)
		{
			Directory.SetCurrentDirectory(path);

			FileFinder fd = new FileFinder();
			if( !fd.FindFirst("*.*") ) return;

			if(path[path.Length-1] != '\\') path += '\\';
			while( fd.FindNext() ) 
			{
				Application.DoEvents();
				if(this._stopOperating) break;

				if(fd.IsDots()) continue;

				if( fd.IsDirectory() ) 
				{
					AddFilesInFolder(path + fd.FileName, pattern, incSub, ignorePrefixed);
				}

				if(ignorePrefixed &&
					(tbPrefix.Text.Length > 0) &&
					(fd.FileName.Length > tbPrefix.Text.Length)
					)
				{
					string prefex = tbPrefix.Text.ToLower();
					if(prefex == fd.FileName.Substring(prefex.Length).ToLower()) continue;
				}

				string ext = string.Concat("*.", fd.FileExt.ToLower(), ";");
				if(pattern.IndexOf(ext) < 0) continue;

				AddFile(path + fd.FileName);
			}
		}

		private void BuildPreAddedFiles()
		{
			_preAddedFiles = new ArrayList(lvFiles.Items.Count);
			foreach(ListViewItem item in lvFiles.Items)
				_preAddedFiles.Add(item.Text);

			//!! Sort it for BinarySearch
			_preAddedFiles.Sort();
		}

		private void AddFile(string filename)
		{
			//!! We have sorted it @ BuildPreAddedFiles
			if( _preAddedFiles.BinarySearch(filename) >= 0) return;

			try 
			{
				FileInfo fi = new FileInfo( filename );
				string[] subItems = new string[lvFiles.Columns.Count];
				subItems[0] = filename;
				subItems[1] = "Not read!";//ExifHelper.GetPicDateTime(filename, EXIFIDCodes.DateTimeOriginal).ToString(PredefinedTimeFormat);
				subItems[2] = fi.CreationTime.ToString(PredefinedTimeFormat);
				subItems[3] = fi.LastWriteTime.ToString(PredefinedTimeFormat);
				ListViewItem item = new ListViewItem(subItems);
				lvFiles.Items.Add(item);
			} 
			catch
			{
			}
		}

		private void btnAdd_Click(object sender, System.EventArgs e)
		{
			dlgFile.InitialDirectory = _lastBaseDir;
			if(DialogResult.OK != dlgFile.ShowDialog(this)) return;
			_lastBaseDir = dlgFile.InitialDirectory;

			BuildPreAddedFiles();
			_lvicFiles.SortByColumn(-1);

			foreach(string filename in dlgFile.FileNames) 
			{
				AddFile(filename);
			}
		}

		private void btnCheckAll_Click(object sender, System.EventArgs e)
		{
			foreach(ListViewItem item in lvFiles.Items) item.Checked = true;
		}

		private void btnUncheckAll_Click(object sender, System.EventArgs e)
		{
			foreach(ListViewItem item in lvFiles.Items) item.Checked = false;
		}

		private void btnCheckRev_Click(object sender, System.EventArgs e)
		{
			foreach(ListViewItem item in lvFiles.Items) item.Checked = !item.Checked;
		}

		private void btnTakenTime_Click(object sender, System.EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			EnableControls(false);

			foreach(ListViewItem item in lvFiles.SelectedItems) {
				if(_stopOperating) break;

				//!! use it as a flag to indicate whether we had taken the time before!
				if(null != item.Tag) continue;

				string text;
				try 
				{
					Image image = Image.FromFile(item.Text);
					text = ExifHelper.GetImageTakenTimeString(image, PredefinedTimeFormat);
				}
				catch(Exception ex)
				{
					text = ex.Message;
				}

				item.SubItems[1].Text = text;
				item.Tag = true;
			}

			EnableControls(true);
			this.Cursor = Cursors.Default;
		}
		#endregion

		#region Other event handlers...
		private void btnExit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void btnFont_Click(object sender, System.EventArgs e)
		{
			dlgFont.Font = _font;
			dlgFont.Color = _color;
			if(DialogResult.OK == dlgFont.ShowDialog(this))
			{
				_font = dlgFont.Font;
				_color = dlgFont.Color;

				TryPreviewSelectedFile();
			}
		}

		private void cbPreview_CheckedChanged(object sender, System.EventArgs e)
		{
			if(cbPreview.Checked) TryPreviewSelectedFile();
			else if(null != _previewForm) _previewForm.Hide();
		}

		private void cbAsSource_CheckedChanged(object sender, System.EventArgs e)
		{
			cmbTargetType.Enabled = !cbAsSource.Checked;
		}

		private void lvFiles_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TryPreviewSelectedFile();
		}

		private void _previewForm_Closing(object sender, CancelEventArgs e)
		{
			_previewForm.Hide();
			e.Cancel = true;
			cbPreview.Checked = false;
		}

		private void OnImageProperty(object sender, System.EventArgs e)
		{
			if(null != lvFiles.FocusedItem) 
			{
				InnerExifForm.ShowFile(this, lvFiles.FocusedItem.Text);

				if(null == lvFiles.FocusedItem.Tag) 
				{
					lvFiles.FocusedItem.SubItems[1].Text = InnerExifForm.TakenTime;
					lvFiles.FocusedItem.Tag = true;
				}
			}
		}

		private void miSelAll_Click(object sender, System.EventArgs e)
		{
			lvFiles.SuspendLayout();
			foreach(ListViewItem item in lvFiles.Items)
			{
				item.Selected = true;
			}
			lvFiles.ResumeLayout();
		}

		private void cmProperty_Popup(object sender, System.EventArgs e)
		{
			int n = cmProperty.MenuItems.Count;
			if(null == lvFiles.FocusedItem)
			{
				if(n == 3) 
				{
					cmProperty.MenuItems.RemoveAt(1);
					cmProperty.MenuItems.RemoveAt(2);
				}
			}
			else if(n == 1)
			{
				cmProperty.MenuItems.Add("-");
				cmProperty.MenuItems.Add("&Property...", new EventHandler(OnImageProperty));
			}
		}

		private void btnAbout_Click(object sender, System.EventArgs e)
		{
			MessageBox.Show(this,@"照片压缩器 Copyright (C) 2021,dorisoy http://www.xamarin.top",
				this.Text,
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		private void _lvicFiles_SortIndexRemoved(object sender, EventArgs e)
		{
			try 
			{
				int old = _lvicFiles.CurrentColumn;
				if(old >= 0) 
				{
					ColumnHeader ch = lvFiles.Columns[old];
					ch.Text = ch.Text.Substring(0, ch.Text.Length - ListViewItemComparer.SortingFlagLength);
				}
			} 
			catch{}
		}

		private void lvFiles_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			_lvicFiles.SortByColumn(e.Column);

			lvFiles.Sort();
			lvFiles.Columns[e.Column].Text += _lvicFiles.SortingFlag;
		}
		#endregion Other event handlers...
	}
}
