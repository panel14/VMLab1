Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("Добро пожаловать в приложение. Введите одну из команд:\n" +
    "-manual_input - для ввода матрицы вручную\n" +
    "-file_input - для ввода данных из файла\n" +
    "-auto_input - для автогенерации матрицы");
while (true)
{
    int dimension = 0;
    double[][]? matrix;
    double[] unknown;

    Console.WriteLine("Введите команду:");

    string? command = Console.ReadLine();
    switch (command)
    {
        case "manual_input":
            Console.WriteLine("Введите размерность матрицы. Размерность матрицы должна быть больше 0");

            dimension = getDimension();

            Console.WriteLine("Введите матрицу (построчно):");
            Func<string?> consoleRead = () => Console.ReadLine();
            matrix = readByUser(dimension);

            if (matrix == null) break;

            execute(matrix, dimension);
            break;

        case "file_input":
            Console.WriteLine("Введите путь к файлу. Чтобы взять матрицу из файла нажмите ENTER\n" +
                "В первой строке файла должно быть одно число - размерность матрицы.");

            string? path;
            while (!checkPath(path = Console.ReadLine()))
                Console.WriteLine("Файла по указанному пути не существует. Повторите ввод:");

            using (var sr = new StreamReader(path))
            {
                Func<string?> fileRead = () => sr.ReadLine();
                matrix = readByFile(fileRead, out dimension);
                if (matrix == null) break;
            }

            execute(matrix, dimension);

            break;

        case "auto_input":
            Console.WriteLine("Введите размерность матрицы. Размерность матрицы должна быть больше 0");

            dimension = getDimension();

            matrix = new double[dimension][];
            unknown = new double[dimension];
            Random random = new Random();

            double[] xValues = new double[dimension];
            for (int i = 0; i < dimension; i++)
            {
                xValues[i] = random.NextDouble() * 10;
            }

            for (int i = 0; i < dimension; i++)
            {
                matrix[i] = new double[dimension + 1];
                for (int j = 0; j < dimension; j++)
                {

                    matrix[i][j] = random.NextDouble() * 10;
                    unknown[i] += xValues[j] * matrix[i][j];
                }
            }
            int inc = 0;
            Array.ForEach(unknown, x => matrix[inc++][dimension] = x);
            execute(matrix, dimension);
            Console.WriteLine("Debug: generated roots:\n" + String.Join("\n", xValues));
            break;

        case "exit":
            return;

        default:
            Console.WriteLine("Неизвестная команда. Повторите ввод.");
            break;
    }

}

Boolean checkMatrixString(string? matrixStr, int dimension)
{
    if (matrixStr == null)
        return false;

    double parseble;
    if (Array.Exists(matrixStr.Trim().Split(" "), x => !Double.TryParse(x, out parseble)))
        return false;
    if (matrixStr.Split(" ").Length != dimension)
        return false;
    return true;
}

int getDimension()
{
    int dimension;
    while (true)
    {
        string? dimStr = Console.ReadLine();
        if (dimStr == null)
        {
            Console.WriteLine("Введите значение:");
            continue;
        }
        if (!Int32.TryParse(dimStr, out dimension) || dimension <= 0)
        {
            Console.WriteLine("Неверный формат ввода. Повторите ввод:");
            continue;
        }
        return dimension;
    }
}

Boolean checkPath(string? path)
{
    if (path == null)
        return false;

    if (!File.Exists(path))
        return false;
    return true;
}

double[][]? readByUser(int dimension)
{
    double[][] matrix = new double[dimension][];
    int i = 0;
    while (i < dimension)
    {
        string? matrixArr;
        while (!checkMatrixString(matrixArr = Console.ReadLine(), dimension))
            Console.WriteLine("Неверный формат ввода. Повторите попытку:");
        double[] currentStr = matrixArr.Trim().Split(" ").Select(Double.Parse).ToArray();

        matrix[i] = currentStr;

        if (checkRowsEquals(matrix, i))
        {
            Console.WriteLine("Определитель данной матрицы равен 0.");
            return null;
        }
        i++;
    }
    if (checkColumnsEquals(matrix, dimension))
    {
        Console.WriteLine("Определитель данной матрицы равен 0.");
        return null;
    }

    Console.WriteLine("Введите свободные члены СЛАУ (в строку):");

    string? unknownVars;
    while (!checkMatrixString(unknownVars = Console.ReadLine(), dimension))
        Console.WriteLine("Неверный формат ввода. Повторите попытку:");
    double[] unknown = unknownVars.Split(" ").Select(Double.Parse).ToArray();
    for (int j = 0; j < matrix.Length; j++)
    {
        Array.Resize(ref matrix[j], dimension + 1);
        matrix[j][dimension] = unknown[j];
    }
    return matrix;
}

