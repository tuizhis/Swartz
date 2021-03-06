﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Swartz.Services
{
    public class ClientHostAddressAccessor : IClientHostAddressAccessor
    {
        private readonly IWorkContextAccessor _wca;

        public ClientHostAddressAccessor(IWorkContextAccessor wca)
        {
            _wca = wca;
        }

        /// <summary>
        ///     Indicates whether the client host address should be read from an HTTP header, specified via
        ///     <see cref="ClientHostAddressHeaderName" />.
        /// </summary>
        public bool EnableClientHostAddressHeader { get; set; }

        /// <summary>
        ///     The HTTP header name to read the client host address from if <see cref="EnableClientHostAddressHeader" /> is set to
        ///     true.
        ///     If the specified header was not found, the system will fall back to the user host address as provided by the
        ///     Request object.
        /// </summary>
        public string ClientHostAddressHeaderName { get; set; }

        public string GetClientAddress()
        {
            var workContext = _wca.GetContext();

            if (workContext?.HttpContext == null)
                return string.Empty;

            var request = workContext.HttpContext.Request;

            if (EnableClientHostAddressHeader && !string.IsNullOrWhiteSpace(ClientHostAddressHeaderName))
            {
                var headerName = ClientHostAddressHeaderName.Trim();
                var customAddresses = ParseAddresses(request.Headers[headerName]).ToArray();

                if (customAddresses.Any())
                    return customAddresses.First();
            }

            return request.UserHostAddress;
        }

        private static IEnumerable<string> ParseAddresses(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                ? value.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                : Enumerable.Empty<string>();
        }
    }
}