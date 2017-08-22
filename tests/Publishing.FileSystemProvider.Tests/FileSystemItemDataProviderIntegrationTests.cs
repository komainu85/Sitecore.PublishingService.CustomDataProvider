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
using Microsoft.Extensions.Logging;
using Publishing.Integration.FileSystemProvider.Model;
using Sitecore.Framework.Publishing.Item;
using Version = Sitecore.Framework.Publishing.Item.Version;
using Sitecore.Framework.Publishing.Data;

namespace Publishing.FileSystemProvider.Tests
{
    public class FileSystemItemDataProviderIntegrationTests
    {
        public static Action<IFixture> AutoSetup()
        {
            return f =>
            {
                f.Register(() =>
                {
                    var ops = new FileSystemConnectionOptions()
                    {
                        IdTableConnection = "Master",
                        IdTablePrefix = "blah",
                        RootFolder = "./CustomItems"
                    };
                    return ops;
                });

                f.Register((ILogger<FileSystemConnection> logger, FileSystemConnectionOptions options) => 
                    new FileSystemConnection("fileSystem", DataAccessContextType.Backend, logger, options));

                var itemIdEntities = new List<ItemIdEntity>()
                {
                    new ItemIdEntity() {Id = Guid.NewGuid(),Key = "133b85182d0348038ed03d7693df50db",ParentId =Guid.NewGuid()},
                    new ItemIdEntity() {Id = Guid.NewGuid(),Key = "8a7a7adc9d604e389dd9d719f1316c25",ParentId =Guid.NewGuid()}
                };

                f.Inject(itemIdEntities);
            };
        }

        [Theory, AutoNSubstituteData]
        public async void GetSkinnyItems_SkinnyModelsReturned(
            FileSystemItemDataProvider sut,
            FileSystemConnection connection,
            List<ItemIdEntity> itemIdEntities)
        {
            //Act
            var items = await sut.GetSkinnyItems(connection, itemIdEntities);
            var skinnyItemModels = items as SkinnyItemModel[] ?? items.ToArray();

            //Assert
            Assert.Equal(2, skinnyItemModels.Count());
            Assert.Equal(Guid.Parse("133b85182d0348038ed03d7693df50db"), skinnyItemModels.FirstOrDefault().ModelId);
            Assert.Equal(Guid.Parse("8a7a7adc9d604e389dd9d719f1316c25"), skinnyItemModels[1].ModelId);

        }

        [Theory, AutoNSubstituteData]
        public async void GetItemModels_ItemModelsReturned(
            FileSystemItemDataProvider sut,
            FileSystemConnection connection,
            List<ItemIdEntity> itemIdEntities)
        {
            //Act
            var items = await sut.GetItemModels(connection, itemIdEntities);
            var resultList = items as Tuple<SkinnyItemModel, ItemModel>[] ?? items.ToArray();

            //Assert
            Assert.Equal(2, resultList.Count());
            Assert.Equal(Guid.Parse("133b85182d0348038ed03d7693df50db"), resultList.FirstOrDefault().Item2.ModelId);
            Assert.Equal(Guid.Parse("8a7a7adc9d604e389dd9d719f1316c25"), resultList[1].Item2.ModelId);
            Assert.Equal(Guid.Parse("133b85182d0348038ed03d7693df50db"), resultList.FirstOrDefault().Item1.ModelId);
            Assert.Equal(Guid.Parse("8a7a7adc9d604e389dd9d719f1316c25"), resultList[1].Item1.ModelId);
        }
    }
}
