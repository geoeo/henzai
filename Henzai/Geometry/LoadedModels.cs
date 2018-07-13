using Veldrid;

namespace Henzai.Geometry{
    public class LoadedModels{
        public Model<VertexPosition> modelP {
            get{return modelP;} set{modelP = value;_hasVertexPosition=true;}
            }
        public Model<VertexPositionTexture> modelPT {
            get{return modelPT;} set{modelPT = value;_hasVertexPositionTexture=true;}
            }
        public Model<VertexPositionNormal> modelPN {
            get{return modelPN;} set{modelPN = value;_hasVertexPositionNormal=true;}
            }
        public Model<VertexPositionColor> modelPC {
            get{return modelPC;} set{modelPC = value;_hasVertexPositionColor=true;}
            }
        public Model<VertexPositionNDCColor> modelP_NDC_C {
           get{return modelP_NDC_C;} set{modelP_NDC_C = value;_hasVertexPositionNDCColor=true;}
            }
        public Model<VertexPositionNormalTexture> modelPNT {
           get{return modelPNT;} set{modelPNT = value;_hasVertexPositionNormalTexture=true;}
            }
        public Model<VertexPositionNormalTextureTangent> modelPNTT {
           get{return modelPNTT;} set{modelPNTT = value;_hasVertexPositionNormalTextureTangent=true;}
            }
        public Model<VertexPositionNormalTextureTangentBitangent> modelPNTTB {
           get{return modelPNTTB;} set{modelPNTTB = value;_hasVertexPositionNormalTextureTangentBitangent=true;}
            }

        private bool _hasVertexPosition = false;
        private bool _hasVertexPositionTexture = false;
        private bool _hasVertexPositionNormal = false;
        private bool _hasVertexPositionColor = false;
        private bool _hasVertexPositionNDCColor = false;
        private bool _hasVertexPositionNormalTexture = false;
        private bool _hasVertexPositionNormalTextureTangent = false;
        private bool _hasVertexPositionNormalTextureTangentBitangent = false;

        public bool HasVertexPosition => _hasVertexPosition;
        public bool HasVertexPositionTexture => _hasVertexPositionTexture;
        public bool HasVertexPositionNormal => _hasVertexPositionNormal;
        public bool HasVertexPositionColor => _hasVertexPositionColor;
        public bool HasVertexPositionNDCColor => _hasVertexPositionNDCColor;
        public bool HasVertexPositionNormalTexture => _hasVertexPositionNormalTexture;
        public bool HasVertexPositionNormalTextureTangent => _hasVertexPositionNormalTextureTangent;
        public bool HasVertexPositionNormalTextureTangentBitangentt => _hasVertexPositionNormalTextureTangentBitangent;

    }
}