// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows.Media.Media3D;

namespace VisualChart3D
{
    /// <summary>
    /// Прямоугольник 3D
    /// </summary>
    /// <remarks>
    ///      0______________1
    ///      /|	          /|
    ///   3 /__________2_/ |
    ///    | 4  ---------|- 5
    ///    |/            | /
    ///    7 ___________ 6 
    /// </remarks>
    public class Bar3D : Mesh3D
    {
        /// <summary>
        /// Прямоугольник 3D
        /// </summary>
        /// <param name="x0">Координата x центра прямоугольника</param>
        /// <param name="y0">Координата y центра прямоугольника</param>
        /// <param name="z0">Координата z центра прямоугольника</param>
        /// <param name="width">Ширина прямоугольника</param>
        /// <param name="lenght">Длина прямоугольника</param>
        /// <param name="height">Глубина прямоугольника</param>
        public Bar3D(double x0, double y0, double z0, double width, double lenght, double height)
        {
            SetMesh();
            SetData(x0, y0, z0, width, lenght, height);
        }

        /// <summary>
        /// Задать сетчатую структуру
        /// </summary>
        private void SetMesh()
        {
            SetSize(8, 12);

            SetTriangle(0, 0, 2, 1);
            SetTriangle(1, 0, 3, 2);
            SetTriangle(2, 1, 2, 5);
            SetTriangle(3, 2, 6, 5);
            SetTriangle(4, 3, 6, 2);
            SetTriangle(5, 3, 7, 6);
            SetTriangle(6, 0, 4, 3);
            SetTriangle(7, 3, 4, 7);
            SetTriangle(8, 4, 6, 7);
            SetTriangle(9, 4, 5, 6);
            SetTriangle(10, 0, 5, 4);
            SetTriangle(11, 0, 1, 5);
        }

        /// <summary>
        /// Установка пространсвернных координат 8 вершин
        /// </summary>
        /// <param name="x0">Координата x центра прямоугольника</param>
        /// <param name="y0">Координата y центра прямоугольника</param>
        /// <param name="z0">Координата z центра прямоугольника</param>
        /// <param name="width">Ширина прямоугольника</param>
        /// <param name="lenght">Длина прямоугольника</param>
        /// <param name="height">Глубина прямоугольника</param>
        private void SetData(double x0, double y0, double z0, double width, double lenght, double height)
        {
            SetPoint(0, x0 - width / 2, y0 + lenght / 2, z0 + height / 2);
            SetPoint(1, x0 + width / 2, y0 + lenght / 2, z0 + height / 2);
            SetPoint(2, x0 + width / 2, y0 - lenght / 2, z0 + height / 2);
            SetPoint(3, x0 - width / 2, y0 - lenght / 2, z0 + height / 2);

            SetPoint(4, x0 - width / 2, y0 + lenght / 2, z0 - height / 2);
            SetPoint(5, x0 + width / 2, y0 + lenght / 2, z0 - height / 2);
            SetPoint(6, x0 + width / 2, y0 - lenght / 2, z0 - height / 2);
            SetPoint(7, x0 - width / 2, y0 - lenght / 2, z0 - height / 2);

            MinPoint = new Point3D(x0 - width / 2, y0 - lenght / 2, z0 - height / 2);
            MaxPoint = new Point3D(x0 + width / 2, y0 + lenght / 2, z0 + height / 2);
        }
    }
}