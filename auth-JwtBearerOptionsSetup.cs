namespace [namespace];
internal sealed class JwtBearerOptionsSetup : IConfigureOptions<JwtBearerOptions>
{
    public IOptions<JwtOptions> _options;
    public JwtBearerOptionsSetup(IOptions<JwtOptions> options) => _options = options;
    public void Configure(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new()
        {
            ClockSkew = new TimeSpan(0, 0, 30),
            ValidateIssuer = true, ValidIssuer = _options.Value.Issuer,
            ValidateAudience = true, ValidAudience = _options.Value.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.Value.SigningKey.ToCharArray()))
        };
        options.Events = new JwtBearerEvents()
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;

                if (string.IsNullOrEmpty(ctx.Error)) ctx.Error = "invalid_token";
                if (string.IsNullOrEmpty(ctx.ErrorDescription)) ctx.ErrorDescription = "A valid security token is required";

                if (ctx.AuthenticateFailure != null && ctx.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
                {
                    var authException = ctx.AuthenticateFailure as SecurityTokenExpiredException;
                    ctx.Response.Headers.Add("x-token-expired", authException!.Expires.ToString("o"));
                    ctx.ErrorDescription = $"Expired on: {authException.Expires:o}";
                }

                return ctx.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = ctx.Error,
                    error_description = ctx.ErrorDescription
                }));
            }
         };
    }
}

// in the same assembly:
namespace [namespace];
internal sealed class JwtOptions
{
    public string SigningKey { get; internal init; }
    public string Issuer { get; internal init; }
    public string Audience { get; internal init; }
}
namespace [namespace];
internal sealed class JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
    private readonly IConfiguration _config;
    private readonly string _sectionName;
    public JwtOptionsSetup(IConfiguration config, string sectionName = "AppSettings:Jwt")
    {
        (_config,_sectionName) = (config, sectionName);
    }
    public void Configure(JwtOptions options)
    {
        _config.GetSection(_sectionName).Bind(options);
        
    }
}
