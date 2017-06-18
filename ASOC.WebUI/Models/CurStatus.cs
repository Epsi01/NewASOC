using ASOC.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASOC.WebUI.Models
{
    public class CurStatus: CURRENT_STATUS
    {
        public decimal statusSum { get; set; }      
    }
}