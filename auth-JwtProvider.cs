namespace Bristows.TRACR.API.AuthenticationTemplate;
internal sealed class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _config;
    private readonly JwtOptions _optionsVal;
    private readonly ILogger<JwtProvider> _logger;
    public JwtProvider(IConfiguration configuration, IOptions<JwtOptions> options, ILogger<JwtProvider> logger)
    {
        (_logger, _optionsVal, _config) = (logger, options.Value, configuration);
    }
    public async string BuildToken(User user)
    {
        Claim[] claims = new Claim[] {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email),
            new (JwtRegisteredClaimNames.Sub, user.Id)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _optionsVal.SigningKey.ToCharArray() ?? _config["AppSettings:Jwt:SigningKey"]!.ToCharArray() 
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


// in the same assembly
public interface IJwtProvider
{
    public string BuildToken(User user);
}
public class User
{
    public string Email { get; internal init; }
    public string Id { get; internal init; }
    public object? Value { get; internal init; }
}
internal sealed class JwtOptions
{
    public string SigningKey { get; internal init; }
    public string Issuer { get; internal init; }
    public string Audience { get; internal init; }
}