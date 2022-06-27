namespace IKVM.Sdk.Maven.Tests.PackageReferenceProject.Lib
{

    public static class Helloworld
    {

        public static string TestJava(string value)
        {
            return IKVM.Sdk.Maven.Tests.PackProject.Lib.Helloworld.TestJava(value);
        }

    }

}
