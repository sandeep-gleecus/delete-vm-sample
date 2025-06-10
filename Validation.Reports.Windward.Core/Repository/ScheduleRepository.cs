using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Validation.Reports.Windward.Core.DbSet;
using Validation.Reports.Windward.Core.Model;

namespace Validation.Reports.Windward.Core.Repository
{
    public class ScheduleRepository<T> : IDisposable where T : class
    {
        private bool _disposed = false;
        //private LogDbSet db = new LogDbSet();

        private readonly SchedulesDbSet _context = null;

        protected DbSet<T> DbSet { get; set; }

        public ScheduleRepository()
        {
            _context = new SchedulesDbSet();
            DbSet = _context.Set<T>();
        }

        public ScheduleRepository(SchedulesDbSet context)
        {
            this._context = context;
        }

        public List<Schedule> GetAll(bool isReady = true)
        {
            if (isReady)
            {
                return _context.Schedules.Where(s => s.Status == "Ready").ToList();
            }
            return _context.Schedules.ToList(); 
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
            _context.Entry<T>(entity).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            DbSet.Remove(DbSet.Find(id));
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void CreateScheduleRecord(IList<Schedule> schedules)
        {
            try
            {
                foreach (Schedule s in schedules)
                {
                    s.AddNewEntry();
                    //System.Diagnostics.Debug.WriteLine(s.ScheduledTime);
                    SaveChanges();
                }

            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                throw new ApplicationException(ex.Message);
            }
        }

        public void DeleteSchedulesByTemplateId(int Id)
        {
            try
            {
                var result = _context.Schedules.Where(s => s.TemplateId == Id);

                foreach (var s in result)
                {
                    DbSet.Remove(DbSet.Find(s.ScheduleId));
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

                //Logging.LogError(ex.Message, string.Empty, string.Empty);
                throw ex;
            }
        }
        public List<Schedule> GetSchedulesByTemplateId(int templateId)
        {
            try
            {

                var result = _context.Schedules.Where(s => s.TemplateId == templateId && s.Status == "Ready");
                
                return result.ToList();
            }
            catch (Exception ex)
            {
                //Logging.LogError(ex.Message, string.Empty, string.Empty);
                throw ex;
            }
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}
