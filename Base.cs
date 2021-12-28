using System;
using System.Windows.Forms;
using System.Collections;

namespace PhotoUtil
{
	public enum LocationType
	{
		TopLeft = 1, TopRight, BottomLeft, BottomRight
	}

	public enum PictureTime
	{
		TakenTime = 1, CreatedTime = 2, ModifiedTime = 3
	}

	public enum PictureType
	{
		Jpeg = 1, Bmp, Gif, Png, Tiff
	}

	#region Class ListViewItemComparer...
	/// ListViewItemComparer, a comparer for ListView sorting.
	/// refer to http://www.cnblogs.com/shensr/ for latest version.
	public class ListViewItemComparer : IComparer 
	{
		private int _col	=	-1;
		private SortOrder _order = SortOrder.None;

		public const int SortingFlagLength = 2;
		public const string AscendingFlag = " ¡ø";
		public const string DescendingFlag = " ¨‹";

		public event EventHandler SortIndexRemoved;

		public int CurrentColumn 
		{
			get { return  _col; }
		}

		public SortOrder Sorting 
		{
			get { return  _order; }
		}

		public string SortingFlag 
		{
			get 
			{
				string str = string.Empty;
				switch(_order)
				{
					case SortOrder.Ascending:
						str = AscendingFlag;
						break;
					case SortOrder.Descending:
						str = DescendingFlag;
						break;
				}
				return str;
			}
		}

		public ListViewItemComparer() 
		{
		}

		public int SortByColumn(int col)
		{
			int old = _col;

			if(null != SortIndexRemoved) SortIndexRemoved(this, EventArgs.Empty);

			if((col == _col) && (_order == SortOrder.Ascending)) _order = SortOrder.Descending;
			else _order = SortOrder.Ascending;

			_col = col;
			return old;
		}

		public int Compare(object x, object y) 
		{
			if(_col < 0 || _order == SortOrder.None) return 0;

			int result = String.Compare(((ListViewItem)x).SubItems[_col].Text, ((ListViewItem)y).SubItems[_col].Text);
			return (_order == SortOrder.Ascending) ? result : -result;
		}
	}
	#endregion
}
