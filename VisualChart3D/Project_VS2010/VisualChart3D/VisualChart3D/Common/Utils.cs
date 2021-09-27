using Microsoft.Win32;
using System;
using System.Windows.Media.Imaging;

namespace VisualChart3D.Common
{
    public static class Utils
    {
        private const string WarningMessageStandartTitle = "Внимание!";
        private const string ErrorMessageStandartTitle = "Ошибка!";
        private const string ExceptionMessageFormat = "{0} Стек вызовов: {1}";
        private const string AdressOfPictureLogFile = "PictureAdressLog.txt";
        private const string PictureDirAdressReadingWarningMessage = "Не удалось чтение мультимедиа лог-файла.";
        private const string PictureDirLogFileWarningMessage = "Не удалось создать мультимедиа лог-файл.";
        public const string BadMatrixType = "Ошибка. Тип исходных данных не соответствует выбранному типу входной матрицы.";

        public static T[,] SafeAllocateMemory<T>(int rows, int columns)
        {
            try
            {
                T[,] value = new T[rows, columns];
                return value;
            }
            catch (OutOfMemoryException e)
            {
                Utils.ShowExceptionMessage(e);
                return null;
            }
        }

        public static double ManhattenDistance(double[] vec1, double[] vec2)
        {
            double distance = 0;

            for (int i = 0; i < vec1.Length; i++)
            {
                distance += Math.Abs(vec1[i] - vec2[i]);
            }

            return distance;
        }

        /// <summary>
        /// Изменение значения TextBlock, лежащего в Label
        /// </summary>
        /// <param name="label">Label, включающий себя TextBlock прямым потомком. </param>
        /// <param name="value">Новое значение поля Text объекта TextBlock</param>
        /// <returns>В случае ошибки кастования - false, иначе true</returns>
        public static bool ChangeLabelTextBlockText(System.Windows.Controls.Label label, string value)
        {
            try
            {
                System.Windows.Controls.TextBlock textBlock = (System.Windows.Controls.TextBlock)label.Content;
                textBlock.Text = value;
                return true;
            }
            catch (System.InvalidCastException)
            {
                Common.Utils.ShowErrorMessage("Ошибка приведения TextBox-поля у объекта System.Windows.Controls.Label.");
                return false;
            }
        }

