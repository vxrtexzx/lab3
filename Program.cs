using System;
using System.IO;
using System.Threading.Tasks;

public class Program
{
    private static Random random = new Random();

    private static Matrix CreateRandom(int rows, int cols)
    {
        double[,] values = new double[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                values[r, c] = random.NextDouble() * 20 - 10;
            }
        }
        return new Matrix(values);
    }

    private static DirectoryInfo CreateDirectory(string path)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        if (dir.Exists) dir.Delete(true);
        dir.Create();
        return dir;
    }

    private static void WriteToDir(Matrix[] matrices, DirectoryInfo dir, string prefix, string extension, Action<Matrix, Stream> write)
    {
        for (int i = 0; i < matrices.Length; i++)
        {
            string fileName = $"{prefix}{i}{extension}";
            MatrixIO.WriteToFile(dir, fileName, matrices[i], write);
            if (i % 10 == 9) Console.WriteLine($"{fileName} write finished");
        }
    }

    public static async Task Main(string[] args)
    {
        // Create random matrices
        Matrix[] a = new Matrix[50];
        Matrix[] b = new Matrix[50];
        for (int i = 0; i < 50; i++)
        {
            a[i] = CreateRandom(100, 100);
            b[i] = CreateRandom(100, 100);
        }

        // Create directories
        DirectoryInfo calcDir = CreateDirectory("calculations");
        DirectoryInfo textDir = CreateDirectory("text");
        DirectoryInfo binaryDir = CreateDirectory("binary");
        DirectoryInfo jsonDir = CreateDirectory("json");

        // Asynchronous tasks for operations
        Task calcTask = Task.Run(async () =>
        {
            // Scalar and sequential multiplications
            Task<Matrix> scalarTask = Task.Run(() => MatrixOperations.Multiply(a[0], b[0]));
            Task<Matrix> sequentialTask = Task.Run(() =>
            {
                Matrix result = a[0];
                for (int i = 1; i < a.Length; i++)
                {
                    result = MatrixOperations.Multiply(result, a[i]);
                    result = MatrixOperations.Multiply(result, b[i]);
                }
                return result;
            });

            var saveResult = async (Task<Matrix> task, string name) =>
            {
                var result = await task;
                Console.WriteLine($"{name} calculation is finished");

                await MatrixIO.WriteToFileAsync(
                    calcDir,
                    name + ".txt",
                    result,
                    (Matrix matrix, Stream stream) => MatrixIO.WriteTextAsync(matrix, stream, ' ')
                );
            };

            await Task.WhenAll(
                saveResult(scalarTask, "scalar"),
                saveResult(sequentialTask, "sequential")
            );
        });

        Task writeAsyncTask = Task.Run(async () =>
        {
            // async write
            Task aCsvTask = MatrixIO.WriteToDirAsync(
                a,
                textDir,
                "a",
                ".txt",
                (matrix, stream) => MatrixIO.WriteTextAsync(matrix, stream, ' ')
            );

            Task bCsvTask = MatrixIO.WriteToDirAsync(
                b,
                textDir,
                "b",
                ".txt",
                (matrix, stream) => MatrixIO.WriteTextAsync(matrix, stream, ' ')
            );

            Task aJsonTask = MatrixIO.WriteToDirAsync(
                a,
                jsonDir,
                "a",
                ".json",
                (matrix, stream) => MatrixIO.WriteJsonAsync(matrix, stream)
            );

            Task bJsonTask = MatrixIO.WriteToDirAsync(
                b,
                jsonDir,
                "b",
                ".json",
                (matrix, stream) => MatrixIO.WriteJsonAsync(matrix, stream)
            );

            await Task.WhenAll(aCsvTask, bCsvTask, aJsonTask, bJsonTask);

            Console.WriteLine("Write async finished");

            // async read
            Task<Matrix[]> csvRead = MatrixIO.ReadFromDirAsync(
                textDir,
                "a",
                ".txt",
                (stream) => MatrixIO.ReadTextAsync(stream, ' ')
            );

            Task<Matrix[]> jsonRead = MatrixIO.ReadFromDirAsync(
                jsonDir,
                "a",
                ".json",
                (stream) => MatrixIO.ReadJsonAsync(stream)
            );

            var readTasks = new List<Task<Matrix[]>> { csvRead, jsonRead };

            while (readTasks.Count > 0)
            {
                var finished = await Task.WhenAny(readTasks);

                var result = await finished;
                readTasks.Remove(finished);

                if (finished == csvRead)
                {
                    Console.WriteLine("Csv finished");
                }
                else
                {
                    Console.WriteLine("Json finished");
                }
            }

            // comparing read matrices with original matrices
            // for simplicity, we assume comparison code here
        });

        // Synchronous write and read
        Console.WriteLine("Write started");
        WriteToDir(a, binaryDir, "a", ".bin", (matrix, stream) =>
        {
            // binary write implementation
        });

        WriteToDir(b, binaryDir, "b", ".bin", (matrix, stream) =>
        {
            // binary write implementation
        });
        Console.WriteLine("Write finished");

        // Assuming synchronous read code here

        await Task.WhenAll(calcTask, writeAsyncTask);
    }
}
