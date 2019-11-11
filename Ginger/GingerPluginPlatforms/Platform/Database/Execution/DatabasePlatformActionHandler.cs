using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using Ginger.Plugin.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.DatabaseLib.Execution
{
    public class DatabasePlatformActionHandler : IPlatformActionHandler
    {

        IDatabase Platformservice;

        public void HandleRunAction(IPlatformService service, ref NodePlatformAction platformAction)
        {
            Platformservice = (IDatabase)service;

            //RestClient = Platformservice.RestClient;

            //try
            //{
            //    GingerHttpRequestMessage Request = GetRequest(platformAction);

            //    GingerHttpResponseMessage Response = RestClient.PerformHttpOperation(Request);
            //    platformAction.Output.Add("Header: Status Code ", Response.StatusCode.ToString());

            //    foreach (var RespHeader in Response.Headers)
            //    {
            //        platformAction.Output.Add("Header: " + RespHeader.Key, RespHeader.Value);
            //    }

            //    platformAction.Output.Add("Request:", Response.RequestBodyString);
            //    platformAction.Output.Add("Response:", Response.Resposne);

            //}

            //catch (Exception ex)
            //{
            //    platformAction.addError(ex.Message);
            //}
        }
    }
}
