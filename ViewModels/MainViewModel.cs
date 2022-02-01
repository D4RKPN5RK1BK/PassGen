using PassGen.Commands;
using PassGen.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PassGen.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private Generator _generator;
        private GenerateCommand _generate;

        public Generator Generator
        {
            get => _generator;
            set
            {
                OnPropertyChaged(nameof(Generator));
                _generator = value;
            }
        }
        public GenerateCommand Generate 
        { 
            get
            {
                return _generate;
            } 
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            Generator = new Generator();
            _generate = new GenerateCommand(o => Generator.UpdateResult(), (enable) => Generator.IsValid);
        }
        

        public void OnPropertyChaged([CallerMemberName]string prop = "")
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
