using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BookLib.Conventions
{
    //自定义ApiExplorerSetting Arribute的GroupName与Api的version对应约定
    public class ApiExplorerGroupPerVersionConvention: IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var controllerNamespace = controller.ControllerType.Namespace;
            var apiVersion = controllerNamespace.ToLower()
                .Split('.')
                .FirstOrDefault(m => m == "v1" || m == "v2");

            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                apiVersion = "v1";
            }

            controller.ApiExplorer.GroupName = apiVersion;
        }
    }
}
