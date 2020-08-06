using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public interface IRDSObject
    {
        double rdsProbability { get; set; } // The chance for this item to drop
        bool rdsUnique { get; set; }        // Only drops once per query
        bool rdsAlways { get; set; }        // Drops always
        bool rdsEnabled { get; set; }       // Can it drop now?

        /// <summary>
        /// Occurs before all the probabilities of all items of the current RDSTable
        /// are summed up together.
        /// This is the moment to modify any settings immediately before a result is calculated.
        /// </summary>
        event EventHandler rdsPreResultEvaluation;
        /// <summary>
        /// Occurs when this RDSObject has been hit by the Result procedure.
        /// (This means, this object will be part of the result set).
        /// </summary>
        event EventHandler rdsHit;
        /// <summary>
        /// Occurs after the result has been calculated and the result set is complete, but before
        /// the RDSTable's Result method exits.
        /// </summary>
        //event ResultEventHandler rdsPostResultEvaluation;

        void OnRDSPreResultEvaluation(EventArgs e);
        void OnRDSHit(EventArgs e);
        //void OnRDSPostResultEvaluation(ResultEventArgs e);
    }
}