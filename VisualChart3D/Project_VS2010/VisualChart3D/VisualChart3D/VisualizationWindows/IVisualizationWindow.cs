using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualChart3D.VisualizationWindows
{
    interface IVisualizationWindow
    {
        public double[,] DataMatrix { get; set; }
        public IList<int> Graph { get; set; }

        public event EventHandler DataChanged;
    }

    public class VisualizationWindowEventArgs : EventArgs
    {
        public double[,] DataSource { get; }
        public IList<int> Graph { get; }

        public VisualizationWindowEventArgs(double[,] data, IList<int> graph)
        {
            DataSource = data;
            Graph = graph;
        }
    }
}
