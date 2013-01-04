using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FubuCore;
using FubuCore.Descriptions;
using FubuCore.Util;
using FubuMVC.Core.Assets.Files;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Runtime.Files;

namespace FubuMVC.Core.Assets.NEW
{
    /*
     * BOOTSTRAPPING
     * 1.) Find all by the file system
     * 2.) "Correct" the naming
     * 3.) Run all asset.config files
     * 4.) Run all this.Asset() expressions
     * 5.) Apply all aliases
     * 6.) Apply other rules
     * 5.) "Warm Up" combinations
     */

    public interface IAssetPipeline
    {
        IAsset Find(string name);
    }

    [ApplicationLevel]
    public class AssetPipeline
    {
        private readonly IList<Asset> _assets = new List<Asset>();

        public IList<Asset> Assets
        {
            get { return _assets; }
        }

        public void Add(IEnumerable<Asset> assets)
        {
            _assets.AddRange(assets);
        }
    }

    [ConfigurationType(ConfigurationType.Discovery)]
    [Title("Asset File Finder")]
    [Description("Searches through the file system inside the application and all the exploded packages for asset files"
        )]
    public class AssetFileFinder : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var settings = graph.Settings.Get<AssetSettings>();
            var pipeline = graph.Settings.Get<AssetPipeline>();

            buildFromFiles(graph, settings, pipeline);

