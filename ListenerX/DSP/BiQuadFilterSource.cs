﻿using CSCore;
using CSCore.DSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListenerX.DSP
{
    public class BiQuadFilterSource : SampleAggregatorBase
    {
        private readonly object _lockObject = new object();
        private BiQuad _biquad;

        public BiQuad Filter
        {
            get { return _biquad; }
            set
            {
                lock (_lockObject)
                {
                    _biquad = value;
                }
            }
        }

        public BiQuadFilterSource(ISampleSource source) : base(source)
        {
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            lock (_lockObject)
            {
                if (Filter != null)
                {
                    for (int i = 0; i < read; i++)
                    {
                        buffer[i + offset] = Filter.Process(buffer[i + offset]);
                    }
                }
            }

            return read;
        }
    }

}
