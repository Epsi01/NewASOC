using ASOC.Domain;
using ASOC.WebUI.Infrastructure.Interfaces;
using ASOC.WebUI.ViewModels;
using PagedList;
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

        public CostController(IRepository<COMPONENT> componentRepositoryParam, IGetList getListParam,
            IRepository<CURRENT_STATUS> statusRepositoryParam)
        {
            componentRepository = componentRepositoryParam;
            getList = getListParam;
            statusRepository = statusRepositoryParam;
        }

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
                    currentCoast = item.MODEL.PRICE.Where(x => x.ID_MODEL.Equals(item.ID))
                            .OrderByDescending(x => x.DATE_ADD).FirstOrDefault().COAST,
                    MODEL = item.MODEL,
                    TYPE = item.TYPE
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
            return View(model);
        }

        public ActionResult Report(CostViewModel modelData)
        {

            return View();
        }
    }
}