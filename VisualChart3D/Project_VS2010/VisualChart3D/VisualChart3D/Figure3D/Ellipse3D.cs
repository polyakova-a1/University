// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows.Media.Media3D;

namespace VisualChart3D
{
    /// <summary>
    /// Эллипс 3D
    /// </summary>
    public class Ellipse3D : Mesh3D
    {
        /// <summary>
        /// Эллипс 3D
        /// </summary>
        /// <param name="a">ширина</param>
        /// <param name="b">длина</param>
        /// <param name="h">высота</param>
        /// <param name="nRes">гладкость</param>
        public Ellipse3D(double a, double b, double h, int nRes)
        {
            SetMesh(nRes);
            SetData(a, b, h, nRes);
        }

        /// <summary>
        /// Задать сетчатую структуру
        /// </summary>
        /// <param name="nRes">кол-во треугольников</param>
        void SetMesh(int nRes)
        {
            int nVertNo = (nRes - 2) * nRes + 2;
            int nTriNo = 2 * nRes * (nRes - 3) + 2 * nRes;
            SetSize(nVertNo, nTriNo);

            int n00, n01, n10;
            int nTriIndex = 0;
            int nI2;
            int i;
            int j = 1;
            for (i = 0; i < nRes; i++)
            {
                if (i == (nRes - 1)) nI2 = 0;
                else nI2 = i + 1;

                n00 = 1 + (j - 1) * nRes + i;
                n10 = 1 + (j - 1) * nRes + nI2;
                n01 = 0;

                SetTriangle(nTriIndex, n00, n10, n01);
                nTriIndex++;
            }
            for (j = 1; j < (nRes - 2); j++)
            {
                for (i = 0; i < nRes; i++)
                {
                    if (i == (nRes - 1)) nI2 = 0;
                    else nI2 = i + 1;
                    n00 = 1 + (j - 1) * nRes + i;
                    n10 = 1 + (j - 1) * nRes + nI2;
                    n01 = 1 + j * nRes + i;
                    int n11 = 1 + j * nRes + nI2;

                    SetTriangle(nTriIndex, n00, n01, n10);
                    SetTriangle(nTriIndex + 1, n01, n11, n10);
                    nTriIndex += 2;
                }
            }

            j = nRes - 2;
            for (i = 0; i < nRes; i++)
            {
                if (i == (nRes - 1)) nI2 = 0;
                else nI2 = i + 1;

                n00 = 1 + (j - 1) * nRes + i;
                n10 = 1 + (j - 1) * nRes + nI2;
                n01 = nVertNo - 1;

                SetTriangle(nTriIndex, n00, n01, n10);
                nTriIndex++;
            }
        }

        /// <summary>
        /// Установка пространсвернных координат
        /// </summary>
        /// <param name="a">ширина</param>
        /// <param name="b">длина</param>
        /// <param name="h">высота</param>
        /// <param name="nRes">кол-во треугольников</param>
        void SetData(double a, double b, double h, int nRes)
        {
            double stepXandY = 2.0f * 3.1415926f / nRes;
            double aZStep = 3.1415926f / ((float)nRes - 1);

            SetPoint(0, 0, 0, h);

            int j;
            for (j = 1; j < (nRes - 1); j++)
            {
                int i;
                for (i = 0; i < nRes; i++)
                {
                    double aXandY = i * stepXandY;
                    double aZAngle = j * aZStep;

                    double x1 = a * System.Math.Sin(aZAngle) * System.Math.Cos(aXandY);
                    double y1 = b * System.Math.Sin(aZAngle) * System.Math.Sin(aXandY);
                    double z1 = h * System.Math.Cos(aZAngle);
                    SetPoint((j - 1) * nRes + i + 1, x1, y1, z1);
                }
            }
            SetPoint((nRes - 2) * nRes + 1, 0, 0, -h);

            MinPoint = new Point3D(-a, -b, -h);
            MaxPoint = new Point3D(a, b, h);
        }
    }
}