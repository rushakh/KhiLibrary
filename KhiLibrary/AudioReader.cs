using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhiLibrary
{
    // For now This is Not used, But I plan to use this as way of lowering the responsibilities of MusicPlayer class and seperating the functionalities even more.
    internal class AudioReader : IWaveProvider
    {
        public WaveFormat WaveFormat => throw new NotImplementedException();

        public int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
