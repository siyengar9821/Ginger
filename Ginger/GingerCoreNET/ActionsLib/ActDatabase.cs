using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.PlugIns;
using GingerCore.Platforms;

namespace GingerCore.Actions
{
    public class ActDatabase : ActPlugIn, IActPluginExecution
    {
        public PlatformAction GetAsPlatformAction()
        {
            PlatformAction platformAction = new PlatformAction(this);

            foreach (ActInputValue aiv in this.InputValues)
            {
                if (!platformAction.InputParams.ContainsKey(aiv.Param))
                {
                    platformAction.InputParams.Add(aiv.Param, aiv.ValueForDriver);
                }
            }


            return platformAction;
        }

        public string GetName()
        {
            return "DatabaseAction";
        }

        // run SP
        //DataTable dt = new DataTable();
        
        //    using (SqlConnection cn = new SqlConnection(Connectionstring))
        //    using (SqlCommand sqlcom = new SqlCommand("sp1", cn))
        //    using (SqlDataAdapter sdaG = new SqlDataAdapter(sqlcom))
        //    {
        //        sqlcom.CommandType = CommandType.StoredProcedure;
        //        cn.Open();
        //        sdaG.Fill(dt);
        //        return dt;
        //    }

    }
}
