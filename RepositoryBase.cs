namespace // i.e. NoelsWhiteboard.DAL.Infrastructure
{
	public abstract class RepositoryBase<TE, TR> where TE : class where TR : DbContext
	{
		#region infrastructure 
		private TR? _localContext;	         //fields 
		private readonly DbSet<TE> _dbSet;
		private readonly ILogger<RepositoryBase<TE, TR>> _logger;

		private IDbFactory<TR> DbFactory { get; }                     //props
		protected TR InitContext => _localContext ?? (_localContext = DbFactory.Init());
		#endregion  
		protected RepositoryBase(ILogger<RepositoryBase<TE, TR>> logger, IDbFactory<TR> dbFactory) 
		{
			DbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
			(_logger, _dbSet) = (logger, InitContext.Set<TE>())
		}
		#region implementation
		public virtual async Task<TE?> FirstOrDefaultAsync(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, 
			IIncludableQueryable<TE,object>>? include = null, bool disableTracking = true)
		{
			IQueryable<TE> query = _dbSet;
			if (disableTracking) query = query.AsNoTracking(); 
			if (include != null) query = include(query);
			return await query.FirstOrDefaultAsync(predicate);
		}
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
		public virtual IQueryable<IGrouping<int, TE>> GroupBy(Expression<Func<TE, int>> keySelector) => _dbSet.GroupBy(keySelector);
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
		}
		public virtual async Task<IEnumerable<TE?>> GetManyAsync(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, 
			IIncludableQueryable<TE, object>>? include = null, bool disableTracking = true)
      		{
			IQueryable<TE> query = _dbSet;
			if (disableTracking) query = query.AsNoTracking(); 
			if (include != null) query = include(query);
			return await query.Where(predicate).ToListAsync();
     	        }
		#endregion
		
		//  [...]
	}
}
 
