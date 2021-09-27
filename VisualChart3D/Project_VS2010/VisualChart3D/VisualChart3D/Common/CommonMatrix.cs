// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VisualChart3D.Common
{
    static class CommonMatrix
    {
        private static readonly int MaxColCount = 2;

        private const string ChangingMinkovskiDegreeErrorMessage = "Ошибка задания порядка расстояния Минковского";
        private const string BadDataFileNameErrorMessage = "Ошибка чтения файла по имени {0}";
        private const string EmptyFileErrorMessage = "Файл не должен быть пустым";
        private const string BadFileStructureErrorMessage = "Некорректная структура файла";
        private const string MatrixConvertingDescriptionFormat = "Конвертирование матрицы объект-признак в матрицу расстояний";

        /// <summary>
        /// Считывание матрицы расстояний
        /// </summary>
        /// <param name="fileName">файл с матрицей расстояний</param>
        /// <returns>массив с матрицей расстояний</returns>
        public static double[,] ReadMatrixDistance(string fileName)
        {
            double[,] result;

            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentNullException(String.Format(BadDataFileNameErrorMessage, fileName));
                }

                string[] sourceData = File.ReadAllLines(fileName);
                result = new double[sourceData.Length, sourceData.Length];
                CultureInfo cult = new CultureInfo("en-US");

                for (int i = 0; i < sourceData.Length; i++)
                {
                    string[] values = sourceData[i].Replace(',', '.').Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int j = 0; j < values.Length; j++)
                    {
                        result[i, j] = double.Parse(values[j], cult);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.ShowExceptionMessage(e);
                return null;
            }

            return result;
        }

        /// <summary>
        /// Считывание матрицы признаков
        /// </summary>
        /// <param name="fileName">файл с матрицей признаков</param>
        /// <returns>массив с матрицей признаков</returns>
        public static double[,] ReadMatrixAttribute(string fileName,
                                                    Func<string, double> converter = null)
        {
            converter = converter ?? ((string value) =>
                                      {
                                          CultureInfo cult = new CultureInfo("en-US");
                                          return double.Parse(value, cult);
                                      });

            double[,] result;

            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentNullException(String.Format(BadDataFileNameErrorMessage, fileName));
                }

                string[] sourceData = File.ReadAllLines(fileName);
                string[] temp = sourceData[0].Split(" \t".ToCharArray(),
                                                    StringSplitOptions.RemoveEmptyEntries);
                int colCount = temp.Length;
                result = new double[sourceData.Length, colCount];

                Parallel.For(0, sourceData.Length, (int i) =>
                {
                    string[] values = sourceData[i].Replace(',', '.')
                                                   .Split(" \t".ToCharArray(),
                                                          StringSplitOptions.RemoveEmptyEntries);

                    for (int j = 0; j < colCount; ++j)
                    {
                        result[i, j] = converter(values[j]);
                    }
                });
            }
            catch (Exception e)
            {
                Utils.ShowExceptionMessage(e);
                return null;
            }

            return result;
        }

        /// <summary>
        /// Считывание матрицы классов "Один-к-кодному"
        /// </summary>
        /// <param name="fileName">файл с матрицей классов "Один-к-кодному"</param>
        /// <returns>массив классов</returns>
        public static string[] ReadMatrixOneToOne(string fileName)
        {
            string[] sourceData;

            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentNullException(String.Format(BadDataFileNameErrorMessage, fileName));
                }

                string[] sourceDataRead = File.ReadAllLines(fileName);
                sourceData = sourceDataRead.Where(s => !string.IsNullOrEmpty(s)).ToArray();

                if (sourceData.Length == 0 || String.IsNullOrEmpty(sourceData[0]))
                {
                    throw new FormatException(EmptyFileErrorMessage);
                }
            }
            catch (Exception e)
            {
                Utils.ShowExceptionMessage(e);
                return null;
            }

            return sourceData;
        }

        /// <summary>
        /// Считывание матрицы классов "Число объектов класса"
        /// </summary>
        /// <param name="fileName">файл с матрицей классов "Число объектов класса"</param>
        /// <returns>массив классов</returns>
        public static string[,] ReadMatrixCountObj(string fileName)
        {
            string[,] result;

            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentNullException(String.Format(BadDataFileNameErrorMessage, fileName));
                }

                string[] sourceData = File.ReadAllLines(fileName);
                string[] temp = sourceData[0].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int colCount = temp.Length;

                if (colCount > MaxColCount)
                {
                    throw new FormatException(BadFileStructureErrorMessage);
                }

                result = new string[sourceData.Length, 2];

                for (int i = 0; i < sourceData.Length; i++)
                {
                    string[] values = sourceData[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    result[i, 0] = values[0];

                    if (values.Length == 2)
                    {
                        result[i, 1] = values[1];
                    }
                }
            }
            catch (Exception e)
            {
                Utils.ShowExceptionMessage(e);
                return null;
            }

            return result;
        }

        /* /// <summary>
         /// Преобразование матрицы признаков в матрицу расстояний
         /// </summary>
         /// <param name="matr">матрица признаков</param>
         /// <param name="countRow">количество строк в матрице признаков</param>
         /// <returns>матрица расстояний</returns>
         public static double[,] ObjectAttributeToDistance(double[,] matr, int countRow, int minkovskiDegree)
         {
             if (minkovskiDegree == 0)
             {
                 throw new ArgumentException(ChangingMinkovskiDegreeErrorMessage);
             }

             int countColumn = matr.GetLength(0);
             //int countColumn = matr.Length / countRow;
             double[,] result = new double[countRow, countRow];

             for (int i = 0; i < countRow; i++)
             {
                 for (int j = 0; j < countRow; j++)
                 {
                     if (i == j)
                     {
                         result[i, j] = 0;
                     }
                     else
                     {
                         double temp = 0;

                         for (int k = 0; k < countColumn; k++)
                         {
                             temp += Math.Pow(Math.Abs(matr[i, k] - matr[j, k]), minkovskiDegree);
                         }

                         temp = Math.Pow(temp, 1d / minkovskiDegree);
                         result[i, j] = result[j, i] = temp;
                     }
                 }
             }

             return result;
         }*/

        /// <summary>
        /// Преобразование матрицы признаков в матрицу расстояний
        /// </summary>
        /// <param name="matr">матрица признаков</param>
        /// <param name="countRow">количество строк в матрице признаков</param>
        /// <returns>матрица расстояний</returns>
        public static double[,] ObjectAttributeToDistance(double[,] matr, int minkovskiDegree)
        {
            ITimer timer = new CustomTimer();
            double[,] result;

            try
            {
                if (minkovskiDegree == 0)
                {
                    throw new ArgumentException(ChangingMinkovskiDegreeErrorMessage);
                }

                int countRow = matr.GetLength(0);
                int countColumn = matr.GetLength(1);
                //double[,] result = new double[countRow, countRow];

                result = Utils.SafeAllocateMemory<double>(countRow, countRow);

                if (result == null)
                {
                    return null;
                }

                timer.Start(MatrixConvertingDescriptionFormat);

                for (int i = 0; i < countRow; i++)
                {
                    //for (int j = 0; j < countRow; j++)
                    Parallel.For(0, countRow, j =>
                    {
                        if (i == j)
                        {
                            result[i, j] = 0;
                        }
                        else
                        {
                            double temp = 0;

                            for (int k = 0; k < countColumn; k++)
                            {
                                temp += Math.Pow(Math.Abs(matr[i, k] - matr[j, k]), minkovskiDegree);
                            }

                            temp = Math.Pow(temp, 1d / minkovskiDegree);
                            result[i, j] = result[j, i] = temp;
                        }
                    });
                }

                timer.Stop();
            }
            catch (Exception e)
            {
                Utils.ShowExceptionMessage(e);
                return null;
            }

            return result;
        }
    }
}
