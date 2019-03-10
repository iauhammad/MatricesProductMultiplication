using MatrixMultiplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMultiplication.Helpers
{
    public class WebHelper
    {
        public static HttpClient InitialiseHttpClient()
        {
            HttpClient httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri(Constants.URI_BASE);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }

        public static async Task<NumbersServiceResponse> InitialiseMatricesDataSets(HttpClient client)
        {
            Console.WriteLine("> {0}: Web request to initialise Matrices A and B.", DateTime.Now.ToString());

            NumbersServiceResponse serviceResponse = null;
            HttpResponseMessage response = await client.GetAsync(string.Format(Constants.URI_DATASET_INITIALISATION, Constants.DATASET_SIZE));
            if (response.IsSuccessStatusCode)
            {
                serviceResponse = await response.Content.ReadAsAsync<NumbersServiceResponse>();
            }
            return serviceResponse;
        }

        public static async Task<NumbersServiceResponse> RetrieveDatasetByRowOrColumn(HttpClient client, string dataset, string type, int index)
        {
            //Console.WriteLine("        > {0}: Retrieving Matrix [{1}] by [{2}] at index [{3}]", DateTime.Now.ToString(), dataset, type, index);

            NumbersServiceResponse serviceResponse = null;
            HttpResponseMessage response = await client.GetAsync(string.Format(Constants.URI_GET_DATASET, dataset, type, index));
            if (response.IsSuccessStatusCode)
            {
                serviceResponse = await response.Content.ReadAsAsync<NumbersServiceResponse>();
            }
            return serviceResponse;
        }

        public static async Task<NumbersServiceResponse> ValidateProductMultiplication(HttpClient client, string hashedResult)
        {
            Console.WriteLine("    > {0}: Make API call to validate hashed result.", DateTime.Now.ToString());

            NumbersServiceResponse serviceResponse = null;
            HttpResponseMessage response = await client.PostAsJsonAsync(Constants.URI_POST_VALIDATE, hashedResult);
            if (response.IsSuccessStatusCode)
            {
                serviceResponse = await response.Content.ReadAsAsync<NumbersServiceResponse>();
            }
            return serviceResponse;
        }
    }
}
