using System;

namespace Caasiope.Database
{
    public static class ReferenceHelper
    {
        //https://stackoverflow.com/questions/1132243/msbuild-doesnt-copy-references-dll-files-if-using-project-dependencies-in-sol
        // DO NOT DELETE THIS CODE UNLESS WE NO LONGER REQUIRE ASSEMBLY SQLITE!!!
        // TODO check if it works in release mode  
        static ReferenceHelper()
        {
            // Assembly SQLITE is used by this file, and that assembly depends on assembly B,
            // but this project does not have any code that explicitly references assembly B. Therefore, when another project references
            // this project, this project's assembly and the assembly A get copied to the project's bin directory, but not
            // assembly B. So in order to get the required assembly B copied over, we add some dummy code here (that never
            // gets called) that references assembly B; this will flag VS/MSBuild to copy the required assembly B over as well.
            var dummyType = typeof(System.Data.SQLite.SQLiteConvert);
            var dummyType2 = typeof(System.Data.SQLite.EF6.SQLiteProviderFactory);
            var dummyType3 = typeof(System.Data.SQLite.Linq.SQLiteProviderFactory);
            var dummyType4 = typeof(System.Data.Entity.SqlServer.SqlFunctions);
            Console.WriteLine(dummyType.FullName);
            Console.WriteLine(dummyType2.FullName);
            Console.WriteLine(dummyType3.FullName);
            Console.WriteLine(dummyType4.FullName);
        }
    }
}