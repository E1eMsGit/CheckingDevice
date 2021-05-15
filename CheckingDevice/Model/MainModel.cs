using System;
using FTD2XX_NET;
using MaterialDesignThemes.Wpf;
using TK158.Model.Dialogs;
using TK158.View;

namespace TK158.Model
{
    public class MainModel
    {
        private uint _numDevices;
        public uint NumDevices => _numDevices;

        public MainModel()
        {
            FTDI device = new FTDI();
            device.GetNumberOfDevices(ref _numDevices);
        }

        public FtdiModel GetFtdiDevice(uint deviceIndex) => new FtdiModel(deviceIndex);

        public byte[] PrepareDataForSend(string dataFromView)
        {
            char[] delimiter = {' '};
            string[] strData = dataFromView.Split(delimiter);
            byte[] dataForSend = new byte[strData.Length];

            for (int bytesCounter = 0; bytesCounter < strData.Length; bytesCounter++)
            {
                if (strData[bytesCounter].Length > 0)
                {
                    dataForSend[bytesCounter] = Convert.ToByte(strData[bytesCounter], 16);
                }
                else // Для фиксированного размера массива DataForSend (сейчас размер зависит от входящей строки).
                {
                    dataForSend[bytesCounter] = 0;
                }
            }

            return dataForSend;
        }
    }
}
