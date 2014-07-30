using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPomodoro
{
    class TimeKeeper
    {


        public event Action<TimeKeeper> UpdateMax;
        public event Action<TimeKeeper> UpdateCount;
        public event Action<TimeKeeper> TimerStop;

        private int max = -1;
        private int count = -1;

        public TimeKeeper(int max)
        {
            this.max = max;
            this.count = 0;
        }

        public void Reset()
        {
            Count = 0;
        }


        public int Max
        {
            get { return max; }
            set
            {
                if (max != value)
                {
                    if (value >= 0)
                    {
                        max = value;
                    }

                    if (UpdateMax != null)
                    {
                        UpdateMax(this);
                    }
                }
            }
        }

        public int Count
        {
            get { return count; }
            set
            {
                if (count != value)
                {
                    count = value;
                    if (UpdateCount != null)
                    {
                        UpdateCount(this);
                    }
                    if (max <= count && TimerStop != null)
                    {
                        TimerStop(this);
                    }
                }
            }
        }

        public int TimeLeft()
        {
            return max - count;
        }
    }
}
