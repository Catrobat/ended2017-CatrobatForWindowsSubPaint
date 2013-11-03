﻿using System.Threading.Tasks;
using Catrobat.IDE.Core.CatrobatObjects;
using Catrobat.IDE.Core.Services;
using Catrobat.IDE.Core.UI;
using Catrobat.IDE.Core.UI.PortableUI;
using Catrobat.IDE.Core.ViewModel;
using Catrobat.IDE.Core.ViewModel.Editor.Costumes;
using Catrobat.IDE.Tests.Services;
using Catrobat.IDE.Tests.Services.Storage;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Catrobat.IDE.Tests.Tests.ViewModel.Editor.Costumes
{
    [TestClass]
    public class CostumeNameChooserViewModelTests
    {
        private PortableImage _imageToSave;

        [ClassInitialize]
        public static void TestClassInitialize(TestContext testContext)
        {
            ServiceLocator.UnRegisterAll();
            ServiceLocator.NavigationService = new NavigationServiceTest();
            ServiceLocator.Register<StorageFactoryTest>(TypeCreationMode.Normal);
            ServiceLocator.Register<StorageTest>(TypeCreationMode.Normal);
            ServiceLocator.Register<ImageResizeServiceTest>(TypeCreationMode.Normal);
        }

        [TestMethod] //, TestCategory("GatedTests")]
        public async Task SaveActionTest()
        {
            _imageToSave = null;
            Messenger.Default.Register<GenericMessage<PortableImage>>(this,
                ViewModelMessagingToken.CostumeImageToSaveListener, CostumeImageReceivedMessageAction);
            
            var navigationService = (NavigationServiceTest)ServiceLocator.NavigationService;
            navigationService.PageStackCount = 2;
            navigationService.CurrentNavigationType = NavigationServiceTest.NavigationType.Initial;
            navigationService.CurrentView = typeof(CostumeNameChooserViewModel);

            var viewModel = new CostumeNameChooserViewModel();
            viewModel.SelectedSize = new ImageSizeEntry { NewHeight = 100, NewWidth = 100};
            viewModel.CostumeName = "TestCostume";

            var project = new Project { ProjectHeader = new ProjectHeader(false) { ProgramName = "TestProject" } };
            var messageContext = new GenericMessage<Project>(project);
            Messenger.Default.Send(messageContext, ViewModelMessagingToken.CurrentProjectChangedListener);

            var sprite = new Sprite();
            var messageContext2 = new GenericMessage<Sprite>(sprite);
            Messenger.Default.Send(messageContext2, ViewModelMessagingToken.CurrentSpriteChangedListener);
            
            var messageContext3 = new GenericMessage<PortableImage>(new PortableImage());
            Messenger.Default.Send(messageContext3, ViewModelMessagingToken.CostumeImageListener);

            await viewModel.SaveCommand.ExecuteAsync(null);

            Assert.IsNotNull(_imageToSave);
            Assert.AreEqual(1, sprite.Costumes.Costumes.Count);
            Assert.AreEqual(_imageToSave, sprite.Costumes.Costumes[0]);
            Assert.AreEqual(NavigationServiceTest.NavigationType.NavigateBack, navigationService.CurrentNavigationType);
            Assert.AreEqual(null, navigationService.CurrentView);
            Assert.AreEqual(0, navigationService.PageStackCount);
        }

        [TestMethod] //, TestCategory("GatedTests")]
        public void CancelActionTest()
        {
            var navigationService = (NavigationServiceTest)ServiceLocator.NavigationService;
            navigationService.PageStackCount = 1;
            navigationService.CurrentNavigationType = NavigationServiceTest.NavigationType.Initial;
            navigationService.CurrentView = typeof(CostumeNameChooserViewModel);

            var viewModel = new CostumeNameChooserViewModel();

            viewModel.CancelCommand.Execute(null);

            Assert.AreEqual(NavigationServiceTest.NavigationType.NavigateBack, navigationService.CurrentNavigationType);
            Assert.AreEqual(null, navigationService.CurrentView);
            Assert.AreEqual(0, navigationService.PageStackCount);
        }

        [TestMethod] //, TestCategory("GatedTests")]
        public void GoBackActionTest()
        {
            var navigationService = (NavigationServiceTest)ServiceLocator.NavigationService;
            navigationService.PageStackCount = 1;
            navigationService.CurrentNavigationType = NavigationServiceTest.NavigationType.Initial;
            navigationService.CurrentView = typeof(CostumeNameChooserViewModel);

            var viewModel = new CostumeNameChooserViewModel();

            viewModel.GoBackCommand.Execute(null);

            Assert.AreEqual(NavigationServiceTest.NavigationType.NavigateBack, navigationService.CurrentNavigationType);
            Assert.AreEqual(null, navigationService.CurrentView);
            Assert.AreEqual(0, navigationService.PageStackCount);
        }

        private void CostumeImageReceivedMessageAction(GenericMessage<PortableImage> message)
        {
            _imageToSave = message.Content;
        }
    }
}
