namespace [namespace];
internal abstract partial class RepositoryBase<TE, TR, TL> where TE : class, new() 
                                                         where TR : DbContext
{
    #region [infrastructure]
    private TR? _localContext;
    private readonly DbSet<TE> _dbSet;
    private readonly ILogger<TL>? _logger;

    private IDbFactory<TR> DbFactory { get; }
    protected TR InitContext => _localContext ??= DbFactory.Init();
    
    private static Exception Ex(object? exc) => (exc == null) ? throw new ArgumentNullException(nameof(exc)) : 
    exc.GetType() == typeof(Exception) ? throw (Exception)exc : throw new Exception((string)exc);
    
    /// <param name="dbFactory"></param>
    /// <param name="logger"></param> 
    /// <exception cref="ArgumentNullException"><see cref="RepositoryBase"/> failed dependancy</exception>
    protected RepositoryBase(IDbFactory<TR> dbFactory, ILogger<TL> logger)
    {
        DbFactory = dbFactory ?? throw Ex<ArgumentNullException, IDbFactory<TR>>(dbFactory);
        (_dbSet, _logger) = (InitContext.Set<TE>(), logger ?? throw Ex<ArgumentNullException>());
    }
    #endregion
    #region [implementation]
    public virtual IQueryable<IGrouping<int, TE>> GroupBy(Expression<Func<TE, int>> keySelector) => _dbSet.GroupBy(keySelector);
    public virtual IEnumerable<TE?> GetMany(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        return query.Where(predicate).ToList();
    }
    public virtual async Task<bool> AnyAsync(Expression<Func<TE, bool>> predicate) => await _dbSet.AnyAsync(predicate);
    public virtual IEnumerable<TE?> GetAll(Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        return query.ToList().AsEnumerable();
    }
    public virtual void Update(TE entity)
    {
        _dbSet.Attach(entity);
        InitContext.Entry(entity).State = EntityState.Modified;
    }
    public virtual TE? GetById(Guid id) => _dbSet.Find(id);
    public virtual async Task<TE?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }
    public virtual void Delete(TE entity)
    {
        _dbSet.Remove(entity);
        _logger!.LogInformation("Removed entity");
    }
    public virtual void Add(TE entity) => _dbSet.Add(entity);
    public virtual void Delete(Expression<Func<TE, bool>> predicate)
    {
        IEnumerable<TE> objects = _dbSet.Where(predicate).AsEnumerable();
        foreach (TE obj in objects) _dbSet.Remove(obj);
    }
    public virtual bool Any(Expression<Func<TE, bool>> predicate) => _dbSet.Any(predicate);
    public virtual IQueryable<TE?> GetAllAsQueryable(Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        return include != null ? include(query) : query;
    }
    public virtual async Task<IEnumerable<TE?>> GetManyAsync(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        return await query.Where(predicate).ToListAsync();
    }
    public virtual async Task<TE?> FirstOrDefaultAsync(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        return await query.FirstOrDefaultAsync(predicate);
    }
    public virtual TE? FirstOrDefault(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        return query.FirstOrDefault(predicate);
    }
    public virtual async Task<IEnumerable<TE?>> GetAllAsync(bool disableTracking = true)
    {
        return disableTracking ? await _dbSet.AsNoTracking().ToListAsync() : await _dbSet.ToListAsync();
    }
    public virtual IEnumerable<TE?> GetMany(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        return query.Where(predicate).ToList();
    }
    public virtual IQueryable<TE?> GetManyAsQueryable(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        return query.Where(predicate);
    }
    public virtual TE? LastOrDefault(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE?>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        return query.LastOrDefault(predicate);
    }
    public virtual TE? Single(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        return query.Single(predicate);
    }
    public virtual TE? SingleOrDefault(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = query.AsNoTracking();
        return query.SingleOrDefault(predicate);
    }
    #endregion
}
