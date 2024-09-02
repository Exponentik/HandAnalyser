using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandAnalyzer
{
    class CorrelatorSpirman
    {
        private static double[] Range(double[] Xr)
        {
            int rangeM1 = 0;
            int rangeM2 = 0;
            double[] X = new double[Xr.Length];
            Xr.CopyTo(X, 0);
            Array.Sort(X);
            double[] dx = new double[X.Length];
            double[] dxe = new double[X.Length];
            List<List<double>> dictionary = new List<List<double>>();
            for (int i = 0; i < X.Length; i++)
            {
                if (i + 1 != X.Length)
                {
                    if (X[i] < X[i + 1])
                    {
                        dictionary.Add(new List<double>());
                        rangeM2 = i + 1;
                        int sum = 0;
                        for (int j = rangeM1; j < rangeM2; j++)
                        {
                            sum += (j + 1);
                        }
                        for (int j = rangeM1; j < rangeM2; j++)
                        {
                            dx[j] = (sum / (double)(rangeM2 - rangeM1));
                        }
                        dictionary[dictionary.Count - 1].Add(X[i]);
                        dictionary[dictionary.Count - 1].Add(dx[rangeM2 - 1]);
                        rangeM1 = rangeM2;
                    }
                }
                else
                {
                    dictionary.Add(new List<double>());
                    int sum = 0;
                    for (int j = rangeM2; j < X.Length; j++)
                    {
                        sum += (j + 1);
                    }
                    for (int j = rangeM2; j < X.Length; j++)
                    {
                        dx[j] = (sum / (double)(X.Length - rangeM2));
                    }
                    dictionary[dictionary.Count - 1].Add(X[i]);
                    dictionary[dictionary.Count - 1].Add(dx[dx.Length - 1]);
                }
            }
            bool flag;
            int k = 0;
            for (int i = 0; i < X.Length; i++)
            {
                flag = true;
                while (flag)
                {
                    if (dictionary[k][0] == Xr[i])
                    {
                        dxe[i] = dictionary[k][1];
                        flag = false;
                        k = 0;
                    }
                    else
                    {
                        k++;
                    }
                }
            }
            return dxe;
        }

        public static double Calculate(double[] X, double[] Y)
        {
            double[] dx;
            double[] dy;
            dx = Range(X);
            dy = Range(Y);
            double sum = 0;
            for (int i = 0; i < dx.Length; i++)
            {
                sum += (dx[i] - dy[i]) * (dx[i] - dy[i]);
            }
            double A = 0;
            double B = 0;

            int countx = 1;
            int county = 1;
            for (int i = 1; i < dx.Length; i++)
            {
                if (dx[i - 1] == dx[i])
                    countx++;
                else
                {
                    A += Math.Pow(countx, 3) - countx;
                    countx = 1;
                }
                if (i == dx.Length - 1)
                {
                    A += Math.Pow(countx, 3) - countx;
                }
            }
            A /= 12;
            for (int i = 1; i < dy.Length; i++)
            {
                if (dy[i - 1] == dy[i])
                    county++;
                else
                {
                    B += Math.Pow(county, 3) - county;
                    county = 1;
                }
                if (i == dy.Length - 1)
                {
                    B += Math.Pow(county, 3) - county;
                }
            }
            B /= 12;


            double p = 1 - (double)(((6 * sum) + A + B) / (Math.Pow(X.Length, 3) - X.Length));
            return p;
        }
    }
}
