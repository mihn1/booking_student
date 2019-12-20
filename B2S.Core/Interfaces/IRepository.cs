using System;
using System.Collections.Generic;
using System.Linq.Expressions;


namespace B2S.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        T GetEntityById(string id);
        IEnumerable<T> GetAllEntity();
        T FindEntity(Expression<Func<T, bool>> match);
        IEnumerable<T> FindAllEntity(Expression<Func<T, bool>> match);
        T AddEntity(T entity);     
        void UpdateEntity(T entity);
        void SaveAll(IEnumerable<T> list);
    }

}
