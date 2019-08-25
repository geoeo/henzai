namespace Henzai.Runtime 
{
    public static class InstancingFlags {

        public const uint EMPTY = 0x0;
        public const uint POSITION = 0x1;
        public const uint VIEW_MATRICES = 0x2;

        public static int GetSizeOfPreEffectFlag(uint flag){
            var size = 0;
            if((flag & POSITION) == POSITION)
                size++;
            if((flag & VIEW_MATRICES) == VIEW_MATRICES)
                size++;

            return size;

        }

        public static int GetArrayIndexForFlag(uint flag){
            var index = -1;
            switch (flag){
                case POSITION:
                    index = 0;
                    break;
                case VIEW_MATRICES:
                    index = 1;
                    break;
                default:
                    throw new System.ArgumentException($"Invalid flag: {flag} for PreEffects array index");
            }
            return index;
        }
    }
}