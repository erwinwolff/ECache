using System;

namespace BlueTiger.ElasticCache.Exceptions
{
    public class ECacheIndexDoesNotExistException : Exception
    {
        internal ECacheIndexDoesNotExistException(string message) :
            base(message)
        {
        }

        private ECacheIndexDoesNotExistException() { }
    }
}