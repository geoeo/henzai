using System;

namespace Henzai.Effects 
{

    public static class PreEffectKeys {
        public static readonly object ShadowMapKey = new object();
        public static readonly object OmniShadowMapKey = new object();

        public static Tuple<uint,object>[] GetFlagKeyTuples() => new Tuple<uint,object>[]{
            Tuple.Create(PreEffectFlags.SHADOW_MAP, ShadowMapKey), 
            Tuple.Create(PreEffectFlags.OMNI_SHADOW_MAPS, OmniShadowMapKey)
            };
    }
}