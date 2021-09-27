using Accord.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VisualChart3D.Common.DataReader
{
    class StandartDataReader : IUniversalReader
    {

        private SourceFileMatrixType _sourceMatrixType;
        private string _sourceMatrixFile;
        private int _minkovskyDegree;
        private double[,] _sourceMatrix;
        private double[,] _distMatrix;
        private double[]  _classes;
        private InputFileType _inputFileType;

        public StandartDataReader(InputFileType inputFileType)
        {
            InputFileType = inputFileType;
            _minkovskyDegree = 2;
        }

        public string ClassNameColumn { get => null; set => throw new NotImplementedException(); }
        public string ObjectNameColumn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string[] ClassNameColumnData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string[] ObjectNameColumnData { get => null; set => throw new NotImplementedException(); }

        public SourceFileMatrixType SourceMatrixType { get => _sourceMatrixType; private set => _sourceMatrixType = value; }
        public string SourceMatrixFile { get => _sourceMatrixFile; private set => _sourceMatrixFile = value; }

        public int MinkovskiDegree { get => _minkovskyDegree; set => _minkovskyDegree = value; }
        public double[,] DistMatrix { get => _distMatrix; }
        public InputFileType InputFileType { get => _inputFileType; set => _inputFileType = value; }

        public List<string> FirstLine => throw new NotImplementedException();

        public List<string> IgnoredColumns { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double[,] SourceMatrix { get => _sourceMatrix; }

        public double[]  Classes { get => _classes; }

        /// <summary>
        /// Проверка на корректность исходной матрицы
        /// </summary>
        /// <returns></returns>
        public bool CheckSourceMatrix(string SourceFile, SourceFileMatrixType SourceType)
        {
            double[,] dataMatrix = null;

            if (string.IsNullOrEmpty(SourceFile) || !File.Exists(SourceFile))
            {
                return false;
            }

            try
            {
                if (SourceType == SourceFileMatrixType.MatrixDistance)
                {
                    dataMatrix = CommonMatrix.ReadMatrixDistance(SourceFile);
                }

                if (SourceType == SourceFileMatrixType.ObjectAttribute)
                {
                    if (InputFileType == InputFileType.SVM)
                    {
                        using var svmReader = new SparseReader(SourceFile);
                        double[][] tmp;
                        (tmp, _classes) = svmReader.ReadDenseToEnd();

                        dataMatrix = tmp.ToTwoDimArray();
                    }
                    else
                    {
                        dataMatrix = CommonMatrix.ReadMatrixAttribute(SourceFile);
                    }
                }

                if (Utils.CheckSourceMatrix(dataMatrix, SourceType))
                {
                    Utils.ShowErrorMessage(Utils.BadMatrixType);
                    return false;
                }

                SourceMatrixType = SourceType;
                SourceMatrixFile = SourceFile;

                if (SourceMatrixType == SourceFileMatrixType.ObjectAttribute)
                {
                    _sourceMatrix = dataMatrix;
                    _distMatrix = CommonMatrix.ObjectAttributeToDistance(dataMatrix, this.MinkovskiDegree);

                    if (_distMatrix == null)
                    {
                        return false;
                    }

                    this.SourceMatrixType = SourceFileMatrixType.MatrixDistance;
                }
                else
                {
                    _distMatrix = dataMatrix;
                }

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
