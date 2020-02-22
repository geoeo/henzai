namespace Henzai.Runtime 
{
    public struct RenderDescription {

        public uint RenderModeFlag {get; private set;}

        public RenderDescription(uint renderModeFlag){
            RenderModeFlag = renderModeFlag;
        }


    }
}