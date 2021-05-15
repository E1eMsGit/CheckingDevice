using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using TK158.Helpers;
using TK158.Model;

namespace TK158.ViewModel
{
    class OutputInformationViewModel : ViewModelBase
    {
        private enum OutputInformationCheckModes
        {
            Mode1 = 1, // Входы информации.
            Mode2 = 2, // Разряды кода.
            Mode3 = 3, // Задержка выдачи на АУ1.
            Mode4 = 4, // Задержка выдачи на АУ2.
            Mode5 = 5, // Задержка выдачи на АУ3.
        }
        private OutputInformationCheckModes CheckModes
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
            }
        }
        private OutputInformationCheckModes _checkModes;
        private Log _log = Log.GetInstance();
        private OutputInformationModel _outputInformationModel = new OutputInformationModel();
        private bool _canStart = true;
        private SettingsModel _settings;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _token;

        public OutputInformationViewModel()
        {
            Messenger.Default.Register<SettingsModel>(this, (o) => _settings = o);
            CheckModes = OutputInformationCheckModes.Mode1;
            _outputInformationModel.PropertyChanged += ModelCodeBits_PropertyChanged;
        }

        public ICollectionView SourceItems => CollectionViewSource.GetDefaultView(
                new ObservableCollection<OutputInformationItem>(_outputInformationModel.OutputInformation)
            );
        public string CodeBitsResult => _outputInformationModel.CodeBits;
        
        public string StartButtonText => _canStart ? "Пуск" : "Стоп";
        public Visibility ProgressBarVisibility { get; set; } = Visibility.Hidden;
        public bool ModePanelEnabled { get; set; } = true;
        public bool IsMode1
        {
            get => CheckModes == OutputInformationCheckModes.Mode1;
            set => Set(ref _checkModes, value ? OutputInformationCheckModes.Mode1 : CheckModes);
        }
        public bool IsMode2
        {
            get => CheckModes == OutputInformationCheckModes.Mode2;
            set => Set(ref _checkModes, value ? OutputInformationCheckModes.Mode2 : CheckModes);
        }
        public bool IsMode3
        {
            get => CheckModes == OutputInformationCheckModes.Mode3;
            set => Set(ref _checkModes, value ? OutputInformationCheckModes.Mode3 : CheckModes);
        }
        public bool IsMode4
        {
            get => CheckModes == OutputInformationCheckModes.Mode4;
            set => Set(ref _checkModes, value ? OutputInformationCheckModes.Mode4 : CheckModes);
        }
        public bool IsMode5
        {
            get => CheckModes == OutputInformationCheckModes.Mode5;
            set => Set(ref _checkModes, value ? OutputInformationCheckModes.Mode5 : CheckModes);
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
                        case OutputInformationCheckModes.Mode1:
                            _outputInformationModel.PrepareMode1(126);
                            break;
                        case OutputInformationCheckModes.Mode2:
                            _outputInformationModel.PrepareMode2(126);
                            break;
                        case OutputInformationCheckModes.Mode3:
                            _outputInformationModel.PrepareMode3(126);
                            break;
                        case OutputInformationCheckModes.Mode4:
                            _outputInformationModel.PrepareMode4(126);
                            break;
                        case OutputInformationCheckModes.Mode5:
                            _outputInformationModel.PrepareMode5(126);
                            break;
                        default:
                            break;
                    }

                    while (!_canStart)
                    {
                        await _outputInformationModel.StartAsync(_token);
                                               
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

        private void ModelCodeBits_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CodeBits")
                RaisePropertyChanged("CodeBitsResult");
        }
    }
}
