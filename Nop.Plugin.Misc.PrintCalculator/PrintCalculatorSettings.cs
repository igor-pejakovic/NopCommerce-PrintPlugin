﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.PrintCalculator
{
    public class PrintCalculatorSettings : ISettings
    {
        public int PrintCategoryID { get; set; }
    }
}
