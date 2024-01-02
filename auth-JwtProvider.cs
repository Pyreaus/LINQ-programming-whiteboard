namespace [namespace];
internal sealed partial class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _config;
    private readonly JwtOptions _optionsVal;
    private readonly ILogger<JwtProvider> _logger;
    public JwtProvider(IConfiguration configuration, IOptions<JwtOptions> options, ILogger<JwtProvider> logger)
    {
        (_logger, _optionsVal, _config) = (logger, options.Value, configuration);
    }
    public string BuildToken(string path = "Jwt", params User[] user)
    {
        Claim[] claims = new Claim[] {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Email, user[0].Email),
            new (JwtRegisteredClaimNames.Sub, user[0].Id),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _optionsVal.SigningKey.ToCharArray() ?? _config[$"{path}:Key"]!.ToCharArray() 
        ));                                                    
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var JWT = new JwtSecurityToken(
            _optionsVal.Issuer, 
            _optionsVal.Audience, 
            claims: claims, 
            signingCredentials: credentials,
            expires: DateTime.UtcNow.AddHours(1)
        );
        var serializedJWT = new JwtSecurityTokenHandler().WriteToken(JWT);

        _logger.LogInformation(100, "{type} instance", typeof(JwtSecurityTokenHandler));
        
        return serializedJWT;
    }
}

// in the same assembly:
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
namespace [namespace];
public class User
{
    public string Email { get; internal init; }
    public string Id { get; internal init; }
    public object? Value { get; internal init; }
}
namespace [namespace];
internal sealed class JwtOptions
{
    public string SigningKey { get; internal init; }
    public string Issuer { get; internal init; }
    public string Audience { get; internal init; }
}
namespace [namespace];
public interface IJwtProvider
{
    public string BuildToken(User user);
}
