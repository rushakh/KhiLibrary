﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhiLibrary
{
    internal class AudioReader : IWaveProvider
    {
        public WaveFormat WaveFormat => throw new NotImplementedException();

        public int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
