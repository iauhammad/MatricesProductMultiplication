using MatrixMultiplication.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMultiplication
{
    class Program
    {
        public static HttpClient client = WebHelper.InitialiseHttpClient();

        #region Version 1: Using jagged arrays

        public static int[][] matrixARows = new int[Constants.DATASET_SIZE][];
        public static int[][] matrixBColumns = new int[Constants.DATASET_SIZE][];

        #endregion

        #region Version 2: Using 2D arrays

        //public static int[,] matrixA = new int[Constants.DATASET_SIZE, Constants.DATASET_SIZE];
        //public static int[,] matrixB = new int[Constants.DATASET_SIZE, Constants.DATASET_SIZE];

        #endregion

        static void Main(string[] args)
        {
            Console.Title = "InvestCloud | Matrix Multiplication";

            Stopwatch stopWatch = null;

            try
            {
                // Initialise Matrices A and Matrices B datasets
                InitialiseDataSet();
                
                stopWatch = Stopwatch.StartNew();

                // Fetch Matrices A and Matrices B data
                FetchAndPopulateMatrices();

                // Product Multiply Matrix A and Matrix B
                ProductMultiplyMatrices();

            }
            catch (Exception e)
            {
                Console.WriteLine("> {0}: Application stopped unexpectedly. An exception occured. {1}", DateTime.Now.ToString(), e.Message);
            }
            finally
            {
                if(client != null)
                {
                    client.Dispose();
                    client = null;
                }
            }
            
            stopWatch.Stop();
            Console.WriteLine("> {0}: Execution time: {1}s | {2}ms for dataset of size: {3}", DateTime.Now.ToString(), Math.Floor(stopWatch.ElapsedMilliseconds / 1000.0), stopWatch.ElapsedMilliseconds, Constants.DATASET_SIZE);

            Console.WriteLine("Application is now idle!");
            Console.ReadLine();
        }

        private static void InitialiseDataSet()
        {
            var response = WebHelper.InitialiseMatricesDataSets(client).GetAwaiter().GetResult();
            if (response == null || !response.Success)
                throw new Exception("An error occured while trying to initialise the datasets.");
        }

        private static void FetchAndPopulateMatrices()
        {
            Console.WriteLine("> {0}: Retrieve Matrices A and B asynchronously.", DateTime.Now.ToString());

            #region Version 1: Using jagged arrays

            var taskMatrixA = Task.Run(() => FetchMatrixAByRows());
            var taskMatrixB = Task.Run(() => FetchMatrixBByColumns());

            #endregion

            #region Version 2: Using 2D arrays

            //var taskMatrixA = Task.Run(() => FetchMatrixA());
            //var taskMatrixB = Task.Run(() => FetchMatrixB());

            #endregion

            Task.WaitAll(taskMatrixA, taskMatrixB);
        }

        #region Version 1: Using jagged arrays

        private static void FetchMatrixAByRows()
        {
            Console.WriteLine("    > {0}: Retrieve Matrix A by rows.", DateTime.Now.ToString());

            List<Task> lstTasks = new List<Task>();
            Parallel.For(0, Constants.DATASET_SIZE, async rowIndex =>
            {

                var task = WebHelper.RetrieveDatasetByRowOrColumn(client, Constants.MATRIX_A, Constants.MATRIX_TYPE_ROW, rowIndex);
                lstTasks.Add(task);

                matrixARows[rowIndex] = new int[Constants.DATASET_SIZE];

                var response = await task;
                if (response == null || !response.Success)
                    throw new Exception(string.Format("An error occured while retrieving matrix data. (Matrix: {0}, Row/Col: {1}, Index: {2})", Constants.MATRIX_A, Constants.MATRIX_TYPE_ROW, rowIndex));

                matrixARows[rowIndex] = JsonConvert.DeserializeObject<int[]>(response.Value.ToString());

            });

            Task.WaitAll(lstTasks.ToArray());
        }

        private static void FetchMatrixBByColumns()
        {
            Console.WriteLine("    > {0}: Retrieve Matrix B by columns.", DateTime.Now.ToString());

            List<Task> lstTasks = new List<Task>();
            Parallel.For(0, Constants.DATASET_SIZE, async colIndex =>
            {

                var task = WebHelper.RetrieveDatasetByRowOrColumn(client, Constants.MATRIX_B, Constants.MATRIX_TYPE_COL, colIndex);
                lstTasks.Add(task);

                matrixBColumns[colIndex] = new int[Constants.DATASET_SIZE];

                var response = await task;
                if (response == null || !response.Success)
                    throw new Exception(string.Format("An error occured while retrieving matrix data. (Matrix: {0}, Row/Col: {1}, Index: {2})", Constants.MATRIX_B, Constants.MATRIX_TYPE_COL, colIndex));

                matrixBColumns[colIndex] = JsonConvert.DeserializeObject<int[]>(response.Value.ToString());

            });

            Task.WaitAll(lstTasks.ToArray());
        }

        private static void ProductMultiplyMatrices()
        {
            string matrixResultConcat = string.Empty;
            string hashedMD5String = string.Empty;

            Console.WriteLine("> {0}: Perform Product Multiply of Datasets A and B.", DateTime.Now.ToString());

            // List of strings to store product of matrices per row
            string[] lstProductResultsPerRow = new string[Constants.DATASET_SIZE];

            // Parallel Loop to do matrices product
            Parallel.For(0, Constants.DATASET_SIZE, rowIndex =>
            {
                string productConcatLTR = string.Empty;
                var currentRow = matrixARows[rowIndex];

                for (int colIndex = 0; colIndex < Constants.DATASET_SIZE; colIndex++)
                {
                    var currentCol = matrixBColumns[colIndex];

                    int temp = 0;
                    for (int i = 0; i < Constants.DATASET_SIZE; i++)
                    {
                        temp += currentRow[i] * currentCol[i];
                    }
                    // DEBUG
                    //Console.WriteLine("Product Multiplication: [{0},{1}] = {2}", rowIndex, colIndex, temp);

                    productConcatLTR = string.Concat(productConcatLTR, temp);
                }

                lstProductResultsPerRow[rowIndex] = productConcatLTR;
            });

            // DEBUG
            //for (int i = 0; i < lstProductResultsPerRow.Length; i++)
            //{
            //    Console.WriteLine("Result Strings concat: Row: {0}, String: {1}", i, lstProductResultsPerRow[i]);
            //}

            // String Concat all rows results
            matrixResultConcat = string.Join("", lstProductResultsPerRow);

            // DEBUG
            //Console.WriteLine("String to hash: " + matrixResultConcat);

            // Hash string with MD5
            var hashedResult = HashWithMD5(matrixResultConcat);

            // DEBUG
            //Console.WriteLine("Hashed String: " + hashedResult);

            // Validate MD5 string
            var response = WebHelper.ValidateProductMultiplication(client, hashedResult).GetAwaiter().GetResult();
            if (response == null)
                throw new Exception("An error occured while trying to validate the hashed result.");
            if (!response.Success)
                throw new Exception(string.Concat("Hashed result string is invalid. ", response.Value));

            Console.WriteLine("    > {0}: Product Multiply hash has been validated. Result: {1}", DateTime.Now.ToString(), response.Value);
        }

        #endregion

        #region Version 2: Using 2D arrays

        //private static void FetchMatrixA()
        //{
        //    Console.WriteLine("    > {0}: Retrieve Matrix A by rows.", DateTime.Now.ToString());

        //    List<Task> lstTasks = new List<Task>();
        //    Parallel.For(0, Constants.DATASET_SIZE, async rowIndex =>
        //    {

        //        var task = WebHelper.RetrieveDatasetByRowOrColumn(client, Constants.MATRIX_A, Constants.MATRIX_TYPE_ROW, rowIndex);
        //        lstTasks.Add(task);

        //        var response = await task;
        //        if (response == null || !response.Success)
        //            throw new Exception(string.Format("An error occured while retrieving matrix data. (Matrix: {0}, Row/Col: {1}, Index: {2})", Constants.MATRIX_A, Constants.MATRIX_TYPE_ROW, rowIndex));

        //        var rowValues = JsonConvert.DeserializeObject<int[]>(response.Value.ToString());
        //        for (int i = 0; i < rowValues.Length; i++)
        //        {
        //            matrixA[rowIndex, i] = rowValues[i];
        //        }

        //    });

        //    Task.WaitAll(lstTasks.ToArray());
        //}

        //private static void FetchMatrixB()
        //{
        //    Console.WriteLine("    > {0}: Retrieve Matrix B by rows.", DateTime.Now.ToString());

        //    List<Task> lstTasks = new List<Task>();
        //    Parallel.For(0, Constants.DATASET_SIZE, async rowIndex =>
        //    {

        //        var task = WebHelper.RetrieveDatasetByRowOrColumn(client, Constants.MATRIX_B, Constants.MATRIX_TYPE_ROW, rowIndex);
        //        lstTasks.Add(task);

        //        var response = await task;
        //        if (response == null || !response.Success)
        //            throw new Exception(string.Format("An error occured while retrieving matrix data. (Matrix: {0}, Row/Col: {1}, Index: {2})", Constants.MATRIX_A, Constants.MATRIX_TYPE_ROW, rowIndex));

        //        var rowValues = JsonConvert.DeserializeObject<int[]>(response.Value.ToString());
        //        for (int i = 0; i < rowValues.Length; i++)
        //        {
        //            matrixB[rowIndex, i] = rowValues[i];
        //        }

        //    });

        //    Task.WaitAll(lstTasks.ToArray());
        //}

        //private static void ProductMultiplyMatrices()
        //{
        //    string matrixResultConcat = string.Empty;
        //    string hashedMD5String = string.Empty;

        //    Console.WriteLine("> {0}: Perform Product Multiply of Datasets A and B.", DateTime.Now.ToString());

        //    // List of strings to store product of matrices per row
        //    string[] lstProductResultsPerRow = new string[Constants.DATASET_SIZE];

        //    // Parallel Loop to do matrices product
        //    Parallel.For(0, Constants.DATASET_SIZE, rowIndex =>
        //    {
        //        string productConcatLTR = string.Empty;

        //        for (int colIndex = 0; colIndex < Constants.DATASET_SIZE; colIndex++)
        //        {
        //            int temp = 0;
        //            for (int i = 0; i < Constants.DATASET_SIZE; i++)
        //            {
        //                temp += matrixA[rowIndex, i] * matrixB[i, colIndex];
        //            }

        //            productConcatLTR = string.Concat(productConcatLTR, temp);
        //        }

        //        lstProductResultsPerRow[rowIndex] = productConcatLTR;
        //    });

        //    // String Concat all rows results
        //    matrixResultConcat = string.Join("", lstProductResultsPerRow);

        //    // Hash string with MD5
        //    var hashedResult = HashWithMD5(matrixResultConcat);

        //    // Validate MD5 string
        //    var response = WebHelper.ValidateProductMultiplication(client, hashedResult).GetAwaiter().GetResult();
        //    if (response == null)
        //        throw new Exception("An error occured while trying to validate the hashed result.");
        //    if (!response.Success)
        //        throw new Exception(string.Concat("Hashed result string is invalid. ", response.Value));

        //    Console.WriteLine("    > {0}: Product Multiply hash has been validated. Result: {1}", DateTime.Now.ToString(), response.Value);
        //}

        #endregion

        private static string HashWithMD5(string stringToHash)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytesToHash = Encoding.ASCII.GetBytes(stringToHash);
                byte[] hashBytes = md5.ComputeHash(bytesToHash);

                StringBuilder sbHashedString = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sbHashedString.Append(hashBytes[i].ToString());
                }
                return sbHashedString.ToString();
            }
        }

    }
}
