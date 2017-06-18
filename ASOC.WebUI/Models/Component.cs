using ASOC.Domain;
using ASOC.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASOC.WebUI.Models
{
    public class Component: CostViewModel
    {
        public decimal sum { get; set; }
        public IEnumerable<CurStatus> curStatus { get; set; }
    }
}