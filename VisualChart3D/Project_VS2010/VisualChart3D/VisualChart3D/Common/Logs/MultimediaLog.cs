using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualChart3D.Common.Logs
{
    class MultimediaLog
    {
        private bool _isPictures;
        private bool _isMultimedia;
        private string _dataPath;
        private MultimediaLoadType _dataLoadType;

        public MultimediaLog(bool isPictures, bool isMultimedia, string dataPath, MultimediaLoadType dataLoadType)
        {
            _isPictures = isPictures;
            _isMultimedia = isMultimedia;
            _dataPath = dataPath;
            _dataLoadType = dataLoadType;
        }

        public bool IsPictures { get => _isPictures; set => _isPictures = value; }
        public bool IsMultimedia { get => _isMultimedia; set => _isMultimedia = value; }
        public string DataPath { get => _dataPath; set => _dataPath = value; }
        public MultimediaLoadType DataLoadType { get => _dataLoadType; set => _dataLoadType = value; }
    }
}
