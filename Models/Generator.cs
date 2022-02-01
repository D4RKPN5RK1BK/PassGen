using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Resources;
using System.Threading.Tasks;
using System.Configuration;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace PassGen.Models
{
    internal class Generator : INotifyPropertyChanged, IDataErrorInfo
    {
        private uint _generationCount;
        private uint _minimumDigits;
        private bool _digitsRequired;
        private bool _specialRequired;
        private bool _lowerAndUpper;
        private uint _passwordLength;

        private Configuration configuration;

        private string _specialArr;
        private string _result;

        public Generator()
        {
            configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            _specialArr = configuration.AppSettings.Settings["SpecialArr"].Value;
            
            _digitsRequired = Boolean.Parse(configuration.AppSettings.Settings["DigitsRequired"].Value);
            _lowerAndUpper = Boolean.Parse(configuration.AppSettings.Settings["LowerAndUpper"].Value);
            _specialRequired = Boolean.Parse(configuration.AppSettings.Settings["SpecialRequired"].Value);
            _minimumDigits = UInt32.Parse(configuration.AppSettings.Settings["MinimumDigits"].Value);
            _generationCount = UInt32.Parse(configuration.AppSettings.Settings["GenerationCount"].Value);
            _passwordLength = UInt32.Parse(configuration.AppSettings.Settings["PasswordLength"].Value);
        }

        public string SpecialArr
        {
            get => _specialArr; 
            set
            {
                OnPropertyChanged(nameof(SpecialArr));
                _specialArr = value;
                configuration.AppSettings.Settings["SpecialArr"].Value = value;
                configuration.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        public uint GenerationCount 
        { 
            get => _generationCount;
            set
            {
                OnPropertyChanged(nameof(GenerationCount));
                configuration.AppSettings.Settings["GenerationCount"].Value = value.ToString();
                configuration.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");
                _generationCount = value;
            }
        }

        public bool NotDigitsRequired
        {
            get => !_digitsRequired;
        }
        public bool DigitsRequired
        {
            get => _digitsRequired;
            set
            {
                OnPropertyChanged(nameof(DigitsRequired));
                OnPropertyChanged(nameof(NotDigitsRequired));
                configuration.AppSettings.Settings["DigitsRequired"].Value = value.ToString();
                configuration.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");
                _digitsRequired = value;
            }
        }
        public bool NotSpecialRequired 
        { 
            get => !_specialRequired;
        }
        public bool SpecialRequired
        {
            get => _specialRequired;
            set
            {
                OnPropertyChanged(nameof(SpecialRequired));
                OnPropertyChanged(nameof(NotSpecialRequired));
                configuration.AppSettings.Settings["SpecialRequired"].Value = value.ToString();
                configuration.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");
                _specialRequired = value;
            }
        }
        public uint MinimumDigits
        {
            get => _minimumDigits;
            set
            {
                if (value > _passwordLength - 4 && _digitsRequired)
                    _errors[nameof(MinimumDigits)] = $"Максимальное количество чисел не может быть больше {(_passwordLength - 4).ToString()}";
                else if (value < 1 && _digitsRequired)
                    _errors[nameof(MinimumDigits)] = "Минимальное количество чисел не может быть меньше единицы";
                else
                    _errors[nameof(MinimumDigits)] = null;

                OnPropertyChanged(nameof(MinimumDigits));

                configuration.AppSettings.Settings["MinimumDigits"].Value = value.ToString();
                configuration.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");

                _minimumDigits = value;
            }
        }
        

        public uint PasswordLength
        {
            get => _passwordLength;
            set
            {
                OnPropertyChanged(nameof(PasswordLength));

                if (value > 100)
                    _errors[nameof(PasswordLength)] = "Длинна пароля не может превышать 100 символов";
                else if (value < 6)
                    _errors[nameof(PasswordLength)] = "Длинна пароля не может быть меньше 6 символов";
                else
                    _errors[nameof(PasswordLength)] = null;

                configuration.AppSettings.Settings["PasswordLength"].Value = value.ToString();
                configuration.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");
                _passwordLength = value;
            }
        }
        public bool LowerAndUpper
        {
            get => _lowerAndUpper;
            set
            {
                OnPropertyChanged(nameof(LowerAndUpper));
                configuration.AppSettings.Settings["LowerAndUpper"].Value = value.ToString();
                configuration.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");
                _lowerAndUpper = value;
            }
        }


        public string Result {
            get
            {
                return _result;
            }
        }


        private Dictionary<string, string> _errors = new Dictionary<string, string>();

        public string Error => throw new NotImplementedException();

        public string this[string columnName] => _errors.ContainsKey(columnName) ? _errors[columnName] : null;

        public bool IsValid { get => _errors.Values.All(o => o == null); }

        public void UpdateResult()
        {
            
            string charArr = "";

            charArr += configuration.AppSettings.Settings["LowerArr"].Value;
            if (_digitsRequired) charArr += configuration.AppSettings.Settings["DigitArr"].Value;
            if (_specialRequired) charArr += configuration.AppSettings.Settings["SpecialArr"].Value;
            charArr += _lowerAndUpper ? configuration.AppSettings.Settings["UpperArr"].Value : configuration.AppSettings.Settings["LowerArr"].Value;
            Random rand = new Random();

            List<char> charList = charArr.ToList();
            _result = "";

            uint successGenerations = _generationCount;

            while (successGenerations > 0)
            {
                string pass = "";
                for (int j = 0; j < _passwordLength; j++)
                {
                    pass += charList[rand.Next(0, charList.Count)];
                }


                if ((!_digitsRequired || _digitsRequired && Regex.Match(pass, "[0-9]").Success) &&
                    (!_specialRequired || _specialRequired && Regex.Match(pass, $"[{_specialArr}]").Success) &&
                    (!_lowerAndUpper || _lowerAndUpper && Regex.Match(pass, "[A-Z]").Success) && 
                    Regex.Match(pass, "[a-z]").Success)
                {
                    _result += pass + "\n";
                    successGenerations--;
                }
            }
            OnPropertyChanged(nameof(Result));
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));

        }

    }
}
