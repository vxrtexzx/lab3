using System;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;

public static class MatrixIO
{
    public static async Task WriteTextAsync(Matrix matrix, Stream stream, char sep = ' ')
    {
        using (var writer = new StreamWriter(stream))
        {
            await writer.WriteLineAsync($"{matrix.Rows}{sep}{matrix.Columns}");
            for (int i = 0; i < matrix.Rows; i++)
            {
                await writer.WriteLineAsync(string.Join(sep.ToString(), Enumerable.Range(0, matrix.Columns).Select(j => matrix[i, j])));
            }
        }
    }

    public static async Task<Matrix> ReadTextAsync(Stream stream, char sep = ' ')
    {
        using (var reader = new StreamReader(stream))
        {
            string? rcLine = await reader.ReadLineAsync();
            int[] rc = rcLine?.Split(sep).Select(int.Parse).ToArray() ?? Array.Empty<int>();
            int rows = rc[0], cols = rc[1];
            double[,] values = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                string[] line = (await reader.ReadLineAsync())?.Split(sep) ?? Array.Empty<string>();
                for (int j = 0; j < cols; j++)
                {
                    values[i, j] = double.Parse(line[j]);
                }
            }

            return new Matrix(values);
        }
    }

    public static async Task WriteJsonAsync(Matrix matrix, Stream stream)
    {
        double[][] values = new double[matrix.Rows][];
        for (int i = 0; i < matrix.Rows; i++)
        {
            values[i] = new double[matrix.Columns];
            for (int j = 0; j < matrix.Columns; j++)
            {
                values[i][j] = matrix[i, j];
            }
        }
        await JsonSerializer.SerializeAsync(stream, values);
    }

    public static async Task<Matrix> ReadJsonAsync(Stream stream)
    {
        double[][]? values = await JsonSerializer.DeserializeAsync<double[][]>(stream);
        if (values == null) throw new InvalidOperationException("Deserialized values cannot be null.");

        int rows = values.Length;
        int cols = values[0].Length;
        double[,] matrixValues = new double[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrixValues[i, j] = values[i][j];
            }
        }
        return new Matrix(matrixValues);
    }

    public static void WriteToFile(DirectoryInfo dir, string fileName, Matrix matrix, Action<Matrix, Stream> write)
    {
        string filePath = Path.Combine(dir.FullName, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            write(matrix, stream);
        }
    }

    public static async Task WriteToFileAsync(DirectoryInfo dir, string fileName, Matrix matrix, Func<Matrix, Stream, Task> write)
    {
        string filePath = Path.Combine(dir.FullName, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await write(matrix, stream);
        }
    }

    public static async Task<Matrix> ReadFromFileAsync(FileInfo fileInfo, Func<Stream, Task<Matrix>> read)
    {
        using (var stream = fileInfo.Open(FileMode.Open))
        {
            return await read(stream);
        }
    }

    public static async Task WriteToDirAsync(Matrix[] matrices, DirectoryInfo dir, string prefix, string extension, Func<Matrix, Stream, Task> write)
    {
        for (int i = 0; i < matrices.Length; i++)
        {
            string fileName = $"{prefix}{i}{extension}";
            string filePath = Path.Combine(dir.FullName, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await write(matrices[i], stream);
            }
            if (i % 10 == 9) Console.WriteLine($"{fileName} write finished");
        }
    }

    public static async Task<Matrix[]> ReadFromDirAsync(DirectoryInfo dir, string prefix, string extension, Func<Stream, Task<Matrix>> read)
    {
        var files = dir.GetFiles()
                       .Where(fileInfo => fileInfo.Name.StartsWith(prefix) && fileInfo.Name.EndsWith(extension))
                       .OrderBy(fileInfo => int.Parse(Path.GetFileNameWithoutExtension(fileInfo.Name).Substring(prefix.Length)))
                       .ToArray();

        Matrix[] matrices = new Matrix[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            using (var stream = files[i].Open(FileMode.Open))
            {
                matrices[i] = await read(stream);
            }
        }
        return matrices;
    }
}
