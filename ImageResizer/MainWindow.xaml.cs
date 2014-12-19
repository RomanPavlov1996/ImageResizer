using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.ComponentModel;

namespace ImageResizer
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Elysium.Controls.Window
	{
		string FolderPath;
		DirectoryInfo ImagesDirectory;
		string OutputPath;
		bool XPercents;
		bool YPercents;
		int Width;
		int Height;

		public MainWindow ( )
		{
			InitializeComponent( );
			lblFolderPath.Content = null;
			prgRing.Visibility = System.Windows.Visibility.Hidden;

			XPercents = cmbWidth.SelectedIndex == 1;
			YPercents = cmbHeight.SelectedIndex == 1;
			Width = int.Parse(txtWidth.Text);
			Height = int.Parse(txtHeight.Text);
		}

		private void btnSelect_Click ( object sender, RoutedEventArgs e )
		{
			Microsoft.Win32.OpenFileDialog OpenFD = new Microsoft.Win32.OpenFileDialog( );
			OpenFD.ShowDialog( );
			FolderPath = OpenFD.FileName.Substring( 0, OpenFD.FileName.LastIndexOf( @"\" ) + 1 );
			lblFolderPath.Content = FolderPath;
			ImagesDirectory = new DirectoryInfo( FolderPath );
		}

		private void btnResize_Click ( object sender, RoutedEventArgs e )
		{
			prgRing.State = Elysium.Controls.ProgressState.Busy;
			prgRing.Visibility = System.Windows.Visibility.Visible;

			BackgroundWorker Resizer = new BackgroundWorker( );
			Resizer.DoWork += Resizer_DoWork;
			Resizer.RunWorkerCompleted += Resizer_RunWorkerCompleted;
			Resizer.RunWorkerAsync( );
		}

		void Resizer_RunWorkerCompleted ( object sender, RunWorkerCompletedEventArgs e )
		{
			prgRing.Visibility = System.Windows.Visibility.Hidden;
			prgRing.State = Elysium.Controls.ProgressState.Normal;
		}

		void Resizer_DoWork ( object sender, DoWorkEventArgs e )
		{
			OutputPath = Environment.GetFolderPath( Environment.SpecialFolder.DesktopDirectory ) + "\\ImageResizerOutput";
			if ( Directory.Exists( OutputPath ) )
			{
				try
				{
					Directory.Delete( OutputPath, true );
				}
				catch
				{
					Directory.Delete( OutputPath, true );
				}
			}
			Directory.CreateDirectory( OutputPath );
			ProcessDirectory( ImagesDirectory, OutputPath );
		}

		private void ProcessDirectory ( DirectoryInfo CurrentDirectory, string PrevPath )
		{
			string DirectoryPath = PrevPath + "\\" + CurrentDirectory.Name;
			Directory.CreateDirectory( DirectoryPath );

			foreach ( FileInfo CurrentFileInfo in CurrentDirectory.GetFiles( ) )
			{
				ProcessImage( CurrentFileInfo, DirectoryPath );
			}

			foreach ( DirectoryInfo CurrentDirectoryInfo in CurrentDirectory.GetDirectories( ) )
			{
				ProcessDirectory( CurrentDirectoryInfo, DirectoryPath );
			}
		}

		private void ProcessImage ( FileInfo CurrentFileInfo, string PrevPath )
		{
			if ( !CurrentFileInfo.Name.Contains( ".db" ) )
			{
				Image CurrentImage;
				using ( FileStream fs = new FileStream( CurrentFileInfo.FullName, FileMode.Open ) )
				{
					CurrentImage = Image.FromStream( fs );
				}
				CurrentImage = ScaleImage( CurrentImage, GetWidth( CurrentImage.Width ), GetHeight( CurrentImage.Height ) );

				using ( FileStream fs = new FileStream( PrevPath + "\\" + CurrentFileInfo.Name, FileMode.Create ) )
				{
					CurrentImage.Save( fs, ImageFormat.Jpeg );
				}
			}
		}

		int GetWidth ( int BaseWidth )
		{
			if ( !XPercents )
			{
				return Width;
			}
			else
			{
				return BaseWidth * Width / 100;
			}
		}

		int GetHeight ( int BaseHeight )
		{
			if ( !YPercents )
			{
				return Height;
			}
			else
			{
				return BaseHeight * Height / 100;
			}
		}

		static Image ScaleImage ( Image source, int width, int height )
		{
			Image dest = new Bitmap( width, height );
			using ( Graphics gr = Graphics.FromImage( dest ) )
			{
				gr.FillRectangle( Brushes.White, 0, 0, width, height );  // Очищаем экран
				gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

				float srcwidth = source.Width;
				float srcheight = source.Height;
				float dstwidth = width;
				float dstheight = height;

				if ( srcwidth <= dstwidth && srcheight <= dstheight )  // Исходное изображение меньше целевого
				{
					int left = ( width - source.Width ) / 2;
					int top = ( height - source.Height ) / 2;
					gr.DrawImage( source, left, top, source.Width, source.Height );
				}
				else if ( srcwidth / srcheight > dstwidth / dstheight )  // Пропорции исходного изображения более широкие
				{
					float cy = srcheight / srcwidth * dstwidth;
					float top = ( ( float ) dstheight - cy ) / 2.0f;
					if ( top < 1.0f ) top = 0;
					gr.DrawImage( source, 0, top, dstwidth, cy );
				}
				else  // Пропорции исходного изображения более узкие
				{
					float cx = srcwidth / srcheight * dstheight;
					float left = ( ( float ) dstwidth - cx ) / 2.0f;
					if ( left < 1.0f ) left = 0;
					gr.DrawImage( source, left, 0, cx, dstheight );
				}

				return dest;
			}
		}
	}
}
