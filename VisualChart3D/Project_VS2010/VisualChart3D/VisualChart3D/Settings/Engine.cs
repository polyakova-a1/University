// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VisualChart3D.Common;
using VisualChart3D.Common.DataReader;

namespace VisualChart3D
{
    /// <summary>
    /// Инициализация и хранение исходных данных. В будущем 
    /// </summary>
    public class Engine
    {
        private const string ClassFileChooseMessage = "Не указан файл с классами объектов";
        private const string IncorrectClassFileStructureMessage = "Некорректная структура файла с классами объектов";
        private const string NotChoosedObjectsCountMessage = "Не указано число объектов в классе";
        private const string BadObjectsCountMessage = "Некорректное число объектов в классе";
        private const string BadObjectsMultiplyMessage = "Некорректное число объектов в классе. Должно быть кратное числу объектов";
        private const string NamesFileChooseMessage = "Не указан файл с именами объектов";
        private const string NamesFileIncorrectStructureMessage = "Некорректная структура файла с именами объектов";

        /// <summary>
        /// Порядок расстояния минковского, соответствующее Евклидову пространству.
        /// </summary>
        private const int EucludianMetricsDegree = 2;

        /// <summary>
        /// Получить количество объектов
        /// </summary>
        public virtual int CountObjects
        {
            get
            {
                if (UniversalReader == null)
                {
                    return 0;
                }

                const int dimensionOfObjects = 0;

                return ArraySource.GetLength(dimensionOfObjects);
            }
        }

        public virtual double[,] ArraySource {
            get {
                return this.UniversalReader.DistMatrix;
            }
        }

        /*/// <summary>
        /// Получить или задать тип исходной матрицы
        /// </summary>
        public SourceFileMatrixType SourceMatrixType { get; set; }*/

        private IUniversalReader _reader;

        public IUniversalReader UniversalReader
        {
            get
            {
                return _reader;
            }

            set
            {
                _reader = value;

                if (value.InputFileType != InputFileType.CSV)
                {
                    return;
                }

                if (!String.IsNullOrEmpty(value.ClassNameColumn))
                {
                    ArrayClassesOneToOne = value.ClassNameColumnData;
                    ClassObjectType = ClassInfoType.OneToOne;
                    ClassObjectSelected = true;
                }

                if (!String.IsNullOrEmpty(value.ObjectNameColumn))
                {
                    _arrayNames = value.ObjectNameColumnData;
                }
            }
        }

        /// <summary>
        /// Тип алгоритма, используемого для визуализации данных
        /// </summary>
        private AlgorithmType _algorithmType;

        /// <summary>
        /// Получить массив с количеством объектов каждого класса
        /// </summary>
        public int[] numberOfObjectsOfClass;

        public string multimediaFolderPath = null;

        public MultimediaLoadType MultimediaLoadingType { get; set; }

        //public bool isMultimediaByObjectID = false;  //добавить обнуление
        //public bool isMultimediaByObjectName = false;  //добавить обнуление
        //public bool isMultimediaByClassInterval = false;  //добавить обнуление
        //public bool isMultimediaByClassStartObjects = false;  //добавить обнуление
        public List<String> classStartPosition;

        public int[] GetClassPositionsOnOneToOneMode(List<String> uniqueClassesNames)
        {
            int[] counts = new int[uniqueClassesNames.Count];
            var groups = _classesName.GroupBy(name => name);

            foreach (var group in groups)
            {
                counts[uniqueClassesNames.IndexOf(group.Key)] = group.Count();
            }
            
            return counts;
        }

        /// <summary>
        /// Получить или задать сущестовование названий классов. Если есть файл с настройкой классов - true, в противном случае false
        /// </summary>
        public bool ClassObjectSelected { get; set; }

        /// <summary>
        ///  Получить массив данных, если файл классов задан "Один-к-одному"
        /// </summary>
        public string[] ArrayClassesOneToOne { get; set; }

        /// <summary>
        /// Получить массив данных, если файл классов задан "Число объектов класса" или "Начало класса"
        /// </summary>
        public virtual string[,] ArrayClassesCountObj { get; private set; }

        /// <summary>
        /// Получить или задать тип классов объектов
        /// </summary>
        public ClassInfoType ClassObjectType { get; set; }

        /// <summary>
        ///  Получить или задать путь к файлу с настройками классов объектов
        /// </summary>
        public string ClassObjectFile { get; set; }

        /// <summary>
        ///  Получить или задать сущестовование имён объектов. Если есть файл с именем объектов - true, в противном случае false
        /// </summary>
        public bool NamesObjectSelected { get; set; }

        /// <summary>
        ///  Получить или задать путь к файлу с иенем объектов
        /// </summary>
        public string NamesObjectFile { get; set; }

        /// <summary>
        /// Получить или задать задани файлов числом объектов класса
        /// </summary>
        public bool ClassEqualSelected { get; set; }

        /// <summary>
        /// Получить или задать строковое прдеставление число объектов в классе
        /// </summary>
        public string ClassEqualCountStr { get; set; }

