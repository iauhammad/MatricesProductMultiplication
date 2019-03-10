using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMultiplication.Helpers
{
    public class Constants
    {
        /// <summary>
        /// Matrices size
        /// </summary>
        public const int DATASET_SIZE = 1000;

        /// <summary>
        /// Matrix named 'A'
        /// </summary>
        public const string MATRIX_A = "A";

        /// <summary>
        /// Matrix named 'B'
        /// </summary>
        public const string MATRIX_B = "B";

        /// <summary>
        /// Matrix data retrieval type by 'row'
        /// </summary>
        public const string MATRIX_TYPE_ROW = "row";

        /// <summary>
        /// Matrix data retrieval type by 'col'
        /// </summary>
        public const string MATRIX_TYPE_COL = "col";

        /// <summary>
        /// API's base URI
        /// </summary>
        public const string URI_BASE = "https://recruitment-test.investcloud.com/";

        /// <summary>
        /// api/numbers/init/{size}
        /// </summary>
        public const string URI_DATASET_INITIALISATION = "api/numbers/init/{0}";

        /// <summary>
        /// api/numbers/{dataset}/{type [row|col]}/{index}
        /// </summary>
        public const string URI_GET_DATASET = "api/numbers/{0}/{1}/{2}";

        /// <summary>
        /// Validate product multiply of datasets. Requires hash string as body
        /// </summary>
        public const string URI_POST_VALIDATE = "api/numbers/validate";

    }
}
