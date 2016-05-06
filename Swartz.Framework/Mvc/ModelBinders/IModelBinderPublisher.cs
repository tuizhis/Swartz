using System.Collections.Generic;

namespace Swartz.Mvc.ModelBinders
{
    public interface IModelBinderPublisher : IDependency
    {
        void Publish(IEnumerable<ModelBinderDescriptor> binders);
    }
}