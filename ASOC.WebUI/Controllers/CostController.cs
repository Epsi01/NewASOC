using ASOC.Domain;
using ASOC.WebUI.Infrastructure.Interfaces;
using ASOC.WebUI.Models;
using ASOC.WebUI.ViewModels;
using PagedList;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASOC.WebUI.Controllers
{
    public class CostController : Controller
    {
        IRepository<COMPONENT> componentRepository;
        IGetList getList;
        IRepository<CURRENT_STATUS> statusRepository;
        IRepository<STATUS_COSTS> statusCostsRepository;

        public CostController(IRepository<COMPONENT> componentRepositoryParam, IGetList getListParam,
            IRepository<CURRENT_STATUS> statusRepositoryParam, IRepository<STATUS_COSTS> statusCostsRepositoryParam)
        {
            componentRepository = componentRepositoryParam;
            getList = getListParam;
            statusRepository = statusRepositoryParam;
            statusCostsRepository = statusCostsRepositoryParam;
        }

        public const string sessionName = "Report";

        public ActionResult Index(int? page, CostViewModel modelData, int? modelID, int? typeID, string price)
        {
            if (modelData.searchString != null)
            {
                page = 1;
            }
            else
            {
                modelData.ID_TYPE = modelData.ID_TYPE;
            }

            modelData.currentFilter = modelData.searchString;

            var components = componentRepository.GetAllList();

            if (modelData.firstDate != null)
                components = components.Where(x => x.DATE_ADD >= modelData.firstDate);
            if (modelData.secondDate != null)
                components = components.Where(x => x.DATE_ADD <= modelData.secondDate);

            if (modelData.ID_TYPE != null)
            {
                components = components.Where(s => s.ID_TYPE.Equals(modelData.ID_TYPE));
            }                       

            decimal searchDigit;
            bool isInt = Decimal.TryParse(modelData.searchString, out searchDigit);

            if (!String.IsNullOrEmpty(modelData.searchString))
            {
                if (!isInt)
                {
                    if (components.Where(m => m.TYPE.NAME.Contains(modelData.searchString)).Count() != 0)
                    {
                        components = components.Where(m => m.TYPE.NAME.Contains(modelData.searchString));
                    }
                    if (components.Where(m => m.MODEL.NAME.Contains(modelData.searchString)).Count() != 0)
                    {
                        components = components.Where(s => s.MODEL.NAME.Contains(modelData.searchString));
                    }
                }
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            List<CostViewModel> componentList = new List<CostViewModel>();

            foreach (COMPONENT item in components)
            {
                componentList.Add(new CostViewModel()
                {
                    ID = item.ID,
                    DATE_ADD = item.DATE_ADD,
                    ID_MODEL = item.ID_MODEL,
                    ID_SERIES = item.ID_SERIES,
                    ID_TYPE = item.ID_TYPE,
                    PARTNUMBER = item.PARTNUMBER,
                    currentStatus = item.CURRENT_STATUS.Where(x => x.ID_COMPLECT.Equals(item.ID))
                        .OrderByDescending(x => x.DATE_STATUS).FirstOrDefault().STATUS.NAME,
                    MODEL = item.MODEL,
                    TYPE = item.TYPE,
                    currentCoast = item.MODEL.PRICE.Where(x => x.ID_MODEL.Equals(item.ID_MODEL))
                            .OrderByDescending(x => x.DATE_ADD).FirstOrDefault().COST                
                });
            }

            int min = Convert.ToInt32(componentList.Min(m => m.currentCoast));
            int max = Convert.ToInt32(componentList.Max(m => m.currentCoast));

            //Проверка на слайдер 
            if (price != null && price != "")
            {
                String[] numbers = price.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                decimal num1 = Convert.ToDecimal(numbers[0]);
                decimal num2 = Convert.ToDecimal(numbers[1]);
                componentList = componentList.FindAll(m => m.currentCoast <= num2);
                componentList = componentList.FindAll(m => m.currentCoast >= num1);
            }

            if (!String.IsNullOrEmpty(modelData.searchString))
            {
                if (isInt)
                {
                    componentList = componentList.FindAll(m => m.currentCoast.Equals(searchDigit));
                }
            }

            CostViewModel model = new CostViewModel
            {
                componentList = componentList.ToPagedList(pageNumber, pageSize),
                typeList = getList.getTypeSelectList(),
                searchString = modelData.searchString,
                currentFilter = modelData.currentFilter,
                statusList = getList.getStatusSelectList(),
                minPrice = min,
                maxPrice = max
            };
            model.compList = componentList;
            Session[sessionName] = model.compList;
            return View(model);
        }

        public ActionResult ReportWeb(IEnumerable<CostViewModel> report, CostViewModel modelData)
        {
            Cost cost = new Cost();
                        
            List<Component> a= new List<Component>();

            //Прогоняем комплектующие
            foreach (var item in report)
            {
                decimal summ = 0;
                //Ищем общую сумму по комплектующему
                IEnumerable<CURRENT_STATUS> b = statusRepository.GetAllList().
                            Where(m => m.ID_COMPLECT.Equals(item.ID) && m.DATE_STATUS <= modelData.firstDate
                            && m.DATE_STATUS >= modelData.secondDate);
                List<CurStatus> cur = new List<CurStatus>();

                foreach (var curStatus in b)
                {                                     
                    // Ищем сумму по одному текущему статусу
                    decimal sumStatus = 0;
                    IEnumerable<STATUS_COSTS> costs = statusCostsRepository.
                 GetAllList().Where(m => m.ID_CURRENT.Equals(curStatus.ID));
                    foreach (var costsItem in costs)
                    {                        
                            sumStatus += Convert.ToDecimal(costsItem.PRICE);                      
                    }                    
                    cur.Add(new CurStatus()
                    {
                      STATUS_COSTS = curStatus.STATUS_COSTS,
                      COMPONENT = curStatus.COMPONENT,
                      REASON = curStatus.REASON,
                      STATUS = curStatus.STATUS,
                      DATE_STATUS = curStatus.DATE_STATUS,
                      ID = curStatus.ID,
                      ID_COMPLECT = curStatus.ID_COMPLECT ,
                      ID_STATUS = curStatus.ID_STATUS,
                      statusSum = sumStatus                      
                    });

                }
                foreach(var nnn in cur)
                {
                    summ += nnn.statusSum;
                }
                a.Add(new Component()
                {
                    ID = item.ID,
                    ID_SERIES = item.ID_SERIES,
                    ID_TYPE = item.ID_TYPE,
                    DATE_ADD = item.DATE_ADD,
                    PARTNUMBER = item.PARTNUMBER,                   
                    CURRENT_STATUS = item.CURRENT_STATUS,
                    TYPE = item.TYPE,
                    MODEL = item.MODEL,
                    sum = summ
                });
            }
            CostViewModel model = new CostViewModel()
            {
                firstStringDate = modelData.firstDate.Value.ToShortDateString(),
                secondStringDate = modelData.secondDate.Value.ToShortDateString(),
                compList = modelData.compList,
                aaaa = a
            };  
          
            return View(model);
        }

        public ActionResult ReportPDF(IEnumerable<CostViewModel> report, CostViewModel modelData)
        {
            CostViewModel model = new CostViewModel()
            {
                firstStringDate = modelData.firstDate.Value.ToShortDateString(),
                secondStringDate = modelData.secondDate.Value.ToShortDateString()
            };
            return new ViewAsPdf(model);             
        }
    }
}