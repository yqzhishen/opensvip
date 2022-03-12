using OpenSvip.Model;

namespace OpenSvip.Adapter;

public interface IProjectConverter<T>
{
    public T Load(string path);

    public void Save(string path, T model);

    public Project Parse(T model);

    public T Build(Project project);
}
