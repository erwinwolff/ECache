using System;

namespace BlueTiger.ElasticCache.Exceptions
{
    public class ECacheEntryNotFoundException : Exception
    {
        internal ECacheEntryNotFoundException(string message) :
            base (message)
        {
        }

        private ECacheEntryNotFoundException() { }
    }
}