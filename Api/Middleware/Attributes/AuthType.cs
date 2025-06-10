namespace Api.Middleware.Attributes;

public enum AuthType
{
    None = 0,
    ApiKey = 1,
    Basic = 2,
    Bearer = 3,
}