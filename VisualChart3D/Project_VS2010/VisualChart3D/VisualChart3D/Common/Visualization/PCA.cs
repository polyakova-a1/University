
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarMathLib;

namespace VisualChart3D.Common.Visualization
{
    public class PCA : BaseVisualizer, IVisualizer
    {
        private const string StringDescriptionFormat = "Principal Component Analysis, размер данных({0}x{1})";
        private readonly ITimer m_timer = new CustomTimer();

        public PCA(double[,] inputMatrix, int cntProjection = MaxAvaibleDimension)
        {
            InputMatrix = inputMatrix;
            Dimensions = cntProjection;
        }

        public double[,] InputMatrix { get; private set; }

        public int Dimensions { get; private set; } = MaxAvaibleDimension;

        public double[,] Projection { get; private set; }

        public int MaximumDimensionsNumber => Dimensions;

        public bool ToProject()
        {
            try
            {
                m_timer.Start(ToString());

                int rowCount = InputMatrix.GetLength(0);
                int colCount = InputMatrix.GetLength(1);
                var range = Enumerable.Range(0, colCount);

                double[] means = range.Select(i => InputMatrix.GetColumn(i).Average())
                                      .ToArray();

                double[] stdDev = range.Select(i => InputMatrix.GetColumn(i).standardDeviation())
                                       .ToArray();
                var stdMatrix = range.Select(i => InputMatrix.GetColumn(i)
                                                             .Select(value => (value - means[i]) / stdDev[i])
                                                             .ToArray())
                                     .ToArray()
                                     .ToTwoDimArray()
                                     .transpose();

                var corMatrix = stdMatrix.transpose().multiply(stdMatrix).multiply(1.0 / rowCount);
                var eigValues = corMatrix.GetEigenValuesAndVectors(out var eigVectors);
                var idx = eigValues.First()
                                   .Select((eigValue, i) => new KeyValuePair<int, double>(i, eigValue))
                                   .OrderByDescending(kvp => kvp.Value)
                                   .Select(kvp => kvp.Key)
                                   .ToArray();

                eigValues[0] = idx.Select(i => eigValues[0][i]).ToArray();
                {
                    var eigVectorsT = eigVectors.ToTwoDimArray().transpose().ToJaggedArray();
                    var sortedEigVectorsT = eigVectors.ToTwoDimArray().transpose().ToJaggedArray();
                    range.ToList().ForEach(i => sortedEigVectorsT[idx[i]] = eigVectors[i]);
                    eigVectors = sortedEigVectorsT.ToTwoDimArray().transpose().ToJaggedArray();
                }

                var transformMatrix = eigVectors.ToTwoDimArray().transpose()
                                                .ToJaggedArray().SubArray(0, Dimensions)
                                                .ToTwoDimArray().transpose();
                Projection = stdMatrix.multiply(transformMatrix);

                m_timer.Stop();
            }
            catch (Exception)
            {
                Projection = null;
                return false;
            }
            
            return true;
        }

        public override string ToString()
        {
            return string.Format(StringDescriptionFormat, InputMatrix.GetLength(0), InputMatrix.GetLength(1));
        }
    }
}
