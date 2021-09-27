using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualChart3D.Common.DataReader
{
    //class FastSCVDataReader : VisualChart3D.Common.DataReader.ICSVReader
    class FastSCVDataReader : IUniversalReader
    {
        private const int Compensation = 0;

        //Косяк. Это должен знать класс, в котором ридер будет полем.
        private InputFileType _windowFileType;

        //Косяк. Этого ридер не должен знать, если только он тут же и преобразует данные
        private int _minkovskiDegree = 2;

        //Аналогично, должно быть в классе выше по иерархии
        private SourceFileMatrixType _inputMatrixType;
        private string _sourceMatrixFile;
        private bool _needRecalculate;
        private List<string> _firstColumn;
        private int _numberOfLinesInFile;
        private List<string> _dataColumn;

        private string _classNameColumn;
        private string _objectNameColumn;
        private string[] _classNameColumnData;
        private string[] _objectNameColumnData;
        private double[,] _sourceMatrix;
        private double[,] _distMatrix;
        private List<string> _ignoredColumns;

        public FastSCVDataReader(InputFileType windowFileType)
        {
            _windowFileType = windowFileType;
            _firstColumn = new List<string>();
            _dataColumn = new List<string>();
            _ignoredColumns = new List<string>();

        }

        private string[] DivideLine(string s, char separator)
        {
            string[] parts;

            s = s.Trim();
            parts = s.Split(separator);

            return parts;
        }

        private bool ReadFirstColumn(string SourceMatrixFile, SourceFileMatrixType SourceMatrixType)
        {
            if (string.IsNullOrEmpty(SourceMatrixFile) || !System.IO.File.Exists(SourceMatrixFile))
            {
                return false;
            }

            //_fileData = new List<List<string>>();

            try
            {
                System.IO.StreamReader fs = new System.IO.StreamReader(SourceMatrixFile);

                string line = fs.ReadLine();
                _firstColumn.Clear();
                _firstColumn.AddRange(line.Trim()
                                          .Split(",".ToCharArray(), 
                                                 StringSplitOptions.RemoveEmptyEntries));

                _numberOfLinesInFile = 0;
                
                while (line != null)
                {
                    //List<string> parts = new List<string>();
                    _numberOfLinesInFile++;
                    //parts = DivideLine(line, ',');
                    //_fileData.Add(parts);
                    line = fs.ReadLine();
                }
            }
            catch (System.IO.IOException e)
            {
                Utils.ShowExceptionMessage(e);
                return false;
            }

            _inputMatrixType = SourceMatrixType;
            _sourceMatrixFile = SourceMatrixFile;
            _needRecalculate = true;

            return true;
        }

        private void SkipFirstLine(System.IO.StreamReader fs)
        {
            fs.ReadLine();
        }

        /// <summary>
        /// возвращает столбец данных на основе его имени 
        /// </summary>
        /// <param name="classNameColumn">имя столбца</param>
        /// <returns>столбец данных</returns>
        private string[] GetColumn(string classColumn)
        {
            System.IO.StreamReader fs = new System.IO.StreamReader(SourceMatrixFile);

            int count = _numberOfLinesInFile;
            string[] data = new string[count - Compensation];
            string[] parts;

            int column = _firstColumn.FindIndex(p => p == classColumn);

            try
            {
                SkipFirstLine(fs);

                string line = fs.ReadLine();

                int counter = 1;
                while (line != null)
                {

                    parts = DivideLine(line, ',');

                    data[counter - Compensation] = parts[column];

                    counter++;
                    line = fs.ReadLine();
                }
            }
            catch (System.IO.IOException e)
            {
                Utils.ShowExceptionMessage(e);
                return null;
                //throw new NotImplementedException();
            }

            _dataColumn.Add(classColumn);
            return data;
        }

        /// <summary>
        /// Возвращает число от 0 до 2 в зависимости от наличия дополнительных стобцов
        /// </summary>
        /// <param name="n"></param>
        /// <param name="m"></param>
        private void CalculateCountOfDeletedColumns(out int n, out int m)
        {
            n = _numberOfLinesInFile - Compensation;
            m = _firstColumn.Count - _dataColumn.Count - _ignoredColumns.Count;
            //m = _firstColumn.Count - Convert.ToInt32(IsNull(_classNameColumn)) - Convert.ToInt32(IsNull(_objectNameColumn));
        }

        private void RecalculateArraySource()
        {
            int n;
            int m;

            try
            {
                CalculateCountOfDeletedColumns(out n, out m);
                _sourceMatrix = new double[n, m];

                System.IO.StreamReader fs = new System.IO.StreamReader(SourceMatrixFile);
                // SkipFirstLine(fs);
                int numericDataIndex = 0;
                string line = fs.ReadLine();

                int lineCounter = 0;
                while (line != null)
                {
                    string[] parts = line.Trim().Split(",".ToCharArray(),
                                                       StringSplitOptions.RemoveEmptyEntries);

                    //data[counter - Compensation] = parts[column];
                    for (int i = 0; i < _firstColumn.Count; i++)
                    {
                        if (_dataColumn.Contains(_firstColumn[i]) || _ignoredColumns.Contains(_firstColumn[i]))
                        {
                            continue;
                        }

                        _sourceMatrix[lineCounter - Compensation, numericDataIndex] = double.Parse(parts[i], System.Globalization.CultureInfo.InvariantCulture);
                        numericDataIndex++;
                    }

                    numericDataIndex = 0;
                    lineCounter++;
                    line = fs.ReadLine();
                }
            }
            catch (Exception e)
            {
                Utils.ShowExceptionMessage(e);
                _sourceMatrix = null;
                return;
            }

            switch (SourceMatrixType)
            {
                case SourceFileMatrixType.MatrixDistance:
                    _distMatrix = _sourceMatrix;
                    break;
                case SourceFileMatrixType.ObjectAttribute:
                    _distMatrix = CommonMatrix.ObjectAttributeToDistance(_sourceMatrix, _minkovskiDegree);
                    _inputMatrixType = SourceFileMatrixType.MatrixDistance;
                    break;
            }
        }


        #region Interface Implementation
        public SourceFileMatrixType SourceMatrixType => _inputMatrixType;

        public string SourceMatrixFile => _sourceMatrixFile;

        public InputFileType InputFileType { get => _windowFileType; set => _windowFileType = value; }
        public int MinkovskiDegree { get => _minkovskiDegree; set => _minkovskiDegree = value; }

        public string ClassNameColumn {
            get {
                return _classNameColumn;
            }

            set {
                _classNameColumn = value;
                if(_classNameColumn == null)
                {
                    return;
                }

                string[] nameColumn = GetColumn(_classNameColumn);
                _classNameColumnData = nameColumn;
                _needRecalculate = true;
            }
        }

        public string ObjectNameColumn {
            get {
                return _objectNameColumn;
            }

            set {
                _objectNameColumn = value;

                if(_objectNameColumn == null)
                {
                    return;
                }

                string[] objectColumn = GetColumn(_objectNameColumn);
                _objectNameColumnData = objectColumn;
                _needRecalculate = true;
            }

        }

        public string[] ClassNameColumnData { get => _classNameColumnData; }
        public string[] ObjectNameColumnData { get => _objectNameColumnData; }

        public List<string> FirstLine => _firstColumn;

        public List<string> IgnoredColumns {
            get {
                if (_ignoredColumns != null)
                {
                    return _ignoredColumns;
                    //return FirstLine.Where(p => _dataColumn.Contains(FirstLine.IndexOf(p))).ToList();
                }

                return new List<string>();
            }

            set {
                if (value.Count > 0)
                {
                    _needRecalculate = true;
                    _ignoredColumns = value;
                }
            }
        }

        public double[,] DistMatrix {
            get {
                if (_needRecalculate)
                {
                    RecalculateArraySource();
                    _needRecalculate = false;
                    _dataColumn.Clear();
                }

                return _distMatrix;
            }
        }

        public double[,] SourceMatrix => _sourceMatrix;

        public bool CheckSourceMatrix(string SourceMatrixFile, SourceFileMatrixType SourceMatrixType)
        {
            return ReadFirstColumn(SourceMatrixFile, SourceMatrixType);
        }
        #endregion 
    }
}
