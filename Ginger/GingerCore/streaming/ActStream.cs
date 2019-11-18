using Amdocs.Ginger.Common;
using Ginger.Reports;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using GingerCore.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.streaming
{
    public class ActStream : RunSetActionBase
    {
        public override bool SupportRunOnConfig => throw new NotImplementedException();

        public override string Type { get { return "Live stream"; } }

        public override void Execute(ReportInfo RI)
        {
            ImageStreamingServer server = new ImageStreamingServer();
            server.Start();


        }

        public override string GetEditPage()
        {
            return "";
        }

        public override List<eRunAt> GetRunOptions()
        {
            List<eRunAt> runat = new List<eRunAt>();

         runat.Add(eRunAt.ExecutionEnd);
            return  runat; 
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
           
        }
    }
}