double[][]? readByFile(Func<string?> read, out int dimension)
{
    string? dimStr = read();
    if (!Int32.TryParse(dimStr, out dimension))
    {
        Console.WriteLine("Неверный формат числа(размерности).");
        return null;
    }

    double[][] matrix = new double[dimension][];

    int i = 0;

    while (i < dimension)
    {
        string? matrixArr;
        if (!checkMatrixString(matrixArr = read(), dimension + 1))
        {
            Console.WriteLine("Неверный формат ввода. Повторите попытку:");
            return null;
        }
        matrix[i] = matrixArr.Trim().Split(" ").Select(Double.Parse).ToArray();

        double[][] separatedByStr = separateMatrix(matrix);
        if (checkRowsEquals(separatedByStr, i))
        {
            Console.WriteLine("Определитель данной матрицы равен 0.");
            return null;
        }
        i++;
    }
    double[][] separated = separateMatrix(matrix);
    if (checkColumnsEquals(separated, dimension))
    {
        Console.WriteLine("Определитель данной матрицы равен 0.");
        return null;
    }
    return matrix;
}

Boolean checkRowsEquals(double[][] matrix, int curRows)
{
    if (curRows == 0)
        return false;
    else
    {
        for (int i = 0; i < curRows; i++)
        {
            bool isEquals = Enumerable.SequenceEqual(matrix[i], matrix[curRows]);
            bool isZero = Array.TrueForAll(matrix[i], x => x == 0);
            if (isEquals || isZero)
                return true;
        }
        return false;
    }
}

double[][] transpose(double[][] matrix, int dimension)
{
    double tmp;
    for (int i = 0; i < dimension; i++)
    {
        for (int j = i; j < dimension; j++)
        {
            tmp = matrix[i][j];
            matrix[i][j] = matrix[j][i];
            matrix[j][i] = tmp;
        }
    }
    return matrix;
}

Boolean checkColumnsEquals(double[][] matrix, int dimension)
{
    double[][] transMatrix = transpose(matrix, dimension);
    for (int i = 0; i < dimension; i++)
    {
        if (checkRowsEquals(transMatrix, i))
            return true;
    }
    matrix = transpose(matrix, dimension);
    return false;
}

double[][] getTrangleMatrix(int dimension, double[][] fullMatrix, out int detCoef)
{
    double[][] matrix = fullMatrix;

    detCoef = 1;

    for (int i = 0; i < dimension - 1; i++)
    {
        int curDim = dimension - i;
        double[] tmpCol = new double[curDim];
        double mainElem = -1;
        for (int j = 0; j < curDim; j++)
        {
            tmpCol[j] = matrix[j + i][i];
            if (Math.Abs(tmpCol[j]) > mainElem)
                mainElem = tmpCol[j];
        }
        int mainInd = Array.IndexOf(tmpCol, mainElem) + i;

        if (tmpCol[0] != mainElem)
        {
            (matrix[i], matrix[mainInd]) = (matrix[mainInd], matrix[i]);
            (tmpCol[0], tmpCol[mainInd - i]) = (tmpCol[mainInd - i], tmpCol[0]);    
            detCoef *= -1;
        }
        mainInd = i;

        double[] factors = new double[curDim];
        int inc = 0;
        Array.ForEach(tmpCol, x => factors[inc++] = -(x / mainElem));

        for (int j = i + 1; j < dimension; j++)
        {
             double[] mainMulty = new double[curDim + 1];
             for (int k = 0; k < mainMulty.Length; k++)
                 mainMulty[k] = matrix[mainInd][k + i] * factors[j - i];

             inc = i;
             Array.ForEach(mainMulty, x => matrix[j][inc++] += x);       
        }
    }

    return matrix;
}

