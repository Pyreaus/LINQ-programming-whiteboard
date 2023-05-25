using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace // i.e. NoelsWhiteboard.DAL.Infrastructure 
{
    /// <summary>
    /// Generic interface for all repositories (see generic repository pattern)
    /// </summary>
    /// <typeparam name="T">Type of the entity for which the repository is used</typeparam>
    public interface IRepository<T> where T : class
    {
        IQueryable<IGrouping<int, T>> GroupBy(Expression<Func<T, int>> keySelector);
        /// <summary>
        /// Asynchronously gets all entities of type T filtered by predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking">True to disable changing tracking, otherwise false. Default is true</param>
        /// <remarks>All entities of type T filtered by predicate</remarks>
        Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, bool disableTracking = true);
        
        /// <summary>
        /// Gets all entities of type T as a Queryable object
        /// </summary>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking">True to disable changing tracking, otherwise false. Default is true</param>
        /// <returns>All entities of type T as a Queryable object</returns>
        IQueryable<T> GetAllAsQueryable(Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, bool disableTracking = true);
        
        
         /// <summary>
        /// Gets a single (or default if not found) entity using delegate filtered by predicate
        /// </summary>
        /// <param name="predicate">A function to test if the element satisfies the condition</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking">True to disable changing tracking, otherwise false. Default is true</param>
        /// <returns>A single (or default if not found) entity using delegate</returns>
        Task<T> SingleOrDefault(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, bool disableTracking = true);

        /// <summary>
        /// Checks if any entity exists using delegate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>True if any entity exists that satisfies the conditions, otherwise false</returns>
        bool Any(Expression<Func<T, bool>> predicate);
        
        /// <summary>
        /// Gets all entities of type T filtered by predicate as a Queryable object
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking">True to disable changing tracking, otherwise false. Default is true</param>
        /// <remarks>All entities of type T filtered by predicate as a Queryable object</remarks>
        IQueryable<T> GetManyAsQueryable(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool disableTracking = true);

        /// <summary>
        /// Gets a single entity using delegate filtered by predicate
        /// </summary>
        /// <param name="predicate">A function to test if the element satisfies the condition</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking">True to disable changing tracking, otherwise false. Default is true</param>
        /// <returns>A single entity using delegate filtered by predicate</returns>
        T Single(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool disableTracking = true);

        /// <summary>
        /// Deletes all entities that satisfy the conditions
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        void Delete(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets an entity by integer id
        /// </summary>
        /// <param name="id">Identifier of the entity</param>
        /// <returns>The entity matching the identifier</returns>
        T GetById(int id);

        /// <summary>
        /// Gets the first entity (or default if not found) using delegate
        /// </summary>
        /// <param name="predicate">A function to test if the element satisfies the condition</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking">True to disable changing tracking, otherwise false. Default is true</param>
        /// <returns>The first entity (or default if not found) using delegate</returns>
        T FirstOrDefault(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool disableTracking = true);

        /// <summary>
        /// Gets the last entity (or default if not found) using delegate
        /// </summary>
        /// <param name="predicate">A function to test if the element satisfies the condition</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking">True to disable changing tracking, otherwise false. Default is true</param>
        /// <returns>The last entity (or default if not found) using delegate</returns>
        T LastOrDefault(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool disableTracking = true);

        /// <summary>
        /// Gets all entities of type T
        /// </summary>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking">True to disable changing tracking, otherwise false. Default is true</param>
        /// <returns>All entities of type T</returns>
        IEnumerable<T> GetAll(Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, bool disableTracking = true);

        /// <summary>
        /// Asynchronously gets all entities of type T
        /// </summary>
        /// <param name="disableTracking">True to disable changing tracking, otherwise false. Default is true</param>
        /// <returns>All entities of type T</returns>
        Task<IEnumerable<T>> GetAllAsync(bool disableTracking = true);

        /// <summary>
        /// Gets all entities of type T filtered by predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking">True to disable changing tracking, otherwise false. Default is true</param>
        /// <remarks>All entities of type T filtered by predicate</remarks>
        IEnumerable<T> GetMany(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool disableTracking = true);
    }
}
