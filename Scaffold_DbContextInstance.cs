    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {     
            CreateMap<Entity, EntityViewModel>().ForMember(
                    dest => dest.Info, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName} {src.dob.getCurrentAge()}")
			).ForMember(dest => dest.Number, opt => opt.Ignore());
        }
    }        //configuring mismatched properties for ViewModel (DTO) mapping 
    
           	 	[HttpGet()]
			public async Task<IActionresult<IEnumerable<EntityDTO>?>> GetAll([FromBody] string filter=null)
			{
				return Ok(await localSet.Select(SourceObj => _mapper.Map(<IEnumerable<TSourceModel>,IEnumerable<EntityDTO>>(SourceObj))); 
 			}                                                       // using the mapped DTO in controller						       
								      
namespace // i.e. NoelsWhiteboard.Api.Context      
{
	public class ReboxContext : DbContext
	{
		public string ConnectionString { get; set; }
		public ReboxContext() {}
		public ReboxContext(DbContextOptions<ReboxContext> options) : base(options) {}
		public ReboxContext(string connectionString) : base() {ConnecitonString=connectionString;}
		
		#region sets
		public virtual DbSet<Comp> Comps { get; set; }
		public virtual DbSet<User> Users { get; set; }
		public virtual DbSet<Entry> Entries { get; set; }
		#endregion
		
		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			options.UseSqlServer(ConnectionString, x => x.MigrationsAssembly("NoelsWhiteboard.Api.Migrations"));
			base.OnConfiguring(optionsBuilder);
		}
		
		public override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>(entity => entitiy.Property(x => x.Id).HasColumnName("ID"));
			modelBuilder.Entity<Entry>().ToView("Entry").HasKey(x => x.EntryId); 
		}
	}	
}
