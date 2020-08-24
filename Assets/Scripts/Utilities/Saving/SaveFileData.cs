using System;

namespace StarSalvager.Utilities.Saving
{
    public struct SaveFileData : IEquatable<SaveFileData>
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }

        public string FilePath { get; set; }


        #region IEquatable

        public bool Equals(SaveFileData other)
        {
            return Name == other.Name && Date.Equals(other.Date);
        }

        public override bool Equals(object obj)
        {
            return obj is SaveFileData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Date.GetHashCode();
            }
        }

        #endregion //IEquatable

    }
}


