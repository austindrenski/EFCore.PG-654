using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;

// ReSharper disable UnusedMember.Global UnusedVariable
namespace EFCore.PG_654
{
    class Program
    {
        static void Main(string[] args)
        {
            var username = args[0];
            var password = args[1];

            var serviceProvider =
                new ServiceCollection()
                    .AddEntityFrameworkNpgsql()
                    .AddDbContext<Context1>(
                        o => o.UseNpgsql(
                            $"Host=localhost;Port=5432;Username={username};Password={password};Database=context1;",
                            x => x.UseNetTopologySuite()))
                    .AddDbContext<Context2>(
                        o => o.UseNpgsql(
                            $"Host=localhost;Port=5432;Username={username};Password={password};Database=context2;"))
                    .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                // Calling these in order (1 -> 2) works, but
                // resolving out of order (2 -> 1) induces the exception.
                var context2 = scope.ServiceProvider.GetService<Context2>();
                var context1 = scope.ServiceProvider.GetService<Context1>();

                // Triggers the exception when resolution was out of order.
                context1.Database.EnsureCreated();
            }

            Console.WriteLine("Completed successfully");
        }

// Unhandled Exception: System.InvalidOperationException: The property 'Point.Boundary' is of an interface type ('IGeometry'). If it is a navigation property manually configure the relationship for this property by casting it to a mapped entity type, otherwise ignore the property using the NotMappedAttribute or 'EntityTypeBuilder.Ignore' in 'OnModelCreating'.
//    at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.PropertyMappingValidationConvention.Apply(InternalModelBuilder modelBuilder)
//    at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.ConventionDispatcher.ImmediateConventionScope.OnModelBuilt(InternalModelBuilder modelBuilder)
//    at Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
//    at System.Lazy`1.ViaFactory(LazyThreadSafetyMode mode)
//    at System.Lazy`1.ExecutionAndPublication(LazyHelper executionAndPublication, Boolean useDefaultConstructor)
//    at System.Lazy`1.CreateValue()
//    at Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel()
//    at Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
//    at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScoped(ScopedCallSite scopedCallSite, ServiceProviderEngineScope scope)
//    at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitConstructor(ConstructorCallSite constructorCallSite, ServiceProviderEngineScope scope)
//    at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScoped(ScopedCallSite scopedCallSite, ServiceProviderEngineScope scope)
//    at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
//    at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService[T](IServiceProvider provider)
//    at Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
//    at Microsoft.EntityFrameworkCore.DbContext.get_InternalServiceProvider()
//    at Microsoft.EntityFrameworkCore.Internal.InternalAccessorExtensions.GetService[TService](IInfrastructure`1 accessor)
//    at Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.get_DatabaseCreator()
//    at Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.EnsureCreated()
    }

    #region Models

    public class TextModel
    {
        public int Id { get; set; }

        public string Text { get; set; }
    }

    public class GeometryModel
    {
        public int Id { get; set; }

        public Point Point { get; set; }
    }

    #endregion

    #region Contexts

    public class Context1 : DbContext
    {
        public DbSet<GeometryModel> GeometryModel { get; set; }

        public Context1(DbContextOptions<Context1> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder builder) => builder.HasPostgresExtension("postgis");
    }

    public class Context2 : DbContext
    {
        public DbSet<TextModel> TextModel { get; set; }

        public Context2(DbContextOptions<Context2> options) : base(options) {}
    }

    #endregion
}