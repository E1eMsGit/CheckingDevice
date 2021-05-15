using System;
using System.Threading;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TK158.Helpers;
using TK158.Model;

namespace TK158.ViewModel
{
    class OtherTestsViewModel : ViewModelBase
    {
        private enum OtherTestsCheckModes
        {
            Mode1 = 1, // Проверка тока потребления.
            Mode2 = 2, // Проверка адресатора.
            Mode3 = 3, // Формирование ЗИ и Тоср.
            Mode4 = 4, // Измерение параметров сигналов Тоср.
            Mode5 = 5, // Измерение параметров сигналов ЗИ1 и ЗИ2.
            Mode6 = 6, // Измерение параметров сигналов ЗИ3 и ЗИ4.
            Mode7 = 7, // Измерение параметров сигнала ЗИ5.
        }
        private OtherTestsCheckModes CheckModes
        {
            get => _checkModes;
            set
            {
                if (_checkModes == value)
                    return;

                Set(ref _checkModes, value);
                RaisePropertyChanged(nameof(IsMode1));
                RaisePropertyChanged(nameof(IsMode2));
                RaisePropertyChanged(nameof(IsMode3));
                RaisePropertyChanged(nameof(IsMode4));
                RaisePropertyChanged(nameof(IsMode5));
                RaisePropertyChanged(nameof(IsMode6));
                RaisePropertyChanged(nameof(IsMode7));
            }
        }
        private OtherTestsCheckModes _checkModes;
        private OtherTestsModel _otherTestsModel = new OtherTestsModel();
        private bool _canStart = true;
        private Log _log = Log.GetInstance();
        private SettingsModel _settings;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _token;

        public OtherTestsViewModel()
        {
            Messenger.Default.Register<SettingsModel>(this, (o) => _settings = o);
            CheckModes = OtherTestsCheckModes.Mode1;
        }

        public string StartButtonText => _canStart ? "Пуск" : "Стоп";
        public Visibility ProgressBarVisibility { get; set; } = Visibility.Hidden;
        public bool ModePanelEnabled { get; set; } = true;
        public bool IsMode1
        {
            get => CheckModes == OtherTestsCheckModes.Mode1;
            set => Set(ref _checkModes, value ? OtherTestsCheckModes.Mode1 : CheckModes);
        }
        public bool IsMode2
        {
            get => CheckModes == OtherTestsCheckModes.Mode2;
            set => Set(ref _checkModes, value ? OtherTestsCheckModes.Mode2 : CheckModes);
        }
        public bool IsMode3
        {
            get => CheckModes == OtherTestsCheckModes.Mode3;
            set => Set(ref _checkModes, value ? OtherTestsCheckModes.Mode3 : CheckModes);
        }
        public bool IsMode4
        {
            get => CheckModes == OtherTestsCheckModes.Mode4;
            set => Set(ref _checkModes, value ? OtherTestsCheckModes.Mode4 : CheckModes);
        }
        public bool IsMode5
        {
            get => CheckModes == OtherTestsCheckModes.Mode5;
            set => Set(ref _checkModes, value ? OtherTestsCheckModes.Mode5 : CheckModes);
        }
        public bool IsMode6
        {
            get => CheckModes == OtherTestsCheckModes.Mode6;
            set => Set(ref _checkModes, value ? OtherTestsCheckModes.Mode6 : CheckModes);
        }
        public bool IsMode7
        {
            get => CheckModes == OtherTestsCheckModes.Mode7;
            set => Set(ref _checkModes, value ? OtherTestsCheckModes.Mode7 : CheckModes);
        }
     
        public RelayCommand StartCommand => new RelayCommand(
            async () =>
            {               
                if (_canStart)
                {
                    _canStart = false;
                    RaisePropertyChanged(nameof(StartButtonText));
                    ProgressBarVisibility = Visibility.Visible;
                    RaisePropertyChanged(nameof(ProgressBarVisibility));
                    ModePanelEnabled = false;
                    RaisePropertyChanged(nameof(ModePanelEnabled));

                    Messenger.Default.Send(new NavBarEnabled { IsEnabled = false });

                    _cancelTokenSource = new CancellationTokenSource();
                    _token = _cancelTokenSource.Token;

                    _log.Content.AppendLine("Проверка начата");

                    switch (CheckModes)
                    {
                        case OtherTestsCheckModes.Mode1:
                            _otherTestsModel.PrepareMode1(Constants.MAX_INFORMATION_LENGTH);
                            break;
                        case OtherTestsCheckModes.Mode2:
                            _otherTestsModel.PrepareMode2(Constants.MAX_INFORMATION_LENGTH);
                            break;
                        case OtherTestsCheckModes.Mode3:
                            _otherTestsModel.PrepareMode3(120);
                            break;
                        case OtherTestsCheckModes.Mode4:
                            _otherTestsModel.PrepareMode4(120);
                            break;
                        case OtherTestsCheckModes.Mode5:
                            _otherTestsModel.PrepareMode5(120);
                            break;
                        case OtherTestsCheckModes.Mode6:
                            _otherTestsModel.PrepareMode6(120);
                            break;
                        case OtherTestsCheckModes.Mode7:
                            _otherTestsModel.PrepareMode7(120);
                            break;
                        default:
                            break;
                    }

                    while (!_canStart)
                    {                       
                        await _otherTestsModel.StartAsync(_token);

                        if (!_settings.IsInfiniteSending)
                        {                          
                            break;
                        }
                    }                    
                }

                _canStart = true;
                RaisePropertyChanged(nameof(StartButtonText));
                ProgressBarVisibility = Visibility.Hidden;
                RaisePropertyChanged(nameof(ProgressBarVisibility));
                ModePanelEnabled = true;
                RaisePropertyChanged(nameof(ModePanelEnabled));

                Messenger.Default.Send(new NavBarEnabled { IsEnabled = true });

                _cancelTokenSource.Cancel();

                _log.Content.AppendLine("Проверка завершена\n");
            });
    }   
}
