using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using TK158.DialogWindows;
using TK158.Helpers;
using TK158.Model;

namespace TK158.ViewModel
{
    class SettingsViewModel : ViewModelBase
    {
        private FtdiDevice[] _devices;
        private SettingsModel _settings;
        private string _fileName;
        private Visibility _closeFileVisibility;
        private byte _selectedBits;
        private bool _isInfiniteFT;
        private Frequency _frequency;
        private WordLength _wordLength;
        private CheckMode _checkMode;
        private List<string> _checkModeTitle;

        public SettingsViewModel()
        {
            Messenger.Default.Register<FtdiDevice[]>(this, (o) => _devices = o);
            Messenger.Default.Register<SettingsModel>(this, (o) => {
                _settings = o;
                SelectedBits = _settings.BitsCount;
                Frequency = (Frequency)_settings.Frequency;
                WordLength = (WordLength)_settings.WordLength;
                IsInfiniteFT = _settings.IsInfiniteFT;
            });

            CheckMode = CheckMode.PassingInformation;
            CloseFileVisibility = Visibility.Hidden;

            _checkModeTitle = new List<string>()
            { 
                "Прохождение информации", 
                "Выходная информация",
                "Остальные", 
                "Тестовая", 
            };

            ListBitsCount = new List<byte>();
            for (byte i = 1; i <= 32; i++)
            {
                ListBitsCount.Add(i);
            }                      
        }

        public string FileName
        {
            get => _fileName;
            set => Set(ref _fileName, value);
        }
        public Visibility CloseFileVisibility 
        { 
            get => _closeFileVisibility;
            set => Set(ref _closeFileVisibility, value);
        }
        public string Mode1Text => _checkModeTitle[0]; 
        public string Mode2Text => _checkModeTitle[1];
        public string Mode3Text => _checkModeTitle[2];
        public string Mode4Text => _checkModeTitle[3];
        public CheckMode CheckMode
        {
            get => _checkMode;
            set
            {
                if (_checkMode == value)
                    return;

                Set(ref _checkMode, value);
                RaisePropertyChanged(nameof(IsMode1));
                RaisePropertyChanged(nameof(IsMode2));
                RaisePropertyChanged(nameof(IsMode3));
                RaisePropertyChanged(nameof(IsMode4));
            }
        }
        public bool IsMode1
        {
            get => CheckMode == CheckMode.PassingInformation;
            set => Set(ref _checkMode, value ? CheckMode.PassingInformation : CheckMode);
        }
        public bool IsMode2
        {
            get => CheckMode == CheckMode.OutputInformation;
            set => Set(ref _checkMode, value ? CheckMode.OutputInformation : CheckMode);
        }
        public bool IsMode3
        {
            get => CheckMode == CheckMode.OtherTests;
            set => Set(ref _checkMode, value ? CheckMode.OtherTests : CheckMode);
        }
        public bool IsMode4
        {
            get => CheckMode == CheckMode.Test;
            set => Set(ref _checkMode, value ? CheckMode.Test : CheckMode);
        }
        public string Address1
        {
            get => _settings.Address1;
            set
            {
                _settings.Address1 = value;
                Properties.Settings.Default.Address1 = value;
                Properties.Settings.Default.Save();
            }
        }
        public string Address2
        {
            get => _settings.Address2;
            set
            {
                _settings.Address2 = value;
                Properties.Settings.Default.Address2 = value;
                Properties.Settings.Default.Save();
            }
        }
        public string Address3
        {
            get => _settings.Address3;
            set
            {
                _settings.Address3 = value;
                Properties.Settings.Default.Address3 = value;
                Properties.Settings.Default.Save();
            }
        }
        public Frequency Frequency
        {
            get => _frequency; 
            set
            {
                if (_frequency == value)
                    return;

                Set(ref _frequency, value);
                RaisePropertyChanged(nameof(Is128kHz));
                RaisePropertyChanged(nameof(Is256kHz));
                RaisePropertyChanged(nameof(Is512kHz));
                RaisePropertyChanged(nameof(Is1024kHz));
            }
        }
        public bool Is128kHz 
        { 
            get => Frequency == Frequency._128kHz;
            set => Set(ref _frequency, value ? Frequency._128kHz : Frequency);
        }
        public bool Is256kHz
        {
            get => Frequency == Frequency._256kHz;
            set => Set(ref _frequency, value ? Frequency._256kHz : Frequency);
        }
        public bool Is512kHz
        {
            get => Frequency == Frequency._512kHz;
            set => Set(ref _frequency, value ? Frequency._512kHz : Frequency);
        }
        public bool Is1024kHz
        {
            get => Frequency == Frequency._1024kHz;
            set => Set(ref _frequency, value ? Frequency._1024kHz : Frequency);
        }
        public WordLength WordLength
        {
            get => _wordLength;
            set
            {
                if (_wordLength == value)
                    return;

                Set(ref _wordLength, value);
                RaisePropertyChanged(nameof(Is16Bit));
                RaisePropertyChanged(nameof(Is32Bit));
            }
        }
        public bool Is16Bit 
        {
            get => WordLength == WordLength._16Bit;
            set => Set(ref _wordLength, value ? WordLength._16Bit : WordLength);
        }
        public bool Is32Bit
        {
            get => WordLength == WordLength._32Bit;
            set => Set(ref _wordLength, value ? WordLength._32Bit : WordLength);
        }
        public bool IsInfiniteFT 
        {
            get => _isInfiniteFT;
            set => Set(ref _isInfiniteFT, value);
        }
        public bool IsInfiniteSending 
        {
            get => _settings.IsInfiniteSending;
            set => _settings.IsInfiniteSending = value; 
        }
        public List<byte> ListBitsCount { get; }
        public byte SelectedBits { 
            get => _selectedBits; 
            set => Set(ref _selectedBits, value); 
        }

