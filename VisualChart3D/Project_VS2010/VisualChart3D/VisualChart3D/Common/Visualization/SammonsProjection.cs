using System;
using System.Collections.Generic;

namespace VisualChart3D.Common.Visualization
{
    public interface ISammon : IVisualizer
    {
        int IterationNumber { get; set; }
        int IterationLimit { get; }
        double IterationStep { get; set; }
        double IterationStepLimit { get; }
        List<double> CalculatedCriteria { get; }
    }
    
    public class SammonsProjection : BaseVisualizer, ISammon
    {
        private const string VisualizationErrorFormat = "Ошибка при работе алгоритма визуализации методом Сэммона: {0}";
        private const string StringDescriptionFormat = "Sammons Map, размер данных({0}x{1}, число итераций - {2} )";
        private const double StartStep = 1000.0;
        private const double MinStep = 1e-10;
        private const double E = 1e-10;
        private const int IterationLimitValue = 10000;
        private const double IterationStepLimitValue = 100000;
        private ITimer _timer;

        private int _iterationsCount = 10;

        private double _iterationStep = 2.0;

        private int _dimensions;
        private int _countOfObjects;

        private double[,] _projection;
        private double[,] _distanceMatrix;

        private List<Double> _calculatedCriteria;

        public SammonsProjection(double[,] distMatrix, int dimensions)
        {
            if (dimensions < 1)
            {
                throw new ArgumentException("Некорректное значение числа размерностей");
            }

            _calculatedCriteria = new List<Double>();
            _distanceMatrix = distMatrix;
            _dimensions = dimensions;
            _timer = new CustomTimer();
        }

        private double EvaluateCriteria(double[,] dm, double[,] distmatrix, double sum2dist)
        {
            double result = 0;
            int dimensions = _dimensions;

            for (int i = 0; i < _countOfObjects; i++)
            {
                for (int j = i + 1; j < _countOfObjects; j++)
                {
                    double sum = 0.0D;
                    double dist = distmatrix[i, j];

                    for (int k = 0; k < dimensions; k++)
                    {
                        sum += (dm[i, k] - dm[j, k]) * (dm[i, k] - dm[j, k]);
                    }

                    sum = Math.Sqrt(sum);

                    result += (sum - dist) * (sum - dist);
                }
            }

            result /= sum2dist;
            _calculatedCriteria.Add(result);
            return result;
        }

        private double[] EvaluateGradient(int index, double[,] dm, double[,] distmatrix, double sum2dist)
        {
            int dimensions = _dimensions;
            double[] result = new double[dimensions];

            for (int i = 0; i < _countOfObjects; i++)
            {
                if (i != index)
                {
                    double sum = 0.0D;
                    double dist = distmatrix[index, i];

                    for (int k = 0; k < dimensions; k++)
                    {
                        sum += (dm[i, k] - dm[index, k]) * (dm[i, k] - dm[index, k]);
                    }

                    sum = Math.Sqrt(sum);

                    for (int k = 0; k < dimensions; k++)
                    {
                        result[k] = ((dm[index, k] - dm[i, k]) * (sum - dist) / sum);
                    }
                }
            }

            for (int k = 0; k < dimensions; k++)
            {
                result[k] = (result[k] * _iterationStep / sum2dist);
            }

            return result;
        }

        private void CorrectProjection(double[,] dm, int counter, int dimensions, double step, double[] gradient)
        {
            for (int k = 0; k < _dimensions; k++)
            {
                dm[counter, k] += step * gradient[k];
            }
        }

        public int Dimensions { get => _dimensions; }

        public int MaximumDimensionsNumber { get => MaxAvaibleDimension; }

        public double[,] Projection { get => _projection; }
        public double IterationStep { get => _iterationStep; set => _iterationStep = value; }
        //public double[,] DataMatrix { get => _distanceMatrix; set => _distanceMatrix = value; }

        public List<double> CalculatedCriteria => _calculatedCriteria;

        public int IterationNumber { get => _iterationsCount; set => _iterationsCount = value; }

        public int IterationLimit => IterationLimitValue;

        public double IterationStepLimit => IterationStepLimitValue;

        public bool ToProject()
        {
            _calculatedCriteria.Clear();
            _countOfObjects = _distanceMatrix.GetLength(0);

            if (IsObjectsCountLessThenMinimal(_countOfObjects))
            { 
                return false;
            }

            _timer.Start(this.ToString());
            
            int[] indexesOfMostRemoteObjects = DisSpace.GetMostestThreeRemoteObjects(_distanceMatrix);

            try
            {
                double[,] dm = new double[_countOfObjects, _dimensions];
                double sum2dist = 0;

                for (int i = 0; i < _dimensions; i++)
                {
                    for (int j = 0; j < _countOfObjects; j++)
                    {
                        dm[j, i] = _distanceMatrix[j, indexesOfMostRemoteObjects[i]];
                    }
                }

                for (int i = 0; i < _countOfObjects; i++)
                {
                    for (int j = i + 1; j < _countOfObjects; j++)
                    {                
                        sum2dist += Math.Pow(_distanceMatrix[i, j], 2);
                    }
                }

                double criteria = EvaluateCriteria(dm, _distanceMatrix, sum2dist);

                int counter = 0;
                int iteration = 1;
                double step = StartStep;

                while ((counter < _countOfObjects) && (criteria > E) && (IterationNumber > iteration))
                {
                    double[] gradient = EvaluateGradient(counter, dm, _distanceMatrix, sum2dist);

                    while (IterationNumber > iteration)
                    {
                        CorrectProjection(dm, counter, _dimensions, -step, gradient);

                        double newcriteria = EvaluateCriteria(dm, _distanceMatrix, sum2dist);

                        iteration++;

                        if (newcriteria < criteria)
                        {
                            criteria = newcriteria;
                            break;
                        }

                        CorrectProjection(dm, counter, _dimensions, step, gradient);
                        step /= _iterationStep;
                    }
                    
                    step = StartStep;
                    counter++;
                }

                _timer.Stop();
                _projection = dm;
                return true;
            }

            catch (Exception ex)
            {
                //На будущее - сделать систему отмены отрисовки в случае эксепшена в любом из методов отрисовки.
                throw new ApplicationException(String.Format(VisualizationErrorFormat, ex.StackTrace));
            }
        }

        public override string ToString()
        {
            return String.Format(StringDescriptionFormat, _distanceMatrix.GetLength(0), _distanceMatrix.GetLength(1), _iterationsCount, IterationStepLimit);
        }
    }
}