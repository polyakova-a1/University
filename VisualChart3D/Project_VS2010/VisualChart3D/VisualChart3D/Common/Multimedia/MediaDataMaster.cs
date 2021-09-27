using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualChart3D.Common.Multimedia
{
    class MediaDataMaster: BaseMultimediaDataMaster
    {
        public MediaDataMaster() : base()
        {

        }

        protected override List<string> MultimediaExtenshions => new List<string> { ".mp4", ".mp3",".wav" };
    }
}
