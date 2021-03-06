﻿using System;
using System.Collections.Generic;

namespace StarSalvager.Utilities.Analytics.SessionTracking.Data
{
    [Serializable]
    public struct SessionData
    {
        public Version Version;
        public string PlayerID;
        public DateTime date;
        public List<WaveData> waves;

        public SessionSummaryData GetSessionSummary()
        {
            return new SessionSummaryData("Session Summary", this);
        }
    }
}