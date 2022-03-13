using OpenSvip.Model;

namespace OpenSvip.Adapter
{
    public interface IProjectConverter<T>
    {
        T Load(string path);

        void Save(string path, T model);
        
        Project Parse(T model);

        T Build(Project project);
    }
}
