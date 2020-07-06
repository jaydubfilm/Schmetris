namespace Recycling
{
    public interface ICustomRecycle
    {
        /// <summary>
        /// Function Called before object is fully recycled. This allows for any custom functionality before item is Recycled.
        /// </summary>
        /// <param name="args"></param>
        void CustomRecycle(params object[] args);
    }
}


