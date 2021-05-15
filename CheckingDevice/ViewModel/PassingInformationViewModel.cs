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
    class PassingInformationViewModel : ViewModelBase
    {      
        private Log _log = Log.GetInstance();
        private PassingInformationModel _passingInformationModel = new PassingInformationModel();
        private bool _canStart = true;
        private SettingsModel _settings;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _token;

        public PassingInformationViewModel()
        {
            Messenger.Default.Register<SettingsModel>(this, (o) => _settings = o);
        }

        public ICollectionView SourceItems => CollectionViewSource.GetDefaultView(
                new ObservableCollection<PassingInformationItem>(_passingInformationModel.PassingInformation)
            );
        public string StartButtonText => _canStart ? "Пуск" : "Стоп";
        public Visibility ProgressBarVisibility { get; set; } = Visibility.Hidden;

        public RelayCommand StartCommand => new RelayCommand(
            async () =>
            {
                if (_canStart)
                {
                    _canStart = false;
                    RaisePropertyChanged(nameof(StartButtonText));
                    ProgressBarVisibility = Visibility.Visible;
                    RaisePropertyChanged(nameof(ProgressBarVisibility));

                    Messenger.Default.Send(new NavBarEnabled { IsEnabled = false }) ;

                    _cancelTokenSource = new CancellationTokenSource();
                    _token = _cancelTokenSource.Token;

                    _log.Content.AppendLine("Проверка начата");

                    _passingInformationModel.Prepare(120);


                    while (!_canStart)
                    {
                        await _passingInformationModel.StartAsync(_token);

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

                Messenger.Default.Send(new NavBarEnabled { IsEnabled = true });

                _cancelTokenSource.Cancel();

                _log.Content.AppendLine("Проверка завершена\n");
            });       
    }
}
