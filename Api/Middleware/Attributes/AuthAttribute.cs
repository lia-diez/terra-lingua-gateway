namespace Api.Middleware.Attributes;

public class AuthAttribute : Attribute
{
    public AuthType Type { get; set; }

    public AuthAttribute(AuthType type = AuthType.None)
    {
        Type = type;
    }
}