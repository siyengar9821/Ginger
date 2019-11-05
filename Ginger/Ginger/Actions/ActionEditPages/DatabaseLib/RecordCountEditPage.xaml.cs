using Amdocs.Ginger.Common;
using GingerCore.Actions;
using System.Windows.Controls;

namespace Ginger.Actions.ActionEditPages.DatabaseLib
{
    /// <summary>
    /// Interaction logic for RecordCountEditPage.xaml
    /// </summary>
    public partial class RecordCountEditPage : Page
    {
        ActDBValidation mAct;

        public RecordCountEditPage(ActDBValidation act)
        {
            InitializeComponent();

            mAct = act;
            xSQLUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActDBValidation.SQL));
            

            //checkQueryType();
            //try
            //{
            //    string DBName = DBNameComboBox.Text;
            //    db = (Database) (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            //    if (!(db == null))
            //    {
            //        if (db.DBType == Database.eDBTypes.Cassandra)
            //        {
            //            Keyspace.Visibility = Visibility.Visible;
            //        }
            //        else
            //        {
            //            Keyspace.Visibility = Visibility.Collapsed;
            //        }
            //    }
            //}
            //catch { }
            //RadioButtonsSection.Visibility = Visibility.Collapsed;
            //FreeSQLStackPanel.Visibility = Visibility.Visible;
            //TableColWhereStackPanel.Visibility = Visibility.Collapsed;
            //DoCommit.Visibility = Visibility.Collapsed;
            //SqlFile.Visibility = Visibility.Collapsed;
            //FreeSQLLabel.Content = @"Record count - SELECT COUNT(1) FROM {Table} - Enter only Table name below (+optional WHERE clause)";
        }

    }
}
