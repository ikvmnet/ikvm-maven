using System;
using System.Collections.Generic;
using System.Linq;

using java.io;
using System.Text;
using System.Threading.Tasks;

namespace IKVM.Sdk.Maven.Tasks
{
    internal class AetherUtils
    {

        public static File findUserSettings()
        {
            return new File(new File(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".m2"), Names.SETTINGS_XML);
        }

    }
}
