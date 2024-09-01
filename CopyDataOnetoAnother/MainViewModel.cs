
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;


namespace CopyDataOnetoAnother;

public class MainViewModel:INotifyPropertyChanged
{
	private int progressValue;
	public int ProgressValue
	{
		get => progressValue;
		set
		{
			progressValue=value;
			OnPropertyChanged();
		}
	}

	private string fromText;
	public string FromText {
		get => fromText; 
		set{
			fromText=value;
			OnPropertyChanged();
		}
	}



	private string toText;
	public string ToText 
	{ 
		get => toText;
		set
		{
			toText=value;
			OnPropertyChanged();
		}
	}

	public RelayCommand FromFile { get; set; }
    public RelayCommand ToFile { get; set; }
    public RelayCommand SuspendCommand { get; set; }
    public RelayCommand ResumeCommand { get; set; }
    public RelayCommand AbortCommand { get; set; }
    public RelayCommand CopyCommand { get; set; }

    private string SelectFile()
    {
        var dlg = new OpenFileDialog()
        {
			Filter = "Text Files (*.txt) | *.txt | All Files (*.*) | *.*",
			RestoreDirectory = true


		};

		if (dlg.ShowDialog() != true)
			return "";
        else return dlg.FileName;
	}

    public MainViewModel()
    {
        FromFile=new(executeFromFile);
		ToFile=new(executeToFile);
		SuspendCommand=new(executeSuspend);
		ResumeCommand=new(executeResume);
		CopyCommand =new(executeCopy);
		AbortCommand=new(executeAbort);
		_suspendEvent =new ManualResetEvent(true);
	}

	private void executeAbort(object obj)
	{
		cancellationTokenSource.Cancel();
	}

	private void executeResume(object obj)
	{
		_suspendEvent.Set();
	}

	private ManualResetEvent _suspendEvent;
	private Thread thread;
	private CancellationTokenSource cancellationTokenSource;

	private void executeSuspend(object obj)
	{
		_suspendEvent.Reset();	
	}

	private void copyContent(string sourceFilePath,string destinationFilePath,IProgress<int> progress,CancellationToken cancellationToken)
	{
		try
		{
			using (FileStream sourceStream = new(sourceFilePath, FileMode.Open, FileAccess.Read))
			using (FileStream destinationStream = new(destinationFilePath, FileMode.Open, FileAccess.Write))
			{
				int byteRead;
				long totalBytes = sourceFilePath.Length;

				long bytesCopy = 0;
				while ((byteRead = sourceStream.ReadByte())!=-1)
				{
					cancellationToken.ThrowIfCancellationRequested();

					_suspendEvent.WaitOne();

					bytesCopy++;
					destinationStream.WriteByte((byte)byteRead);
					int percentComplete = (int)((bytesCopy * 100)/totalBytes);
					progress.Report(percentComplete);
					Thread.Sleep(100);
				}


			}
		}
		catch(Exception ex) 
		{
			MessageBox.Show($"ERROR!:{ex.Message}");
		}
	}

	private void executeCopy(object obj)
	{
		cancellationTokenSource=new();
		var progressIndigator=new Progress<int>(value=>ProgressValue=value);

		thread = new Thread(() =>copyContent(fromText, toText, progressIndigator, cancellationTokenSource.Token));

		thread.Start();
		
	}

	private void executeToFile(object obj)
	{
		ToText=SelectFile();
	}

	private void executeFromFile(object obj)
	{
        FromText= SelectFile();
	}



	protected void OnPropertyChanged([CallerMemberName] string name = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	public event PropertyChangedEventHandler? PropertyChanged;
}
