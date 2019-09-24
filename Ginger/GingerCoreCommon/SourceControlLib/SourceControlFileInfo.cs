#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;

namespace GingerCoreNET.SourceControl
{
    public class SourceControlFileInfo 
    {      

        public enum eRepositoryItemStatus
        {
            New,
            Equel,
            Modified,
            [EnumValueDescription("Modified And Resolved")]
            ModifiedAndResolved,
            Deleted,
            Unknown,
            LockedByMe,
            LockedByAnotherUser
        }

        public eImageType ImageType
        {
            get
            {
                switch (Status)
                {
                    case eRepositoryItemStatus.New:
                        return eImageType.SourceControlNew;

                    case eRepositoryItemStatus.Modified:
                        return eImageType.SourceControlModified;

                    case eRepositoryItemStatus.Equel:
                        return eImageType.SourceControlEquel;

                    case eRepositoryItemStatus.LockedByMe:
                        return eImageType.SourceControlLockedByMe;

                    case eRepositoryItemStatus.LockedByAnotherUser:
                        return eImageType.SourceControlLockedByAnotherUser;

                    case eRepositoryItemStatus.Unknown:
                        return eImageType.SourceControlError;

                    default:
                        return eImageType.SourceControlDeleted;
                }
            }
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public string SolutionPath { get; set; }
        public string FileType { get; set; }
        public eRepositoryItemStatus Status { get; set; }
        public bool Selected { get; set; }
        public bool Locked { get; set; }
        public string LockedOwner { get; set; }
        public string LockComment { get; set; }
        public string Diff { get; set; }
      
    }
}
