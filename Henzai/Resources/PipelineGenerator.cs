using Veldrid;
using Henzai.Runtime;
using Henzai.Core.VertexGeometry;


namespace Henzai
{
    // TODO: Investigate splitting this up / VertexStruct abstraction
    public static class PipelineGenerator
    {

        /// <summary>
        /// For now this is only used with cube maps!
        /// </summary>
        public static GraphicsPipelineDescription GeneratePipelineP<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer,
            ResourceLayout[] effectLayouts) where T : struct, VertexLocateable {

            var resourceLayout = new ResourceLayout[] {
                    sceneRuntimeState.CameraResourceLayout,
                    modelRuntimeState.TextureResourceLayout};
            
            var completeResourceLayout = new ResourceLayout[resourceLayout.Length + effectLayouts.Length];
            resourceLayout.CopyTo(completeResourceLayout,0);
            effectLayouts.CopyTo(completeResourceLayout,resourceLayout.Length);

            return new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = rasterizerState,
                PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                ResourceLayouts = completeResourceLayout,
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: modelRuntimeState.VertexLayouts,
                    shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                ),
                Outputs = framebuffer.OutputDescription
            };
        }

        public static GraphicsPipelineDescription GeneratePipelinePN<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer,
            ResourceLayout[] effectLayouts) where T : struct, VertexLocateable {

            var resourceLayout = new ResourceLayout[] {
                    sceneRuntimeState.CameraResourceLayout,
                    sceneRuntimeState.LightResourceLayout,
                    sceneRuntimeState.MaterialResourceLayout};

            var completeResourceLayout = new ResourceLayout[resourceLayout.Length + effectLayouts.Length];
            resourceLayout.CopyTo(completeResourceLayout,0);
            effectLayouts.CopyTo(completeResourceLayout,resourceLayout.Length);

            return new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = rasterizerState,
                PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                ResourceLayouts = completeResourceLayout,
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: modelRuntimeState.VertexLayouts,
                    shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                ),
                Outputs = framebuffer.OutputDescription
            };
        }
        public static GraphicsPipelineDescription GeneratePipelinePNTTB<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer,
            ResourceLayout[] effectLayouts) where T : struct, VertexLocateable {

            var resourceLayout = new ResourceLayout[] {
                        sceneRuntimeState.CameraResourceLayout,
                        sceneRuntimeState.LightResourceLayout,
                        sceneRuntimeState.SpotLightResourceLayout,
                        sceneRuntimeState.MaterialResourceLayout,
                        modelRuntimeState.TextureResourceLayout};

            var completeResourceLayout = new ResourceLayout[resourceLayout.Length + effectLayouts.Length];
            resourceLayout.CopyTo(completeResourceLayout,0);
            effectLayouts.CopyTo(completeResourceLayout,resourceLayout.Length);

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = rasterizerState,
                    PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                    ResourceLayouts = completeResourceLayout,
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: modelRuntimeState.VertexLayouts,
                        shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                    ),
                    Outputs = framebuffer.OutputDescription
                };
        }
        public static GraphicsPipelineDescription GeneratePipelinePC<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer,
            ResourceLayout[] effectLayouts) where T : struct, VertexLocateable {

            var resourceLayout = new ResourceLayout[] {sceneRuntimeState.CameraResourceLayout};

            var completeResourceLayout = new ResourceLayout[resourceLayout.Length + effectLayouts.Length];
            resourceLayout.CopyTo(completeResourceLayout,0);
            effectLayouts.CopyTo(completeResourceLayout,resourceLayout.Length);

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = rasterizerState,
                    PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                    ResourceLayouts = completeResourceLayout,
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: modelRuntimeState.VertexLayouts,
                        shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                    ),
                    Outputs = framebuffer.OutputDescription
                };
        }
    
        public static GraphicsPipelineDescription GeneratePipelinePT<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState,
            RasterizerStateDescription rasterizerState, 
            Framebuffer framebuffer,
            ResourceLayout[] effectLayouts) where T : struct, VertexLocateable {

            var resourceLayout = new ResourceLayout[] {
                        sceneRuntimeState.CameraResourceLayout,
                        modelRuntimeState.TextureResourceLayout};

            var completeResourceLayout = new ResourceLayout[resourceLayout.Length + effectLayouts.Length];
            resourceLayout.CopyTo(completeResourceLayout,0);
            effectLayouts.CopyTo(completeResourceLayout,resourceLayout.Length);

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = rasterizerState,
                    PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                    ResourceLayouts = completeResourceLayout,
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: modelRuntimeState.VertexLayouts,
                        shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                    ),
                    Outputs = framebuffer.OutputDescription
            };
        }

        public static GraphicsPipelineDescription GenerateShadowMapPipeline<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            //TODO: Build resource layout outside and pass as parameter

            return new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = rasterizerState,
                PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                ResourceLayouts = new ResourceLayout[] {sceneRuntimeState.CameraResourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: modelRuntimeState.VertexPreEffectsLayouts,
                    shaders: new Shader[] {modelRuntimeState.VertexPreEffectsShader,modelRuntimeState.FragmentShadowMapShader}
                ),
                Outputs = framebuffer.OutputDescription
            };
        }        
    }
}