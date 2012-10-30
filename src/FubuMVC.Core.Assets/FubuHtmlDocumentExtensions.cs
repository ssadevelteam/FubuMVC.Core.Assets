using FubuMVC.Core.UI;

namespace FubuMVC.Core.Assets
{
    public static class FubuHtmlDocumentExtensions
    {
        public static void WriteAssetsToHead(this FubuHtmlDocument document)
        {
            document.Head.Append(document.WriteAssetTags());
        }

        public static void WriteScriptsToBody(this FubuHtmlDocument document)
        {
            document.Body.Append(document.WriteScriptTags());
        }

    }
}