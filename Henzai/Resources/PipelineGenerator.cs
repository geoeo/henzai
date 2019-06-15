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
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            return new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = rasterizerState,
                PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                ResourceLayouts = new ResourceLayout[] {
                    sceneRuntimeState.CameraResourceLayout,
                    modelRuntimeState.TextureResourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: modelRuntimeState.VertexLayouts,
                    shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                ),
                Outputs = framebuffer.OutputDescription
            };
        }

        public static GraphicsPipelineDescription GenerateShadowMapPipelineP<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            return new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = rasterizerState,
                PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                ResourceLayouts = new ResourceLayout[] {sceneRuntimeState.CameraResourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: modelRuntimeState.VertexLayouts,
                    shaders: new Shader[] {modelRuntimeState.VertexShadowMapShader,modelRuntimeState.FragmentShadowMapShader}
                ),
                Outputs = framebuffer.OutputDescription
            };
        }

        public static GraphicsPipelineDescription GeneratePipelinePN<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            return new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = rasterizerState,
                PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                ResourceLayouts = new ResourceLayout[] {
                    sceneRuntimeState.CameraResourceLayout,
                    sceneRuntimeState.LightResourceLayout,
                    sceneRuntimeState.MaterialResourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: modelRuntimeState.VertexLayouts,
                    shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                ),
                Outputs = framebuffer.OutputDescription
            };
        }

        public static GraphicsPipelineDescription GenerateShadowMapPipelinePN<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            return new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = rasterizerState,
                PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                ResourceLayouts = new ResourceLayout[] {sceneRuntimeState.CameraResourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: modelRuntimeState.VertexLayouts,
                    shaders: new Shader[] {modelRuntimeState.VertexShadowMapShader,modelRuntimeState.FragmentShadowMapShader}
                ),
                Outputs = framebuffer.OutputDescription
            };
        }

        public static GraphicsPipelineDescription GeneratePipelinePNTTB<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = rasterizerState,
                    PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                    ResourceLayouts = new ResourceLayout[] {
                        sceneRuntimeState.CameraResourceLayout,
                        sceneRuntimeState.LightResourceLayout,
                        sceneRuntimeState.SpotLightResourceLayout,
                        sceneRuntimeState.MaterialResourceLayout,
                        modelRuntimeState.TextureResourceLayout},
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: modelRuntimeState.VertexLayouts,
                        shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                    ),
                    Outputs = framebuffer.OutputDescription
                };
        }

        public static GraphicsPipelineDescription GenerateShadowMapPipelinePNTTB<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = rasterizerState,
                    PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                    ResourceLayouts = new ResourceLayout[] {sceneRuntimeState.CameraResourceLayout},
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: modelRuntimeState.VertexLayouts,
                        shaders: new Shader[] {modelRuntimeState.VertexShadowMapShader,modelRuntimeState.FragmentShadowMapShader}
                    ),
                    Outputs = framebuffer.OutputDescription
                };
        }

        public static GraphicsPipelineDescription GeneratePipelinePC<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = rasterizerState,
                    PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                    ResourceLayouts = new ResourceLayout[] {
                        sceneRuntimeState.CameraResourceLayout},
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: modelRuntimeState.VertexLayouts,
                        shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                    ),
                    Outputs = framebuffer.OutputDescription
                };
        }

        public static GraphicsPipelineDescription GenerateShadowMapPipelinePC<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = rasterizerState,
                    PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                    ResourceLayouts = new ResourceLayout[] {sceneRuntimeState.CameraResourceLayout},
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: modelRuntimeState.VertexLayouts,
                        shaders: new Shader[] {modelRuntimeState.VertexShadowMapShader,modelRuntimeState.FragmentShadowMapShader}
                    ),
                    Outputs = framebuffer.OutputDescription
                };
        }

    
        public static GraphicsPipelineDescription GeneratePipelinePT<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState,
            RasterizerStateDescription rasterizerState, 
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = rasterizerState,
                    PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                    ResourceLayouts = new ResourceLayout[] {
                        sceneRuntimeState.CameraResourceLayout,
                        modelRuntimeState.TextureResourceLayout},
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: modelRuntimeState.VertexLayouts,
                        shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                    ),
                    Outputs = framebuffer.OutputDescription
            };
        }

        public static GraphicsPipelineDescription GenerateShadowMapPipelinePT<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            RasterizerStateDescription rasterizerState,
            Framebuffer framebuffer) where T : struct, VertexLocateable {

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = rasterizerState,
                    PrimitiveTopology = modelRuntimeState.PrimitiveTopology,
                    ResourceLayouts = new ResourceLayout[] {sceneRuntimeState.CameraResourceLayout},
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: modelRuntimeState.VertexLayouts,
                        shaders: new Shader[] {modelRuntimeState.VertexShadowMapShader,modelRuntimeState.FragmentShadowMapShader}
                    ),
                    Outputs = framebuffer.OutputDescription
            };
        }

    }
}