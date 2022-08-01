using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public event PropertyChangingEventHandler? PropertyChanging;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void OnPropertyChanging([CallerMemberName] string name = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));
        }

        public bool SetProperty<T>([NotNullIfNotNull("newValue")] ref T feild, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(feild, newValue))
                return false;

            OnPropertyChanging(propertyName);

            feild = newValue;

            OnPropertyChanged(propertyName);

            return true;
        }

        bool isBusy;

        public bool IsBusy 
        { 
            get => isBusy; 
            set
            {
                if (SetProperty(ref isBusy, value))
                    OnPropertyChanged(nameof(IsNotBusy));
            } 
        }

        public bool IsNotBusy => !IsBusy;
    }
}
