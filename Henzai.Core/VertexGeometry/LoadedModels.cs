using Henzai.Core.Materials;

namespace Henzai.Core.VertexGeometry
{
    public class LoadedModels<U>{
        public Model<VertexPosition, U> modelP {
            get{return _modelP;} set{_modelP = value;HasVertexPosition=true;}
            }
        public Model<VertexPositionTexture, U> modelPT {
            get{return _modelPT;} set{_modelPT = value;HasVertexPositionTexture=true;}
            }
        public Model<VertexPositionNormal, U> modelPN {
            get{return _modelPN;} set{_modelPN = value;HasVertexPositionNormal=true;}
            }
        public Model<VertexPositionColor, U> modelPC {
            get{return _modelPC;} set{_modelPC = value;HasVertexPositionColor=true;}
            }
        public Model<VertexPositionNDCColor, U> modelP_NDC_C {
           get{return _modelP_NDC_C;} set{_modelP_NDC_C = value;HasVertexPositionNDCColor=true;}
            }
        public Model<VertexPositionNormalTexture, U> modelPNT {
           get{return _modelPNT;} set{_modelPNT = value;HasVertexPositionNormalTexture=true;}
            }
        public Model<VertexPositionNormalTextureTangent, U> modelPNTT {
           get{return _modelPNTT;} set{_modelPNTT = value;HasVertexPositionNormalTextureTangent=true;}
            }
        public Model<VertexPositionNormalTextureTangentBitangent, U> modelPNTTB {
           get{return _modelPNTTB;} set{_modelPNTTB = value;HasVertexPositionNormalTextureTangentBitangentt=true;}
            }

        private Model<VertexPosition, U> _modelP;
        private Model<VertexPositionColor, U> _modelPC;
        private Model<VertexPositionNDCColor, U> _modelP_NDC_C;
        private Model<VertexPositionTexture, U> _modelPT;
        private Model<VertexPositionNormal, U> _modelPN;
        private Model<VertexPositionNormalTexture, U> _modelPNT;
        private Model<VertexPositionNormalTextureTangent, U> _modelPNTT;
        private Model<VertexPositionNormalTextureTangentBitangent, U> _modelPNTTB;

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