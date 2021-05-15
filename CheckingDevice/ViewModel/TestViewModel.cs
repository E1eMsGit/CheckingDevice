using System;
using System.Collections.Generic;
using TK158.Model;
using TK158.DialogWindows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using TK158.Helpers;
using System.Text;
using System.Threading;

namespace TK158.ViewModel
{
    class TestViewModel : ViewModelBase
    {
        private DataFile _dataFile;
        private FtdiDevice[] _devices;
        private SettingsModel _settings;
        private string _inputData = string.Empty;
        private StringBuilder _outputDataA = new StringBuilder();
        private StringBuilder _outputDataB = new StringBuilder();
        private bool _canStart = true;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _token;

        public TestViewModel()
        {
            Messenger.Default.Register<DataFile>(this, (o) =>
            {
                _dataFile = o;

                if (_dataFile != null && _dataFile.FileData != null)
                {
                    InputData = string.Join(" ", _dataFile.FileData);                  
                }
                else
                {
                    InputData = string.Empty;
                }
            });
            Messenger.Default.Register<FtdiDevice[]>(this, (o) => _devices = o);
            Messenger.Default.Register<SettingsModel>(this, (o) => _settings = o);
        }

        public string InputData
        {
            get => _inputData;
            set => Set(ref _inputData, value);
        }
        public string OutputDataA
        {
            get => _outputDataA.ToString();
            set 
            {
                _outputDataA.Append(value);
                RaisePropertyChanged();
            }
        }
        public string OutputDataB
        {
            get => _outputDataB.ToString();
            set
            {
                _outputDataB.Append(value);
                RaisePropertyChanged();
            }
        }
        public IEnumerable<FtdiDevice> DeviceList => _devices;

        public RelayCommand WriteReadDeviceCommand => new RelayCommand(
            async () =>
            {
                if (_canStart)
                {
                    _canStart = false;
                    _outputDataA.Clear();
                    _outputDataB.Clear();

                    byte[] massA = null;
                    byte[] massB = null;

                    Messenger.Default.Send(new NavBarEnabled { IsEnabled = false });

                    _cancelTokenSource = new CancellationTokenSource();
                    _token = _cancelTokenSource.Token;

                    for (int i = 0; i < _devices.Length; i++)
                    {
                        if (_devices[i] == null) break;
                        _devices[i].Purge();
                    }

                    try
                    {
                        var _dataForSend = TestsDataTA1004M1.GetArrayFromHexString(InputData);

                        if (_dataForSend.Length % 2 == 0)
                        {
                            massA = PrepareData(_dataForSend, 0);
                            massB = PrepareData(_dataForSend, 1);
                        }
                        else
                        {
                            Dialog.ShowNotificationDialog("Ошибка", "Введите четное количество символов");
                        }


                        if (massA != null && massB != null)
                        {
                            while (!_canStart)
                            {
                                for (int i = 0; i < massA.Length; i++)
                                {
                                    if (_token.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    await _devices[0].WriteByteAsync(massA[i]).ContinueWith(t => _devices[1].WriteByteAsync(massB[i]));
                                    
                                    if (_settings.WordLength == (byte)WordLength._32Bit)
                                    {
                                        await _devices[0].WriteByteAsync(0x0).ContinueWith(t => _devices[1].WriteByteAsync(0x0));
                                    }

                                    await _devices[0].ReadBytesAsync().ContinueWith(t => OutputDataA = $"{BitConverter.ToString(t.Result).Replace("-", " ")} ");
                                    await _devices[1].ReadBytesAsync().ContinueWith(t => OutputDataB = $"{BitConverter.ToString(t.Result).Replace("-", " ")} ");
                                }

                                if (!_settings.IsInfiniteSending)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (FormatException)
                    {
                        Dialog.ShowNotificationDialog("Ошибка", "Некорректный ввод");
                    }
                }

                _canStart = true;
                Messenger.Default.Send(new NavBarEnabled { IsEnabled = true });

                _cancelTokenSource.Cancel();

            }, () => InputData.Length > 0);

        /// <summary>
        /// Получение массива данных для отправки на устройство.
        /// </summary>
        /// <param name="data">Преобразованные в массив байт данные из View (полученные из метода PrepareDataForSend).</param>
        /// <param name="index">С какого места начинать считывать данные из data.</param>
        /// <returns>Данные для отправки на устройство.</returns>
        private byte[] PrepareData(byte[] data, int index)
        {
            int arraySize = data.Length / 2;
            byte[] dataArray = new byte[arraySize];

            for (int i = 0, j = index; i < arraySize; i++, j += 2)
            {
                dataArray[i] = data[j];
            }

            return dataArray;
        }
    }
}