        /// <summary>
        /// Получить список названий классов
        /// </summary>
        public virtual List<String> ClassesName
        {
            get
            {
                if (_classesName == null)
                {
                    _classesName = GetListNamesClass();
                    return _classesName;
                }

                return _classesName;
            }

            set
            {
                _classesName = value;
            }
        }

        /// <summary>
        /// Получить список неповторяющихся названий классов
        /// </summary>
        public List<String> UniqClassesName
        {
            get
            {
                if (_uniqClassesName == null)
                {
                    _uniqClassesName = GetUniqClass();
                    return _uniqClassesName;
                }

                return _uniqClassesName;
            }

            set
            {
                _uniqClassesName = value;
            }
        }

        /// <summary>
        /// Получить список имён объектов
        /// </summary>
        public virtual List<String> NamesObjects
        {
            get
            {
                if (_namesObjects == null)
                {
                    _namesObjects = GetNamesObject();
                    return _namesObjects;
                }

                return _namesObjects;
            }

            set { }
        }

        /*/// <summary>
        ///  Получить или задать тип метрики для построения
        /// </summary>
        public FastMapMetricType Metrics { get; set; } //= MetricsEnum.Euclidean;*/

        /// <summary>
        /// Тип используемого алгоритма для визуализации данных
        /// </summary>
        public AlgorithmType AlgorithmType
        {
            get
            {
                return _algorithmType;
            }

            set
            {
                _algorithmType = value;
            }
        }

        public bool IsMultimediaAudio { get; internal set; }
        public bool IsMultimediaPicture { get; internal set; }

        /// <summary>
        /// число объектов в классе
        /// </summary>
        private int _classEqualCount;

        /// <summary>
        /// список имен объектов
        /// </summary>
        private List<string> _namesObjects;

        /// <summary>
        /// Уникальные имена классов
        /// </summary>
        private List<string> _uniqClassesName;

        /// <summary>
        /// массив имён объектов
        /// </summary>
        private string[] _arrayNames;

        /// <summary>
        /// список названий классов
        /// </summary>
        private List<string> _classesName;

        public Engine()
        {
            ClassObjectSelected = false;
            NamesObjectSelected = false;
        }

        public Engine(Engine engine)
        {
            UniversalReader = engine.UniversalReader;
            AlgorithmType = engine.AlgorithmType;
            MultimediaLoadingType = engine.MultimediaLoadingType;
            IsMultimediaPicture = engine.IsMultimediaPicture;
            IsMultimediaAudio = engine.IsMultimediaAudio;


            /*isMultimediaByObjectID = engine.isMultimediaByObjectID;
            isMultimediaByObjectName = engine.isMultimediaByObjectName;
            isMultimediaByClassInterval = engine.isMultimediaByClassInterval;
            isMultimediaByClassStartObjects = engine.isMultimediaByClassStartObjects;*/

            classStartPosition = engine.classStartPosition;
            multimediaFolderPath = engine.multimediaFolderPath;
            ClassObjectSelected = engine.ClassObjectSelected;
            ArrayClassesOneToOne = engine.ArrayClassesOneToOne;
            ArrayClassesCountObj = engine.ArrayClassesCountObj;
            ClassObjectType = engine.ClassObjectType;
            ClassObjectFile = engine.ClassObjectFile;
            NamesObjectSelected = engine.NamesObjectSelected;
            NamesObjectFile = engine.NamesObjectFile;
            _namesObjects = engine._namesObjects;
            _uniqClassesName = engine._uniqClassesName;
            _arrayNames = engine._arrayNames;
            _classesName = engine._classesName;
            numberOfObjectsOfClass = engine.numberOfObjectsOfClass;
        }

