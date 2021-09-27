// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualChart3D.Common;

namespace VisualChart3D
{
    /// <summary>
    /// Класс для хранения исходных данных для визуализации дочерених элементов
    /// </summary>
    public class SubsidiaryEngine : Engine
    {
        private const int CapacityConstant = 5;
        private const string FileCreatingErrorMessage = "Не удалось создать файл {0}";
        private const string SavingMatrixFileFormat = ".part";

        public SubsidiaryEngine(Engine sf, int[] selectedIdx)
            : base(sf)
        {
            _selectedIdx = selectedIdx;
        }

        /// <summary>
        /// список названий классов
        /// </summary>
        private List<string> _classesName;

        /// <summary>
        /// список имен объектов
        /// </summary>
        private List<string> _namesObjects;

        /// <summary>
        /// Исходная матрица
        /// </summary>
        private double[,] _arraySource;

        /// <summary>
        /// Индексы выделенных объектов
        /// </summary>
        private readonly int[] _selectedIdx;

        /// <summary>
        /// Вычислить иходную матрицу
        /// </summary>
        /// <returns>Исходная матрицы</returns>
        private double[,] GetSourceMatrix()
        {
            double[,] result;
            int countOfObjects = CountObjects;
            double[,] arraySource = base.ArraySource;

            if (UniversalReader.SourceMatrixType == SourceFileMatrixType.MatrixDistance)
            {
                result = new double[countOfObjects, countOfObjects];

                for (int i = 0; i < countOfObjects; i++)
                {
                    for (int j = 0; j < countOfObjects; j++)
                    {
                        if (i == j)
                        {
                            result[i, j] = 0;
                        }
                        else
                        {
                            result[i, j] = arraySource[_selectedIdx[i], _selectedIdx[j]];
                        }
                    }
                }

            }
            else if (UniversalReader.SourceMatrixType == SourceFileMatrixType.ObjectAttribute
                || UniversalReader.SourceMatrixType == SourceFileMatrixType.ObjectAttribute3D)
            {
                int countColumn = arraySource.GetLength(1);
                result = new double[countOfObjects, countColumn];

                for (int i = 0; i < countOfObjects; i++)
                {
                    for (int j = 0; j < countColumn; j++)
                    {
                        result[i, j] = arraySource[_selectedIdx[i], j];
                    }
                }

            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            string fileName = UniversalReader.SourceMatrixFile + SavingMatrixFileFormat;
            try
            {
                using (WriteTextToFile file = new WriteTextToFile(fileName))
                {
                    if (UniversalReader.SourceMatrixType == SourceFileMatrixType.MatrixDistance)
                    {
                        for (int i = 0; i < countOfObjects; i++)
                        {
                            StringBuilder str = new StringBuilder(CountObjects * CapacityConstant);

                            for (int j = 0; j < countOfObjects; j++)
                            {
                                str.Append(result[i, j].ToString() + '\t');
                            }
                            file.WriteLine(str.ToString());
                        }
                    }
                    else if (UniversalReader.SourceMatrixType == SourceFileMatrixType.ObjectAttribute
                        || UniversalReader.SourceMatrixType == SourceFileMatrixType.ObjectAttribute3D)
                    {
                        //int countColumn = arraySource.Length / base.CountObjects;
                        int countColumn = arraySource.GetLength(1);

                        for (int i = 0; i < countOfObjects; i++)
                        {
                            StringBuilder str = new StringBuilder(countColumn * CapacityConstant);
                            for (int j = 0; j < countColumn; j++)
                            {
                                str.Append(result[i, j].ToString() + '\t');
                            }
                            file.WriteLine(str.ToString());
                        }
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch
            {
                Utils.ShowErrorMessage(String.Format(FileCreatingErrorMessage, fileName));
            }
            return result;
        }

        /// <summary>
        /// Получить количество объектов
        /// </summary>
        public override int CountObjects {
            get {
                if (_selectedIdx != null)
                {
                    return _selectedIdx.Length;
                }

                return 0;
            }
        }

        /// <summary>
        ///  Получить массив с исходной матрицей
        /// </summary>
        public override double[,] ArraySource {
            get {
                //Заменить функцией, принимающей тип object и чекающей его на нулл. Или мб есть стандартные яз. конструкции. 
                if (_arraySource != null)
                {
                    return _arraySource;
                }

                _arraySource = GetSourceMatrix();

                return _arraySource;
            }
        }

        /// <summary>
        /// Получить список названий классов
        /// </summary>
        public override List<String> ClassesName {
            get {
                if (_classesName != null)
                {
                    return _classesName;
                }

                _classesName =
                    new List<string>(base.ClassesName).Where((s, i) => _selectedIdx.Any(i1 => i == i1)).ToList();
                string fileName = UniversalReader.SourceMatrixFile + ".partClass";

                try
                {
                    StringBuilder str = new StringBuilder(_classesName.Count * CapacityConstant);

                    _classesName.ForEach(p => str.AppendLine(p));

                    //foreach (string s in _classesName)
                    //{
                    //    str.AppendLine(s);
                    //}

                    using (WriteTextToFile file = new WriteTextToFile(fileName))
                    {
                        file.Write(str.ToString());
                    }
                }
                catch
                {
                    Utils.ShowErrorMessage(String.Format(FileCreatingErrorMessage, fileName));
                }

                return _classesName;
            }
        }

        /// <summary>
        /// Получить список имён объектов
        /// </summary>
        public override List<String> NamesObjects {
            get {
                return _namesObjects ??
                       (_namesObjects =
                           new List<string>(base.NamesObjects).Where((s, i) => _selectedIdx.Any(i1 => i == i1)).ToList());
            }
        }
    }

    /*
            /// <summary>
        /// Класс для хранения исходных данных для визуализации дочерених элементов
        /// </summary>
        public class SettingsFilesSubsidiary : Engine
        {
            public SettingsFilesSubsidiary(Engine sf, int[] selectedIdx)
                : base(sf)
            {
                _selectedIdx = selectedIdx;
            }

            /// <summary>
            /// Получить количество объектов
            /// </summary>
            public override int CountObjects
            {
                get
                {
                    if (_selectedIdx != null)
                        return _selectedIdx.Length;
                    return 0;
                }
            }


            /// <summary>
            ///  Получить массив с исходной матрицей
            /// </summary>
            public override double[,] ArraySource
            {
                get
                {
                    if (_arraySource != null)
                        return _arraySource;
                    _arraySource = GetSourceMatrix();
                    return _arraySource;
                }

            }

            /// <summary>
            /// Получить список названий классов
            /// </summary>
            public override List<String> ClassesName
            {
                get
                {
                    if (_classesName != null)
                        return _classesName;
                    _classesName =
                        new List<string>(base.ClassesName).Where((s, i) => _selectedIdx.Any(i1 => i == i1)).ToList();
                    string fileName = this.SourceMatrixFile + ".partClass";
                    try
                    {
                        StringBuilder str = new StringBuilder(_classesName.Count * 5);
                        foreach (string s in _classesName)
                        {
                            str.AppendLine(s);
                        }
                        using (WriteTextToFile file = new WriteTextToFile(fileName))
                        {
                            file.Write(str.ToString());
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Не удалось создать файл " + fileName);
                    }
                    return _classesName;


                }
            }

            /// <summary>
            /// Получить список имён объектов
            /// </summary>
            public override List<String> NamesObjects
            {
                get
                {
                    return _namesObjects ??
                           (_namesObjects =
                               new List<string>(base.NamesObjects).Where((s, i) => _selectedIdx.Any(i1 => i == i1)).ToList());
                }
            }


            /// <summary>
            /// список названий классов
            /// </summary>
            private List<string> _classesName;

            /// <summary>
            /// список имен объектов
            /// </summary>
            private List<string> _namesObjects;

            /// <summary>
            /// Исходная матрица
            /// </summary>
            private double[,] _arraySource;

            /// <summary>
            /// Индексы выделенных объектов
            /// </summary>
            private readonly int[] _selectedIdx;

            /// <summary>
            /// Вычислить иходную матрицу
            /// </summary>
            /// <returns>Исходная матрицы</returns>
            private double[,] GetSourceMatrix()
            {
                double[,] result;
                switch (SourceMatrix)
                {
                    case SourceFileMatrix.MatrixDistance:
                        result = new double[CountObjects, CountObjects];
                        for (int i = 0; i < CountObjects; i++)
                        {
                            for (int j = 0; j < CountObjects; j++)
                            {
                                if (i == j)
                                    result[i, j] = 0;
                                else
                                    result[i, j] = base.ArraySource[_selectedIdx[i], _selectedIdx[j]];
                            }
                        }
                        break;
                    case SourceFileMatrix.ObjectAttribute:
                        int countColumn = base.ArraySource.Length / base.CountObjects;
                        result = new double[CountObjects, countColumn];
                        for (int i = 0; i < CountObjects; i++)
                            for (int j = 0; j < countColumn; j++)
                                result[i, j] = base.ArraySource[_selectedIdx[i], j];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                string fileName = this.SourceMatrixFile + ".part";
                try
                {
                    using (WriteTextToFile file = new WriteTextToFile(fileName))
                    {
                        switch (SourceMatrix)
                        {
                            case SourceFileMatrix.MatrixDistance:
                                for (int i = 0; i < CountObjects; i++)
                                {
                                    StringBuilder str = new StringBuilder(CountObjects*5);
                                    for (int j = 0; j < CountObjects; j++)
                                    {
                                        str.Append(result[i, j].ToString() + '\t');
                                    }
                                    file.WriteLine(str.ToString());
                                }
                                break;
                            case SourceFileMatrix.ObjectAttribute:
                                int countColumn = base.ArraySource.Length/base.CountObjects;
                                for (int i = 0; i < CountObjects; i++)
                                {
                                    StringBuilder str = new StringBuilder(countColumn * 5);
                                    for (int j = 0; j < countColumn; j++)
                                    {
                                        str.Append(result[i, j].ToString() + '\t');
                                    }
                                    file.WriteLine(str.ToString());
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Не удалось создать файл " + fileName);
                }
                return result;
            }
        }
        */
}

