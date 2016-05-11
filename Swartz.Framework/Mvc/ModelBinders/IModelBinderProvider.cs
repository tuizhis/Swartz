using System.Collections.Generic;

namespace Swartz.Mvc.ModelBinders
{
    public interface IModelBinderProvider : IDependency
    {
        IEnumerable<ModelBinderDescriptor> GetModelBinders();
    }
}