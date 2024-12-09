using System.ComponentModel.Design;

namespace Lab03;

public class HomoryCalculator
{
    private SMCalculator _smc;
    private double[,] _pnTableAfterSM;
    private double[] _goalCoefsAfterSM;
    private List<double> _goalCoefsAfterDSM;
    private int _ybColCountForDSM;
    private int _additionalVarsForDSM;
    private List<int> _ybColForDSM;
    private List<double> _p0ColForDsm;
    private List<double> _zRowForDSM;
    private List<List<double>> _pnTableForDSM;
    private int basicNum;
    private DsmCalculator _dsm;

    public HomoryCalculator()
    {
        SMCalculator smCalculator = new SMCalculator();
        _smc = smCalculator;
        PrepareDataAfterSM();
        DsmCalculator dsmCalculator = new DsmCalculator(_pnTableAfterSM, _goalCoefsAfterSM, _additionalVarsForDSM, _ybColForDSM, _p0ColForDsm, _zRowForDSM);
        _dsm = dsmCalculator;
        while (!CheckIsFinal())
        {
            PrepareDataAfterDSM();
            dsmCalculator = new DsmCalculator(_pnTableAfterSM, _goalCoefsAfterSM, _additionalVarsForDSM, _ybColForDSM, _p0ColForDsm, _zRowForDSM);
            _dsm = dsmCalculator;
        }
    }

    private bool CheckIsFinal()
    {
        List<double> results = _dsm.GetP0Col();
        foreach (var el in results)
        {
            if (el % 1 > 0.001)
                return false;
        }

        return true;
    }
    public void PrepareDataAfterDSM()
    {
        List<List<double>> tableData = _dsm.GetPnTAble();
        int tableRowsCount_new = tableData.Count + 1;
        int tableColumnsCount_new = tableData[0].Count + 1;
        _pnTableForDSM = [];
        for (var i = 0; i < tableRowsCount_new; i++)
        {
            _pnTableForDSM.Add([]);
            for (var j = 0; j < tableColumnsCount_new; j++)
            {
                if(j + 1 < tableColumnsCount_new && i+1 < tableRowsCount_new)
                    _pnTableForDSM[i].Add(tableData[i][j]);
                else
                {
                    _pnTableForDSM[i].Add(0);
                }
            }
        }
        
        _zRowForDSM = _dsm.GetZRow();
        _zRowForDSM.Add(0);
        List<double> p0col = _dsm.GetP0Col();
        List<double> limitationCoefs = GetNewLimitationCoefsAfterDSM(tableData, p0col);
        for (var i = 0; i < tableColumnsCount_new; i++)
        {
            _pnTableForDSM[_pnTableForDSM.Count - 1][i] = limitationCoefs[i+1];
        }
        p0col.Add(limitationCoefs[0]);

        _additionalVarsForDSM = _smc.GetCountOfAdditionalVars() + 1;

        _goalCoefsAfterDSM = _dsm.GetCnRow();
        _goalCoefsAfterDSM.Add(0);
        //Getting ybCol & Getting _p0Col
        _ybColForDSM = _dsm.GetYbCol();
        _p0ColForDsm = p0col;
        _ybColForDSM.Add(basicNum);

    }
    public List<double> GetNewLimitationCoefsAfterDSM(List<List<double>> table, List<double> p0Col)
    {
        double fractional_part = 0;
        int row = 0;
        for (var i = 0; i < p0Col.Count; i++)
        {
            if (p0Col[i] / (double)10 > fractional_part)
            {
                fractional_part = p0Col[i] / (double)10;
                row = i;
            }
        }

        List<double> limitationCoefs = [];
        limitationCoefs.Add(p0Col[row]);
        for (var i = 0; i < table[0].Count + 1; i++)
        {
            if (i + 1 == table[0].Count + 1)
            {
                limitationCoefs.Add(1);
            }
            else
                limitationCoefs.Add(table[row][i] * -1);
        }

        basicNum = table[0].Count;
        return ConvertToIntegerCoefficients(limitationCoefs);
    }

