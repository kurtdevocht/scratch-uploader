using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Scratch.Uploader
{
	internal class Model : INotifyPropertyChanged
	{

		#region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
		
		private string m_sourceDir;
		private string m_filePrefix;
		private string m_scratchUserName;
		private string m_scratchPassword;
		private ObservableCollection<string> m_logLines = new ObservableCollection<string>();

		public Model()
		{
			var now = DateTime.Now;
			this.FilePrefix = string.Format( "{0:0000}-{1:00}-{2:00} - ", now.Year, now.Month, now.Day );
		}

		public string SourceDir
		{
			get
			{
				return m_sourceDir;
			}
			set
			{
				if ( m_sourceDir == value )
				{
					return;
				}

				m_sourceDir = value;
				this.OnPropertyChanged();
			}
		}

		public string FilePrefix
		{
			get
			{
				return m_filePrefix;
			}
			set
			{
				if ( m_filePrefix == value )
				{
					return;
				}

				m_filePrefix = value;
				this.OnPropertyChanged();
			}
		}

		public string ScratchUserName
		{
			get
			{
				return m_scratchUserName;
			}
			set
			{
				if ( m_scratchUserName == value )
				{
					return;
				}

				m_scratchUserName = value;
				this.OnPropertyChanged();
			}
		}

		public string ScratchPassword
		{
			get
			{
				return m_scratchPassword;
			}
			set
			{
				if ( m_scratchPassword == value )
				{
					return;
				}

				m_scratchPassword = value;
				this.OnPropertyChanged();
			}
		}

		public ObservableCollection<string> LogLines
		{
			get
			{
				return m_logLines;
			}
		}

		public void Log( string message )
		{
			m_logLines.Insert( 0, message );
		}

		protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
		{
			if ( this.PropertyChanged == null )
			{
				return;
			}

			var e = new PropertyChangedEventArgs( propertyName );
			this.PropertyChanged( this, e );
		}      
	}
}