namespace Henzai.Geometry{
    public class LoadedModels{
        public Model<VertexPosition> modelP {
            get{return _modelP;} set{_modelP = value;_hasVertexPosition=true;}
            }
        public Model<VertexPositionTexture> modelPT {
            get{return _modelPT;} set{_modelPT = value;_hasVertexPositionTexture=true;}
            }
        public Model<VertexPositionNormal> modelPN {
            get{return _modelPN;} set{_modelPN = value;_hasVertexPositionNormal=true;}
            }
        public Model<VertexPositionColor> modelPC {
            get{return _modelPC;} set{_modelPC = value;_hasVertexPositionColor=true;}
            }
        public Model<VertexPositionNDCColor> modelP_NDC_C {
           get{return _modelP_NDC_C;} set{_modelP_NDC_C = value;_hasVertexPositionNDCColor=true;}
            }
        public Model<VertexPositionNormalTexture> modelPNT {
           get{return _modelPNT;} set{_modelPNT = value;_hasVertexPositionNormalTexture=true;}
            }
        public Model<VertexPositionNormalTextureTangent> modelPNTT {
           get{return _modelPNTT;} set{_modelPNTT = value;_hasVertexPositionNormalTextureTangent=true;}
            }
        public Model<VertexPositionNormalTextureTangentBitangent> modelPNTTB {
           get{return _modelPNTTB;} set{_modelPNTTB = value;_hasVertexPositionNormalTextureTangentBitangent=true;}
            }

        private Model<VertexPosition> _modelP;
        private Model<VertexPositionColor> _modelPC;
        private Model<VertexPositionNDCColor> _modelP_NDC_C;
        private Model<VertexPositionTexture> _modelPT;
        private Model<VertexPositionNormal> _modelPN;
        private Model<VertexPositionNormalTexture> _modelPNT;
        private Model<VertexPositionNormalTextureTangent> _modelPNTT;
        private Model<VertexPositionNormalTextureTangentBitangent> _modelPNTTB;

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