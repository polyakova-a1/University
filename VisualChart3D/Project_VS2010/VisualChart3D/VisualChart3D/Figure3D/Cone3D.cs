// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows.Media.Media3D;

namespace VisualChart3D
{
    /// <summary>
    /// Конус 3D
    /// </summary>
    public class Cone3D : Mesh3D
    {
        /// <summary>
        /// Создать конус 3D
        /// </summary>
        /// <param name="a">ширина</param>
        /// <param name="b">длина</param>
        /// <param name="h">высота</param>
        /// <param name="nRes">гладкость</param>
        public Cone3D(double a, double b, double h, int nRes)
        {
            SetMesh(nRes);
            SetData(a, b, h, nRes);
        }

        /// <summary>
        /// Задать сетчатую структуру
        /// </summary>
        /// <param name="nRes">кол-во треугольников</param>
        private void SetMesh(int nRes)
        {
            int nVertNo = nRes + 2;
            int nTriNo = 2 * nRes;

            SetSize(nVertNo, nTriNo);

            for (int i = 0; i < nRes - 1; i++)
            {
                SetTriangle(i, i, i + 1, nRes + 1);
                SetTriangle(nRes + i, i + 1, i, nRes);
            }

            SetTriangle(nRes - 1, nRes - 1, 0, nRes + 1);
            SetTriangle(2 * nRes - 1, 0, nRes - 1, nRes);
        }

        /// <summary>
        /// Установка пространсвернных координат
        /// </summary>
        /// <param name="a">ширина</param>
        /// <param name="b">длина</param>
        /// <param name="h">высота</param>
        /// <param name="nRes">кол-во треугольников</param>
        private void SetData(double a, double b, double h, int nRes)
        {
            double stepXandY = 2.0f * 3.1415926f / ((double)nRes);

            for (int i = 0; i < nRes; i++)
            {
                double aXandY = i * stepXandY;
                SetPoint(i, a * System.Math.Cos(aXandY), b * System.Math.Sin(aXandY), 0);
            }

            SetPoint(nRes, 0, 0, 0);
            SetPoint(nRes + 1, 0, 0, h);

            MinPoint = new Point3D(-a, -b, 0);
            MaxPoint = new Point3D(a, b, h);
        }
    }
}
