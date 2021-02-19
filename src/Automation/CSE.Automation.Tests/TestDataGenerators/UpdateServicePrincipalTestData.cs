using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.TestDataGenerators
{
    public class UpdateServicePrincipalTestData : IEnumerable<object[]>
    {

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                //new ServicePrincipalUpdateTestData
                //{

                //}

            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
