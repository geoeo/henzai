using System;
using Veldrid;
using Henzai;
using Henzai.Runtime.Render;

namespace Henzai
{
    public static class ResourceGenerator
    {

        public static ResourceLayout GenerateResourceLayout(DisposeCollectorResourceFactory factory, string name,ResourceKind resourceKind, ShaderStages shaderStages){
            var resourceLayoutElementDescription = new ResourceLayoutElementDescription(name,resourceKind,shaderStages);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions = {resourceLayoutElementDescription};
            var resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
            return factory.CreateResourceLayout(resourceLayoutDescription);
        }

        public static ResourceSet GenrateResourceSet(DisposeCollectorResourceFactory factory, ResourceLayout resourceLayout, BindableResource[] bindableResources){
            ResourceSetDescription resourceSetDescription = new ResourceSetDescription(resourceLayout,bindableResources);
            return factory.CreateResourceSet(resourceSetDescription);
        }
    }
}