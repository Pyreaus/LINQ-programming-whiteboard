
public abstract class RepositoryBase<TE, TR> where TE : class where TR : DbContext
{
	#region properties & fields
	private TR _localContext;	//fields
	private readonly DbSet<TE> _dbSet;
	
	private IDbFactory<TR> DbFactory { get; set }   //properties
	protected TR InitContext => _localContext ?? (_localContext = DbFactory.Init());
	#endregion
	
	protected RepositoryBase(IDbFactory<TR> dbFactory)
	{
		DbFactory = dbFactory;
		_dbSet = InitContext.Set<TE>();
	}
	
	#region implementation
	public virtual async Task<IEnumerable<TE>> FirstOrDefaultAsync(Expression<Func<TE, bool>> predicate, Func<IQueryable<TE>, IIncludableQueryable<TE, object>> include = null, bool disableTracking = true)
    {
        IQueryable<TE> query = _dbSet;
        if (disableTracking != null) query = query.AsNoTracking(); 
        if (include != null) query = include(query);
    }
}
 
