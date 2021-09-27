// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows.Media.Media3D;

namespace VisualChart3D
{
    /// <summary>
    /// Пирамида 3D
    /// </summary>
    /// <remarks>
    ///			 0  
    ///		  /  | \		 
    ///      /   1  \
    ///     /  /   \ \  
    ///   3/----------2 
    /// </remarks>
    public class Pyramid3D : Mesh3D
    {
        public Pyramid3D(double size)
        {
            SetMesh();
            double w = size;
            double l = size * System.Math.Sqrt(3) / 2;
            double h = size * System.Math.Sqrt(2.0 / 3.0);
            SetData(w, l, h);
        }

        public Pyramid3D(double w, double l, double h)
        {
            SetMesh();
            SetData(w, l, h);
        }

        /// <summary>
        /// Задать сетчатую структуру
        /// </summary>
        void SetMesh()
        {
            SetSize(4, 4);
            SetTriangle(0, 0, 2, 1);
            SetTriangle(1, 0, 3, 2);
            SetTriangle(2, 0, 1, 3);
            SetTriangle(3, 1, 2, 3);
        }

        /// <summary>
        /// Установка пространсвернных координат
        /// </summary>
        /// <param name="w">ширина</param>
        /// <param name="l">длина</param>
        /// <param name="h">высота</param>
        public void SetData(double w, double l, double h)
        {
            SetPoint(0, 0, 0, h);
            SetPoint(1, 0, l / 2, 0);
            SetPoint(2, +w / 2, -l / 2, 0);
            SetPoint(3, -w / 2, -l / 2, 0);

            MinPoint = new Point3D(-w / 2, -l / 2, -h / 2);
            MaxPoint = new Point3D(w / 2, l / 2, h / 2);
        }
    }
}