            applyPathRooting(settings, pipeline);
        }

        private static void buildFromFiles(BehaviorGraph graph, AssetSettings settings, AssetPipeline pipeline)
        {
            settings.MimeTypes.Each(mimetype => {
                var files = new AssetFilesCollection(mimetype);

                mimetype.Extensions.Each(ext => {
                    FileSet matcher = FileSet.Deep("*" + ext);
                    graph.Files.FindFiles(matcher).Each(files.AddFile);
                });

                pipeline.Add(files);
            });
        }

        private static void applyPathRooting(AssetSettings settings, AssetPipeline pipeline)
        {
            settings.RootedFolders.Each(folder => {
                AssetFolder.AllFolders().Each(sub => {
                    var prefix = folder + "/" + sub + "/";
                    pipeline.Assets.Where(asset => asset.Name.StartsWith(prefix))
                            .Each(asset => { asset.Name = asset.Name.Substring(prefix.Length); });
                });
            });
        }
    }

    public class AssetFilesCollection : IEnumerable<Asset>
    {
        private readonly Cache<string, Asset> _assets;
        private readonly MimeType _mimeType;

        public AssetFilesCollection(MimeType mimeType)
        {
            _mimeType = mimeType;
            _assets = new Cache<string, Asset>(name => new Asset(name, mimeType));
        }

        public MimeType MimeType
        {
            get { return _mimeType; }
        }

        public IEnumerator<Asset> GetEnumerator()
        {
            return _assets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddFile(IFubuFile file)
        {
            _assets[file.RelativePath.ToLower()].AddFile(file);
        }
    }


    public class Asset : TracedNode, IAsset
    {
        private readonly MimeType _mimeType;

        public Asset(string name, MimeType mimeType)
        {
            Name = name;
            _mimeType = mimeType;
        }

        public Asset(string name)
        {
            Name = name;

            MimeType mimeType = MimeType.MimeTypeByFileName(Name);
            if (mimeType != null)
            {
                _mimeType = mimeType;
            }
        }

        public string Name { get; set; }

        public MimeType MimeType
        {
            get { return _mimeType; }
        }

        public IList<AssetSource> Sources { get; set; }

        public static Asset ForFile(string file, string packageName)
        {
            var asset = new Asset(file);
            asset.Sources.Add(new AssetSource
            {
                File = file,
                PackageName = packageName
            });

            return asset;
        }

        public string Extension()
        {
            return Path.GetExtension(Name);
        }

        public IEnumerable<string> AllExtensions()
        {
            return Name.Split('.').Skip(1).Select(x => "." + x);
        }

        public string LibraryName()
        {
            return Name.Split('/').Last();
        }

        public void AddFile(IFubuFile file)
        {
            Sources.Add(new AssetSource(file));
        }

        private readonly IList<IAsset> _dependencies = new List<IAsset>();
        private readonly IList<IOrderRule> _rules = new List<IOrderRule>(); 

        public void AddDependency(IAsset asset, params IOrderRule[] ordering)
        {
            _dependencies.Add(asset);
            _rules.AddRange(ordering);
        }

        public void AddToPlan(IAssetPlan plan)
        {
            
        }
    }

    // TODO -- maybe we do something that puts the asset tag directly on this thing
    // TODO -- need to ignore the url in the combinations
    public class AssetSource : TracedNode
    {
        public AssetSource(IFubuFile file)
        {
            PackageName = file.Provenance;
            File = file.Path;
        }

        public AssetSource()
        {
        }

        public string PackageName { get; set; }
        public string Url { get; set; }

        /// <summary>
        ///     Absolute path of the asset
        /// </summary>
        public string File { get; set; }

        public static AssetSource ForUrl(string packageName, string url)
        {
            return new AssetSource
            {
                PackageName = packageName,
                Url = url
            };
        }
    }


    public interface IAsset
    {
        void AddToPlan(IAssetPlan plan);
        string Name { get; }
        MimeType MimeType { get; }
    }

    public interface IAssetPlan
    {
        void OrderBy(IOrderRule rule);
        void AddFile(Asset asset);
    }

    public interface IOrderRule : IComparer<AssetFile>
    {
        bool Matches(AssetFile one, AssetFile two);
    }

    public class AlphabeticOrderRule : IOrderRule
    {
        public int Compare(AssetFile x, AssetFile y)
        {
            return String.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }

        public bool Matches(AssetFile one, AssetFile two)
        {
            return true;
        }
    }

    public class FileSorter : IComparer<AssetFile>
    {
        private readonly IEnumerable<IOrderRule> _rules;

        public FileSorter(IEnumerable<IOrderRule> rules)
        {
            _rules = rules.Union(new IOrderRule[] {new AlphabeticOrderRule()}).ToList();
        }

        public int Compare(AssetFile one, AssetFile two)
        {
            IOrderRule rule = _rules.First(x => x.Matches(one, two));
            return rule.Compare(one, two);
        }
    }

    [ApplicationLevel]
    public class AssetSettings
    {
        private readonly IList<MimeType> _mimeTypes = new List<MimeType>();
        private readonly IList<string> _rootedFolders = new List<string>();

        public AssetSettings()
        {
            MimetypeIsAsset(MimeType.TrueTypeFont);
            MimetypeIsAsset(MimeType.Gif);
            MimetypeIsAsset(MimeType.Jpg);
            MimetypeIsAsset(MimeType.Bmp);
            MimetypeIsAsset(MimeType.Png);
            MimetypeIsAsset(MimeType.Javascript);
            MimetypeIsAsset(MimeType.Css);

            RootedFolder("content");
        }

        public IEnumerable<MimeType> MimeTypes
        {
            get { return _mimeTypes; }
        }

        /// <summary>
        ///     Tells the asset pipeline to treat a file extension as a Javascript file
        /// </summary>
        /// <param name="extension"></param>
        public void ExtensionIsJavascript(string extension)
        {
            MimeType.Javascript.AddExtension(extension);
        }

        /// <summary>
        ///     Tells the asset pipeline to treat a file extension as a stylesheet
        /// </summary>
        /// <param name="extension">File extension preceeded with a period</param>
        public void ExtensionIsStylesheet(string extension)
        {
            MimeType.Css.AddExtension(extension);
        }

        public void MimetypeIsAsset(MimeType mimeType)
        {
            _mimeTypes.Fill(mimeType);
        }

        /// <summary>
        ///     Any folder listed as "rooted" will get the special treatment of url's
        ///     Example, "content" is rooted by default, so that a file named
        ///     content/scripts/jquery.js will just be named "jquery.js"
        ///     The second folder is the name of an app folder
        /// </summary>
        /// <param name="folder"></param>
        public void RootedFolder(string folder)
        {
            _rootedFolders.Fill(folder.ToLower());
        }

        public IEnumerable<string> RootedFolders
        {
            get { return _rootedFolders; }
        }
    }
}