using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmoteka_WPF
{
    /// <summary>
    /// Represents errors that occur during film-related operations.
    /// </summary>
    /// <remarks>This exception is intended for internal use to signal issues specific to film processing or
    /// management. Use this exception to distinguish film-specific errors from general exceptions within the
    /// application.</remarks>
    internal class FilmException : Exception
    {
        public FilmException() : base() { }
        public FilmException(string message) : base(message) { }
        public FilmException(string message, Exception innerException) : base(message, innerException) { }
    }
}
