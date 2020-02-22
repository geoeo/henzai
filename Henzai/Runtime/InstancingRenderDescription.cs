namespace Henzai.Runtime 
{
    public struct InstancingRenderDescription {

        public uint RenderModeFlag {get; private set;}
        public uint PreEffectsFlag {get; private set;}

        public InstancingRenderDescription(uint dataFlag, uint renderFlag){
            RenderModeFlag = dataFlag;
            PreEffectsFlag = renderFlag;
        }


    }
}