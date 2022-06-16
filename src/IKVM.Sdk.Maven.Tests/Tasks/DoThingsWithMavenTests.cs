using IKVM.Sdk.Maven.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Sdk.Maven.Tests.Tasks
{

    [TestClass]
    public class DoThingsWithMavenTests
    {

        [TestMethod]
        public void Foo()
        {
            var e = new DoThingsWithMaven();
            e.Execute();
        }

    }

}