        public RelayCommand OpenFileCommand => new RelayCommand(
            () =>
            {
                var dataFile = new DataFile();
   
                dataFile.Open();

                if (dataFile.DialogResult == true)
                {
                    FileName = dataFile.FileName;
                    CloseFileVisibility = Visibility.Visible;
                    if (dataFile.Tk158Settings != null)
                    {
                        Frequency = (Frequency)dataFile.Tk158Settings[0];
                        WordLength = (WordLength)dataFile.Tk158Settings[1];
                        SelectedBits = dataFile.Tk158Settings[2];
                        IsInfiniteFT =  Convert.ToBoolean(dataFile.Tk158Settings[3]);
                        Messenger.Default.Send(dataFile);
                    }                    
                }
            });
        public RelayCommand CloseFileCommand => new RelayCommand(
            () =>
            {
                DataFile dataFile = null;
                FileName = string.Empty;
                CloseFileVisibility = Visibility.Hidden;
                Messenger.Default.Send(dataFile);
            });
        public RelayCommand ChangeModeCommand => new RelayCommand(
            () => Messenger.Default.Send(new CheckModeParameters() 
            {
                CheckModeTitle = _checkModeTitle[(int)CheckMode],
                CheckMode = (int)CheckMode 
            }));
        public RelayCommand SendTK158SettingsCommand => new RelayCommand(
            async () =>
            {             
                _settings.Frequency = (byte)Frequency;               
                _settings.WordLength = (byte)WordLength;

                Properties.Settings.Default.Frequency = _settings.Frequency;
                Properties.Settings.Default.BitsCount = SelectedBits;
                Properties.Settings.Default.IsInfiniteFT = IsInfiniteFT;
                Properties.Settings.Default.WordLength = _settings.WordLength;
                Properties.Settings.Default.Save();

                var wordLengthWithMod = Is16Bit ? SelectedBits : (byte)(SelectedBits | 0x80);
                wordLengthWithMod = IsInfiniteFT ? (byte)(wordLengthWithMod | 0x40) : wordLengthWithMod;

                await _devices[0].WriteByteAsync(_settings.Frequency)
                    .ContinueWith(t => _devices[1].WriteByteAsync(wordLengthWithMod));
                await _devices[0].ReadBytesAsync().ContinueWith(t => _devices[1].ReadBytesAsync());
      
                if (_settings.WordLength == (byte)WordLength._32Bit)
                {
                    await _devices[0].WriteByteAsync(0x0).ContinueWith(t => _devices[1].WriteByteAsync(0x0));
                    await _devices[0].ReadBytesAsync().ContinueWith(t => _devices[1].ReadBytesAsync());
                }
            });
    }

    class CheckModeParameters
    {
        public string CheckModeTitle { get; set; }
        public int CheckMode { get; set; }
    }

}
