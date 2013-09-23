using System;
using System.Collections.Generic;

namespace Craftitude
{
    public class RepositoryConfiguration
    {
        public Uri Uri { get; set; }

        public IEnumerable<string> Subscriptions { get; set; }
    }
}