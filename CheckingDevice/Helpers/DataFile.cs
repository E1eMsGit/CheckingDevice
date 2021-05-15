using Microsoft.Win32;
using System;
using System.IO;

namespace TK158.Helpers
{
    public class DataFile
    {
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public bool? DialogResult { get; private set; }
        public byte[] FileData { get; set; }
        public byte[] Tk158Settings { get; private set; }

        public void Open()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "txt files (*.txt)|*.txt"
            };

            DialogResult = openFileDialog.ShowDialog();
            
            if (DialogResult == true)
            {
                FileName = openFileDialog.SafeFileName;
                FilePath = openFileDialog.FileName;
                ReadFile();
            }
        }

        private async void ReadFile()
        {
            using (StreamReader fileStream = new StreamReader(File.OpenRead(FilePath)))
            {
                await fileStream.ReadToEndAsync()
                    .ContinueWith(t => TestsDataTA1004M1.GetArrayFromHexString(t.Result.Replace("\n", "").Replace("\r", "")))
                    .ContinueWith(t =>
                    {
                        if (!t.IsFaulted)
                        {
                            // Проверям не пустой ли файл и имеет ли минимум 5 байт для настроек.
                            if (t.Result != null && t.Result.Length > 5)
                            {
                                Tk158Settings = new byte[5];
                                FileData = new byte[t.Result.Length - 5];
                                Array.Copy(t.Result, 0, Tk158Settings, 0, Tk158Settings.Length);
                                Array.Copy(t.Result, 5, FileData, 0, FileData.Length);
                            }
                        }     
                    });
            }
        }
    }    
}
