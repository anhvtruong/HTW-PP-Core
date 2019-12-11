using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;

namespace OclAspectTest
{
    public static class OclExtensionMethods
    {
        #region Integer Extension Methods
        public static int Max(this int i, int i2) => i > i2 ? i : i2;
        public static int Min(this int i, int i2) => i < i2 ? i : i2;
        public static int Div(this int i, int i2) => i / i2;
        public static int Mod(this int i, int i2) => i % i2;
        public static int Abs(this int i) => Math.Abs(i);
        #endregion

        #region Real Extension Methods
        public static double Max(this double r, double r2) => r > r2 ? r : r2;
        public static double Min(this double r, double r2) => r < r2 ? r : r2;
        public static double Abs(this double r) => Math.Abs(r);
        public static double Floor(this double r) => Math.Floor(r);
        public static double Round(this double r) => Math.Round(r);

        // Not in OCL Grammatik
        public static double Ceiling(this double r) => Math.Ceiling(r);
        #endregion

        #region String Extension Methods
        public static int Size(this string s) => s.Length;
        public static string Concat(this string s, string s2) => string.Concat(s, s2);
        public static int ToInt(this string s)
        {
            _ = Int32.TryParse(s, out int number);
            return number;
        }
        public static double ToReal(this string s)
        {
            double number;
            NumberStyles style = NumberStyles.AllowDecimalPoint;
            CultureInfo cultureEn = CultureInfo.CreateSpecificCulture("en-En");
            CultureInfo cultureFr = CultureInfo.CreateSpecificCulture("fr-Fr");
            if (Double.TryParse(s, style, cultureEn, out number))
                return number;
            else
            {
                _ = Double.TryParse(s, style, cultureFr, out number);
                return number;
            }
        }
        #endregion

        #region List Extension Methods
        public static int Size<T>(this List<T> list) => list.Count;
        public static bool Excludes<T>(this List<T> list, T obj) => !list.Contains(obj);
        public static int Count<T>(this List<T> list, T obj)
        {
            int count = 0;
            foreach (T e in list)
                if (e.Equals(obj))
                    count++;
            return count;
        }
        public static bool IncludesAll<T>(this List<T> list, List<T> list2)
        {
            foreach (var e2 in list2)
                if (!list.Contains(e2))
                    return false;
            return true;
        }
        public static bool ExcludesAll<T>(this List<T> list, List<T> list2)
        {
            foreach (var e2 in list2)
                if (list.Contains(e2))
                    return false;
            return true;
        }
        public static bool IsEmpty<T>(this List<T> list) => list.Count == 0;
        public static bool NotEmpty<T>(this List<T> list) => list.Count > 0;

        // Not finished yet
        public static bool IsUnique<T>(this List<T> list, T obj)
        {
            return true;
        }
        public static List<T> SortedBy<T>(this List<T> list, T obj)
        {
            return list;
        }
        #endregion
    }
}
