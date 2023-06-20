namespace BtcTrader.Domain.User.Exceptions;

[Serializable]
public class UserNotFoundException : Exception
{
    public UserNotFoundException ()
    {}
    public UserNotFoundException (string message) 
        : base()
    {}

    public UserNotFoundException (string message, Exception innerException)
        : base (message, innerException)
    {}    
}

