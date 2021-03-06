﻿using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gem.Network.Protocol;
using Gem.Network.Utilities;

namespace Gem.Network.Tests.NetworkProtocol
{
    using Extensions;
    using Gem.Network.Messages;
    using Gem.Network.Client;

    [NetworkPackage("Test")]
    public class Package
    {
        public string Name { get; set; }
        public float Depth { get; set; }
    }

    [NetworkPackage("SameTest")]
    public class AnotherPackage
    {
        public string Name { get; set; }
        public float Depth { get; set; }
    }

    /// <summary>
    /// Summary description for NetworkProtocolTests
    /// </summary>
    [TestClass]
    public class NetworkProtocolTests
    {

        [TestMethod]
        public void ObjectPropertiesAreResolvedCorrectly()
        {
            var package = new Package();

            var packageTypes = package.GetPropertyTypes();

            var expectedTypes = new List<Type> 
            { 
                typeof(string), 
                typeof(float)
            };

            CollectionAssert.AreEqual(expectedTypes, packageTypes.ToList());
        }

        [TestMethod]
        public void TypesTaggedWithNetworkPackageAreResolved()
        {
            var typesTaggedWithNetworkPackage = new List<Type>();
            var attributeProfiles = new List<string>();

            AttributeResolver.DoWithAllTypesWithAttribute<NetworkPackageAttribute>((type, attribute) =>
            {
                typesTaggedWithNetworkPackage.Add(type);
                attributeProfiles.Add(attribute.Profile);
            });
            var expectedTypes = new List<Type>
            {
                typeof(Package),
                typeof(AnotherPackage) 
            };
            CollectionAssert.AreEqual(expectedTypes, typesTaggedWithNetworkPackage);

            var expectedProfiles = new List<string>
            {
              "Test",
              "SameTest"
            };
            CollectionAssert.AreEqual(expectedProfiles, attributeProfiles);
        }

        //[TestMethod]
        public void ProtocolManagerSuccessfulyCachesType()
        {
            var typeAndAttributeForTest = ProtocolResolver.Provider["Test", 3];
            var typeAndAttributeForSameTest = ProtocolResolver.Provider["SameTest", 4];

            Assert.AreEqual(typeAndAttributeForTest.Type, typeof(Package));
            Assert.AreEqual(typeAndAttributeForSameTest.Type, typeof(AnotherPackage));
        }

        [TestMethod]
        public void ProtocolHandlerInvokesAction()
        {
            string expectedPackage = string.Empty;

            var protocolInvocation = GemClient.Profile("Test")
                                     .CreateNetworkProtocolEvent<Package>()
                                     .HandleIncoming((sender,package) => expectedPackage = package.Name);
        }

    }
}