        public static void FisherYatesShuffle<T>(this T[] array)
        {
            Random rnd = new Random();
            for (int i = array.Length - 1; i > 0; i--)
            {
                // Pick random positoin:
                int pos = rnd.Next(i + 1);

                // Swap:
                T tmp = array[i];
                array[i] = array[pos];
                array[pos] = tmp;
            }
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static double[][] GetAnotherStyleOfData(double[,] array)
        {
            int firstDim = array.GetLength(0);
            int secondDim = array.GetLength(1);
            double[][] returnedArray = new double[firstDim][];

            for (int j = 0; j < firstDim; j++)
            {
                returnedArray[j] = new double[secondDim];
            }

            for (int i = 0; i < firstDim; i++)
            {
                for (int j = 0; j < secondDim; j++)
                {
                    returnedArray[i][j] = array[i, j];
                }
            }

            return returnedArray;
        }

        public static double[,] GetNormalizedData(double[,] array)
        {
            int firstDim = array.GetLength(0);
            int secondDim = array.GetLength(1);

            double[,] outputArray = new double[firstDim, secondDim];

            GetMinAndMaxByDimensions(array, firstDim, secondDim,
                       out double maxX, out double maxY, out double maxZ,
                       out double minX, out double minY, out double minZ);

            for (int i = 0; i < firstDim; i++)
            {
                outputArray[i, 0] = (array[i, 0] - minX) / (maxX - minX);
                outputArray[i, 1] = (array[i, 1] - minY) / (maxY - minY);
                outputArray[i, 2] = (array[i, 2] - minZ) / (maxZ - minZ);
            }

            return outputArray;
        }

        public static double[,] ExchangeDataByDim(double[][] oldArray, int firstDim, int secondDim)
        {
            double[,] outputArray = new double[firstDim, secondDim];

            GetMinAndMaxByDimensions(oldArray, firstDim, secondDim,
            out double maxX, out double maxY, out double maxZ,
            out double minX, out double minY, out double minZ);

            for (int i = 0; i < firstDim; i++)
            {
                outputArray[i, 0] = (oldArray[i][0] - minX) / (maxX - minX);
                outputArray[i, 1] = (oldArray[i][1] - minY) / (maxY - minY);
                outputArray[i, 2] = (oldArray[i][2] - minZ) / (maxZ - minZ);

            }

            return outputArray;
        }

        public static double[] GetLineOfMatrix(double[,] array, int lineIndex)
        {
            int lineLength = array.GetLength(1);
            double[] line = new double[lineLength];

            for (int j = 0; j < lineLength; j++)
            {
                line[j] = array[lineIndex, j];
            }

            return line;
        }

        public static void ShowWarningMessage(string message, string title = WarningMessageStandartTitle)
        {
            System.Windows.Forms.MessageBox.Show(message,
                title,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Warning);
        }


        public static void ShowErrorMessage(string message, string title = ErrorMessageStandartTitle)
        {
            System.Windows.Forms.MessageBox.Show(message,
                title,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
        }

        public static void ShowExceptionMessage(Exception e)
        {
            ShowErrorMessage(String.Format(ExceptionMessageFormat, e.Message, e.StackTrace));
        }

        private static void GetMinAndMax(double[][] array, int firstDim, int secondDim, out double min, out double max)
        {
            max = array[0][0];
            min = array[0][0];
            for (int i = 0; i < firstDim; i++)
            {
                for (int j = 0; j < secondDim; j++)
                {
                    max = array[i][j] > max ? array[i][j] : max;
                    min = array[i][j] < min ? array[i][j] : min;
                }
            }
        }

        public static void GetMinAndMax(double[,] array, int firstDim, int secondDim, out double min, out double max)
        {
            max = array[0, 0];
            min = array[0, 0];
            for (int i = 0; i < firstDim; i++)
            {
                for (int j = 0; j < secondDim; j++)
                {
                    max = array[i, j] > max ? array[i, j] : max;
                    min = array[i, j] < min ? array[i, j] : min;
                }
            }
        }

        public static void GetMinAndMaxByDimensions(double[,] array, int firstDim, int secondDim,
            out double maxX, out double maxY, out double maxZ,
            out double minX, out double minY, out double minZ)
        {
            maxX = array[0, 0];
            minX = array[0, 0];

            maxY = array[0, 1];
            minY = array[0, 1];

            maxZ = array[0, 2];
            minZ = array[0, 2];

            for (int i = 0; i < firstDim; i++)
            {
                maxX = array[i, 0] > maxX ? array[i, 0] : maxX;
                minX = array[i, 0] < minX ? array[i, 0] : minX;

                maxY = array[i, 1] > maxY ? array[i, 1] : maxY;
                minY = array[i, 1] < minY ? array[i, 1] : minY;

                maxZ = array[i, 2] > maxZ ? array[i, 2] : maxZ;
                minZ = array[i, 2] < minZ ? array[i, 2] : minZ;
            }

            if (maxX == minX)
            {
                minX = 0;
            }

            if (maxY == minY)
            {
                minY = 0;
            }

            if (maxZ == minZ)
            {
                minZ = 0;
            }
        }

        public static void GetMinAndMaxByDimensions(double[][] array, int firstDim, int secondDim,
            out double maxX, out double maxY, out double maxZ,
            out double minX, out double minY, out double minZ)
        {
            maxX = array[0][0];
            minX = array[0][0];

            maxY = array[0][1];
            minY = array[0][1];

            maxZ = array[0][2];
            minZ = array[0][2];

            for (int i = 0; i < firstDim; i++)
            {
                maxX = array[i][0] > maxX ? array[i][0] : maxX;
                minX = array[i][0] < minX ? array[i][0] : minX;

                maxY = array[i][1] > maxY ? array[i][1] : maxY;
                minY = array[i][1] < minY ? array[i][1] : minY;

                maxZ = array[i][2] > maxZ ? array[i][2] : maxZ;
                minZ = array[i][2] < minZ ? array[i][2] : minZ;
            }
        }

        /// <summary>
        /// Открыть файл
        /// </summary>
        /// <param name="lb">отображение расположения</param>
        public static bool OpenFile(System.Windows.Controls.TextBox lb)
        {
            OpenFileDialog ofDlg = new OpenFileDialog
            {
                Multiselect = false
            };

            if (lb.Text != null && !String.IsNullOrEmpty(lb.Text.ToString()))
            {
                ofDlg.InitialDirectory = lb.Text.ToString();
            }

            ofDlg.RestoreDirectory = true;

            if (ofDlg.ShowDialog().Value)
            {
                lb.Text = ofDlg.FileName;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверка соответствия считанной матрицы и выбранную структуры читаемой матрицы.
        /// Если мат. расстояний, то она должна быть диагональна и квадратна.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="sourceMatrixType"></param>
        /// <returns></returns>
        public static bool CheckSourceMatrix(double[,] array, SourceFileMatrixType sourceMatrixType)
        {
            if (sourceMatrixType != SourceFileMatrixType.MatrixDistance)
            {
                return false;
            }

            if (array.GetLength(0) != array.GetLength(1))
            {
                return true;
            }

            for (int i = 0; i < array.GetLength(0); i++)
            {
                if (array[i, i] != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompareStrings(string A, string B)
        {
            return (string.Compare(A, B, true) == 0);
        }


        /// <summary>
        /// Загрузка изображения по представленному пути как BitmapImage
        /// </summary>
        /// <param name="sourcePath">Путь к файлу с картинкой</param>
        /// <returns>Картинка в формате BitmapImage</returns>
        public static BitmapImage GetPictureFromSource(string sourcePath)
        {
            BitmapImage bitImage = new BitmapImage();

            bitImage.BeginInit();

            bitImage.UriSource = new Uri(sourcePath, UriKind.RelativeOrAbsolute);

            bitImage.EndInit();

            return bitImage;
        }


        /*public static string[] ReadDataFromMultimediaLogFile(string pathToAnyMatrix)
        {
            string[] data;
            string pathToLogFile = System.IO.Path.GetPathRoot(pathToAnyMatrix) + AdressOfPictureLogFile;
            try
            {
                if (System.IO.File.Exists(pathToLogFile))
                {
                    data = System.IO.File.ReadAllLines(pathToLogFile);
                    return data;
                }
            }
            catch
            {
                Utils.ShowWarningMessage(PictureDirAdressReadingWarningMessage);
            }

            return null;
        }

        public static void WriteDataToMultimediaLog(string pathToAnyMatrix, string pathToPictureFolder, string pictureLoadingType)
        {
            if (String.IsNullOrEmpty(pathToAnyMatrix))
            {
                return;
            }


            string pathToPictureContentAdressLog = System.IO.Path.GetPathRoot(pathToAnyMatrix) + AdressOfPictureLogFile;

            //string pathToPictureContentAdressLog = pathToAnyMatrix.Remove(pathToAnyMatrix.LastIndexOf('\\') + 1)
            //    + AdressOfPictureLogFile;
            try
            {
                using (WriteTextToFile file = new WriteTextToFile(pathToPictureContentAdressLog))
                {
                    file.WriteLine(pathToPictureFolder);
                    file.WriteLine(pictureLoadingType);
                }
            }
            catch
            {
                Utils.ShowWarningMessage(PictureDirLogFileWarningMessage);
            }
        }*/

        /// <summary>
        /// Создание десериализованной копии объекта типа Т из json-файла
        /// </summary>
        /// <typeparam name="T">Тип десериализованного объекта</typeparam>
        /// <param name="pathToAnyMatrix">Путь к файлу с сериализованным представлением объекта</param>
        /// <returns>Десериализованная копия объекта</returns>
        public static T ToDeserialize<T>(string pathToAnyMatrix)
        {
            if (String.IsNullOrEmpty(pathToAnyMatrix))
            {
                return default(T);
            }

            string fileContent = String.Empty;
            string pathToLogFile = System.IO.Path.GetDirectoryName(pathToAnyMatrix) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(pathToAnyMatrix) + AdressOfPictureLogFile;

            try
            {
                if (System.IO.File.Exists(pathToLogFile))
                {
                    fileContent = System.IO.File.ReadAllText(pathToLogFile);
                }
                else
                {
                    return default(T);
                }
            }
            catch
            {
                Utils.ShowWarningMessage(PictureDirAdressReadingWarningMessage);
                return default(T);
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fileContent);
        }

        /// <summary>
        /// Создание сериализованной копии объекта и ее сохранение в json-файл
        /// </summary>
        /// <param name="pathToAnyMatrix">Путь к любой из матрицы данных для создания файла в этой же папке</param>
        /// <param name="serializeableObject">Объект, подвергающийся сериализации</param>
        public static void ToSerialize(string pathToAnyMatrix, object serializeableObject)
        {
            if (String.IsNullOrEmpty(pathToAnyMatrix))
            {
                return;
            }

            //string pathToLogFile = System.IO.Path.GetPathRoot(pathToAnyMatrix) + AdressOfPictureLogFile;
            string pathToLogFile = System.IO.Path.GetDirectoryName(pathToAnyMatrix) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(pathToAnyMatrix) + AdressOfPictureLogFile;
            string fileContent = Newtonsoft.Json.JsonConvert.SerializeObject(serializeableObject);

            try
            {
                System.IO.File.WriteAllText(pathToLogFile, fileContent);
            }
            catch
            {
                ShowWarningMessage(PictureDirLogFileWarningMessage);
                return;
            }
        }
    }
    /// <summary>
    /// Compare y and x in reverse order
    /// </summary>
    public class ReverseComparer : System.Collections.Generic.IComparer<double>
    {
        public int Compare(double x, double y)
        {
            return y.CompareTo(x);
        }
    }
}