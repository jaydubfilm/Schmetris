
namespace StarSalvager
{
    public interface IComponent: ILevel
    {
        COMPONENT_TYPE Type { get; set; }
    }
}