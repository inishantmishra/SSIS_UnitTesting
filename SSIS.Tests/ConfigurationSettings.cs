using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSIS.Tests
{
    public class ConfigurationSettings
    {
            public string ConnectionString { get; set; }
            public string TemplatePath { get; set; }
            public string ProjectPath { get; set; }
            public string OriginalFilePath { get; set; }
    }
}
