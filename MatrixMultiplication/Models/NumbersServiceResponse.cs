using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMultiplication.Models
{
    public class NumbersServiceResponse
    {
        public object Value { get; set; }
        public string Cause { get; set; }
        public bool Success { get; set; }
    }
}
