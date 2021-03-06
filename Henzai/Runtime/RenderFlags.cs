namespace Henzai.Runtime 
{
    //TODO: Ununsed for now
    public static class RenderFlags
     {
        public const uint NONE = 0x000;
        // 4 bits reserved for pre effects
        public const uint PRE_EFFECTS_MASK = 0x00F;
        // 4 bits reserved for normal render mode
        public const uint NORMAL = 0x0F0;
        // 4 bits reserved for post render effects
        public const uint POST_EFFECTS_MASK = 0xF00;
        public const uint SHADOW_MAP = 0x001;
        public const uint OMNI_SHADOW_MAPS = 0x002;
        public const uint PRE_EFFCTS_TOTAL_COUNT = 2;
        public const uint EFFCTS_TOTAL_COUNT = 3;
        public const uint  NORMAL_ARRAY_INDEX = 0;

        public static readonly uint[] ALL_RENDER_FLAGS = new uint[]{NORMAL,SHADOW_MAP,OMNI_SHADOW_MAPS};
        public static int GetSizeOfPreEffectFlag(uint flag){
            var preEffectsOnly = flag&PRE_EFFECTS_MASK;
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
                case NORMAL:
                    index = 0;
                    break;
                case SHADOW_MAP:
                    index = 1;
                    break;
                case OMNI_SHADOW_MAPS:
                    index = 2;
                    break;
                default:
                    throw new System.ArgumentException($"Invalid flag: {flag} for PreEffects array index");
            }
            return index;
        }        

        public static int GetPreEffectArrayIndexForFlag(uint flag){
            var preEffectsOnly = flag & PRE_EFFECTS_MASK;
            var index = -1;
            switch (preEffectsOnly){
                case NONE:
                    index = -1;
                    break;
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

        public static uint[] GetAllPreEffectFor(uint id){
            var preEffectsOnly = id & PRE_EFFECTS_MASK;
            var flagArray = new uint[GetSizeOfPreEffectFlag(id)];
            var index = 0;

            //Order doesnt matter
            if((preEffectsOnly & SHADOW_MAP) == SHADOW_MAP)
                flagArray[index++] = SHADOW_MAP;
            if((preEffectsOnly & OMNI_SHADOW_MAPS) == OMNI_SHADOW_MAPS)
                flagArray[index++] = OMNI_SHADOW_MAPS;

            return flagArray;
        }
    }
}