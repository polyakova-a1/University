using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualChart3D.Common.DataReader
{
    public interface IDataReader
    {
        /// <summary>
        /// Получить или задать тип исходной матрицы
        /// </summary>
        SourceFileMatrixType SourceMatrixType { get; }

        /// <summary>
        /// Степень Минковского для преобразования объект-признак в мат. расстояний.
        /// Инициализировать двойкой
        /// </summary>
        int MinkovskiDegree { get; set; }

        /// <summary>
        /// Массив данных, поступаюший на вход методов визуализации.
        /// </summary>        

        bool CheckSourceMatrix(string SourceMatrixFile, SourceFileMatrixType SourceMatrixType);

        InputFileType InputFileType { get; set; }

        double[,] DistMatrix { get; }

        double[,] SourceMatrix { get; }

        /// <summary>
        /// Получить или задать путь к файлу с исходной матрицей
        /// </summary>
        string SourceMatrixFile { get; }
    }

    public interface ICSVReader: IDataReader
    {
        string ClassNameColumn { get; set; }

        string ObjectNameColumn { get; set; }

        string[] ClassNameColumnData { get; }

        string[] ObjectNameColumnData { get; }

        List<string> FirstLine { get; }

        List<string> IgnoredColumns { get; set; }
    }

    /// <summary>
    /// Костыль из-за февральского дедлайна. Решить позже отдельными классами для имен и тип-классов объектов
    /// </summary>
    public interface IUniversalReader: ICSVReader
    {
 
    }
}
