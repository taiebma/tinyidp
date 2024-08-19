
namespace tinyidp.Exceptions;

public class TinyidpKeyException : Exception
{
    public TinyidpKeyException(string message) : base(message)
    {}
    public TinyidpKeyException(string message, Exception ex) : base(message, ex)
    {}
}

public class TinyidpTokenException : Exception
{
    public string error_description {get; } = "";

    public TinyidpTokenException(string message) : base(message)
    {}
    public TinyidpTokenException(string message, string desc) : base(message)
    { 
        error_description = desc;
    }

    public TinyidpTokenException(string message, Exception ex) : base(message, ex)
    {}
    public TinyidpTokenException(string message, string desc, Exception ex) : base(message, ex)
    {
        error_description = desc;
    }
}

public class TinyidpCredentialException : Exception
{
    public string error_description {get; } = "";

    public TinyidpCredentialException(string message) : base(message)
    {}
    public TinyidpCredentialException(string message, string desc) : base(message)
    { 
        error_description = desc;
    }

    public TinyidpCredentialException(string message, Exception ex) : base(message, ex)
    {}
    public TinyidpCredentialException(string message, string desc, Exception ex) : base(message, ex)
    {
        error_description = desc;
    }
}
