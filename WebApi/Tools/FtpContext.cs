namespace WebApi.Tools
{
    /// <summary>
    /// Context of a request. There is only one FTP context per request of the server. 
    /// Represents the general information about the request.
    /// </summary>
    public class FtpContext
    {
        public string User { get; set; }
        public string Pass { get; set; }
    }
}