        /// <summary>
        /// Проверка на корректоности файла с классами
        /// </summary>
        /// <returns></returns>
        public bool CheckClassObject()
        {
            if (!ClassObjectSelected)
            {
                return false;
            }

            if (string.IsNullOrEmpty(ClassObjectFile))
            {
                return false;
            }

            if (!File.Exists(ClassObjectFile))
            {
                return false;
            }

            try
            {
                //Костыль. Решить позже.
                if (UniversalReader.InputFileType == InputFileType.CSV && (!String.IsNullOrEmpty(UniversalReader.ClassNameColumn)))
                {
                    ArrayClassesOneToOne = UniversalReader.ClassNameColumnData;
                }
                else
                {
                    switch (ClassObjectType)
                    {
                        case ClassInfoType.OneToOne:

                            ArrayClassesOneToOne = CommonMatrix.ReadMatrixOneToOne(ClassObjectFile);
                            break;

                        case ClassInfoType.CountObj:
                            ArrayClassesCountObj = CommonMatrix.ReadMatrixCountObj(ClassObjectFile);
                            break;

                        case ClassInfoType.StartObjects:
                            ArrayClassesCountObj = CommonMatrix.ReadMatrixCountObj(ClassObjectFile);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка на корректность файла с названиями объектов
        /// </summary>
        /// <returns></returns>
        public bool CheckNamesObject()
        {
            if (!NamesObjectSelected || string.IsNullOrEmpty(NamesObjectFile) || !File.Exists(NamesObjectFile))
            {
                return false;
            }

            try
            {
                _arrayNames = CommonMatrix.ReadMatrixOneToOne(NamesObjectFile);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка содержимого файлов, выбранных юзером
        /// </summary>
        /// <returns></returns>
        public string Validation()
        {
            StringBuilder errors = new StringBuilder();

            if (ClassObjectSelected)
            {
                if (string.IsNullOrEmpty(ClassObjectFile))
                {
                    errors.AppendLine(ClassFileChooseMessage);
                }
                else if (!CheckClassObject())
                {
                    errors.AppendLine(IncorrectClassFileStructureMessage);
                }
            }
            else
            {
                if (ClassEqualSelected)
                {
                    if (string.IsNullOrEmpty(ClassEqualCountStr))
                    {
                        errors.AppendLine(NotChoosedObjectsCountMessage);
                    }
                    else
                    {
                        if (!int.TryParse(ClassEqualCountStr, out _classEqualCount) || _classEqualCount <= 0)
                        {
                            errors.AppendLine(BadObjectsCountMessage);
                        }
                        else if (CountObjects % _classEqualCount != 0)
                        {
                            errors.AppendLine(BadObjectsMultiplyMessage);
                        }
                    }
                }
            }

            if (NamesObjectSelected)
            {
                if (UniversalReader?.ObjectNameColumnData != null)
                {
                    _arrayNames = UniversalReader.ObjectNameColumnData;
                }
                else if (string.IsNullOrEmpty(NamesObjectFile))
                {
                    errors.AppendLine(NamesFileChooseMessage);
                }
                else if (!CheckNamesObject())
                {
                    errors.AppendLine(NamesFileIncorrectStructureMessage);
                }
            }

            return errors.ToString();
        }

        /// <summary>
        /// Получить имена классов
        /// </summary>
        /// <returns>список имён классов</returns>
        private List<string> GetListNamesClass()
        {
            classStartPosition = new List<String>();
            int class_border = 0;
            classStartPosition.Add("0");
            List<string> result = new List<string>();

            if (!ClassObjectSelected)
            {
                if (ClassEqualSelected)
                {
                    for (int i = 0; i < CountObjects; i++)
                    {
                        for (int j = 0; j < _classEqualCount; j++)
                        {
                            result.Add((i + 1).ToString());
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < CountObjects; i++)
                    {
                        result.Add("1");
                    }
                }
            }
            else
            {
                try
                {
                    switch (ClassObjectType)
                    {
                        case ClassInfoType.OneToOne:
                            result = ArrayClassesOneToOne.ToList();
                            break;

                        case ClassInfoType.CountObj:
                            for (int i = 0; i < ArrayClassesCountObj.Length / 2; i++)
                            {
                                int count = int.Parse(ArrayClassesCountObj[i, 0]);
                                class_border += count;
                                classStartPosition.Add(class_border.ToString());
                                string name = string.IsNullOrEmpty(ArrayClassesCountObj[i, 1])
                                    ? (i + 1).ToString()
                                    : ArrayClassesCountObj[i, 1];

                                for (int j = 0; j < count; j++)
                                {
                                    result.Add(name);
                                }
                            }

                            break;

                        case ClassInfoType.StartObjects:
                            int oldCount = int.Parse(ArrayClassesCountObj[0, 0]);

                            for (int i = 1; i < ArrayClassesCountObj.Length / 2; i++)
                            {
                                int count = int.Parse(ArrayClassesCountObj[i, 0]);
                                classStartPosition.Add(count.ToString());
                                string name = string.IsNullOrEmpty(ArrayClassesCountObj[i - 1, 1])
                                    ? (i).ToString()
                                    : ArrayClassesCountObj[i - 1, 1];

                                for (int j = oldCount; j < count; j++)
                                {
                                    result.Add(name);
                                }

                                oldCount = count;
                            }
                            int idx = ArrayClassesCountObj.Length / 2 - 1;
                            string nme = string.IsNullOrEmpty(ArrayClassesCountObj[idx, 1])
                                ? (idx + 1).ToString()
                                : ArrayClassesCountObj[idx, 1];

                            for (int j = oldCount; j < CountObjects; j++)
                            {
                                result.Add(nme);
                            }

                            classStartPosition.Add((CountObjects).ToString()); //конец не важен, главное - границы
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (FormatException)
                {
                    return null;
                }
            }

            return result;
        }

        /// <summary>
        /// Получить список уникальных имён класссов
        /// </summary>
        /// <returns>список уникальных имён классов</returns>
        public List<string> GetUniqClass()
        {
            if (ClassesName == null)
            {
                return null;
            }

            HashSet<string> result = new HashSet<string>();
            foreach (string s in ClassesName.Where(s => !result.Contains(s)))
            {
                result.Add(s);
            }

            return result.ToList();
        }

        /// <summary>
        /// Получитьс имена объектов
        /// </summary>
        /// <returns>список имён объектов</returns>
        protected List<string> GetNamesObject()
        {
            List<string> result = new List<string>();

            if (NamesObjectSelected)
            {
                return _arrayNames.ToList();
            }

            for (int i = 0; i < CountObjects; i++)
            {
                result.Add((i + 1).ToString());
            }

            return result;
        }
    }
}
