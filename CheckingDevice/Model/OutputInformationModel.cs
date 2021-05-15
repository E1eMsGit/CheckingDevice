using System;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using TK158.Helpers;
using System.Threading.Tasks;
using System.Linq;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TK158.Model
{
    class OutputInformationModel :  INotifyPropertyChanged
    {
        private DataFile _dataFile;
        private FtdiDevice[] _devices;
        private SettingsModel _settings;
        private byte[] _txA;
        private List<byte[]> _rxA = new List<byte[]>();
        private List<byte[]> _rxB = new List<byte[]>();
        private byte _modeNumber;
        private string _codeBits;

        public OutputInformationModel()
        {
            Messenger.Default.Register<DataFile>(this, (o) => _dataFile = o);
            Messenger.Default.Register<FtdiDevice[]>(this, (o) => _devices = o);
            Messenger.Default.Register<SettingsModel>(this, (o) => _settings = o);

            OutputInformation = new List<OutputInformationItem>()
            {
                new OutputInformationItem("0", string.Empty),
                new OutputInformationItem("1", string.Empty),
                new OutputInformationItem("2", string.Empty),
                new OutputInformationItem("3", string.Empty),
                new OutputInformationItem("4", string.Empty),
                new OutputInformationItem("5", string.Empty),
                new OutputInformationItem("6", string.Empty),
                new OutputInformationItem("7", string.Empty),
                new OutputInformationItem("8", string.Empty),
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public List<OutputInformationItem> OutputInformation { get; }
        public string CodeBits
        {
            get => _codeBits;
            set
            {
                this.MutateVerbose(ref _codeBits, value, args => PropertyChanged?.Invoke(this, args));
            }
        }

        /// <summary>
        /// Начать проверку асинхронно.
        /// </summary>
        /// <param name="token">Токен для отмены проверки.</param>
        public async Task StartAsync(CancellationToken token)
        {
            if (_devices != null)
            {
                for (int i = 0; i < _txA.Length; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    await _devices[0].WriteByteAsync(_txA[i]).ContinueWith(t => _devices[1].WriteByteAsync(Constants.TX_B));
                    
                    if (_settings.WordLength == (byte)WordLength._32Bit)
                    {
                        await _devices[0].WriteByteAsync(0x0).ContinueWith(t => _devices[1].WriteByteAsync(0x0));
                    }

                    await _devices[0].ReadBytesAsync().ContinueWith(t => { if (_modeNumber == 1 || _modeNumber == 2) _rxA.Add(t.Result); });
                    await _devices[1].ReadBytesAsync().ContinueWith(t => { if (_modeNumber == 1 || _modeNumber == 2) _rxB.Add(t.Result); });

                    // "Входы информации".
                    if (_modeNumber == 1)
                    {
                        if (_rxA.Count == OutputInformation.Count)
                        {
                            for (int j = 0; j < OutputInformation.Count; j++)
                            {
                                // Тут какая то логика по получению значения из А (из _rxA[j][0]).
                                // Тут какая то логика по получению значения из В (из _rxB[j][0]).
                                // Тут надо сложить полученные значения из А и В (А | В) и записать в OutputInformation[j].Data.
                                OutputInformation[j].Data = Convert.ToString(_rxA[j][0] | _rxB[j][0], 2).PadLeft(8, '0');
                            }
                            _rxA.Clear();
                            _rxB.Clear();
                        }                                            
                    }
                    // "Разряды кода".
                    else if (_modeNumber == 2)
                    {
                        // Тут какая то логика по получению значения из А.
                        // Тут какая то логика по получению значения из В.
                        // Тут надо сложить полученные значения из А и В (А | В) и записать в CodeBits.
                        CodeBits = Convert.ToString(_rxA[i][0] | _rxB[i][0], 2).PadLeft(8, '0');
                    }
                }
            }
#if DEBUG
            // Заглушка без прибора.
            else
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < 256; i++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        if (_modeNumber == 1)
                        {
                            OutputInformation[0].Data = Convert.ToString(i, 2).PadLeft(8, '0');
                            OutputInformation[1].Data = Convert.ToString(i, 2).PadLeft(8, '0');
                            OutputInformation[2].Data = Convert.ToString(i, 2).PadLeft(8, '0');
                            OutputInformation[3].Data = Convert.ToString(i, 2).PadLeft(8, '0');
                            OutputInformation[4].Data = Convert.ToString(i, 2).PadLeft(8, '0');
                            OutputInformation[5].Data = Convert.ToString(i, 2).PadLeft(8, '0');
                            OutputInformation[6].Data = Convert.ToString(i, 2).PadLeft(8, '0');
                            OutputInformation[7].Data = Convert.ToString(i, 2).PadLeft(8, '0');
                            OutputInformation[8].Data = Convert.ToString(i, 2).PadLeft(8, '0');
                        } 
                        else if (_modeNumber == 2)
                        {
                            CodeBits = Convert.ToString(i, 2).PadLeft(8, '0');
                        }
                        Thread.Sleep(20);
                    }
                });
            }
#endif
        }
        /// <summary>
        /// Подготовка данных для режима "Входы информации".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode1(byte informationLength)
        {
            Init(1);

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData1(_settings.Address1, _settings.Address2, _settings.Address3, informationLength);
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Разряды кода".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode2(byte informationLength)
        {
            Init(2);

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData3(_settings.Address1, informationLength);
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Задержка выдачи на АУ1".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode3(byte informationLength)
        {
            Init(3);

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData3(_settings.Address1,  informationLength);
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Задержка выдачи на АУ2".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode4(byte informationLength)
        {
            Init(4);

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData4(_settings.Address2, informationLength);
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Задержка выдачи на АУ3".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode5(byte informationLength)
        {
            Init(5);

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData5(_settings.Address3, informationLength);
            }
        }
        /// <summary>
        /// Сброс устройств и очистка списков.
        /// </summary>
        private void Init(byte modeNumber)
        {
            _modeNumber = modeNumber;
            CodeBits = string.Empty;

            _rxA.Clear();
            _rxB.Clear();
            
            if (_devices != null)
            {
                for (int i = 0; i < _devices.Length; i++)
                {
                    _devices[0].Purge();
                }
            }

            foreach (var item in OutputInformation)
            {
                item.Data = string.Empty;
            }

        }        
    }

    class OutputInformationItem : INotifyPropertyChanged
    {
        private string _data;

        public OutputInformationItem(string address, string data)
        {
            Address = address;
            Data = data;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string Address { get; private set; }
        public string Data 
        { 
            get => _data;
            set 
            {
                this.MutateVerbose(ref _data, value, args => PropertyChanged?.Invoke(this, args));
            } 
        }        
    }
}
