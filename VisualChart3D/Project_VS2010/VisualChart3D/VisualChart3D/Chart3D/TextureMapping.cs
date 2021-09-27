// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace VisualChart3D
{
    /// <summary>
    /// Установка цвета
    /// </summary>
    public class TextureMapping
    {
        /// <summary>
        /// Цвет в 3D модели
        /// </summary>
        public DiffuseMaterial Material;

        /// <summary>
        /// Использовать псевдо цвет
        /// </summary>
        private bool _isPseudoColor;

        public TextureMapping()
        {
            SetRGBMaping();
        }

        public void SetRGBMaping()
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(64, 64, 96, 96, PixelFormats.Bgr24, null);
            writeableBitmap.Lock();

            unsafe
            {
                // Получить указатель на задний буфер
                byte* pStart = (byte*)(void*)writeableBitmap.BackBuffer;
                int nL = writeableBitmap.BackBufferStride;

                for (int r = 0; r < 16; r++)
                {
                    for (int g = 0; g < 16; g++)
                    {
                        for (int b = 0; b < 16; b++)
                        {
                            int nX = (g % 4) * 16 + b;
                            int nY = r * 4 + g / 4;

                            *(pStart + nY * nL + nX * 3 + 0) = (byte)(b * 17);
                            *(pStart + nY * nL + nX * 3 + 1) = (byte)(g * 17);
                            *(pStart + nY * nL + nX * 3 + 2) = (byte)(r * 17);
                        }
                    }
                }
            }
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, 64, 64));

            // Разблокировать задний буфер и сделать его открытым для показа
            writeableBitmap.Unlock();

            ImageBrush imageBrush = new ImageBrush(writeableBitmap) { ViewportUnits = BrushMappingMode.Absolute };
            Material = new DiffuseMaterial { Brush = imageBrush };

            _isPseudoColor = false;
        }

        public void SetPseudoMaping()
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(64, 64, 96, 96, PixelFormats.Bgr24, null);
            writeableBitmap.Lock();

            unsafe
            {
                byte* pStart = (byte*)(void*)writeableBitmap.BackBuffer;
                int nL = writeableBitmap.BackBufferStride;

                for (int nY = 0; nY < 64; nY++)
                {
                    for (int nX = 0; nX < 64; nX++)
                    {
                        int nI = nY * 64 + nX;
                        double k = ((double)nI) / 4095;

                        Color color = PseudoColor(k);

                        *(pStart + nY * nL + nX * 3 + 0) = color.B;
                        *(pStart + nY * nL + nX * 3 + 1) = color.G;
                        *(pStart + nY * nL + nX * 3 + 2) = color.R;
                    }
                }

            }
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, 64, 64));

            writeableBitmap.Unlock();

            ImageBrush imageBrush = new ImageBrush(writeableBitmap) { ViewportUnits = BrushMappingMode.Absolute };

            Material = new DiffuseMaterial { Brush = imageBrush };

            _isPseudoColor = true;
        }

        public Point GetMappingPosition(Color color)
        {
            return GetMappingPosition(color, _isPseudoColor);
        }

        public static Point GetMappingPosition(Color color, bool bPseudoColor)
        {
            if (bPseudoColor)
            {
                double r = ((double)color.R) / 255;
                double g = ((double)color.G) / 255;
                double b = ((double)color.B) / 255;

                double k;

                if ((b >= g) && (b > r))
                {
                    k = 0.25 * g;
                }
                else if ((g > b) && (b >= r))
                {
                    k = 0.25 + 0.25 * (1 - b);
                }
                else if ((g >= r) && (r > b))
                {
                    k = 0.5 + 0.25 * r;
                }
                else
                {
                    k = 0.75 + 0.25 * (1 - g);
                }

                int nI = (int)(k * 4095);
                if (nI < 0) nI = 0;
                if (nI > 4095) nI = 4095;

                int nY = nI / 64;
                int nX = nI % 64;

                double x1 = nX;
                double y1 = nY;

                return new Point(x1 / 64, y1 / 64);
            }
            else
            {
                int nR = (color.R) / 17;
                int nG = (color.G) / 17;
                int nB = (color.B) / 17;

                int nX = (nG % 4) * 16 + nB;
                int nY = nR * 4 + nG / 4;

                double x1 = nX;
                double y1 = nY;

                return new Point(x1 / 63, y1 / 63);
            }
        }

        static public Color PseudoColor(double k)
        {
            if (k < 0) k = 0;
            if (k > 1) k = 1;

            double g, b;
            double r;

            if (k < 0.25)
            {
                r = 0;
                g = 4 * k;
                b = 1;
            }
            else if (k < 0.5)
            {
                r = 0;
                g = 1;
                b = 1 - 4 * (k - 0.25);
            }
            else if (k < 0.75)
            {
                r = 4 * (k - 0.5);
                g = 1;
                b = 0;
            }
            else
            {
                r = 1;
                g = 1 - 4 * (k - 0.75);
                b = 0;
            }

            byte rResult = (byte)(r * 255 + 0.0);
            byte gResult = (byte)(g * 255 + 0.0);
            byte bResult = (byte)(b * 255 + 0.0);

            return Color.FromRgb(rResult, gResult, bResult);
        }
    }
}

