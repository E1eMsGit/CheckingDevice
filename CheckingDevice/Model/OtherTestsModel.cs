using GalaSoft.MvvmLight.Messaging;
using System.Threading;
using System.Threading.Tasks;
using TK158.Helpers;

namespace TK158.Model
{
    class OtherTestsModel
    {      
        private DataFile _dataFile;
        private FtdiDevice[] _devices;
        private SettingsModel _settings;
        private byte[] _txA;

        public OtherTestsModel()
        {
            Messenger.Default.Register<DataFile>(this, (o) => _dataFile = o);
            Messenger.Default.Register<FtdiDevice[]>(this, (o) => _devices = o);
            Messenger.Default.Register<SettingsModel>(this, (o) => _settings = o);
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

                    await _devices[0].ReadBytesAsync().ContinueWith(t => _devices[1].ReadBytesAsync());
                }
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Проверка тока потребления".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode1(byte informationLength)
        {
            Init();

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData6(informationLength);
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Проверка адресатора".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode2(byte informationLength)
        {
            Init();

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData7(informationLength);
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Формирование ЗИ и Тоср".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode3(byte informationLength)
        {
            Init();

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData2(_settings.Address1, _settings.Address2, _settings.Address3, informationLength);
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Измерение параметров сигналов Тоср".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode4(byte informationLength)
        {
            Init();

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData3(_settings.Address1, informationLength, true);
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Измерение параметров сигналов ЗИ1 и ЗИ2".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode5(byte informationLength)
        {
            Init();

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData3(_settings.Address1, informationLength);
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Измерение параметров сигналов ЗИ3 и ЗИ4".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode6(byte informationLength)
        {
            Init();

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData4(_settings.Address2, informationLength);
            }
        }
        /// <summary>
        /// Подготовка данных для режима "Измерение параметров сигнала ЗИ5".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public void PrepareMode7(byte informationLength)
        {
            Init();

            if (_dataFile == null)
            {
                _txA = TestsDataTA1004M1.GetTestData5(_settings.Address3, informationLength);
            }
        }
        /// <summary>
        ///  Сброс устройств.
        /// </summary>
        private void Init()
        {
            if (_devices != null)
            {
                for (int i = 0; i < _devices.Length; i++)
                {
                    _devices[0].Purge();
                }
            }
        }
    }
}
