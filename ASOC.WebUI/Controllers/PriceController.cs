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
    public class PriceController : Controller
    {
        IRepository<PRICE> priceRepository;

        public PriceController(IRepository<PRICE> priceRepositoryParam)
        {
            priceRepository = priceRepositoryParam;
        }

        // GET: Index
        public ActionResult Index(int? page, PriceViewModel modelData, int? modelID, string price)
        {
            if (modelData.searchString != null)
            {
                page = 1;
            }
            else
            {
                modelData.searchString = modelData.currentFilter;
            }

            modelData.currentFilter = modelData.searchString;

            IEnumerable<PRICE> priceLog = priceRepository.GetAllList();

            int min = Convert.ToInt32(priceLog.Min(m => m.COAST));
            int max = Convert.ToInt32(priceLog.Max(m => m.COAST));

            //Проверка на слайдер 
            if (price != null && price != "")
            {
                String[] numbers = price.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                decimal num1 = Convert.ToDecimal(numbers[0]);
                decimal num2 = Convert.ToDecimal(numbers[1]);
                priceLog = priceLog.Where(m => m.COAST <= num2);
                priceLog = priceLog.Where(m => m.COAST >= num1);
            }

            if (modelID != null)
                priceLog = priceLog.Where(m => m.ID_MODEL.Equals(Convert.ToDecimal(modelID)));

            if (!String.IsNullOrEmpty(modelData.searchString))
            {
                decimal searchDigit;
                bool isInt = Decimal.TryParse(modelData.searchString, out searchDigit);

                if (isInt)
                {
                    priceLog = priceLog.Where(s => s.COAST.Equals(searchDigit)).
                        OrderBy(s => s.DATE_ADD);
                }
                else                
                {
                    priceLog = priceLog.Where(s => s.MODEL.NAME.Contains(modelData.searchString)).
                        OrderBy(s => s.DATE_ADD);
                }
            }

            if (modelData.firstDate != null)
                priceLog = priceLog.Where(x => x.DATE_ADD >= modelData.firstDate);
            if (modelData.secondDate != null)
                priceLog = priceLog.Where(x => x.DATE_ADD <= modelData.secondDate);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            PriceViewModel model = new PriceViewModel()
            {
                priceList = priceLog.ToPagedList(pageNumber, pageSize),
                currentFilter = modelData.currentFilter, 
                searchString = modelData.searchString,
                firstDate = modelData.firstDate,
                secondDate = modelData.secondDate,
                minPrice = min,
                maxPrice = max
            };
            return View(model);
        }

        // GET: Delete
        public ActionResult Delete(int? id)
        {

            if (id == null)
            {
                return HttpNotFound();
            }

            PRICE price = priceRepository.GetAllList().FirstOrDefault(x => x.ID.Equals(Convert.ToDecimal(id)));

            if (price == null)
            {
                return HttpNotFound();
            }
            return View(price);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            priceRepository.Delete(id);
            priceRepository.Save();
            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            return View();
        }        

        [HttpPost]
        public ActionResult Create(PRICE price)
        {
            if (ModelState.IsValid)
            {
                priceRepository.Create(price);
                priceRepository.Save();
                return RedirectToAction("Index");
            }
            return View(price);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            PRICE price = priceRepository.GetAllList().First(x => x.ID.Equals(Convert.ToDecimal(id)));
            if (price == null)
            {
                return HttpNotFound();
            }
            return View(price);
        }

        [HttpPost]
        public ActionResult Edit(PRICE price)
        {
            if (ModelState.IsValid)
            {
                priceRepository.Update(price);
                priceRepository.Save();
                return RedirectToAction("Index");
            }
            return View(price);
        }
    }
}