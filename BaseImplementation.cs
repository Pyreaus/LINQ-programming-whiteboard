namespace // i.e. NoelsWhiteboard.DAL.Infrastructure
{
	public abstract class RepositoryBase<TE, TR, TL> where TE : class where TR : DbContext
	{
		#region [infrastructure] 
		private TR? _localContext;                                    
		private readonly DbSet<TE> _dbSet;
		private readonly ILogger<TL>? _logger;
		
		private static T NullArg<T>(T arg) => throw new ArgumentNullException(nameof(arg));                  
		protected TR InitContext => _localContext ??= DbFactory.Init();
		private IDbFactory<TR> DbFactory { get; }   
       		#endregion
       		protected RepositoryBase(IDbFactory<TR> dbFactory, ILogger<TL> logger)
       		{
           		 DbFactory = dbFactory ?? NullArg<IDbFactory<TR>>(dbFactory!);
           		 (_dbSet, _logger) = (InitContext.Set<TE>(), logger ?? NullArg<ILogger<TL>>(logger!));
       		}
		#region [implementation]
		public virtual async Task<TE?> FirstOrDefaultAsync(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, 
			IIncludableQueryable<TE,object>>? include = null, bool disableTracking = true)
		{
			IQueryable<TE> query = _dbSet;
			if (disableTracking) query = query.AsNoTracking(); 
			if (include != null) query = include(query);
			return await query.FirstOrDefaultAsync(predicate);
		}
		public virtual IQueryable<IGrouping<int, TE>> GroupBy(Expression<Func<TE, int>> keySelector) => _dbSet.GroupBy(keySelector);
		public virtual IQueryable<TE?> GetAllAsQueryable(Func<IQueryable<TE>, IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
        	{
          		 IQueryable<TE> query = _dbSet;
         	         if (disableTracking) query = query.AsNoTracking();
         		 return include != null ? include(query) : query;
     		}
		public virtual async Task<IEnumerable<TE?>> GetAllAsync(bool disableTracking = true)
       		{
          		 return disableTracking ? await _dbSet.AsNoTracking().ToListAsync() : await _dbSet.ToListAsync();
        	}
		public virtual void Update(TE entity)
		{
			_dbSet.Attach(entity);
			InitContext.Entry(entity).state = EntityState.Modified;
		}
		public virtual void Delete(TE entity) => _dbSet.Remove(entity);
		public virtual void Delete(Expression<Func<TE, bool>> predicate)
		{
			IEnumerable<TE> objects = _dbSet.Where(predicate).AsEnumerable();
			foreach (TE obj in objects) _dbSet.Remove(obj);
			_logger.LogWarning("removed entity");
		}
		public virtual async Task<IEnumerable<TE?>> GetManyAsync(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, 
			IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
      		{
			IQueryable<TE> query = _dbSet;
			if (disableTracking) query = query.AsNoTracking(); 
			if (include != null) query = include(query);
			return await query.Where(predicate).ToListAsync();
     	        }
		
		//  [...]
		#endregion
	}
}
 
