/// FileFinder, an util class like CFileFind in MFC.
/// refer to http://www.cnblogs.com/shensr/ for latest version.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace PhotoUtil
{
	public enum FileAttributes 
	{
		Readonly             = 0x00000001,
		Hidden               = 0x00000002,
		System               = 0x00000004,
		Directory            = 0x00000010,
		Archive              = 0x00000020,
		Encrypted            = 0x00000040,
		Normal               = 0x00000080,
		Temporary            = 0x00000100,
		//Sparse_file          = 0x00000200,
		//Reparse_point        = 0x00000400,
		Compressed           = 0x00000800,
		All					 = 0x7fffffff
	}

	public class FileFindEventArgs : EventArgs
	{
		readonly string	_directory;
		FileFinder.FileFindData _ffd;
		bool	_cancel;

		internal FileFindEventArgs(string directory, FileFinder.FileFindData ffd)
		{
			if(directory.Length > 0 && directory[directory.Length - 1] != '\\')
				directory += '\\';

			_directory	= directory;
			_ffd		= ffd;
			_cancel		= false;
		}

		public FileAttributes Attributes {
			get { return (FileAttributes)_ffd.fileAttributes; }
		}

		public string ParentDirectory {
			get { return _directory; }
		}

		public string FileName {
			get { return _ffd.fileName; }
		}

		public string PathName {
			get { return _directory + _ffd.fileName; }
		}

		public bool Cancle {
			get { return  _cancel; }
			set { _cancel = value; }
		}
	}

	public delegate void FileFindEventHandler(object sender, FileFindEventArgs e);


	/// <summary>
	/// Summary description for FileFinder.
	/// </summary>
	public sealed class FileFinder
	{
		FileFindData	_findData;
		IntPtr			_handle = new IntPtr(-1);

		#region Interop

		const int FILE_ATTRIBUTE_READONLY             = 0x00000001;
		const int FILE_ATTRIBUTE_HIDDEN               = 0x00000002;
		const int FILE_ATTRIBUTE_SYSTEM               = 0x00000004;
		const int FILE_ATTRIBUTE_DIRECTORY            = 0x00000010;
		const int FILE_ATTRIBUTE_ARCHIVE              = 0x00000020;
		const int FILE_ATTRIBUTE_ENCRYPTED            = 0x00000040;
		const int FILE_ATTRIBUTE_NORMAL               = 0x00000080;
		const int FILE_ATTRIBUTE_TEMPORARY            = 0x00000100;
		const int FILE_ATTRIBUTE_SPARSE_FILE          = 0x00000200;
		const int FILE_ATTRIBUTE_REPARSE_POINT        = 0x00000400;
		const int FILE_ATTRIBUTE_COMPRESSED           = 0x00000800;
		const int FILE_ATTRIBUTE_OFFLINE              = 0x00001000;
		const int FILE_ATTRIBUTE_NOT_CONTENT_INDEXED  = 0x00002000;

		[ StructLayout( LayoutKind.Sequential, CharSet=CharSet.Auto )]
		internal class FileFindData 
		{
			public int  fileAttributes = 0;
			// creationTime was an embedded FILETIME structure.
			public int  creationTime_lowDateTime = 0 ;
			public int  creationTime_highDateTime = 0;
			// lastAccessTime was an embedded FILETIME structure.
			public int  lastAccessTime_lowDateTime = 0;
			public int  lastAccessTime_highDateTime = 0;
			// lastWriteTime was an embedded FILETIME structure.
			public int  lastWriteTime_lowDateTime = 0;
			public int  lastWriteTime_highDateTime = 0;
			public int  nFileSizeHigh = 0;
			public int  nFileSizeLow = 0;
			public int  dwReserved0 = 0;
			public int  dwReserved1 = 0;
			[ MarshalAs( UnmanagedType.ByValTStr, SizeConst=256 )]
			public String  fileName = null;
			[ MarshalAs( UnmanagedType.ByValTStr, SizeConst=14 )]
			public String  alternateFileName = null;
		}

		[DllImport( "Kernel32.dll", CharSet=CharSet.Auto )]
		static extern IntPtr FindFirstFile( String fileName, [ In, Out ] FileFindData findFileData );

		[DllImport( "Kernel32.dll", CharSet=CharSet.Auto )]
		static extern bool FindNextFile( IntPtr findFile, [ In, Out ] FileFindData findFileData );

		#endregion

		public event FileFindEventHandler FileFinded;

		public string FileName 
		{
			get { return _findData.fileName; }
		}

		public string FileExt {
			get {
				int pos = _findData.fileName.LastIndexOf('.');
				return (pos < 0) ? string.Empty : _findData.fileName.Substring(++pos);
			}
		}

		public FileAttributes Attributes 
		{
			get { return (FileAttributes)_findData.fileAttributes; }
		}

		public FileFinder()
		{
			_findData = new FileFindData();
		}

		public bool FindFirst(string fileName)
		{
			_handle = FindFirstFile(fileName, _findData);
			return (_handle.ToInt32() != -1);
		}

		public bool FindNext()
		{
			if(_handle.ToInt32() == -1) return false;

			return FindNextFile(_handle, _findData);
		}

		public static bool MatchesCheck(string[] regExes, string filename)
		{
			foreach(string regEx in regExes) {
				Regex re = new System.Text.RegularExpressions.Regex(regEx, RegexOptions.IgnoreCase);

				if( re.IsMatch(filename) ) return true;
			}

			return false;
		}

		public static string[] Patterns2RegExes(string patterns)
		{
			string[] regExes = patterns.Split(';');
			for(int i = 0; i < regExes.Length; i++) 
			{
				regExes[i] = Pattern2RegEx(regExes[i].Trim());
			}
			return regExes;
		}

		static string Pattern2RegEx(string pattern)
		{
			char[] ret = new char[pattern.Length * 2];
			int pos = 0;
			for(int i = 0; i < pattern.Length; i++) {
				switch(pattern[i]) {
					case '^':
					case '.':
					case '[':
					case '$':
					case '(':
					case ')':
					case '|':
					case '+':
					case '{':
					case '\\':
						ret[pos++] = '\\';
						ret[pos++] = pattern[i];
						break;
					case '*':
					case '?':
						ret[pos++] = '.';
						ret[pos++] = pattern[i];
						break;
					default:
						ret[pos++] = pattern[i];
						break;
				}
			}

			return new string(ret, 0, pos);
		}

		public void FindFiles(string patterns, string directory, FileAttributes attributes, bool incSubDir)
		{
			FindFiles(Patterns2RegExes(patterns.ToLower()), directory, attributes, incSubDir);
		}

		public void FindFiles(string[] regExes, string directory, FileAttributes attributes, bool incSubDir)
		{
			string curDir = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(directory);

			try 
			{
				if(!FindFirst("*.*")) return;

				while(FindNext()) {
					if(IsDots()) continue;

					if( ((this.Attributes & attributes) == attributes)
						&& MatchesCheck(regExes, this.FileName))
					{
						FileFindEventArgs e = new FileFindEventArgs(directory, _findData);
						FileFinded(this, e);

						if(e.Cancle) break;
					}

					if(((this.Attributes & FileAttributes.Directory) != 0)
						&& incSubDir
						)
					{
						FindFiles(regExes, directory + _findData.fileName, attributes, incSubDir);
					}
				}
			}
			finally {
				Directory.SetCurrentDirectory(curDir);
			}
		}

		#region Is...
		public bool IsNormal()
		{
			return (_findData.fileAttributes == 0);
		}

		public bool IsArchived()
		{
			return ((_findData.fileAttributes & FILE_ATTRIBUTE_ARCHIVE) != 0);
		}

		public bool IsHidden()
		{
			return ((_findData.fileAttributes & FILE_ATTRIBUTE_HIDDEN) != 0);
		}

		public bool IsTemporary()
		{
			return ((_findData.fileAttributes & FILE_ATTRIBUTE_TEMPORARY) != 0);
		}

		public bool IsCompressed()
		{
			return ((_findData.fileAttributes & FILE_ATTRIBUTE_COMPRESSED) != 0);
		}

		public bool IsEncrypted()
		{
			return ((_findData.fileAttributes & FILE_ATTRIBUTE_ENCRYPTED) != 0);
		}

		public bool IsReadOnly()
		{
			return ((_findData.fileAttributes & FILE_ATTRIBUTE_READONLY) != 0);
		}

		public bool IsSystem()
		{
			return ((_findData.fileAttributes & FILE_ATTRIBUTE_SYSTEM) != 0);
		}

		public bool IsSubDirectory()
		{
			return ((_findData.fileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0) && !IsDots();
		}

		public bool IsDirectory()
		{
			return ((_findData.fileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0);
		}

		public bool IsDots()
		{
			return (_findData.fileName == "." || _findData.fileName == "..");
		}
		#endregion
	}
}
