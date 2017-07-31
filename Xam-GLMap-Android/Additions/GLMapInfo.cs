namespace GLMap
{
    public partial class GLMapInfo
    {
        public GLMap.GLMapInfoState State
        {
            get
            {
                return (GLMap.GLMapInfoState)getState().Ordinal();
            }
        }
    }
}