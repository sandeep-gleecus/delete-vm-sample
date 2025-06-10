//using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMScheduler.Data
{
    public class LogRepository<T> : IDisposable where T : class
    {
        private bool disposed = false;

        private readonly SchedulerContext context = null;

        protected DbSet<T> DbSet { get; set; }

        public LogRepository()
        {
            context = new SchedulerContext();
            DbSet = context.Set<T>();
        }

        public List<T> GetAll()
        {
            return DbSet.ToList();
        }

        public T Find(int id)
        {
            return DbSet.Find(id);
        }

        public void Add(T entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            context.Entry<T>(entity).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            DbSet.Remove(DbSet.Find(id));
        }

        public int SaveChanges()
        {
            return context.SaveChanges();
        }

        public void Dispose()
        {
            if (disposed) return;
            context.Dispose();
            disposed = true;
        }
    }







    public class SchedulerRepository<T> : IDisposable where T : class
    {
        private bool disposed = false;

        private readonly SchedulerContext context = null;

        protected DbSet<T> DbSet { get; set; }

        public SchedulerRepository()
        {
            context = new SchedulerContext();
            DbSet = context.Set<T>();
        }

        public List<T> GetAll()
        {
            return DbSet.ToList();
        }

        public T Find(int id)
        {
            return DbSet.Find(id);
        }

        public void Add(T entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            context.Entry<T>(entity).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            DbSet.Remove(DbSet.Find(id));
        }

        public int SaveChanges()
        {
            return context.SaveChanges();
        }

        public void Dispose()
        {
            if (disposed) return;
            context.Dispose();
            disposed = true;
        }
    }

}
