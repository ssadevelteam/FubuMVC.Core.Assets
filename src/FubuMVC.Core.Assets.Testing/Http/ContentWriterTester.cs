using System;
using System.Collections.Generic;
using FubuMVC.Core.Assets.Content;
using FubuMVC.Core.Assets.Files;
using FubuMVC.Core.Assets.Http;
using FubuMVC.Core.Runtime;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;
using System.Linq;
using FubuCore;

namespace FubuMVC.Tests.Assets.Http
{
    [TestFixture]
    public class when_writing_an_image : InteractionContext<ContentWriter>
    {
        private AssetPath theAssetPath;
        private AssetFile theFile;
        private bool wasWritten;
        private Action<IEnumerable<AssetFile>> theAction;
        private IEnumerable<AssetFile> _files;

        protected override void beforeEach()
        {
            _files = null;
            theAssetPath = new AssetPath("images/icon.gif");

            // Precondition here
            theAssetPath.IsBinary().ShouldBeTrue();

            theFile = new AssetFile(theAssetPath.ToFullName()){
                FullPath = theAssetPath.ToFullName().ToFullPath()
            };

            MockFor<IAssetFileGraph>().Stub(x => x.Find(theAssetPath))
                .Return(theFile);

            theAction = files => _files = files;

            wasWritten = ClassUnderTest.Write(theAssetPath, theAction);
        }

        [Test]
        public void should_have_returned_the_single_file()
        {
            wasWritten.ShouldBeTrue();
        }

        [Test]
        public void should_call_through_with_the_one_file()
        {
            _files.Single().ShouldEqual(theFile);
        }

        [Test]
        public void should_have_written_the_actual_image_file_to_the_output_writer()
        {
            MockFor<IOutputWriter>().AssertWasCalled(x => x.WriteFile(MimeType.Gif, theFile.FullPath, null));
        }
    }

    [TestFixture]
    public class when_writing_textual_output : InteractionContext<ContentWriter>
    {
        private AssetFile[] theFiles;
        private bool wasReturned;
        private const string theContent = "blah blah blah";
        private Action<IEnumerable<AssetFile>> theAction;
        private IEnumerable<AssetFile> _files;

        protected override void beforeEach()
        {
            var assetPath = new AssetPath("scripts/combo1.js");
            assetPath.IsBinary().ShouldBeFalse();

            theFiles = new AssetFile[]{
                new AssetFile("script1.js"){FullPath = "1.js"},
                new AssetFile("script2.js"){FullPath = "2.js"},
                new AssetFile("script3.js"){FullPath = "3.js"},
                new AssetFile("script4.js"){FullPath = "4.js"},
            };

            MockFor<IContentPlanCache>().Stub(x => x.SourceFor(assetPath))
                .Return(MockFor<IContentSource>());

            MockFor<IContentSource>().Expect(x => x.GetContent(MockFor<IContentPipeline>()))
                .Return(theContent);

            MockFor<IContentSource>().Stub(x => x.Files).Return(theFiles);

            theAction = files => _files = files;

            wasReturned = ClassUnderTest.Write(assetPath, theAction);
        }

        [Test]
        public void should_execute_the_content_plan()
        {
            VerifyCallsFor<IContentSource>();
        }

        [Test]
        public void returns_all_the_files_from_the_content_plan_source()
        {
            _files.ShouldHaveTheSameElementsAs(theFiles);

            wasReturned.ShouldBeTrue();
        }

        [Test]
        public void should_write_out_the_contents()
        {
            MockFor<IOutputWriter>().AssertWasCalled(x => x.Write(MimeType.Javascript, theContent));
        }


    }

    




}