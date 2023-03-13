using RandomNameGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFM.Testcases.Employee
{
    public class UseNameRandom
    {
        [Fact]
        public void Generate()
        {
            Random rand = new Random(DateTime.Now.Second); // we need a random variable to select names randomly

            RandomName nameGen = new RandomName(rand); // create a new instance of the RandomName class

            string name = nameGen.GenerateOnlyName(Sex.Male); // generate a male name, with one middal name.
            string surname = nameGen.GenerateOnlyName(Sex.Male);
        }
    }
}
