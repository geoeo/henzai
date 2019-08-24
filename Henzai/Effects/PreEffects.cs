namespace Henzai.Effects 
{

    //TODO: Make TOTAL_PRE_EFFECTS calculate automatically




    public static class PreEffects {

        public const uint NO_EFFECTS = 0x0;
        public const uint SHADOW_MAP = 0x1;
        public const uint OMNI_SHADOW_MAPS = 0x2;

        public static int GetSizeOfPreEffectFlag(uint flag){
            var size = 0;
            if((flag & SHADOW_MAP) == SHADOW_MAP)
                size++;
            if((flag & OMNI_SHADOW_MAPS) == OMNI_SHADOW_MAPS)
                size++;

            return size;

        }

        public static int GetArrayIndexForFlag(uint flag){
            var index = -1;
            switch (flag){
                case SHADOW_MAP:
                    index = 0;
                    break;
                case OMNI_SHADOW_MAPS:
                    index = 1;
                    break;
                default:
                    throw new System.ArgumentException($"Invalid flag: {flag} for PreEffects array index");
            }
            return index;
        }


        //Keys for ModelRuntimeDescriptor EventHandlerLists - put this in separate class
        public static readonly object ShadowMapKey = new object();
        public static readonly object OmniShadowMapKey = new object();
    }
}