using System;
using System.Globalization;

namespace Lab01
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var sm = new SMCalculator();
        }
    }

    public class SMCalculator
    {
        private int AmountOfFreeVariables { get; set; }
        private int AmountOfBasisVariables { get; set; }
        private int SystemPartsAmount { get; set; }
        private double[,] _systemCoefs;
        private double[,] _tableData;
        private double[] _goalFunctionCoefs;
        private int _tableRowCount;
        private int _tableColumnCount;
        private int _tableCount = 0;

        public SMCalculator()
        {
            PrintInstruction();
            if (GetDataFromUser())
            {
                PrintSystem();
                PrintGoalFunction();
                CreateTable();
                PrintSt();
                CalculateAllTables();
            }
            else
            {
                Console.WriteLine("Can`t get data from user! Wrong value!");
            }
            

        }
        public SMCalculator(int amountOfFreeVariables, int systemPartsAmount)
        {
            AmountOfFreeVariables = amountOfFreeVariables;
            SystemPartsAmount = systemPartsAmount;
            AmountOfBasisVariables = SystemPartsAmount;
            _systemCoefs = new double[SystemPartsAmount, AmountOfFreeVariables + AmountOfBasisVariables + 1];
        }

        private static bool IsGoodString(string? str)
        {
            if (str != null)
                for (var i = 0; i < str.Length; i++)
                {
                    if (!char.IsDigit(str.ElementAt(i)) && !char.IsWhiteSpace(str.ElementAt(i)) &&
                        Convert.ToChar(str.ElementAt(i)) != '-')
                        return false;
                }

            return true;
        }

        public bool GetDataFromUser()
        {
            Console.Write("1. Enter row count: ");
            var str1 = Console.ReadLine();
            if (IsGoodString(str1))
            {
                SystemPartsAmount = Convert.ToInt32(str1);
            }
            else
            {
                Console.WriteLine("ERROR! Wrong value!");
                return false;
            }
            Console.Write("2. Enter column count: ");
            str1 = Console.ReadLine();
            if (IsGoodString(str1))
            {
                AmountOfFreeVariables = Convert.ToInt32(str1);
            }
            else
            {
                Console.WriteLine("ERROR! Wrong value!");
                return false;
            }
            PrepareToWork();
            Console.WriteLine("3. Enter coefficients of your system WITHOUT right sides:");
            for (var i = 0; i < SystemPartsAmount; i++)
            {
                var res = Console.ReadLine();
                if (IsGoodString(res))
                {
                    var coefs = res?.Split(" ");
                    if (coefs == null) continue;
                    for (var j = 0; j < coefs.Length; j++)
                    {
                        _systemCoefs[i, j] = Convert.ToDouble(coefs[j]);
                    }
                }
                else
                {
                    Console.WriteLine("ERROR! Wrong value!");
                    return false;
                }
                
            }
            Console.WriteLine("4. Enter values of right sides of system:");
            var str = Console.ReadLine();
            string[]? strArr;
            if (IsGoodString(str))
            {
                strArr = str?.Split(" ");
                if (strArr != null)
                    for (var i = 0; i < strArr.Length; i++)
                    {
                        _systemCoefs[i, AmountOfBasisVariables + AmountOfFreeVariables] = Convert.ToInt32(strArr[i]);
                    }
            }
            else
            {
                Console.WriteLine("ERROR! Wrong value!");
                return false;
            }
            

            Console.WriteLine("5. Enter your goal function coefficients:");
            str = Console.ReadLine();
            if (IsGoodString(str))
            {
                strArr = str?.Split(" ");
                _goalFunctionCoefs = new double[AmountOfFreeVariables];
                if (strArr != null)
                {
                    for (var i = 0; i < strArr.Length; i++)
                    {
                        _goalFunctionCoefs[i] = Convert.ToInt32(strArr[i]);
                    }
                }
                else
                {
                    Console.WriteLine("ERROR! Wrong value!");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("ERROR! Wrong value!");
                return false;
            }
            return true;
        }

        public void PrintInstruction()
        {
            Console.WriteLine("1. Enter row count!");
            Console.WriteLine("2. Enter columns count!");
            Console.WriteLine("3. Enter coefficients of your system WITHOUT right sides!");
            Console.WriteLine("Example: ");
            Console.WriteLine("Your system: ");
            Console.WriteLine("1*x1 <= 30");
            Console.WriteLine("1*x2 <= 70");
            Console.WriteLine("3*x1 + 4*x2 <= 120");
            Console.WriteLine();
            Console.WriteLine("You enter:");
            Console.WriteLine("1 0");
            Console.WriteLine("0 1");
            Console.WriteLine("3 4");
            Console.WriteLine("REMEMBER! After line of coefficients you should press 'Enter' to start nem line!");
            Console.WriteLine("4. Enter values of right sides of system!");
            Console.WriteLine("Example: ");
            Console.WriteLine("30 70 120");
            Console.WriteLine("5. Enter your goal function coefficients!");
            Console.WriteLine("Example: ");
            Console.WriteLine("Your goal function:");
            Console.WriteLine("1*x1 + 3*x2 - 4*x3");
            Console.WriteLine("You should enter: ");
            Console.WriteLine("1 3 -4");
        }

        public void PrepareToWork()
        {
            AmountOfBasisVariables = SystemPartsAmount;
            _systemCoefs = new double[SystemPartsAmount, AmountOfFreeVariables + AmountOfBasisVariables + 1];
            for (var i = 0; i < SystemPartsAmount; i++)
            {
                for (var j = AmountOfFreeVariables; j < AmountOfFreeVariables + AmountOfBasisVariables; j++)
                {
                    if (j - AmountOfFreeVariables == i)
                        _systemCoefs[i, j] = 1;
                }
            }
        }

        public void PrintSystem()
        {
            for (var i = 0; i < SystemPartsAmount; i++)
            {
                var xIndex = 1;
                for (var j = 0; j < AmountOfFreeVariables + AmountOfBasisVariables + 1; j++)
                {
                    if (j + 1 == AmountOfFreeVariables + AmountOfBasisVariables + 1)
                    {

                        Console.Write(" = " + _systemCoefs[i, j] + "\n");
                    }
                    else
                    {
                        if (_systemCoefs[i, j] != 0)
                        {
                            Console.Write(_systemCoefs[i, j] + "*x" + xIndex);
                            if (xIndex + 1 <= AmountOfFreeVariables + i + 1)
                                Console.Write(" + ");
                        }
                    }
                    xIndex++;
                }
            }
        }

        private void CalculateAllTables()
        {
            while (IsFinalTable() == false)
            {
                CalculateNextTable();
                PrintSt();
            }

            PrintResult();
        }

        private void PrintResult()
        {
            double q = 0;
            var x_arr = new double[AmountOfFreeVariables];
            for (var i = 0; i < x_arr.Length; i++)
                x_arr[i] = 0;
            for (var i = 1; i < _tableRowCount - 1; i++)
            {
                if (_tableData[i, 0] <= AmountOfFreeVariables && _tableData[i, 0] > 0)
                    x_arr[(int)_tableData[i, 0] - 1] = _tableData[i, 2];
                q += _tableData[i, 1] * _tableData[i, 2];
            }
            
            Console.WriteLine("Q = " + q);
            Console.Write("X* = (");
            for (var i = 0; i < x_arr.Length; i++)
            {
                if(i+1 == x_arr.Length)
                    Console.Write(x_arr[i]);
                else
                    Console.Write(x_arr[i] + ", ");
            }

            Console.Write(")\n");
        }

        private bool IsFinalTable()
        {
            for (var i = 3; i < _tableColumnCount; i++)
            {
                if(_tableData[_tableRowCount - 1, i] < 0)
                    return false;
            }
            return true;
        }

        private void CalculateNextTable()
        {
            var keyCol = FindKeyColumn();
            var keyRow = FindKeyRow();
            var keyValue = _tableData[keyRow, keyCol];
            var newTable = new double[_tableRowCount, _tableColumnCount];
            //Copying table
            for (var i = 0; i < _tableRowCount; i++)
            {
                for (var j = 0; j < _tableColumnCount; j++)
                {
                    newTable[i, j] = _tableData[i, j];
                }
            }

            newTable[keyRow, 0] = keyCol - 2;
            //Filling key value row and col
            for (var i = 2; i < _tableColumnCount-1; i++)
            {
                newTable[keyRow, i] = _tableData[keyRow, i] / keyValue;
            }
            for (var i = 1; i < _tableRowCount - 1; i++)
            {
                newTable[i, keyCol] = 0;
            }
            newTable[keyRow, keyCol] = 1;
            //Filling rest of table
            for (int i = 1; i < _tableRowCount-1; i++)
            {
                for (int j = 2; j < _tableColumnCount-1; j++)
                {
                    if(i != keyRow && j != keyCol)
                        newTable[i, j] = _tableData[i, j] - ((_tableData[keyRow, j] * _tableData[i, keyCol]) / keyValue);
                }
            }
            //Filling basis cols
            for (int i = 1; i < _tableRowCount - 1; i++)
            {
                FillBasisCol((int)newTable[i, 0] + 2, newTable);
            }
            //Filling Cb col
            newTable[keyRow, 1] = _tableData[0, keyCol];
            //Filling last row
            for (var i = 3; i < _tableColumnCount - 1; i++)
            {
                double sum = 0;
                for (var j = 1; j < _tableRowCount - 1; j++)
                {
                    sum += newTable[j, 1] * newTable[j, i];
                }

                newTable[_tableRowCount - 1, i] = sum - newTable[0, i];
            }
            //Filling last col
            keyCol = FindKeyColumn(newTable);
            for (var i = 1; i < _tableRowCount - 1; i++)
            {
                newTable[i, _tableColumnCount - 1] = newTable[i, 2] / newTable[i, keyCol];
            }

            for (var i = 0; i < _tableRowCount; i++)
            {
                for (var j = 0; j < _tableColumnCount; j++)
                {
                    _tableData[i, j] = newTable[i, j];
                }
            }
            _tableData = newTable;
        }

        private void FillBasisCol(int col, double[,] table)
        {
            for (var i = 1; i < _tableRowCount - 1; i++)
            {
                if (table[i, 0] == (col - 2))
                {
                    table[i, col] = 1;
                }
                else
                {
                    table[i, col] = 0;
                }
            }
        }
        private int FindKeyColumn()
        {
            var pos = 0;
            double min = 0;
            for (var i = 3; i < _tableColumnCount; i++)
            {
                if (_tableData[_tableRowCount - 1, i] < min)
                {
                    min = _tableData[_tableRowCount - 1, i];
                    pos = i;
                }
            }

            return pos;
        }
        private int FindKeyColumn(double[,] table)
        {
            var pos = 0;
            double min = 0;
            for (var i = 3; i < _tableColumnCount; i++)
            {
                if (table[_tableRowCount - 1, i] < min)
                {
                    min = table[_tableRowCount - 1, i];
                    pos = i;
                }
            }

            return pos;
        }
        private int FindKeyRow()
        {
            var pos = 0;
            double min = double.MaxValue;
            for (var i = 1; i < _tableRowCount-1; i++)
            {
                if(_tableData[i, _tableColumnCount - 1] > 0)
                    if (_tableData[i, _tableColumnCount - 1] < min)
                    {
                        min = _tableData[i, _tableColumnCount - 1];
                        pos = i;
                    }
            }

            return pos;
        }

        private void PrintSt()
        {
            _tableCount++;
            Console.WriteLine("ST-" + _tableCount);
            PrintLine();
            Console.Write("|       |       |       |");
            for (var i = 1; i < AmountOfFreeVariables + AmountOfBasisVariables + 1; i++)
            {
                var charNum = 1 + i.ToString().Length + 3 + _tableData[0, i + 2].ToString(CultureInfo.InvariantCulture).Length;
                var spaceNum = 0;
                if(charNum < 7)
                    spaceNum = 7 - charNum;
                Console.Write("c" + i + " = " + _tableData[0, i + 2]);
                if(spaceNum != 0)
                    for (var j = 0; j < spaceNum; j++)
                        Console.Write(" ");
                Console.Write("|");
            }
                
            Console.Write("\n");
            PrintLine();
            Console.Write("|Xb     |Cb     |P0     |");
            for (var i = 1; i < AmountOfFreeVariables + AmountOfBasisVariables + 1; i++)
            {
                Console.Write("P" + i + "     |");
            }
            Console.Write("\n");
            PrintLine();
            for (int i = 1; i < _tableRowCount - 1; i++)
            {
                for (var j = 0; j < _tableColumnCount; j++)
                {
                    if(j == 0)
                        Console.Write("|X" + _tableData[i, j] + "     |");
                    else
                    {
                        var numLength = _tableData[i, j].ToString(CultureInfo.InvariantCulture).Length;
                        var spaceCount = 7 - numLength;
                        Console.Write(_tableData[i,j]);
                        for (var k = 0; k < spaceCount; k++)
                        {
                            Console.Write(" ");
                        }
                        Console.Write("|");
                    }
                }
                Console.WriteLine();
                PrintLine();
            }
            //Printing last row
            Console.Write("|   Q   |   =   |");
            var nl = _tableData[_tableRowCount-1, 2].ToString(CultureInfo.InvariantCulture).Length;
            var sc = 7 - nl;
            Console.Write(_tableData[_tableRowCount - 1, 2]);
            for (var k = 0; k < sc; k++)
            {
                Console.Write(" ");
            }
            Console.Write("|");
            for (var j = 3; j < _tableColumnCount; j++)
            {
                    var numLength = _tableData[_tableRowCount - 1, j].ToString(CultureInfo.InvariantCulture).Length;
                    var spaceCount = 7 - numLength;
                    Console.Write(_tableData[_tableRowCount - 1, j]);
                    for (var k = 0; k < spaceCount; k++)
                    {
                        Console.Write(" ");
                    }
                    Console.Write("|");
            }
            Console.WriteLine();
            PrintLine();
            int row = FindKeyRow();
            int col = FindKeyColumn();
            Console.WriteLine("Key value = " + _tableData[row, col] + " Position: X" + _tableData[row, 0] + ", P" + (col - 2));
        }

        private void PrintLine()
        {
            for (var i = 0; i < _tableColumnCount; i++)
            {
                Console.Write(i + 1 == _tableColumnCount ? "_________" : "________");
            }
            Console.WriteLine();
        }

        public void PrintGoalFunction()
        {
            Console.Write("Q = ");
            for (var i = 0; i < AmountOfFreeVariables; i++)
            {
                if (i + 1 == AmountOfFreeVariables)
                {
                    Console.Write(_goalFunctionCoefs[i] + "*x" + i + " = ");
                }
                else
                {
                    Console.Write(_goalFunctionCoefs[i] + "*x" + i + " + ");
                }
            }
            Console.WriteLine();
        }

        public void CreateTable()
        {
            _tableColumnCount = 3 + AmountOfBasisVariables + AmountOfFreeVariables + 1;
            _tableRowCount = 1 + SystemPartsAmount + 1;
            _tableData = new double[_tableRowCount, _tableColumnCount];
            //Filling first column with basis variables
            for (var i = 1; i < _tableRowCount - 1; i++)
            {
                _tableData[i, 0] = AmountOfFreeVariables + i;
            }
            //Filling first row
            for (var i = 3; i < _tableColumnCount; i++)
            {
                if (i - 3 < AmountOfFreeVariables)
                    _tableData[0, i] = _goalFunctionCoefs[i - 3];
                else
                    _tableData[0, i] = 0;
            }
            //Filling last row
            for (var i = 3; i < _tableColumnCount; i++)
            {
                if(_tableData[0, i] != 0)
                    _tableData[_tableRowCount - 1, i] = _tableData[0, i] * -1;
                else
                {
                    _tableData[_tableRowCount - 1, i] = 0;
                }
            }
            //Filling third column 
            for (var i = 1; i < _tableRowCount - 1; i++)
            {
                _tableData[i, 2] = _systemCoefs[i-1, AmountOfFreeVariables + AmountOfBasisVariables];
            }
            //Filling other parts of table
            for (var i = 1; i < _tableRowCount - 1; i++)
            {
                for (var j = 3; j < _tableColumnCount; j++)
                {
                    if (j - 3 < AmountOfFreeVariables)
                        _tableData[i, j] = _systemCoefs[i - 1, j - 3];
                    else
                    {
                        if (j - 2 == (int)_tableData[i, 0])
                        {
                            _tableData[i, j] = 1;
                        }
                        else
                        {
                            _tableData[i, j] = 0;
                        }
                    }

                }
            }
            //Filling last column
            int colNum = FindKeyColumn();
            for (int i = 1; i < _tableRowCount - 1; i++)
            {
                _tableData[i, _tableColumnCount - 1] = _tableData[i, 2] / _tableData[i, colNum];
            }
        }

        //public void FillSecondColumn()
        //{
        //    for (var i = 1; i < _tableRowCount; i++)
        //    {
        //        _tableData[i, 1] = _tableData[0, (int)_tableData[i, 0] + 2];
        //    }
        //}
        //public void SetResultColumnToSystem(double[] results)
        //{
        //    for (var i = 0; i < SystemPartsAmount; i++)
        //    {
        //        _systemCoefs[i, AmountOfBasisVariables + AmountOfFreeVariables] = results[i];
        //    }
        //}
        //public void SetAmountOfNonBasisVars(int count)
        //{
        //    AmountOfFreeVariables = count;
        //}
        //public void SetAmountOfSystemParts(int count)
        //{
        //    SystemPartsAmount = count;
        //}
    }
}
