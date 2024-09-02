using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandAnalyzer
{
    class Abs_value
    {
        private static double[] decimate(double[] X, int delay)
        {
            int outputLength = (X.Length + delay - 1) / delay;
            double[] resultArray = new double[outputLength];

            for(int i = 0; i < Convert.ToInt32(X.Length/delay); i++)
            {
                resultArray[i] = X[i * delay];
            }

            //for (int i = 0, j = 0; i < X.Length; i += delay, j++)
            //{
            //    resultArray[j] = X[i];
            //}
            return resultArray;
        }
        private static double[] ApplyMovingAverageFilter(double[] inputArray, int windowSize)
        {
            int n = inputArray.Length;
            double[] outputArray = new double[n];
            try
            {
                // Вычисление кумулятивной суммы для первого окна
                double windowSum = 0;
                for (int i = 0; i < windowSize; i++)
                {
                    windowSum += inputArray[i];
                }

                // Применение фильтра скользящего среднего к первому окну
                for (int i = 0; i < windowSize; i++)
                {
                    outputArray[i] = windowSum / windowSize;
                }

                // Применение фильтра скользящего среднего к остальным элементам
                for (int i = windowSize; i < n; i++)
                {
                    windowSum = windowSum - inputArray[i - windowSize] + inputArray[i];
                    outputArray[i] = windowSum / windowSize;
                }
            }
            catch
            {
                windowSize = windowSize / 10;
                // Вычисление кумулятивной суммы для первого окна
                double windowSum = 0;
                for (int i = 0; i < windowSize; i++)
                {
                    windowSum += inputArray[i];
                }

                // Применение фильтра скользящего среднего к первому окну
                for (int i = 0; i < windowSize; i++)
                {
                    outputArray[i] = windowSum / windowSize;
                }

                // Применение фильтра скользящего среднего к остальным элементам
                for (int i = windowSize; i < n; i++)
                {
                    windowSum = windowSum - inputArray[i - windowSize] + inputArray[i];
                    outputArray[i] = windowSum / windowSize;
                }
            }
            return outputArray;
        }
        public static double calculate_abs(double[] X, int window_size, int delay)
        {
            var result_list = new List<double>();
            var dec_sign = decimate(X, delay);
            var filter_array = ApplyMovingAverageFilter(dec_sign, window_size);
            for (int i = 0; i < dec_sign.Length; i ++)
            {
                result_list.Add(Math.Abs(filter_array[i] - dec_sign[i]));
            }
            return result_list.ToArray().Sum();
        }
    }
}