void printMatrix(double[][] matrix)
{
    double[] unknown;
    double[][] splited = splitMatrix(matrix, out unknown);

    int maxLength = 0;
    string[] mtxStr = new string[splited.Length];

    for (int i = 0; i < splited.Length; i++)
    {
        double[] rounded = new double[splited[i].Length];
        int inc = 0;
        Array.ForEach(splited[i], x => rounded[inc++] = Math.Round(x, 2));
        
        string mtx = string.Join(" ", rounded);
        if (mtx.Length > maxLength) maxLength = mtx.Length;

        mtxStr[i] = mtx;
    }
    for (int i = 0; i < mtxStr.Length; i++)
    {
        mtxStr[i] += new string(' ', maxLength - mtxStr[i].Length);
        mtxStr[i] += " | " + Math.Round(unknown[i], 2);
    }
    Console.WriteLine(string.Join("\n", mtxStr));
    Console.WriteLine();
}

double getDet(double[][] matrix, int detCoef)
{
    double det = detCoef;
    int count = 0;
    for (int i = 0; i < matrix.Length; i++)
    {
        det *= matrix[i][count];
        count++;
    }
    return det;
}

double[] getRoots(double[][] fullMatrix, int dimension)
{
    double[] unknown;
    double[][] matrix = splitMatrix(fullMatrix, out unknown);

    double[] roots = new double[dimension];
    double lastRoot = unknown[dimension - 1] /matrix[dimension - 1][dimension - 1];
    roots[dimension - 1] = lastRoot;

    for (int i = dimension - 2; i >= 0; i--)
    {
        double freeVal = unknown[i];
        double acc = 0;
        for (int j = dimension - 1; j > i; j--)
            acc += matrix[i][j] * roots[j];
        roots[i] = (freeVal - acc) / matrix[i][i];
    }
    return roots;
}

void printSth(double[] roots, string sth)
{
    int inc = 0;
    Array.ForEach(roots, x => Console.WriteLine("{0}{1} = {2}", sth, inc++, x));
    Console.WriteLine();
}

double[] getErrors(double[][] fullMatrix,double[] roots)
{
    double[] unknown;
    double[][] matrix = splitMatrix(fullMatrix, out unknown);

    double[] errors = new double[matrix.Length];
    double[] leftValues = new double[matrix.Length];

    for (int i = 0; i < matrix.Length; i++)
    {
        int count = 0;
        Array.ForEach(matrix[i], x => leftValues[i] += x * roots[count++]);
    }
    int inc1 = 0;
    int inc2 = 0;
    Array.ForEach(unknown, x => errors[inc1++] = Math.Abs(x - leftValues[inc2++]));

    return errors;
}

double[][] splitMatrix(double[][] fullMatrix, out double[] unknown)
{
    int len = fullMatrix.Length;
    unknown = new double[len];
    double[][] matrix = new double[len][];
    for (int i = 0; i < len; i++)
    {
        matrix[i] = new double[len];
        Array.Copy(fullMatrix[i], matrix[i], len);
        unknown[i] = fullMatrix[i][len];
    }
    return matrix;
}

double[][] separateMatrix(double[][] fullMatrix)
{
    double[][] matrix = new double[fullMatrix.Length][];
    int inc = 0;
    foreach(double[] row in fullMatrix)
    {
        if (row == null)
            break;
        matrix[inc] = new double[row.Length - 1];
        Array.Copy(row, matrix[inc++], row.Length - 1);
    }
    return matrix;
}

void execute(double[][] matrix, int dimension)
{
    Console.WriteLine("Введённая матрица (расширенная):");
    printMatrix(matrix);

    int detCoef;
    double[][] trangle = getTrangleMatrix(dimension, matrix, out detCoef);
    Console.WriteLine("Треугольная матрица:");
    printMatrix(trangle);

    double determinant = getDet(matrix, detCoef);
    Console.WriteLine("Определитель:\nΔ = {0}\n", determinant);

    double[] roots = getRoots(trangle, dimension);
    Console.WriteLine("Корни уравнения:");
    printSth(roots, "X");

    Console.WriteLine("Невязки:");
    double[] errors = getErrors(trangle, roots);
    printSth(errors, "\u03B4");
}


enum InputWay
{
    USER,
    FILE
}
