using ASOC.Domain;
using ASOC.WebUI.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASOC.WebUI.Infrastructure.Repositories
{
    public class StatusCostsRepository : IRepository<STATUS_COSTS>
    {
        private Entities db = new Entities();

        public void Create(STATUS_COSTS n)
        {
            db.Entry(n).State = System.Data.Entity.EntityState.Added;
        }
        public void Update(STATUS_COSTS n)
        {
            db.Entry(n).State = System.Data.Entity.EntityState.Modified;
        }
        public void Save()
        {
            db.SaveChanges();
        }
        public void Delete(int id)
        {
            PRICE u = db.PRICE.Find(id);
            db.PRICE.Remove(u);
        }

        public IEnumerable<STATUS_COSTS> GetAllList()
        {
            return db.STATUS_COSTS.ToList();
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}