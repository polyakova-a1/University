using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualChart3D.Common
{
    interface ITimer
    {
        void Start(string operationDescription);

        void Stop();

        void DisplayTime();
    }

    class CustomTimer : ITimer
    {
        System.Diagnostics.Stopwatch sw;

        private const string CalculatedTimeTitle = "Затраченное время";
        private const int SymbolsAfterComma = 3;
        private const string BadTimerStart = "Ошибка. Повторный запуск уже ведущего отсчет таймера.";
        private const string BadTimerEnd = "Ошибка. Попытка остановить еще не запущенный отчет таймера.";
        private const string TimeElapsedFormat = "Выполнение операции \"{0}\" заняло {1} сек.";
        private bool _isCountDown;
        private string _description;

        public CustomTimer()
        {
            sw = new System.Diagnostics.Stopwatch();
            _isCountDown = false;
        }

        public void DisplayTime()
        {
            double time = sw.Elapsed.TotalSeconds;
            time = Math.Round(time, SymbolsAfterComma);

            if (time == 0)
            {
                time = Math.Pow(10, -SymbolsAfterComma);
            }

            Utils.ShowWarningMessage(String.Format(TimeElapsedFormat, _description, time.ToString()), CalculatedTimeTitle);
        }

        public void Start(string operationDescription)
        {
            if (_isCountDown)
            {
                throw new InvalidOperationException(BadTimerStart);
            }

            _description = operationDescription;

            sw.Reset();
            sw.Start();
            _isCountDown = true;
        }

        public void Stop()
        {
            if (!_isCountDown)
            {
                throw new InvalidOperationException(BadTimerEnd);
            }

            sw.Stop();
            DisplayTime();
            _isCountDown = false;
        }
    }
}