    public void PrepareDataAfterSM()
    {
        double[,] tableData = _smc.GetTableData();
        int tableRowsCount_new = _smc.GetTableRowsCount() + 1;
        int tableColumnsCount_new = _smc.GetTableColumnCount() + 1;
        double[,] pnTable = new double[tableRowsCount_new, tableColumnsCount_new];
        for (var i = 0; i < tableRowsCount_new; i++)
        {
            for (var j = 0; j < tableColumnsCount_new; j++)
            {
                if (i + 1 < tableRowsCount_new && j + 1 != tableColumnsCount_new)
                {
                    pnTable[i, j] = tableData[i+1, j+3];
                }
                else
                {
                    pnTable[i, j] = 0;
                }
            }
        }

        _zRowForDSM = [];
        for (var i = 0; i < tableColumnsCount_new; i++)
        {
            _zRowForDSM.Add(tableData[tableData.GetLength(0) - 1, i+3]);
        }

        double[] limitationCoefs = GetNewLimitationCoefs(tableData, tableRowsCount_new, tableColumnsCount_new);
        for (var i = 0; i < tableColumnsCount_new; i++)
        {
            pnTable[tableRowsCount_new-1, i] = limitationCoefs[i + 1];
        }

        _pnTableAfterSM = pnTable;
        _additionalVarsForDSM = _smc.GetCountOfAdditionalVars() + 1;
        
        _goalCoefsAfterSM = new double[_additionalVarsForDSM + _smc.GetCountOfFreeVars()];
        for (var i = 0; i < _additionalVarsForDSM + _smc.GetCountOfFreeVars(); i++)
        {
            if(i + 1 == _additionalVarsForDSM + _smc.GetCountOfFreeVars() + 1)
                _goalCoefsAfterSM[i] = 0;
            else
                _goalCoefsAfterSM[i] = tableData[0, i + 3];
        }
        //Getting ybCol & Getting _p0Col
        _ybColForDSM = [];
        _p0ColForDsm = [];
        for (var i = 0; i < tableData.GetLength(0) - 2; i++)
        {
            _ybColForDSM.Add((int)tableData[i+1, 0]);
            _p0ColForDsm.Add(tableData[i + 1, 2]);
        }
        _p0ColForDsm.Add(limitationCoefs[0]);
        _ybColForDSM.Add(basicNum);
        
    }

    public double[] GetNewLimitationCoefs(double[,] table, int rows, int cols)
    {
        double fractional_part = 0;
        int row = 0;
        for (var i = 1; i < rows; i++)
        {
            if (table[i, 3] / (double)10 > fractional_part)
            {
                fractional_part = table[i, 2] / (double)10;
                row = i;
            }
        }

        double[] limitationCoefs = new double[cols + 1];
        for (var i = 0; i < limitationCoefs.Length; i++)
        {
            if (i + 1 == limitationCoefs.Length)
            {
                limitationCoefs[i] = 1;
            }
            else
                limitationCoefs[i] = table[row, i+2] * -1;
        }

        basicNum = table.GetLength(1) - 4 + 1;
        return ConvertToIntegerCoefficients(limitationCoefs);
    }
    static List<double> ConvertToIntegerCoefficients(List<double> coefficients)
    {
        // Знаходимо знаменники дробових частин всіх чисел
        List<int> denominators = coefficients
            .Select(coef => GetDenominator(coef))
            .ToList();

        // Знаходимо найменше спільне кратне знаменників
        int lcm = denominators.Aggregate(1, (current, denom) => Lcm(current, denom));

        // Множимо всі коефіцієнти на НСК
        return coefficients.Select(coef => Math.Round(coef * lcm)).ToList();
    }
    
    static double[] ConvertToIntegerCoefficients(double[] coefficients)
    {
        // Знаходимо знаменники дробових частин всіх чисел
        int[] denominators = coefficients
            .Select(coef => GetDenominator(coef))
            .ToArray();

        // Знаходимо найменше спільне кратне знаменників
        int lcm = denominators.Aggregate(1, (current, denom) => Lcm(current, denom));

        // Множимо всі коефіцієнти на НСК
        return coefficients.Select(coef => Math.Round(coef * lcm)).ToArray();
    }

    static int GetDenominator(double value)
    {
        // Преобразуем в дробь с использованием точного формата
        decimal dec = (decimal)value;
        int maxDecimalPlaces = 10; // Обмеження на максимальну кількість десяткових знаків

        // Підраховуємо кількість десяткових знаків
        int decimalPlaces = 0;
        while (dec != Math.Round(dec) && decimalPlaces < maxDecimalPlaces)
        {
            dec *= 10;
            decimalPlaces++;
        }

        return (int)Math.Pow(10, decimalPlaces);
    }

    static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    static int Lcm(int a, int b)
    {
        return (a / Gcd(a, b)) * b;
    }
}