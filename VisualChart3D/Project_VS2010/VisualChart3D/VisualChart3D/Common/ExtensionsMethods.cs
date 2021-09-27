// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VisualChart3D.Common
{
    public static class ExtensionsMethods
    {
        private const string Bar = "Брусок";
        private const string Cone = "Конус";
        private const string Cylinder = "Цилиндр";
        private const string Ellipse = "Шар";
        private const string Pyramid = "Пирамида";
        private const string Shape = "Shape";

        /// <summary>
        /// Преобразует цвет Windows Forms в цвет WPF
        /// </summary>
        /// <param name="color">Цвет WindowsForms</param>
        /// <returns>Цвет WPF</returns>
        public static System.Windows.Media.Color ToColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Получить русское название из <see cref="Shapes"/>
        /// </summary>
        /// <param name="shape">текущая фигура</param>
        /// <returns>Русское название</returns>
        public static string GetRusName(this Shapes shape)
        {
            switch (shape)
            {
                case Shapes.Bar3D:
                    return Bar;
                case Shapes.Cone3D:
                    return Cone;
                case Shapes.Cylinder3D:
                    return Cylinder;
                case Shapes.Ellipse3D:
                    return Ellipse;
                case Shapes.Pyramid3D:
                    return Pyramid;
                default:
                    throw new ArgumentOutOfRangeException(Shape);
            }
        }

        public static T[,] ToTwoDimArray<T>(this T[][] source)
        {
            int firstDim = source.GetLength(0);
            int secondDim = source[0].Length;
            for (var i = 1; i < firstDim; ++i)
            {
                if (secondDim != source[i].Length)
                    throw new IndexOutOfRangeException("Array must be a rectangular size");
            }

            var result = new T[firstDim, secondDim];
            for (int i = 0; i < firstDim; ++i)
            {
                for (int j = 0; j < secondDim; ++j)
                {
                    result[i, j] = source[i][j];
                }
            }

            return result;
        }

        public static T[][] ToJaggedArray<T>(this T[,] twoDimArray)
        {
            int rowsFirstIndex = twoDimArray.GetLowerBound(0);
            int rowsLastIndex = twoDimArray.GetUpperBound(0);
            int numberOfRows = rowsLastIndex + 1;

            int columnsFirstIndex = twoDimArray.GetLowerBound(1);
            int columnsLastIndex = twoDimArray.GetUpperBound(1);
            int numberOfColumns = columnsLastIndex + 1;

            T[][] jaggedArray = new T[numberOfRows][];
            for (int i = rowsFirstIndex; i <= rowsLastIndex; i++)
            {
                jaggedArray[i] = new T[numberOfColumns];

                for (int j = columnsFirstIndex; j <= columnsLastIndex; j++)
                {
                    jaggedArray[i][j] = twoDimArray[i, j];
                }
            }

            return jaggedArray;
        }

        public static ((T, int), (T, int)) MinMaxIndex<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null)
                throw new ArgumentNullException("Source was null.");

            if (comparer == null)
                comparer = Comparer<T>.Default;

            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Source was empty");

            int minIndex = 0;
            T minValue = enumerator.Current;

            int maxIndex = 0;
            T maxValue = enumerator.Current;

            int index = 0;
            while (enumerator.MoveNext())
            {
                ++index;
                if (comparer.Compare(enumerator.Current, minValue) < 0)
                {
                    minIndex = index;
                    minValue = enumerator.Current;
                }

                if (comparer.Compare(enumerator.Current, maxValue) > 0)
                {
                    maxIndex = index;
                    maxValue = enumerator.Current;
                }
            }

            return ((minValue, minIndex), (maxValue, maxIndex));
        }

        public static ((T, int), (T, int)) MinMaxIndex<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("Source was null.");

            return source.MinMaxIndex(null);
        }

        public static (T, int) MinIndex<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null)
                throw new ArgumentNullException("Source was null.");

            if (comparer == null)
                comparer = Comparer<T>.Default;

            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Source was empty");

            int minIndex = 0;
            T minValue = enumerator.Current;

            int index = 0;
            while (enumerator.MoveNext())
            {
                ++index;
                if (comparer.Compare(enumerator.Current, minValue) < 0)
                {
                    minIndex = index;
                    minValue = enumerator.Current;
                }
            }

            return (minValue, minIndex);
        }

        public static (T, int) MinIndex<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("Source was null.");

            return source.MinIndex(null);
        }

        public static (TResult, int) MinIndex<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> proj)
        {
            if (source == null)
                throw new ArgumentNullException("Source was null.");

            return source.Select(proj).MinIndex();
        }

        public static (T, int) MaxIndex<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null)
                throw new ArgumentNullException("Source was null.");

            if (comparer == null)
                comparer = Comparer<T>.Default;

            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Source was empty");

            int maxIndex = 0;
            T maxValue = enumerator.Current;

            int index = 0;
            while (enumerator.MoveNext())
            {
                ++index;
                if (comparer.Compare(enumerator.Current, maxValue) > 0)
                {
                    maxIndex = index;
                    maxValue = enumerator.Current;
                }
            }

            return (maxValue, maxIndex);
        }

        public static (T, int) MaxIndex<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("Source was null.");

            return source.MaxIndex(null);
        }

        public static (TResult, int) MaxIndex<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> proj)
        {
            if (source == null)
                throw new ArgumentNullException("Source was null.");

            return source.Select(proj).MaxIndex();
        }

        public static T Min<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null)
                throw new ArgumentNullException("Source was null.");

            return source.MinIndex(comparer).Item1;
        }

        public static T Max<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null)
                throw new ArgumentNullException("Source was null.");

            return MaxIndex(source, comparer).Item1;
        }

        public static TResult AdjacentAggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source,
                                                                               TAccumulate seed,
                                                                               Func<TAccumulate, TSource, TSource, TAccumulate> func,
                                                                               Func<TAccumulate, TResult> resultSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException($"'{nameof(source)}' parameter was null.");
            }

            if (func == null)
            {
                throw new ArgumentNullException($"'{nameof(func)}' parameter was null.");
            }

            if (resultSelector == null)
            {
                throw new ArgumentNullException($"'{nameof(resultSelector)}' parameter was null.");
            }

            var it = source.GetEnumerator();
            if (!it.MoveNext())
            {
                return resultSelector(seed);
            }

            var left = it.Current;
            if (!it.MoveNext())
            {
                return resultSelector(seed);
            }

            do
            {
                var right = it.Current;
                seed = func(seed, left, right);
                left = right;
            }
            while (it.MoveNext());

            return resultSelector(seed);
        }

        public static TAccumulate AdjacentAggregate<TSource, TAccumulate>(this IEnumerable<TSource> source,
                                                                          TAccumulate seed,
                                                                          Func<TAccumulate, TSource, TSource, TAccumulate> func)
        {

            return AdjacentAggregate(source, seed, func, obj => obj);
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }

    public class FuncEqualityComparer<T> : IEqualityComparer<T>
    {
        readonly Func<T, T, bool> _comparer;
        readonly Func<T, int> _hash;

        public FuncEqualityComparer(Func<T, T, bool> comparer)
            : this(comparer, t => 0) // NB Cannot assume anything about how e.g., t.GetHashCode() interacts with the comparer's behavior
        {
        }

        public FuncEqualityComparer(Func<T, T, bool> comparer, Func<T, int> hash)
        {
            _comparer = comparer;
            _hash = hash;
        }

        public bool Equals(T x, T y)
        {
            return _comparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _hash(obj);
        }
    }
}