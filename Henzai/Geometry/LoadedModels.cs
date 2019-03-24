using Henzai.Core;

namespace Henzai.Geometry{
    public class LoadedModels{
        public Model<VertexPosition, Material> modelP {
            get{return _modelP;} set{_modelP = value;HasVertexPosition=true;}
            }
        public Model<VertexPositionTexture, Material> modelPT {
            get{return _modelPT;} set{_modelPT = value;HasVertexPositionTexture=true;}
            }
        public Model<VertexPositionNormal, Material> modelPN {
            get{return _modelPN;} set{_modelPN = value;HasVertexPositionNormal=true;}
            }
        public Model<VertexPositionColor, Material> modelPC {
            get{return _modelPC;} set{_modelPC = value;HasVertexPositionColor=true;}
            }
        public Model<VertexPositionNDCColor, Material> modelP_NDC_C {
           get{return _modelP_NDC_C;} set{_modelP_NDC_C = value;HasVertexPositionNDCColor=true;}
            }
        public Model<VertexPositionNormalTexture, Material> modelPNT {
           get{return _modelPNT;} set{_modelPNT = value;HasVertexPositionNormalTexture=true;}
            }
        public Model<VertexPositionNormalTextureTangent, Material> modelPNTT {
           get{return _modelPNTT;} set{_modelPNTT = value;HasVertexPositionNormalTextureTangent=true;}
            }
        public Model<VertexPositionNormalTextureTangentBitangent, Material> modelPNTTB {
           get{return _modelPNTTB;} set{_modelPNTTB = value;HasVertexPositionNormalTextureTangentBitangentt=true;}
            }

        private Model<VertexPosition, Material> _modelP;
        private Model<VertexPositionColor, Material> _modelPC;
        private Model<VertexPositionNDCColor, Material> _modelP_NDC_C;
        private Model<VertexPositionTexture, Material> _modelPT;
        private Model<VertexPositionNormal, Material> _modelPN;
        private Model<VertexPositionNormalTexture, Material> _modelPNT;
        private Model<VertexPositionNormalTextureTangent, Material> _modelPNTT;
        private Model<VertexPositionNormalTextureTangentBitangent, Material> _modelPNTTB;

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