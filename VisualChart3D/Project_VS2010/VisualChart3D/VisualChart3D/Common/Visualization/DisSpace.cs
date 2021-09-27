// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;

namespace VisualChart3D.Common.Visualization
{
    public class DisSpace
    {
        private const SourceFileMatrixType RequiredDataType = SourceFileMatrixType.MatrixDistance;
        private const string StringDescriptionFormat = "Dissimilarity Space, размер данных({0}x{1})";
        private const int ObjectsCount = 3;
        private const int ArrayIndexCompensator = 1;


        private const double EmptyObjectCompensator = 0.1;
        private const int DefaultFirstObjectID = 1;
        private const int DefaultSecondObjectID = 2;
        private const int DefaultThirdObjectID = 3;

        private const int FirstObjectArrayIndex = 0;
        private const int SecondObjectArrayIndex = 1;
        private const int ThirdObjectArrayIndex = 2;

        private int _firstBasisObject;
        private int _secondBasisObject;
        private int _thirdBasisObject;
        private bool _basicObjectsColorMode;
        private double[,] _arraySource;
        private int _countOfObjects;
        private ITimer _timer;
        private Space _space = Space.TwoDimensional;
        private double[,] _coords;
        private int[] _basicObjects;
        private int[] _referencedObjects;

        public DisSpace(double[,] arraySource, int countOfObjects)
        {
            _timer = new CustomTimer();
            _firstBasisObject = DefaultFirstObjectID;
            _secondBasisObject = DefaultSecondObjectID;
            _thirdBasisObject = DefaultThirdObjectID;
            _coords = null;
            _basicObjectsColorMode = false;
            _arraySource = arraySource;
            _countOfObjects = countOfObjects;
        }

