namespace Ava.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Amadeus DbSets
    public DbSet<AmadeusOAuthToken> AmadeusOAuthTokens { get; set; }

    // TravelPayouts DbSets
    public DbSet<TPCityIATACode> TPCityIATACodes { get; set; }

    // Add DbSets for your models
    public DbSet<AvaClient> AvaClients { get; set; }
    public DbSet<AvaUser> AvaUsers { get; set; }
    public DbSet<AvaUserSysPreference> AvaUserSysPreferences { get; set; }
    public DbSet<AvaSystemLog> AvaSystemLogs { get; set; }
    public DbSet<AvaClientLicense> AvaClientLicenses { get; set; }
    public DbSet<AvaEmployeeRecord> AvaEmployees { get; set; }
    public DbSet<AvaSalesRecord> AvaSalesRecords { get; set; }
    public DbSet<AvaClientSupportedDomain> AvaClientSupportedDomains{ get; set; }
    public DbSet<AvaJwtTokenResponse> AvaJwtTokenResponses { get; set; }
    public DbSet<FlightSearchCriteria> FlightSearchCriterias { get; set; }

    // Custom DbSets
    public DbSet<IATAAirportCodes> IATAAirportCodes { get; set; }

    public DbSet<FlightOfferSearchRequestDTO> FlightOfferSearchRequestDTOs { get; set; }

    // Client Stuff


    // Attribs
    public DbSet<SupportedCountry> SupportedCountries { get; set; }
    public DbSet<SupportedCurrency> SupportedCurrencies { get; set; }
    public DbSet<SupportedDialCode> SupportedDialCodes { get; set; }
    public DbSet<SupportedTaxId> SupportedTaxIds { get; set; }
    


    // location stuff for travelpolicies
    public DbSet<TravelPolicy> TravelPolicies { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<Continent> Continents { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<TravelPolicyDisabledCountry> TravelPolicyDisabledCountries { get; set; }



    // Custom classes that are only intermediate code and will be removed in future updates
    public DbSet<QantasCustomIATACode> QantasCustomIATACodes { get; set; }
    public DbSet<QantasCustomBookingRequest> QantasCustomBookingRequest { get; set; }

    // Wikipedia classes
    public DbSet<AircraftTypeDesignator> AircraftTypeDesignators { get; set; }

    // GitHub
    public DbSet<GitHubRepoOAuthToken> GitHubRepoOAuthTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mark Coordinates as an Owned Entity
        modelBuilder.Entity<TPCityIATACode>()
            .OwnsOne(c => c.Coordinates);

        // Mark NameTranslations as an Owned Entity
        modelBuilder.Entity<TPCityIATACode>()
            .OwnsOne(c => c.NameTranslations);

        // Configure the one-to-many relationship between AvaClient and TravelPolicy.
        modelBuilder.Entity<TravelPolicy>()
            .HasOne(tp => tp.AvaClient)
            .WithMany(ac => ac.TravelPolicies)
            .HasForeignKey(tp => tp.AvaClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure the DefaultTravelPolicy relationship for AvaClient.
        modelBuilder.Entity<AvaClient>()
            .HasOne(ac => ac.DefaultTravelPolicy)
            .WithMany() // No navigation property on TravelPolicy for default.
            .HasForeignKey(ac => ac.DefaultTravelPolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure many-to-many relationships for TravelPolicy.
        modelBuilder.Entity<TravelPolicy>()
            .HasMany(tp => tp.Regions)
            .WithMany();

        modelBuilder.Entity<TravelPolicy>()
            .HasMany(tp => tp.Continents)
            .WithMany();
        
        modelBuilder.Entity<TravelPolicy>()
            .HasMany(tp => tp.Countries)
            .WithMany();

        // Explicit composite key configuration for disabled countries.
        modelBuilder.Entity<TravelPolicyDisabledCountry>()
            .HasKey(tpdc => new { tpdc.TravelPolicyId, tpdc.CountryId });

        // Configure one-to-many for Region and Continent.
        modelBuilder.Entity<Continent>()
            .HasOne(c => c.Region)
            .WithMany(r => r.Continents)
            .HasForeignKey(c => c.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure one-to-many for Continent and Country.
        modelBuilder.Entity<Country>()
            .HasOne(c => c.Continent)
            .WithMany(ct => ct.Countries)
            .HasForeignKey(c => c.ContinentId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
