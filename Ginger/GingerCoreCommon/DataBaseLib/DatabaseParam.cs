using System;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.DataBaseLib
{
    public class DatabaseParam : RepositoryItemBase
    {
        private string mName;
        [IsSerializedForLocalRepository]        
        public String Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(nameof(Name)); } } }

        private string mValue;
        [IsSerializedForLocalRepository]        
        public String Value { get { return mValue; } set { if (mValue != value) { mValue = value; OnPropertyChanged(nameof(Value)); } } }


        public override string ItemName
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }
    }
}
