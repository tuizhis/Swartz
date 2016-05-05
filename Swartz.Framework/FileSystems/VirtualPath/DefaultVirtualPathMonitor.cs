using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using Swartz.Caching;

namespace Swartz.FileSystems.VirtualPath
{
    public class DefaultVirtualPathMonitor : IVirtualPathMonitor
    {
        public IVolatileToken WhenPathChanges(string virtualPath)
        {
            throw new NotImplementedException();
        }

        public void Signal(string key, object value, CacheItemRemovedReason reason)
        {
            var virtualPath = Convert.ToString(value);
            
            
        }

        public class Token : IVolatileToken
        {
            public Token(string virtualPath)
            {
                IsCurrent = true;
                VirtualPath = virtualPath;
            }

            public bool IsCurrent { get; set; }
            public string VirtualPath { get; }

            public override string ToString()
            {
                return $"IsCurrent: {IsCurrent}, VirtualPath: \"{VirtualPath}\"";
            }
        }

        class Thunk
        {
            private readonly Weak<DefaultVirtualPathMonitor> _weak;

            public Thunk(DefaultVirtualPathMonitor provider)
            {
                _weak = new Weak<DefaultVirtualPathMonitor>(provider);
            }

            public void Signal(string key, object value, CacheItemRemovedReason reason)
            {
                var provider = _weak.Target;
                provider?.Signal(key, value, reason);
            }
        }
    }
}
