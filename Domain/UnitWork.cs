using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Z.EntityFramework.Plus;

namespace LanguageModeSwitcherWpf.Domain;

public class UnitWork<U> where U : UserDataContext
{
    private U _context;

    public UnitWork(U context)
    {
        _context = context;
    }

    public U GetDbContext()
    {
        return _context;
    }

    public T? FirstOrDefault<T>(Expression<Func<T, bool>> exp) where T : class
    {
        return _context.Set<T>().FirstOrDefault(exp);
    }

    public int GetCount<T>(Expression<Func<T, bool>> exp = null) where T : class
    {
        return Filter(exp).Count();
    }

    /// <summary>
    /// 根据过滤条件，获取记录
    /// </summary>
    /// <param name="exp">The exp.</param>
    public IQueryable<T> Find<T>(Expression<Func<T, bool>> exp = null) where T : class
    {
        return Filter(exp);
    }

    /// <summary>
    /// 根据过滤条件，获取记录
    /// </summary>
    /// <param name="exp">The exp.</param>
    public IQueryable<T> Finds<T>(Expression<Func<T, bool>> exp = null) where T : class
    {
        return Filters(exp);
    }

    public bool IsExist<T>(Expression<Func<T, bool>> exp) where T : class
    {
        return _context.Set<T>().Any(exp);
    }

    public void Add<T>(T entity) where T : class
    {
        _context.Set<T>().Add(entity);
    }

    public void Update<T>(T entity) where T : class
    {
        var entry = _context.Entry(entity);
        entry.State = EntityState.Modified;

        //如果数据没有发生变化
        if (!_context.ChangeTracker.HasChanges())
        {
            entry.State = EntityState.Unchanged;
        }
    }

    public void BulkUpdate<T>(List<T> entities) where T : class
    {
        _context.Set<T>().BulkUpdate(entities);
    }

    public void Delete<T>(T entity) where T : class
    {
        _context.Set<T>().Remove(entity);
    }
    public void BulkDelete<T>(IEnumerable<T> entity) where T : class
    {
        _context.Set<T>().RemoveRange(entity);
    }

    public void Save()
    {
        var transaction = _context.Database.BeginTransaction();
        _context.SaveChanges();
        transaction.Commit();
    }

    private IQueryable<T> Filter<T>(Expression<Func<T, bool>> exp) where T : class
    {
        //var dbSet = _context.Set<T>().AsNoTracking().AsQueryable();
        var dbSet = _context.Set<T>().AsQueryable();
        if (exp != null)
            dbSet = dbSet.Where(exp);
        return dbSet;
    }

    private IQueryable<T> Filters<T>(Expression<Func<T, bool>> exp) where T : class
    {
        var dbSet = _context.Set<T>().AsQueryable();
        if (exp != null)
            dbSet = dbSet.Where(exp);
        return dbSet;
    }
}