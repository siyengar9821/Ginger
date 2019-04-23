﻿using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Plugin.Core;
using System;
using System.Timers;

namespace UnitTests.RecordingLibTest
{
    public class TestDriver : IRecord
    {
        public bool LearnAdditionalDetails { get; set; }
        public event ElementRecordedEventHandler ElementRecorded;
        public event PageChangedHandler PageChanged;
        public Timer mGetRecordingTimer;
        int i = 0;

        public void StartRecording()
        {
            i = 0;
            mGetRecordingTimer = new Timer(1000);
            mGetRecordingTimer.Elapsed += MGetRecordingTimer_Elapsed;
            mGetRecordingTimer.Start();
        }

        private void MGetRecordingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DoRecording();
        }

        private void DoRecording()
        {
            string name = "Name_" + Convert.ToString(i);

            ElementInfo eInfo = new ElementInfo();
            eInfo.ElementName = name;

            ElementActionCongifuration eleArgs = new ElementActionCongifuration();
            if (i == 1 || i == 3)
            {
                eleArgs.LocateBy = eLocateBy.ByName.ToString();
                eleArgs.LocateValue = name;
                eleArgs.ElementValue = "aaa";
                eleArgs.Operation = "SetText";
                eleArgs.Type = "TextBox";
                eleArgs.Description = "input Text " + name;
                eInfo.ElementTypeEnum = eElementType.TextBox;
            }
            else
            {
                eleArgs.LocateBy = eLocateBy.ByID.ToString();
                eleArgs.LocateValue = name;
                eleArgs.ElementValue = "cc";
                eleArgs.Operation = "Click";
                eleArgs.Type = "Button";
                eleArgs.Description = "input button " + name;
                eInfo.ElementTypeEnum = eElementType.Button;
            }

            if (LearnAdditionalDetails)
            {
                eleArgs.LearnedElementInfo = eInfo;
            }

            PageChangedEventArgs pageArgs = new PageChangedEventArgs();
            if (i != 2)
            {
                pageArgs.PageURL = "www.google.com";
                pageArgs.PageTitle = "Google";
            }
            else
            {
                pageArgs.PageURL = "www.new.com";
                pageArgs.PageTitle = "New";
            }

            OnPageChanged(pageArgs);

            OnLearnedElement(eleArgs);
            i++;
        }

        public void StopRecording()
        {
            if (mGetRecordingTimer != null)
            {
                mGetRecordingTimer.Stop();
                mGetRecordingTimer.Dispose();
            }
        }

        protected void OnLearnedElement(ElementActionCongifuration e)
        {
            ElementRecorded?.Invoke(this, e);
        }

        protected void OnPageChanged(PageChangedEventArgs e)
        {
            PageChanged?.Invoke(this, e);
        }
    }
}
