//-----------------------------------------------------------------------
// <copyright file="GeometricNetworkException.cs" company="Studio A&T s.r.l.">
//  Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// class Geometric Network Exception
    /// </summary>
    [Serializable]
    public class GeometricNetworkException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the GeometricNetworkException class
        /// </summary>
        public GeometricNetworkException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the GeometricNetworkException class
        /// </summary>
        /// <param name="message">message error</param>
        public GeometricNetworkException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GeometricNetworkException class
        /// </summary>
        /// <param name="message">message error</param>
        /// <param name="innerException">object Exception</param>
        public GeometricNetworkException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GeometricNetworkException class
        /// </summary>
        /// <param name="info">object SerializationInfo</param>
        /// <param name="context">object StreamingContext</param>
        protected GeometricNetworkException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
