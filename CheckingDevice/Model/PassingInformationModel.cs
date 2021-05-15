using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using TK158.Helpers;

namespace TK158.Model
{
    class PassingInformationModel
    {
        private DataFile _dataFile;
        private FtdiDevice[] _devices;
        private SettingsModel _settings;
        private byte[] _txA;
        private List<byte[]> _rxA = new List<byte[]>();
        private List<byte[]> _rxB = new List<byte[]>();

        public PassingInformationModel()
        {
            Messenger.Default.Register<DataFile>(this, (o) => _dataFile = o);
            Messenger.Default.Register<FtdiDevice[]>(this, (o) => _devices = o);
            Messenger.Default.Register<SettingsModel>(this, (o) => _settings = o);

            PassingInformation = new List<PassingInformationItem>()
            {
                new PassingInformationItem("0", "00000001", string.Empty),
                new PassingInformationItem("1", "00000010", string.Empty),
                new PassingInformationItem("2", "00000011", string.Empty),
                new PassingInformationItem("3", "Счетчик", string.Empty),
                new PassingInformationItem("4", "11111110", string.Empty),
                new PassingInformationItem("5", "11111110", string.Empty),
                new PassingInformationItem("6", "00000000", string.Empty),
                new PassingInformationItem("7", "00000000", string.Empty),
                new PassingInformationItem("8", "11111111", string.Empty),
                new PassingInformationItem("9", "11111111", string.Empty),
            };
        }

        public List<PassingInformationItem> PassingInformation { get; }

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

                    await _devices[0].ReadBytesAsync().ContinueWith(t => _rxA.Add(t.Result));
                    await _devices[1].ReadBytesAsync().ContinueWith(t => _rxB.Add(t.Result));

                    if (_rxA.Count == PassingInformation.Count)
                    {
                        for (int j = 0; j < PassingInformation.Count; j++)
                        {
                            // Тут какая то логика по получению значения из А (из _rxA[j][0]).
                            // Тут какая то логика по получению значения из В (из _rxB[j][0]).
                            // Тут надо сложить полученные значения из А и В (А | В) и записать в PassingInformation[j].Data.
                            PassingInformation[j].Result = Convert.ToString(_rxA[j][0] | _rxB[j][0], 2).PadLeft(8, '0');
                        }
                        _rxA.Clear();
                        _rxB.Clear();
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

                        PassingInformation[0].Result = Convert.ToString(i, 2).PadLeft(8, '0');
                        PassingInformation[1].Result = Convert.ToString(i, 2).PadLeft(8, '0');
                        PassingInformation[2].Result = Convert.ToString(i, 2).PadLeft(8, '0');
                        PassingInformation[3].Result = Convert.ToString(i, 2).PadLeft(8, '0');
                        PassingInformation[4].Result = Convert.ToString(i, 2).PadLeft(8, '0');
                        PassingInformation[5].Result = Convert.ToString(i, 2).PadLeft(8, '0');
                        PassingInformation[6].Result = Convert.ToString(i, 2).PadLeft(8, '0');
                        PassingInformation[7].Result = Convert.ToString(i, 2).PadLeft(8, '0');
                        PassingInformation[8].Result = Convert.ToString(i, 2).PadLeft(8, '0');
                        PassingInformation[9].Result = Convert.ToString(i, 2).PadLeft(8, '0');
                        Thread.Sleep(20);
                    }
                });
            }
#endif
        }
        /// <summary>
        /// Подготовка данных для режима "Проверка прохождения информации".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void Prepare(byte informationLength)
        {
            Init();

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData2(_settings.Address1, _settings.Address2, _settings.Address3, informationLength);
            }
        }
        /// <summary>
        /// Сброс устройств и очистка списков.
        /// </summary>
        private void Init()
        {
            _rxA.Clear();
            _rxB.Clear();

            if (_devices != null)
            {
                for (int i = 0; i < _devices.Length; i++)
                {
                    _devices[0].Purge();
                }
            }

            foreach (var item in PassingInformation)
            {
                item.Result = string.Empty;
            }
        }
    }

    class PassingInformationItem : INotifyPropertyChanged
    {
        private string _result;
        public PassingInformationItem(string address, string denomination, string result)
        {
            Address = address;
            Denomination = denomination;
            Result = result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string Address { get; private set; }
        public string Denomination { get; private set; }
        public string Result
        {
            get => _result;
            set
            {
                this.MutateVerbose(ref _result, value, args => PropertyChanged?.Invoke(this, args));
            }
        }
    }
}
