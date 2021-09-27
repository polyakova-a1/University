// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using VisualChart3D.Common;

namespace VisualChart3D
{
    /// <summary>
    /// Настройки классов объктов
    /// </summary>
    public class ClassVisualisationSettings
    {

        /// <summary>
        /// Получить или задать количество полигонов
        /// </summary>
        public string CountPoligonStr { get; set; }

        public const double OptimalSize = 0.05;

        /// <summary>
        /// Получить или задать размер объектов
        /// </summary>
        public string SizeObjectStr { get; set; }

        /// <summary>
        /// Получить количество полигонов
        /// </summary>
        public int CountPoligon
        {
            get
            {
                return _countPoligon;
            }
            set
            {
                CountPoligonStr = value.ToString();
                string error = ValidadionTextField("Количество полигонов", 3, 20, CountPoligonStr, out value);
                if (!String.IsNullOrEmpty(error))
                {
                    throw new ArgumentOutOfRangeException("value", error);
                }

                _countPoligon = value;
            }
        }

        /// <summary>
        /// Получить размер объектов
        /// </summary>
        public double SizeObject
        {
            get
            {
                return _sizeObject;
            }
            set
            {
                SizeObjectStr = value.ToString();
                string error = ValidadionTextField("Размер объектов", 0, 1000, SizeObjectStr, out value);

                if (!String.IsNullOrEmpty(error))
                {
                    throw new ArgumentOutOfRangeException("value", error);
                }

                _sizeObject = value;
            }
        }


        /// <summary>
        /// количество полигонов
        /// </summary>
        private int _countPoligon;

        /// <summary>
        /// размер объектов
        /// </summary>
        private double _sizeObject;

        /// <summary>
        /// Массив настроек кажого класса
        /// </summary>
        public AloneSettClass[] ArrayClass;

        /// <summary>
        /// Получить по имени класса объект настроек класса
        /// </summary>
        /// <param name="name">имя класса</param>
        /// <returns>объект настроек класса</returns>
        public AloneSettClass GetSettingClass(string name)
        {
            return ArrayClass.FirstOrDefault(cls => cls.NameClass.Equals(name));
        }

        public ClassVisualisationSettings()
        {
        }

        /// <summary>
        /// Задать настройки по умолчанию
        /// </summary>
        /// <param name="namesClass">Список неповторяющихся имён классов</param>
        public ClassVisualisationSettings(IList<string> namesClass)
        {
            Random random = new Random();

            CountPoligonStr = "13";
            CountPoligon = 13;
            SizeObjectStr = "20";
            SizeObject = OptimalSize;
            ArrayClass = new AloneSettClass[namesClass.Count];

            for (int i = 0; i < ArrayClass.Length; i++)
            {
                ArrayClass[i] = new AloneSettClass
                {
                    ColorObject = Color.FromRgb((byte)random.Next(0, 255), (byte)random.Next(0, 255),
                        (byte)random.Next(0, 255)),
                    Shape = Shapes.Ellipse3D,
                    NameClass = namesClass[i]
                };
            }
        }

        /// <summary>
        /// Преобразование текстового значение в числовое и проверка его на корректность 
        /// </summary>
        /// <param name="nameField">Название поля</param>
        /// <param name="startRange">Начало диапазона</param>
        /// <param name="endRange">Конец диапазона</param>
        /// <param name="fieldStr">Строковое значение поля</param>
        /// <param name="result">Полученное числовое значение поля</param>
        /// <returns>Строка с текстом ошибки</returns>
        private string ValidadionTextField(string nameField, int startRange, int endRange, string fieldStr,
            out int result)
        {
            result = -1;
            StringBuilder errorsStr = new StringBuilder();

            if (string.IsNullOrEmpty(fieldStr))
            {
                errorsStr.AppendFormat("Отсутвует значение поля \"{0}\"{1}", nameField, Environment.NewLine);
            }
            else
            {
                if (int.TryParse(fieldStr, out result))
                {
                    if (result < startRange || result > endRange)
                    {
                        errorsStr.AppendFormat("\"{0}\" не может быть меньше {1} и больше {2}{3}", nameField, startRange,
                            endRange, Environment.NewLine);
                    }
                }
                else
                {
                    errorsStr.AppendFormat("Некорректное значение поля \"{0}\"{1}", nameField, Environment.NewLine);
                }
            }

            return errorsStr.ToString();
        }

        /// <summary>
        /// Преобразование текстового значение в числовое и проверка его на корректность 
        /// </summary>
        /// <param name="nameField">Название поля</param>
        /// <param name="startRange">Начало диапазона</param>
        /// <param name="endRange">Конец диапазона</param>
        /// <param name="fieldStr">Строковое значение поля</param>
        /// <param name="result">Полученное числовое значение поля</param>
        /// <returns>Строка с текстом ошибки</returns>
        private string ValidadionTextField(string nameField, int startRange, int endRange, string fieldStr,
            out float result)
        {
            result = -1;
            StringBuilder errorsStr = new StringBuilder();

            if (string.IsNullOrEmpty(fieldStr))
            {
                errorsStr.AppendFormat("Отсутвует значение поля \"{0}\"{1}", nameField, Environment.NewLine);
            }
            else
            {
                if (float.TryParse(fieldStr, out result))
                {
                    if (result < startRange || result > endRange)
                    {
                        errorsStr.AppendFormat("\"{0}\" не может быть меньше {1} и больше {2}{3}", nameField, startRange,
                            endRange, Environment.NewLine);
                    }
                }
                else
                {
                    errorsStr.AppendFormat("Некорректное значение поля \"{0}\"{1}", nameField, Environment.NewLine);
                }
            }

            return errorsStr.ToString();
        }

        /// <summary>
        /// Преобразование текстового значение в числовое и проверка его на корректность 
        /// </summary>
        /// <param name="nameField">Название поля</param>
        /// <param name="startRange">Начало диапазона</param>
        /// <param name="endRange">Конец диапазона</param>
        /// <param name="fieldStr">Строковое значение поля</param>
        /// <param name="result">Полученное числовое значение поля</param>
        /// <returns>Строка с текстом ошибки</returns>
        private string ValidadionTextField(string nameField, int startRange, int endRange, string fieldStr,
            out double result)
        {
            result = -1;
            StringBuilder errorsStr = new StringBuilder();

            if (string.IsNullOrEmpty(fieldStr))
            {
                errorsStr.AppendFormat("Отсутвует значение поля \"{0}\"{1}", nameField, Environment.NewLine);
            }
            else
            {
                if (double.TryParse(fieldStr, out result))
                {
                    if (result < startRange || result > endRange)
                    {
                        errorsStr.AppendFormat("\"{0}\" не может быть меньше {1} и больше {2}{3}", nameField, startRange,
                            endRange, Environment.NewLine);
                    }
                }
                else
                {
                    errorsStr.AppendFormat("Некорректное значение поля \"{0}\"{1}", nameField, Environment.NewLine);
                }
            }

            return errorsStr.ToString();
        }

        /// <summary>
        /// Валидация стрковых значений
        /// </summary>
        /// <returns>Строка с текстом ошибки</returns>
        public string ValidationStrValues()
        {
            StringBuilder errorsStr = new StringBuilder();
            errorsStr.Append(ValidadionTextField("Количество полигонов", 3, 20, CountPoligonStr, out _countPoligon));
            errorsStr.Append(ValidadionTextField("Размер объектов", 0, 150, SizeObjectStr, out _sizeObject));
            return errorsStr.ToString();
        }
    }
}