namespace Henzai.Runtime 
{
    public static class InstancingKeys {

        //Keys for ModelRuntimeDescriptor EventHandlerLists - put this in separate class
        public static readonly object PositionKey = new object();
        public static readonly object ViewMatricesKey = new object();
        public static object[] GetKeys() => new object[]{PositionKey, ViewMatricesKey};

    }
}