using Plossum.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverCapsuleStressTool
{
    [CommandLineManager(ApplicationName = "RandomWriteXML",
       Copyright = "Microsoft")]
    class Options
    {
        [CommandLineOption(Description = "Displays this help text")]
        public bool Help = false;

        [CommandLineOption(Description = "Specifies the input file", MinOccurs = 1)]
        public string Name
        {
            get { return mName; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new InvalidOptionValueException(
                        "The name must not be empty", false);
                mName = value;
            }
        }

        private string mName;
    }
}
