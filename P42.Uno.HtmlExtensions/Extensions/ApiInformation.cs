using System;
using System.Collections.Generic;
using Uno.Foundation.Logging;

namespace P42.Web.WebView2.Core;

public class ApiInformation
{
    internal static void TryRaiseNotImplemented(string type, string memberName, LogLevel errorLogLevelOverride = LogLevel.Error)
    {
        var message = $"The member {memberName} is not implemented. For more information, visit https://aka.platform.uno/notimplemented#m={Uri.EscapeDataString(type + "." + memberName)}";
        throw new NotImplementedException(message);
    }

}
