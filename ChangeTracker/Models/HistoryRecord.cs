using System;

namespace ChangeTracker.Models
{
    [Serializable]
    public sealed class HistoryRecord : IEquatable<HistoryRecord>
    {
        public string Directory { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string TimeSpentAsString
        {
            get
            {
                int hours = TimeSpentAsTimeSpan.Hours;
                int minutes = TimeSpentAsTimeSpan.Minutes;

                if (minutes < 1)
                    minutes = 0;
                else if (minutes < 15)
                    minutes = 15;
                else if (minutes < 30)
                    minutes = 30;
                else if (minutes < 45)
                    minutes = 45;
                else
                {
                    minutes = 0;
                    ++hours;
                }

                return new TimeSpan(hours, minutes, 0).ToString().Remove(5);
            }
        }

        public TimeSpan TimeSpentAsTimeSpan
        {
            get
            {
                if (Start >= End)
                    return new TimeSpan(0, 0, 0);

                TimeSpan span = End - Start;
                return span;
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
