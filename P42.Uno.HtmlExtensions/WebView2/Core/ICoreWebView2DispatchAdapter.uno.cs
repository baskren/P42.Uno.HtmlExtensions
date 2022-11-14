using Uno;

namespace P42.Web.WebView2.Core
{
    //[WindowsRuntimeType("Microsoft.Web.WebView2.Core")]
    //[Guid("7888A42D-18F3-5966-80CB-8CC25351BD0A")]
    public interface ICoreWebView2DispatchAdapter
    {
        object WrapNamedObject(string name, ICoreWebView2DispatchAdapter adapter);

        object WrapObject(object unwrapped, ICoreWebView2DispatchAdapter adapter);

        object UnwrapObject(object wrapped);

        void Clean();
    }
}
