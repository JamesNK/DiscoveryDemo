using System.Text;

namespace Discovery.Internal;

public class ProtocolHelpers
{
    public static readonly BalancerAttributesKey<string> HostOverrideKey = new BalancerAttributesKey<string>("HostOverride");

    public static Status CreateStatusFromException(string summary, Exception ex, StatusCode? statusCode = null)
    {
        var exceptionMessage = ConvertToRpcExceptionMessage(ex);
        statusCode ??= StatusCode.Internal;

        return new Status(statusCode.Value, summary + " " + exceptionMessage, ex);
    }

    public static string ConvertToRpcExceptionMessage(Exception ex)
    {
        // RpcException doesn't allow for an inner exception. To ensure the user is getting enough information about the
        // error we will concatenate any inner exception messages together.
        return ex.InnerException == null ? $"{ex.GetType().Name}: {ex.Message}" : BuildErrorMessage(ex);
    }

    private static string BuildErrorMessage(Exception ex)
    {
        // Concatenate inner exceptions messages together.
        var sb = new StringBuilder();
        var first = true;
        Exception? current = ex;
        do
        {
            if (!first)
            {
                sb.Append(' ');
            }
            else
            {
                first = false;
            }
            sb.Append(current.GetType().Name);
            sb.Append(": ");
            sb.Append(current.Message);
        }
        while ((current = current.InnerException) != null);

        return sb.ToString();
    }
}
