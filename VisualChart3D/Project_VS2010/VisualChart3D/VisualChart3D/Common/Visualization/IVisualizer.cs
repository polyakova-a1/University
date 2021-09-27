using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualChart3D.Common.Visualization
{
    public interface IVisualizer
    {
        int Dimensions { get; }
        //double[,] DataMatrix { get; set; }
        double[,] Projection { get; }
        int MaximumDimensionsNumber { get; }

        bool ToProject();  
        string ToString();
    }

    public abstract class BaseVisualizer
    {
        private const string MinimalOjectsCountFormat = "Выбраное число объектов, равное {0}, менее минимального значения, равного {1}";

        protected const int MaxAvaibleDimension = 3;
        protected const int MinimalCalculatingObjects = 3;

        protected bool IsObjectsCountLessThenMinimal(int objectsCount)
        {
            if (objectsCount < MinimalCalculatingObjects)
            {
                Utils.ShowWarningMessage(string.Format(MinimalOjectsCountFormat, objectsCount, MinimalCalculatingObjects));
                return true;
            }

            return false;
        }
    }
}
