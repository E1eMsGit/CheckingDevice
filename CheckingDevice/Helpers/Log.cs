using Microsoft.Win32;
using System.IO;
using System.Text;
using System;

namespace TK158.Helpers
{
    class Log
    {      
        private static Log _instance;

        private Log()
        {
            Content = new StringBuilder();
        }

        public StringBuilder Content { get; set; }

        public static Log GetInstance() 
        {
            if (_instance == null)
            {
                _instance = new Log();
            }

            return _instance; 
        }

        public void SaveLog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "txt files (*.txt)|*.txt",
                FileName = $"Отчет {DateTime.Now.ToShortDateString()}"
            };

            
            if (saveFileDialog.ShowDialog() == true)
            {
                using (var stream = new StreamWriter(new FileStream(saveFileDialog.FileName, FileMode.Create), Encoding.UTF8))
                {
                    stream.Write(Content);
                }
            }
        }
    }
}
