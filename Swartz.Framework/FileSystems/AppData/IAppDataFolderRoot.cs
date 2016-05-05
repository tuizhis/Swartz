using System.Web.Hosting;

namespace Swartz.FileSystems.AppData
{
    public interface IAppDataFolderRoot : ISingletonDependency
    {
        /// <summary>
        ///     Virtual path of root ("~/App_Data")
        /// </summary>
        string RootPath { get; }

        /// <summary>
        ///     Physical path of root (typically: MapPath(RootPath))
        /// </summary>
        string RootFolder { get; }
    }

    public class AppDataFolderRoot : IAppDataFolderRoot
    {
        public string RootPath => "~/App_Data";
        public string RootFolder => HostingEnvironment.MapPath(RootPath);
    }
}