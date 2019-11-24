namespace Henzai.Runtime 
{
    public struct InstancingRenderDescription {

        public uint DataFlag {get; private set;}
        public uint PreEffectsFlag {get; private set;}

        public InstancingRenderDescription(uint dataFlag, uint renderFlag){
            DataFlag = dataFlag;
            PreEffectsFlag = renderFlag;
        }


    }
}