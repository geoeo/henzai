using System;

namespace Henzai.Runtime 
{
    public static class InstancingKeys {

        //Keys for ModelRuntimeDescriptor EventHandlerLists - put this in separate class
        public static readonly object PositionKey = new object();
        public static readonly object ViewMatricesKey = new object();
        public static Tuple<uint,object>[] GetFlagKeyTuples() => new Tuple<uint,object>[]{
            Tuple.Create(InstancingFlags.POSITION, PositionKey), 
            Tuple.Create(InstancingFlags.VIEW_MATRICES, ViewMatricesKey)
            };

    }
}