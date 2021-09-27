using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualChart3D.Common.DataBinding
{
    public class ReferencedObjectsViewModel : BaseViewModel, System.ComponentModel.INotifyPropertyChanged
    {
        private ObservableCollection<string> _referedObjectsInfo;

        public ObservableCollection<string> ReferedObjectsInfo {
            get {
                if (_referedObjectsInfo == null)
                {
                    _referedObjectsInfo = new ObservableCollection<string>();
                }

                return _referedObjectsInfo;
            }

            set {
                _referedObjectsInfo = value;
                NotifyPropertyChanged();
            }
        }
    }
}
