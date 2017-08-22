using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using Publishing.FileSystemProvider.Model;
using Sitecore.Framework.AutoFixture.NSubstitute.Attributes;
using Xunit;
using FluentAssertions;
using Publishing.Integration.FileSystemProvider.Model;
using Sitecore.Framework.Publishing.Item;
using Version = Sitecore.Framework.Publishing.Item.Version;
using Sitecore.Framework.Publishing.Data;

namespace Publishing.FileSystemProvider.Tests
{
    public class FileSystemItemDataProviderFixture
    {
        public static Action<IFixture> AutoSetup()
        {
            return f =>
            {
                f.Inject(DataAccessContextType.Backend);
                f.Freeze<Language>();
                f.Freeze<Version>();

                var itemModel = f.Create<ItemModel>();

                var language = f.Create<Language>();

                var version = f.Create<Version>();

                itemModel.Languages.Add(new LanguageModel() { Language = language.Name, Versions = new List<VersionModel>() { new VersionModel() { Number = version.Number } } });

                f.Inject(itemModel);
            };
        }

        [Theory, AutoNSubstituteData]
        public void Ctor_InvalidArgs_Throws(GuardClauseAssertion assertion)
        {
            assertion.VerifyConstructors<FileSystemItemDataProvider>();
        }

        [Theory, AutoNSubstituteData]
        public async void GetSkinnyItems_ConnectionNull_ExceptionThrown(FileSystemItemDataProvider sut, ItemIdEntity[] itemIdEntity)
        {
            // Act
            var exception = await Assert.ThrowsAsync(typeof(ArgumentNullException), async () => await sut.GetSkinnyItems(null, itemIdEntity));

            // Assert
            exception.Message.Should().Be("connection should not be null.\r\nParameter name: connection");
        }

        [Theory, AutoNSubstituteData]
        public async void GetSkinnyItems_IdItemEntityNull_ExceptionThrown(FileSystemItemDataProvider sut, IFileSystemConnection connection)
        {
            // Act
            var exception = await Assert.ThrowsAsync(typeof(ArgumentNullException), async () => await sut.GetSkinnyItems(connection, null));
        }

        [Theory, AutoNSubstituteData]
        public async void GetSkinnyItems_IdItemEntityEmpty_ReturnsEmpty(FileSystemItemDataProvider sut, IFileSystemConnection connection)
        {
            // Act
            var actual = await sut.GetSkinnyItems(connection, new List<ItemIdEntity>());

            // Assert
            actual.Should().BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async void GetSkinnyItems_SkinnyModelsReturned(FileSystemConnection connection,
            DataProviderFile file,
            ItemIdEntity[] itemIdEntities,
            ItemModel itemModel,
            Language language,
            Version version)
        {
            //Arrange
            var sut = Substitute.ForPartsOf<FileSystemItemDataProviderWrapper>();

            var dataProviderFiles = new List<DataProviderFile>() { new DataProviderFile(itemIdEntities.FirstOrDefault().Key, "json", "/test.json") };
            sut.GetAllFilesWrapper(connection).Returns(dataProviderFiles);

            var skinnyModel = new SkinnyItemModel();
            sut.GetSkinnyModelWrapper(dataProviderFiles.FirstOrDefault(), itemIdEntities.FirstOrDefault()).Returns(skinnyModel);

            //Act
            var items = await sut.GetSkinnyItems(connection, itemIdEntities);

            //Assert
            Assert.Equal(skinnyModel, items.FirstOrDefault());
        }

        [Theory, AutoNSubstituteData]
        public async void Get_ItemModels_ConnectionNull_ExceptionThrown(FileSystemItemDataProvider sut, ItemIdEntity[] itemIdEntity, Guid[] ids)
        {
            // Act
            var exception = await Assert.ThrowsAsync(typeof(ArgumentNullException), async () => await sut.GetItemModels(null, itemIdEntity));

            // Assert
            exception.Message.Should().Be("connection should not be null.\r\nParameter name: connection");
        }

        [Theory, AutoNSubstituteData]
        public async void Get_ItemModels_IdItemEntityEmpty_ReturnsEmpty(FileSystemItemDataProvider sut, IFileSystemConnection connection)
        {
            // Act
            var actual = await sut.GetItemModels(connection, new List<ItemIdEntity>());

            // Assert
            actual.Should().BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async void Get_ItemModels_ReturnsModels(FileSystemConnection connection,
            ItemIdEntity[] itemIdEntities,
            Guid[] ids,
            DataProviderFile dataProviderFile,
            ItemModel itemModel)
        {
            var sut = Substitute.ForPartsOf<FileSystemItemDataProviderWrapper>();

            var skinnyItemModel = new SkinnyItemModel() { ItemId = ids[0], File = dataProviderFile };
            var skinnyItemModels = new List<SkinnyItemModel>() { skinnyItemModel };
            sut.GetSkinnyItems(connection, Arg.Any<IReadOnlyCollection<ItemIdEntity>>()).Returns( skinnyItemModels);

            sut.GetModelWrapper(dataProviderFile).Returns( itemModel);

            //Act
            var items = await sut.GetItemModels(connection, itemIdEntities);

            //Assert
            Assert.Equal(skinnyItemModel, items.FirstOrDefault().Item1);
            Assert.Equal(itemModel, items.FirstOrDefault().Item2);
        }
    }

    public class FileSystemItemDataProviderWrapper : FileSystemItemDataProvider
    {
        public virtual IEnumerable<DataProviderFile> GetAllFilesWrapper(IFileSystemConnection connection)
        {
            return null;
        }

        protected override IEnumerable<DataProviderFile> GetAllFiles(IFileSystemConnection connection)
        {
            return GetAllFilesWrapper(connection);
        }

        public virtual Task<SkinnyItemModel> GetSkinnyModelWrapper(DataProviderFile file, ItemIdEntity itemIdEntity)
        {
            return null;
        }

        protected override Task<SkinnyItemModel> GetSkinnyModel(DataProviderFile file, ItemIdEntity itemIdEntity)
        {
            return GetSkinnyModelWrapper(file, itemIdEntity);
        }

        public virtual Task<ItemModel> GetModelWrapper(DataProviderFile dataProviderFile)
        {
            return null;
        }

        protected override Task<ItemModel> GetModel(DataProviderFile dataProviderFile)
        {
            return GetModelWrapper(dataProviderFile);
        }
    }
}
