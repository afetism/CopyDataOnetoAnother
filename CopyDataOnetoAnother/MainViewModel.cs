
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;


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
		CopyCommand=new(executeCopy);
	}

	private void executeSuspend(object obj)
	{
		
	}

	private void copyContent(string sourceFilePath,string destinationFilePath,IProgress<int> progress)
	{
		using(FileStream sourceStream=new(sourceFilePath, FileMode.Open, FileAccess.Read))
		 using(FileStream destinationStream=new(destinationFilePath, FileMode.Open, FileAccess.Write))
		{
			int byteRead;
			long totalBytes=sourceFilePath.Length;

			long bytesCopy = 0;
			while ((byteRead = sourceStream.ReadByte())!=-1)
			{

				bytesCopy++;
				destinationStream.WriteByte((byte)byteRead);
				int percentComplete=(int)((bytesCopy * 100)/totalBytes);
				progress.Report(percentComplete);
				Thread.Sleep(100);
			}
			

		}

	}

	private void executeCopy(object obj)
	{
		var progressIndigator=new Progress<int>(value=>ProgressValue=value);

		var thread = new Thread(() =>copyContent(fromText, toText, progressIndigator));
		thread.Start();
		Thread.Sleep(5000);
		
	

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
