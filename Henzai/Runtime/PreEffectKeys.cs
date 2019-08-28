using System;

namespace Henzai.Runtime 
{

    public static class PreEffectKeys {
        public static readonly object ShadowMapKey = new object();
        public static readonly object OmniShadowMapKey = new object();

        public static Tuple<uint,object>[] GetFlagKeyTuples() => new Tuple<uint,object>[]{
            Tuple.Create(RenderFlags.SHADOW_MAP, ShadowMapKey), 
            Tuple.Create(RenderFlags.OMNI_SHADOW_MAPS, OmniShadowMapKey)
            };
    }
}