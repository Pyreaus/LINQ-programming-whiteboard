    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {     
            CreateMap<Offer, OfferViewModel>().ForMember(
                    dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}");
                ).ForMember(
                    dest => dest.Number, opt => opt.Ignore());
        }
    }        //configuring mismatched properties for DTO mapping 
            

namespace // i.e. NoelsWhiteboard.Api.Context
{
	public class ReboxContext : DbContext
	{
		public string ConnectionString { get; set; }
		public ReboxContext()
		{
		}
		public ReboxContext(DbContextOptions<ReboxContext> options) : base(options) {}
		public ReboxContext(string connectionString) : base()
		{
			ConnecitonString = connectionString;
		}
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
