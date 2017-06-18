using ASOC.Domain;
using ASOC.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASOC.WebUI.Models
{
    public class Cost: STATUS_COSTS
    {
        //Список комплектующих,  сумма 
        public IEnumerable<Component> compList { get; set; }

                                           // Список current статусов комплектующих
                                           // Сумма по каждому статусу 
                                           // Список затрат

    }
}