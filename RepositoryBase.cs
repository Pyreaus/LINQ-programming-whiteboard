// using imports 

namespace // i.e. NoelsWhiteboard.Api.Context.Infrastructure
{
	public abstract class RepositoryBase<TE, TR> where TE : class where TR : DbContext
	{
		#region infrastructure 
		private TR _localContext;  //fields
		private readonly DbSet<TE> _dbSet;

		private IDbFactory<TR> DbFactory { get; set }  //props
		protected TR InitContext => _localContext ?? (_localContext = DbFactory.Init());
		#endregion
		protected RepositoryBase(IDbFactory<TR> dbFactory) //injecting context-specific dbFactory 
		{
			DbFactory = dbFactory;
			_dbSet = InitContext.Set<TE>();
		}
		#region implementation
		public IQueryable<IGrouping<int, TEntityType>> GroupBy(Expression<Func<TEntityType, int>> keySelector)
       		{
            		return _dbSet.GroupBy(keySelector);
       		}
		public virtual async Task<IEnumerable<TE>> FirstOrDefaultAsync(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>,
			IIncludableQueryable<TE, object>> include = null, bool disableTracking = true)
		{
			IQueryable<TE> query = _dbSet;
			if (disableTracking) query = query.AsNoTracking(); 
			if (include != null) query = include(query);
			return await query.FirstOrDefaultAsync(predicate);
		}
		public virtual void Delete(TE entity)
		{
			_dbSet.Remove(entity);
		}
		public virtual void Delete(Expression<Func<TE, bool>> predicate)
		{
			IEnumerable<TE> objects = _dbSet.Where(predicate).AsEnumerable();
			foreach (obj in objects) _dbSet.Remove(obj);
		}
		#endregion
		
		//  [...]
	}
 





























