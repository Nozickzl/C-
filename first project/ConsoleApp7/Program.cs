using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using MeasureOfRisk;
using Option;
using System.IO;
using System.Linq;



static class Constants
{
    public const double Pi = 3.14159;
}
namespace Program
{
    public class MyClass
    {
        static void Main(string[] args)
        {
            var path = @"C:\Users\Zhilin Cheng\Desktop\C++\AMZN.csv";
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();
                string Name = null;
                string[] stock1 = new string[106];

                for (int i = 0; i < stock1.Length; i++)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    Name = fields[0];
                    stock1[i] = Name;
                }

                double[] stock = new double[106];
                for (int i = 0; i < stock.Length; i++)
                {
                    stock[i] = double.Parse(stock1[i]);
                }


                Console.WriteLine("The volatility of the stock is :");
                double Vol = MeasureOfRisk.Volatility.Vol(stock);
                Console.WriteLine(Vol);                        //calculate Vol;

                Console.WriteLine("The value at risk of the stock is :");
                double alpha = 0.05;
                double VaR = MeasureOfRisk.ValueAtRisk.VaR(stock, alpha);
                Console.WriteLine(VaR);                       //calculate VaR

                Console.WriteLine("The option price estimated by Monte Carlo will be:");
                double rf = 0.0249;
                double q = 0.01;
                double S0 = stock[0];
                double sigma = 0.005;
                double t = 1;
                double K = 1600;
                int m = 252;
                long n = 10;
                Boolean call =true ;
                double[] S = Option.StockPrice.GBM(S0, rf, q, sigma, t, m, n);
                double optionPrice = 0;
                if(call==true)
                {
                    optionPrice = Option.OptionPrice.Call(S, rf, t, K, m, n);
                }
                else
                {
                    optionPrice = Option.OptionPrice.Put(S, rf, t, K, m, n);

                }
                Console.WriteLine(S);                        //predict price of option
            }
        }
    }
}
namespace MeasureOfRisk
{
    public class Average
    {
        public static double Avg(double[] v)
        {
            double sum = 0;
            double avg;
            for (int i = 0; i < v.Length; i++)
            {
                sum += v[i];
            }
            avg = sum / v.Length;
            return avg;
        }
    }
    public class Volatility
    {
        public static double Vol(double[] v)
        {
            double sum = 0;
            double avg = MeasureOfRisk.Average.Avg(v);
            for (int i = 0; i < v.Length; i++)
            {
                double temp = Math.Pow((v[i] - avg),2);
                sum += temp;
            }
            double vol = Math.Pow((sum / (v.Length - 1)) , 0.5);
            return vol;
        }
    }
    public class ValueAtRisk
    {
        public static double VaR(double[] v, double alpha)
        {
            Array.Sort(v);
            int n = (int) Math.Round(v.Length * alpha,0);
            if (n == 0) return v[0];
            else return v[n - 1];
        }
    }

}
namespace Option
{

    public class StockPrice
    {
        static public double RandomGauss()
        {
            Random rand = new Random(); //reuse this if you are generating many
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        }
        static public double[] GBM(double s0, double rf, double q, double sigma, double t, int m, long n)  // use Geometric Brownian motion to do the simulation and get the stock price 
        {
            double dT = t / m;
            double[,] s = new double[m + 1, n];
            double[] stockPrice = new double[n];
            for (int j = 0; j < n; j++)
            {
                s[0, j] = s0;
                for (int i = 0; i < m + 1; i++)
                {
                    double Z = StockPrice.RandomGauss();
                    s[i, j] = s[i - 1, j] * Math.Exp((rf - q - 0.5 * sigma *sigma) * dT + sigma * Math.Sqrt(dT) * Z);
                }
                stockPrice[j] = s[m + 1, j];
            }
            return stockPrice;
        }
    }
    public class OptionPrice
    {
        static public double Call(double[] s, double rf, double t, double k, int m, long n)
        {
            double[] optionValue = new double[n];
            for (int j = 0; j < n; j++)
            {
                optionValue[j] = Math.Max(s[j] - k, 0);
            }
            double optionPrice = MeasureOfRisk.Average.Avg(optionValue);
            return optionPrice;
        }
        static public double Put(double[] s, double rf, double t, double k, int m, long n)
        {
            double[] optionValue = new double[n];
            for (int j = 0; j < n; j++)
            {
                optionValue[j] = Math.Max(k - s[j], 0);
            }
            double optionPrice = MeasureOfRisk.Average.Avg(optionValue);
            return optionPrice;
        }
    }
}
