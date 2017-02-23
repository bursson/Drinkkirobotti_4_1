using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OperatorUI
{
    public sealed class ViewModel : INotifyPropertyChanged
    {
        private int _counter;

        public int Counter
        {
            get { return _counter; }
            set
            {
                if (value == _counter) return;
                _counter = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
