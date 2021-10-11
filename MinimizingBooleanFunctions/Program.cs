using System;
using System.Collections.Generic;

namespace MinimizingBooleanFunctions
{
    class Program
    {
        static int[] vector = new int[16];
        static int[][] truthTable = new int[16][];
        static List<Constituent> function = new List<Constituent>();
        static List<Constituent> DNF;

        static void Main(string[] args)
        {
            // input
            InputVector();
            CreateTruthTable();
            PrintTruthTable();

            // DNF
            CreateDNF();
            PrintFunction(function);
            DNF = function;

            // abbreviated DNF
            CreateADNF();
        }

        static void InputVector()
        {
            Console.WriteLine("Введите вектор");
            for (int i = 0; i < vector.Length; i++)
            {
                do
                {
                    Console.WriteLine($"Введите элемент {i + 1}");
                } while (!int.TryParse(Console.ReadLine(), out vector[i]) || !(vector[i] == 0 || vector[i] == 1));
            }
        }

        static void CreateTruthTable()
        {
            for (int x1 = 0; x1 < 2; x1++)
            {
                for (int x2 = 0; x2 < 2; x2++)
                {
                    for (int x3 = 0; x3 < 2; x3++)
                    {
                        for (int x4 = 0; x4 < 2; x4++)
                        {
                            int[] row = { x1, x2, x3, x4, vector[x1 * 8 + x2 * 4 + x3 * 2 + x4] };
                            truthTable[x1 * 8 + x2 * 4 + x3 * 2 + x4] = row;
                        }
                    }
                }
            }
        }

        static void PrintTruthTable()
        {
            Console.WriteLine("Таблица истиности");
            Console.WriteLine(" x | y | z | w | F ");
            Console.WriteLine("-------------------");
            for (int i = 0; i < truthTable.Length; i++)
            {
                int[] row = truthTable[i];
                Console.WriteLine($" {row[0]} | {row[1]} | {row[2]} | {row[3]} | {row[4]}");
            }
        }

        // DNF = disjunctive normal form
        static void CreateDNF()
        {
            for (int i = 0; i < truthTable.Length; i++)
            {
                if (truthTable[i][4] == 1)
                {
                    int[] row = truthTable[i];
                    function.Add(new Constituent(row[0], row[1], row[2], row[3]));
                }
            }
        }

        // ADNF = abbreviated disjunctive normal form
        static void CreateADNF()
        {
            Console.WriteLine("====\nСкДНФ");

            bool mergeHappened = true;

            while (mergeHappened)
            {
                mergeHappened = false;

                SortedSet<int> usedElements = new SortedSet<int>();

                // merging
                for (int i = function.Count - 1; i > 0; i--)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (function[i].IsMerge(function[j]))
                        {
                            mergeHappened = true;

                            usedElements.Add(i);
                            usedElements.Add(j);

                            function.Add(function[i].Merge(function[j]));
                        }
                    }
                }

                // delete used constituents
                while (usedElements.Count > 0)
                {
                    function.RemoveAt(usedElements.Max);
                    usedElements.Remove(usedElements.Max);
                }

                if (mergeHappened)
                    PrintFunction(function);

                // to do
                // delete equal elements
            }
        }

        static void PrintFunction(List<Constituent> function)
        {
            string res = "f(x, y, z, w) = ";

            if (function.Count > 0)
            {
                for (int i = 0; i < function.Count; i++)
                {
                    res += function[i].ToString() + " \\/ ";
                }
                res = res.Remove(res.Length - 3);                
            }

            Console.WriteLine(res);
        }
    }

    class Constituent : IConstituent
    {
        /*
        contain constituent in text format xyzw
        0 - false
        1 - true
        2 - element not included into constituent
        */
        private string con = "2222";
        public int Length;

        public Constituent(string stringFormat)
        {
            con = stringFormat;
            Length = (con[0] != '2' ? con[0] : 0) + (con[1] != '2' ? con[1] : 0) + (con[2] != '2' ? con[2] : 0) + (con[3] != '2' ? con[3] : 0);
        }

        public Constituent(int x, int y, int z, int w)
        {
            con = x.ToString() + y.ToString() + z.ToString() + w.ToString();
            Length = x != 2 ? 1 : 0 + y != 2 ? 1 : 0 + z != 2 ? 1 : 0 + w != 2 ? 1 : 0;
        }

        public string GetInteriorFormat()
        {
            return con;
        }

        public bool IsMerge(Constituent constituent)
        {
            // only to equal number of elements
            if (this.Length != constituent.Length) return false;

            int sum = int.Parse(con) + int.Parse(constituent.GetInteriorFormat());
            /*
            In theory, if sum of interior forms have only one "1" and one odd number,
            then merging can be completed. It means we can check sum == 1 => merging can be completed.
            */
            int oddSum = 0;
            while (sum > 0)
            {
                if (sum % 2 == 1)
                    oddSum += sum % 10;
                sum /= 10;
            }

            return oddSum == 1;
        }

        public Constituent Merge(Constituent constituent)
        {
            if (!IsMerge(constituent)) throw new Exception("Merging can't be completed");

            string con2 = constituent.GetInteriorFormat();
            int[] op1 = { int.Parse(con[0].ToString()), int.Parse(con[1].ToString()), int.Parse(con[2].ToString()), int.Parse(con[3].ToString()) };
            int[] op2 = { int.Parse(con2[0].ToString()), int.Parse(con2[1].ToString()), int.Parse(con2[2].ToString()), int.Parse(con2[3].ToString()) };
            int[] res = new int[4];

            for (int i = 0; i < 4; i++)
            {
                res[i] = op1[i] + op2[i] - 1;
            }

            // return to normal form
            for (int i = 0; i < 4; i++)
            {
                if (res[i] == 0 || res[i] == 3)
                {
                    res[i] = 2;
                }
                else if (res[i] == -1)
                {
                    res[i] = 0;
                }
            }

            return new Constituent(res[0], res[1], res[2], res[3]);
        }
        
        public override string ToString()
        {
            string res = "";

            if (con[0] != '2')
            {
                if (con[0] == '0')
                    res += '\'';
                res += 'x';                
            }
            if (con[1] != '2')
            {
                if (con[1] == '0')
                    res += '\'';
                res += 'y';                
            }
            if (con[2] != '2')
            {
                if (con[2] == '0')
                    res += '\'';
                res += 'z';                
            }
            if (con[3] != '2')
            {
                if (con[3] == '0')
                    res += '\'';
                res += 'w';                
            }

            return res;
        }
    }

    interface IConstituent
    {
        public bool IsMerge(Constituent constituent);

        public Constituent Merge(Constituent constituent);
    }
}
