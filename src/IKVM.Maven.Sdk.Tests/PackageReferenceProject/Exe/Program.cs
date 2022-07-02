using System;
using System.Collections.Generic;
using System.Text;

using IKVM.Maven.Sdk.Tests.PackageReferenceProject.Lib;

namespace IKVM.Maven.Sdk.Tests.PackageReferenceProject.Exe
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            Console.WriteLine(Helloworld.TestJava(args[0]));
        }

    }

}
