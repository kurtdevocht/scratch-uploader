using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows;

namespace Scratch.Uploader
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Model m_model = new Model();
		public MainWindow()
		{
			InitializeComponent();
			this.DataContext = m_model;
		}

		private void m_btnBrowse_Click( object sender, RoutedEventArgs e )
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();
			if ( dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK )
			{
				m_model.SourceDir = dialog.SelectedPath;
			}
		}

		private void m_btnConvertFiles_Click( object sender, RoutedEventArgs e )
		{
			m_model.Log( "*** Conversion started ***" );
			var inDir = m_model.SourceDir + "\\";
			var outDir = inDir + "__out\\";

			try
			{
				if ( Directory.Exists( outDir ) )
				{
					MessageBox.Show( "Outdir already exists...", "Kaput", MessageBoxButton.OK, MessageBoxImage.Error );
					return;
				}

				Directory.CreateDirectory( outDir );

				foreach ( var file in Directory.GetFiles( inDir, "*.sb2" ) )
				{
					var fileName = this.BuildFileName( System.IO.Path.GetFileName( file ) );
					File.Copy( file, outDir + fileName );

					m_model.Log( "Copied '" + file + "' > '" + outDir + fileName + "'" );
				}

				foreach ( var dir in Directory.GetDirectories( inDir ) )
				{
					if ( outDir.StartsWith( dir ) )
					{
						continue;
					}

					foreach ( var file in Directory.GetFiles( dir, "*.sb2" ) )
					{
						var fileName = this.BuildFileName(
							  new DirectoryInfo( dir ).Name
							+ " - "
							+ System.IO.Path.GetFileName( file ) );

						File.Copy( file, outDir + fileName );
						m_model.Log( "Copied '" + file + "' > '" + outDir + fileName + "'" );
					}
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( "Exception: " + ex.ToString(), "Kaput", MessageBoxButton.OK, MessageBoxImage.Error );
				return;
			}
		}

		private void m_btnUpload_Click( object sender, RoutedEventArgs e )
		{
			if ( string.IsNullOrEmpty( m_model.ScratchUserName ) || string.IsNullOrEmpty( m_model.ScratchPassword ) )
			{
				MessageBox.Show(
					"Fill in user name and password!" );
				return;
			}

			m_model.Log( "*** Start to upload ***" );

			try
			{
				var baseAddress = new Uri( "http://scratch.mit.edu/services/upload" );
				var fileNames = Directory.EnumerateFiles( m_model.SourceDir + "\\__out\\" );
				foreach ( var fileName in fileNames )
				{
					if ( !fileName.EndsWith( ".sb2" ) )
					{
						continue;
					}

					var cookieContainer = new CookieContainer();
					using ( var handler = new HttpClientHandler() { CookieContainer = cookieContainer } )
					using ( var client = new HttpClient( handler ) { BaseAddress = baseAddress } )
					using ( var formData = new MultipartFormDataContent() )

					using ( var streamBinaryFile = File.OpenRead( fileName ) )
					{
						var process = Process.Start( fileName );
						Thread.Sleep( 8000 );
						var hWnd = process.MainWindowHandle;
						var rect = new Win32.RECT();
						Win32.GetWindowRect( hWnd, ref rect );

						var screenshot = new Bitmap( 435, 335 );
						var g = Graphics.FromImage( screenshot );

						// Project area coordinates: topleft = (14,103) botright = (492,462)
						g.CopyFromScreen( rect.Left + 35, rect.Top + 125, 0, 0, screenshot.Size, CopyPixelOperation.SourceCopy );

						var screenshotFileName = fileName + ".png";
						screenshot.Save( screenshotFileName, System.Drawing.Imaging.ImageFormat.Png );

						process.CloseMainWindow();
						screenshot.Dispose();

						using ( var streamPreviewImage = File.OpenRead( screenshotFileName ) )
						{

							// Add the HttpContent objects to the form data

							var contentUserName = new StringContent( m_model.ScratchUserName);
							contentUserName.Headers.Remove( "Content-Type" );
							formData.Add( contentUserName, "\"username\"" );

							var contentScratchKey = new StringContent( "ch4ng3me" );
							contentScratchKey.Headers.Remove( "Content-Type" );
							formData.Add( contentScratchKey, "\"scratchkey\"" );

							var contentPassword = new StringContent( m_model.ScratchPassword );
							contentPassword.Headers.Remove( "Content-Type" );
							formData.Add( contentPassword, "\"password\"" );

							var projectName = System.IO.Path.GetFileNameWithoutExtension( fileName );
							var contentProjectName = new StringContent( projectName );
							contentProjectName.Headers.Remove( "Content-Type" );
							formData.Add( contentProjectName, "\"project_name\"" );


							var contentBinaryFile = new StreamContent( streamBinaryFile );
							contentBinaryFile.Headers.Add( "Content-Type", "application/octet-stream" );

							formData.Add( contentBinaryFile, "\"binary_file\"", "\"binary_file\"" );

							var contentTags = new StringContent( "" );
							contentProjectName.Headers.Remove( "Content-Type" );
							formData.Add( contentProjectName, "\"tags\"" );

							var contentPreviewImage = new StreamContent( streamPreviewImage );
							contentPreviewImage.Headers.Add( "Content-Type", "image/png" );

							formData.Add( contentPreviewImage, "\"preview_image\"", "\"preview_image\"" );
							
							var response = client.PostAsync( "", formData ).Result;

							if ( !response.IsSuccessStatusCode )
							{
								m_model.Log( "*** Failed to upload '" + fileName + "'" );
							}
							else
							{
								m_model.Log( "Uploaded '" + fileName + "'" );
							}

							
						}
					}
				}

			}
			catch ( Exception ex )
			{
				MessageBox.Show( "Exception: " + ex.ToString(), "Kaput", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}

		private string BuildFileName( string file )
		{
			return m_model.FilePrefix + file;
		}
	}
}
