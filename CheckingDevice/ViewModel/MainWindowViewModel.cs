using System.Windows;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using TK158.Model;
using GalaSoft.MvvmLight.Messaging;
using System;
using FTD2XX_NET;
using TK158.Helpers;
using MaterialDesignThemes.Wpf;
using TK158.DialogWindows;

namespace TK158.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        private Log _log = Log.GetInstance();
        private MainWindow _window = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        private int _modeNumber;
        private string _modeTitle;
        private TestViewModel _testViewModel;
        private SettingsViewModel _settingsViewModel;
        private PassingInformationViewModel _passingInformationViewModel;
        private OutputInformationViewModel _outputInformationViewModel;
        private OtherTestsViewModel _otherTestsViewModel;
        private FtdiDevice[] _devices;
        private uint _numDevices;
        private string _title;
        private bool _homeButtonCheckStatus;
        private bool _settingsButtonCheckStatus;
        private bool _mainPanelEnabled;
        private bool _navBarEnabled;

        public MainWindowViewModel(TestViewModel testViewModel, SettingsViewModel settingsViewModel,
            PassingInformationViewModel passingInformationViewModel, OutputInformationViewModel outputInformationViewModel,
            OtherTestsViewModel otherTestsViewModel)
        {
            Messenger.Default.Register<CheckModeParameters>(this, (o) => 
            { 
                _modeTitle = o.CheckModeTitle; 
                _modeNumber = o.CheckMode;
            });
            Messenger.Default.Register<NavBarEnabled>(this, (o) => NavBarEnabled = o.IsEnabled);

            _testViewModel = testViewModel;
            _settingsViewModel = settingsViewModel;
            _passingInformationViewModel = passingInformationViewModel;
            _outputInformationViewModel = outputInformationViewModel;
            _otherTestsViewModel = otherTestsViewModel;

            _log.Content.AppendLine($"Отчет {DateTime.Now}");

            new FTDI().GetNumberOfDevices(ref _numDevices);
#if DEBUG
            Console.WriteLine($"NUMBER OF DEVICES ---- {_numDevices}");
#endif
            if (_numDevices > 0)
            {
                _devices = new FtdiDevice[_numDevices];

                for (uint i = 0; i < _devices.Length; i++)
                {
                    _devices[i] = new FtdiDevice(i);
                    _devices[i].Open();
                }

                Messenger.Default.Send(_devices);
            }

            var _settings = new SettingsModel {
                Address1 = Properties.Settings.Default.Address1,
                Address2 = Properties.Settings.Default.Address2,
                Address3 = Properties.Settings.Default.Address3,
                Frequency = Properties.Settings.Default.Frequency,
                BitsCount = Properties.Settings.Default.BitsCount,
                IsInfiniteFT = Properties.Settings.Default.IsInfiniteFT,
                WordLength = Properties.Settings.Default.WordLength,
            };

            Messenger.Default.Send(_settings);
        }

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        public bool HomeButtonCheckStatus
        {
            get => _homeButtonCheckStatus;
            set
            {
                Set(ref _homeButtonCheckStatus, value);
                RaisePropertyChanged(() => HomeButtonEnabled);

                if (value == true)
                {
                    Title = _modeTitle;
                    _log.Content.AppendLine($"\n{Title}");

                    switch (_modeNumber)
                    {
                        case (int)CheckMode.PassingInformation:
                            ShowPassingInformationView();
                            break;
                        case (int)CheckMode.OutputInformation:
                            ShowOutputInformationView();
                            break;
                        case (int)CheckMode.OtherTests:
                            ShowOtherTestsView();
                            break;
                        case (int)CheckMode.Test:
                            ShowTestView();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        public bool SettingsButtonCheckStatus
        {
            get => _settingsButtonCheckStatus;
            set
            {
                Set(ref _settingsButtonCheckStatus, value);
                RaisePropertyChanged(() => SettingsButtonEnabled);

                if (value == true)
                {
                    Title = "Настройки";
                    MainPanelEnabled = true;
                    ShowSettingsView();

                    CheckIsDeviceOpen();
                }
            }
        }

        public bool MainPanelEnabled
        {
            get => _mainPanelEnabled;
            set => Set(ref _mainPanelEnabled, value);
        }
        public bool NavBarEnabled
        {
            get => _navBarEnabled;
            set => Set(ref _navBarEnabled, value);
        }
        public bool HomeButtonEnabled => !HomeButtonCheckStatus;
        public bool SettingsButtonEnabled => !SettingsButtonCheckStatus;
        
        public RelayCommand CloseWindowCommand => new RelayCommand(
            () =>
            {
                if (_devices != null)
                {
                    for (uint i = 0; i < _devices.Length; i++)
                    {
                        _devices[i].Close();
                    }
                }

                Application.Current.Shutdown();
            });
        public RelayCommand LoadedWindowCommand => new RelayCommand(
            () =>
            {
                // Для того чтобы CheckMode в настройках сразу отработал на вкладке Home.
                _settingsViewModel.ChangeModeCommand.Execute(null);         
                // Для открытия при старте вкладки Настройки.
                SettingsButtonCheckStatus = true;
            });
        public RelayCommand SaveLogCommand => new RelayCommand(() => _log.SaveLog());
        public RelayCommand OpenHelpCommand => new RelayCommand(() => System.Diagnostics.Process.Start("help.html"));
        
        private void ShowTestView()
        {
            if (_window.mainPanel.Children.Count > 0)
            {
                _window.mainPanel.Children.Clear();
            }            
            _window.mainPanel.Children.Add(new View.TestView() { DataContext = _testViewModel });
        }
        private void ShowSettingsView()
        {
            if (_window.mainPanel.Children.Count > 0)
            {
                _window.mainPanel.Children.Clear();
            }
            _window.mainPanel.Children.Add(new View.SettingsView() { DataContext = _settingsViewModel });
        }
        private void ShowPassingInformationView()
        {
            if (_window.mainPanel.Children.Count > 0)
            {
                _window.mainPanel.Children.Clear();
            }
            _window.mainPanel.Children.Add(new View.PassingInformationView() { DataContext = _passingInformationViewModel });
        }
        private void ShowOutputInformationView()
        {
            if (_window.mainPanel.Children.Count > 0)
            {
                _window.mainPanel.Children.Clear();
            }
            _window.mainPanel.Children.Add(new View.OutputInformationView() { DataContext = _outputInformationViewModel });
        }
        private void ShowOtherTestsView()
        {
            if (_window.mainPanel.Children.Count > 0)
            {
                _window.mainPanel.Children.Clear();
            }
            _window.mainPanel.Children.Add(new View.OtherTestsView() { DataContext = _otherTestsViewModel });
        }
        private void CheckIsDeviceOpen()
        {
            if (_devices != null && _devices[0].IsDeviceOpen == true && _devices[1].IsDeviceOpen == true)
            {
                MainPanelEnabled = true;
                NavBarEnabled = true;
            }
            else
            {
#if DEBUG
                // Для отладки без устройства.
                MainPanelEnabled = true;
                NavBarEnabled = true;
#else
                Dialog.ShowNotificationDialog("Ошибка", "Подключите ТК158 и перезапустите программу");
                MainPanelEnabled = false;
                NavBarEnabled = false;
#endif
            }
        }

    }

    class NavBarEnabled
    {
        public bool IsEnabled { get; set; }
    }
}
