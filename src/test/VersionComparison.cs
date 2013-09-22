using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Craftitude
{
    namespace Tests
    {
        [TestClass]
        public class VersionComparison
        {
            [TestMethod]
            public void Comparison1()
            {
                Debug.Assert(((PackageVersion)"3.4").CompareTo("4.1") < 0);
                Debug.Assert(((PackageVersion)"3.4.2").CompareTo("3.2.1") > 0);
                Debug.Assert(((PackageVersion)"3.4-snapshot").CompareTo("3.4") > 0);
                Debug.Assert(((PackageVersion)"3.4-b12").CompareTo("3.4-b13") < 0);
                Debug.Assert(((PackageVersion)"0:3.4.2-cr1+git-3281afe0").CompareTo("0:3.4.2-cr1") == 0);
            }
        }
    }
}
