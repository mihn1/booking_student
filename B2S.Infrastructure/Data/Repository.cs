using B2S.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace B2S.Infrastructure.Data
{
    public class Repository<T> : IRepository<T>  where T : class
    {
        protected readonly AppDbContext _dbContext;

        public Repository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public T AddEntity(T entity)
        {
            _dbContext.Add(entity);
            _dbContext.SaveChanges();
            return entity;
        }

        public IEnumerable<T> FindAllEntity(Expression<Func<T, bool>> match)
        {
            return _dbContext.Set<T>().Where(match).ToList();
        }

        public T FindEntity(Expression<Func<T, bool>> match)
        {
            return _dbContext.Set<T>().SingleOrDefault(match);
        }

        public IEnumerable<T> GetAllEntity()
        {
            return _dbContext.Set<T>();
        }

        public T GetEntityById(string id)
        {
           return  _dbContext.Set<T>().Find(id);
        }

        public void UpdateEntity(T entity)
        {
            _dbContext.Update(entity);
            _dbContext.SaveChanges();
        }

        public void SaveAll(IEnumerable<T> list)
        {
            _dbContext.AddRange(list);
            _dbContext.SaveChanges();
        }

    }
}
