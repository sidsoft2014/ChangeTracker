using System;

namespace ChangeTracker.Models
{
    [Serializable]
    public sealed class HistoryRecord : IEquatable<HistoryRecord>
    {
        public string Directory { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string TimeSpent
        {
            get
            {
                if (Start == default(DateTime) || End == default(DateTime))
                    return "00:00";

                TimeSpan span = End - Start;
                string result = span.ToString().Remove(5);

                return result;
            }
        }
        public int ChangedFilesCount { get; set; }

        public bool Equals(HistoryRecord other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as HistoryRecord;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}: {2}", Start, End, Directory);
        }
    }
}