        #region STATIC
        private static int GetMaxColumnValueIndex(double[,] sourceArray, int column, int columnCount)
        {
            double value = Double.MinValue;
            int index = -1;

            for (int i = 0; i < columnCount; i++)
            {
                if (sourceArray[column, i] > value)
                {
                    value = sourceArray[column, i];
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// Поиск самого удаленного объекта относительно двух выбранных объектов.
        /// </summary>
        /// <param name="sourceArray">Матрица расстояний</param>
        /// <param name="firstIndex">Позиция столбца первого выбранного объекта в матрице расстояний</param>
        /// <param name="secondIndex">Позиция столбца второго выбранного объекта в матрице расстояний</param>
        /// <param name="objectsCount">Количество объектов в столбце матрицы расстояний</param>
        /// <returns>Позиция столбца объекта, самого удаленного от двух искомых объектов</returns>
        private static int GetTwoColumnsSumMaxIndex(double[,] sourceArray, int firstIndex, int secondIndex, int objectsCount)
        {
            double value = Double.MinValue;
            int index = -1;

            double iterationSum;

            for (int i = 0; i < objectsCount; i++)
            {
                if (firstIndex == i)
                {
                    continue;
                }

                if (secondIndex == i)
                {
                    continue;
                }

                iterationSum = sourceArray[firstIndex, i] + sourceArray[secondIndex, i];

                if (iterationSum > value)
                {
                    value = iterationSum;
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// Получить три самых удаленных друг от друга объекта в матрцие расстояний
        /// </summary>
        /// <param name="sourceArray">Матрица расстояний</param>
        /// <returns>Массив с номерами трех самых удаленных друг от друга объектов</returns>
        public static int[] GetMostestThreeRemoteObjects(double[,] sourceArray)
        {
            const int countOfRemoteObjects = 3;
            int objectsCount = sourceArray.GetLength(0);
            int[] returnedIndexes = new int[countOfRemoteObjects];

            //Находим самый далекий объект
            returnedIndexes[0] = GetMostRemoteObject(sourceArray, objectsCount);

            //Затем в его столбце ищем самый далекий от него
            returnedIndexes[1] = GetMaxColumnValueIndex(sourceArray, returnedIndexes[0], objectsCount);

            //Затем суммируем расстояния в двух стобцах и ищем объект с самым большим значением.
            returnedIndexes[2] = GetTwoColumnsSumMaxIndex(sourceArray, returnedIndexes[0], returnedIndexes[1], objectsCount);

            return returnedIndexes;
        }


        /// <summary>
        /// Расчет суммы расстояний объекта относительно всех остальных в матрице расстояний
        /// </summary>
        /// <param name="SourceArray">Матрица расстояний</param>
        /// <param name="classStartObject">Начальная позиция обхода столбца матрицы</param>
        /// <param name="classEndObject">Конечная позиция обхода столбца матрицы</param>
        /// <param name="currentObject">Столбец матрицы, соответствующий искомому объекту</param>
        /// <returns>Сумма расстояний для искомого объкта</returns>
        public static double FindSumOfDistances(double[,] SourceArray, int classStartObject, int classEndObject, int currentObject)
        {
            double sumfOfDistances = 0;

            for (int i = classStartObject; i < classEndObject; i++)
            {
                sumfOfDistances += SourceArray[currentObject, i];
            }

            return sumfOfDistances;
        }

        public static int GetMostRemoteObject(double[,] sourceArray, int objectsCount)
        {
            double[] mostRemoteObjects = new double[objectsCount];
            int[] mostRemoteObjectIndexes = new int[objectsCount];

            for (int i = 0; i < objectsCount; i++)
            {
                mostRemoteObjectIndexes[i] = i;
                mostRemoteObjects[i] = FindSumOfDistances(sourceArray, 0, objectsCount, i);
            }

            ReverseComparer reverseComparer = new ReverseComparer();
            Array.Sort(mostRemoteObjects, mostRemoteObjectIndexes, reverseComparer);

            return mostRemoteObjectIndexes[0];
        }
        #endregion

        //private void CalculateReferencedObjects(double[,] SourceArray, int[] countOfClassObjects)

        //Такая проблема. Если у нас объекты лежат не в полосах классов (1 1 1 2 2 2 2 а 1 2 1 2 1 2, то неверно сработает алгоритм. Важно пофиксить в мае)
        private void CalculateReferencedObjects(int[] countOfClassObjects)
        {
            int countOfClass;
            int currentClassLastElement;

            if (countOfClassObjects == null)
            {
                countOfClass = 1;
                currentClassLastElement = _arraySource.GetLength(0)-ArrayIndexCompensator;
            }
            else
            {
                countOfClass = countOfClassObjects.Length;
                currentClassLastElement = countOfClassObjects[0] - ArrayIndexCompensator;
            }

           _referencedObjects = new int[countOfClass];
            
            int currentClassFirstElement = 0;
            int numberOfCurrentClass = 0;
            int referencedObjectForCurrentClass = 0;
            double referencedDistance = FindSumOfDistances(_arraySource, currentClassFirstElement,
                   currentClassLastElement, 0);
            double currentReferencedDistance = 0;

            for (int i = 0; i < _arraySource.GetLength(0); i++)
            {               
                if ((currentReferencedDistance = FindSumOfDistances(_arraySource, currentClassFirstElement,
                   currentClassLastElement, i)) < referencedDistance)
                {
                    referencedDistance = currentReferencedDistance;
                    referencedObjectForCurrentClass = i;
                }

                /*referencedDistance = currentReferencedDistance < referencedDistance
                    ? currentReferencedDistance : referencedDistance;*/

                if (i == currentClassLastElement)
                {
                    _referencedObjects[numberOfCurrentClass] = referencedObjectForCurrentClass + ArrayIndexCompensator;
                    
                    if (i < _arraySource.GetLength(0) - ArrayIndexCompensator)
                    {
                        numberOfCurrentClass++;
                        i++;

                        currentClassFirstElement = i;
                        currentClassLastElement += countOfClassObjects[numberOfCurrentClass];

                        referencedDistance = FindSumOfDistances(_arraySource, currentClassFirstElement,
                       currentClassLastElement, currentClassFirstElement);
                        referencedObjectForCurrentClass = currentClassFirstElement;
                    }
                }
            }
        }

        public List<string> GetReferencedObjectsWithClassNames(Engine engine)
        {
            /*settFile.UniqClassesName, settFile.numberOfObjectsOfClass*/

            const string ReferencedObjectsWithClassNamesFormat = "Класс - {0}, № Эталона - {1}, Объект - {2}.";

            if (_referencedObjects == null)
            {
                CalculateReferencedObjects(engine.numberOfObjectsOfClass);
            }

            List<string> ReferencedObjectsWithClassNames = new List<string>();

            for (int i = 0; i < _referencedObjects.Length; i++)
            {
                //ReferencedObjectsWithClassNames.Add("Класс - " + engine.UniqClassesName[i] + ", № Эталона - " + _referencedObjects[i] + ".");
                ReferencedObjectsWithClassNames.Add(String.Format(ReferencedObjectsWithClassNamesFormat, engine.UniqClassesName[i], _referencedObjects[i] + ArrayIndexCompensator, engine.NamesObjects[this._referencedObjects[i]]));
            }

            /*if (engine.NamesObjects != null)
            {
                ReferencedObjectsWithClassNames.Add("азазаза");
            }
            else
            {
                
            }*/

            return ReferencedObjectsWithClassNames;
        }

        public DisSpace SetBasicObjects(int firstObject, int secondObject, int thirdObject)
        {
            _firstBasisObject = firstObject;
            _secondBasisObject = secondObject;
            _thirdBasisObject = thirdObject;
            return this;
        }

        public double[,] ToProject()
        {
            _timer.Start(String.Format(StringDescriptionFormat,this.ArraySource.GetLength(0), this.ArraySource.GetLength(1)));

            _coords = new double[_countOfObjects, ObjectsCount];

            if (_space == Space.TwoDimensional)
            {
                for (int j = 0; j < _countOfObjects; j++)
                {
                    _coords[j, FirstObjectArrayIndex] = _arraySource[_firstBasisObject - ArrayIndexCompensator, j];
                    _coords[j, SecondObjectArrayIndex] = _arraySource[_secondBasisObject - ArrayIndexCompensator, j];
                    _coords[j, ThirdObjectArrayIndex] = EmptyObjectCompensator;
                }
            }
            else
            {
                for (int j = 0; j < _countOfObjects; j++)
                {
                    _coords[j, FirstObjectArrayIndex] = _arraySource[_firstBasisObject - ArrayIndexCompensator, j];
                    _coords[j, SecondObjectArrayIndex] = _arraySource[_secondBasisObject - ArrayIndexCompensator, j];
                    _coords[j, ThirdObjectArrayIndex] = _arraySource[_thirdBasisObject - ArrayIndexCompensator, j];
                }
            }

            _timer.Stop();
            return _coords;
        }

        #region PROPERTY
        public int[] BasicObjectsArray {
            get {
                _basicObjects = new int[ObjectsCount];
                _basicObjects[FirstObjectArrayIndex] = _firstBasisObject;
                _basicObjects[SecondObjectArrayIndex] = _secondBasisObject;
                _basicObjects[ThirdObjectArrayIndex] = _thirdBasisObject;

                return _basicObjects;
            }

            set {
                _basicObjects = value;
            }
        }

        public int FirstBasisObject { get => _firstBasisObject; set => _firstBasisObject = value; }
        public int SecondBasisObject { get => _secondBasisObject; set => _secondBasisObject = value; }
        public int ThirdBasisObject { get => _thirdBasisObject; set => _thirdBasisObject = value; }
        public Space Space { get => _space; set => _space = value; }
        public bool BasicObjectsColorMode { get => _basicObjectsColorMode; set => _basicObjectsColorMode = value; }
        public double[,] ArraySource { get => _arraySource; }
        public int BasicObjectsNumber { get => (int)_space; }
    #endregion
    }
}