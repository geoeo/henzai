using Henzai.Core;
using Henzai.Core.Materials;

namespace Henzai.Geometry{
    public class LoadedModels{
        public Model<VertexPosition, RealtimeMaterial> modelP {
            get{return _modelP;} set{_modelP = value;HasVertexPosition=true;}
            }
        public Model<VertexPositionTexture, RealtimeMaterial> modelPT {
            get{return _modelPT;} set{_modelPT = value;HasVertexPositionTexture=true;}
            }
        public Model<VertexPositionNormal, RealtimeMaterial> modelPN {
            get{return _modelPN;} set{_modelPN = value;HasVertexPositionNormal=true;}
            }
        public Model<VertexPositionColor, RealtimeMaterial> modelPC {
            get{return _modelPC;} set{_modelPC = value;HasVertexPositionColor=true;}
            }
        public Model<VertexPositionNDCColor, RealtimeMaterial> modelP_NDC_C {
           get{return _modelP_NDC_C;} set{_modelP_NDC_C = value;HasVertexPositionNDCColor=true;}
            }
        public Model<VertexPositionNormalTexture, RealtimeMaterial> modelPNT {
           get{return _modelPNT;} set{_modelPNT = value;HasVertexPositionNormalTexture=true;}
            }
        public Model<VertexPositionNormalTextureTangent, RealtimeMaterial> modelPNTT {
           get{return _modelPNTT;} set{_modelPNTT = value;HasVertexPositionNormalTextureTangent=true;}
            }
        public Model<VertexPositionNormalTextureTangentBitangent, RealtimeMaterial> modelPNTTB {
           get{return _modelPNTTB;} set{_modelPNTTB = value;HasVertexPositionNormalTextureTangentBitangentt=true;}
            }

        private Model<VertexPosition, RealtimeMaterial> _modelP;
        private Model<VertexPositionColor, RealtimeMaterial> _modelPC;
        private Model<VertexPositionNDCColor, RealtimeMaterial> _modelP_NDC_C;
        private Model<VertexPositionTexture, RealtimeMaterial> _modelPT;
        private Model<VertexPositionNormal, RealtimeMaterial> _modelPN;
        private Model<VertexPositionNormalTexture, RealtimeMaterial> _modelPNT;
        private Model<VertexPositionNormalTextureTangent, RealtimeMaterial> _modelPNTT;
        private Model<VertexPositionNormalTextureTangentBitangent, RealtimeMaterial> _modelPNTTB;

        public bool HasVertexPosition { get; private set; } = false;
        public bool HasVertexPositionTexture { get; private set; } = false;
        public bool HasVertexPositionNormal { get; private set; } = false;
        public bool HasVertexPositionColor { get; private set; } = false;
        public bool HasVertexPositionNDCColor { get; private set; } = false;
        public bool HasVertexPositionNormalTexture { get; private set; } = false;
        public bool HasVertexPositionNormalTextureTangent { get; private set; } = false;
        public bool HasVertexPositionNormalTextureTangentBitangentt { get; private set; } = false;

    }
}