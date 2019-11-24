using System;

namespace Henzai.Runtime 
{

    public static class PreEffectEventHandlerKeys {
        public static readonly object ShadowMapKey = new object();
        public static readonly object OmniShadowMapKey = new object();

        public static object[] GetKeys() => new object[]{
            ShadowMapKey, 
            OmniShadowMapKey
            };
    }
}