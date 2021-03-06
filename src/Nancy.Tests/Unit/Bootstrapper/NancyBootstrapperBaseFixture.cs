﻿namespace Nancy.Tests.Unit.Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FakeItEasy;

    using Nancy.Bootstrapper;
    using Nancy.Tests.Fakes;

    using Xunit;

    internal class FakeBootstrapperBaseImplementation : NancyBootstrapperBase<object>
    {
        public INancyEngine FakeNancyEngine { get; set; }
        public object FakeContainer { get; set; }
        public object AppContainer { get; set; }
        public IModuleKeyGenerator Generator { get; set; }
        public IEnumerable<TypeRegistration> TypeRegistrations { get; set; }
        public List<ModuleRegistration> PassedModules { get; set; }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(c => c.ModuleKeyGenerator = typeof(FakeModuleKeyGenerator));
            }
        }

        public FakeBootstrapperBaseImplementation()
        {
            FakeNancyEngine = A.Fake<INancyEngine>();
            FakeContainer = new object();

            Generator = new Fakes.FakeModuleKeyGenerator();
        }

        protected override INancyEngine GetEngineInternal()
        {
            return this.FakeNancyEngine;
        }

        protected override IModuleKeyGenerator GetModuleKeyGenerator()
        {
            return this.Generator;
        }

        /// <summary>
        /// Get all NancyModule implementation instances
        /// </summary>
        /// <param name="context">The current context</param>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="NancyModule"/> instances.</returns>
        public override IEnumerable<NancyModule> GetAllModules(NancyContext context)
        {
            return this.PassedModules.Select(m => (NancyModule)Activator.CreateInstance(m.ModuleType));
        }

        /// <summary>
        /// Retrieves a specific <see cref="NancyModule"/> implementation based on its key
        /// </summary>
        /// <param name="moduleKey">Module key</param>
        /// <param name="context">The current context</param>
        /// <returns>The <see cref="NancyModule"/> instance that was retrived by the <paramref name="moduleKey"/> parameter.</returns>
        public override NancyModule GetModuleByKey(string moduleKey, NancyContext context)
        {
            return
                this.PassedModules.Where(m => String.Equals(m.ModuleKey, moduleKey, StringComparison.InvariantCulture))
                    .Select(m => (NancyModule)Activator.CreateInstance(m.ModuleType))
                    .FirstOrDefault();
        }

        protected override void ConfigureApplicationContainer(object existingContainer)
        {
            this.AppContainer = existingContainer;
        }

        protected override object GetApplicationContainer()
        {
            return FakeContainer;
        }

        /// <summary>
        /// Register the bootstrapper's implemented types into the container.
        /// This is necessary so a user can pass in a populated container but not have
        /// to take the responsibility of registering things like INancyModuleCatalog manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override void RegisterBootstrapperTypes(object applicationContainer)
        {
        }

        protected override void RegisterTypes(object container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            this.TypeRegistrations = typeRegistrations;
        }

        protected override void RegisterCollectionTypes(object container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrationsn)
        {
        }

        protected override void RegisterModules(object container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            PassedModules = new List<ModuleRegistration>(moduleRegistrationTypes);
        }

        protected override void RegisterInstances(object container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
        }

        public BeforePipeline PreRequest
        {
            get { return this.BeforeRequest; }
            set { this.BeforeRequest = value; }
        }

        public AfterPipeline PostRequest
        {
            get { return this.AfterRequest; }
            set { this.AfterRequest = value; }
        }
    }

    internal class FakeBootstrapperBaseGetModulesOverride : NancyBootstrapperBase<object>
    {
        public IEnumerable<ModuleRegistration> RegisterModulesRegistrationTypes { get; set; }
        public IEnumerable<ModuleRegistration> ModuleRegistrations { get; set; }

        protected override IEnumerable<ModuleRegistration> Modules
        {
            get
            {
                return ModuleRegistrations;
            }
        }

        public FakeBootstrapperBaseGetModulesOverride()
        {
            ModuleRegistrations = new List<ModuleRegistration>() { new ModuleRegistration(this.GetType(), "FakeBootstrapperBaseGetModulesOverride") };
        }

        /// <summary>
        /// Get all NancyModule implementation instances
        /// </summary>
        /// <param name="context">The current context</param>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="NancyModule"/> instances.</returns>
        public override IEnumerable<NancyModule> GetAllModules(NancyContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a specific <see cref="NancyModule"/> implementation based on its key
        /// </summary>
        /// <param name="moduleKey">Module key</param>
        /// <param name="context">The current context</param>
        /// <returns>The <see cref="NancyModule"/> instance that was retrived by the <paramref name="moduleKey"/> parameter.</returns>
        public override NancyModule GetModuleByKey(string moduleKey, NancyContext context)
        {
            throw new NotImplementedException();
        }

        protected override INancyEngine GetEngineInternal()
        {
            return A.Fake<INancyEngine>();
        }

        protected override IModuleKeyGenerator GetModuleKeyGenerator()
        {
            return new Fakes.FakeModuleKeyGenerator();
        }

        protected override object GetApplicationContainer()
        {
            return new object();
        }

        /// <summary>
        /// Register the bootstrapper's implemented types into the container.
        /// This is necessary so a user can pass in a populated container but not have
        /// to take the responsibility of registering things like INancyModuleCatalog manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override void RegisterBootstrapperTypes(object applicationContainer)
        {
        }

        protected override void RegisterTypes(object container, IEnumerable<TypeRegistration> typeRegistrations)
        {
        }

        protected override void RegisterCollectionTypes(object container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrationsn)
        {
        }

        protected override void RegisterModules(object container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            this.RegisterModulesRegistrationTypes = moduleRegistrationTypes;
        }

        protected override void RegisterInstances(object container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
        }
    }

    public class NancyBootstrapperBaseFixture
    {
        private FakeBootstrapperBaseImplementation _Bootstrapper;

        /// <summary>
        /// Initializes a new instance of the NancyBootstrapperBaseFixture class.
        /// </summary>
        public NancyBootstrapperBaseFixture()
        {
            _Bootstrapper = new FakeBootstrapperBaseImplementation();
            _Bootstrapper.Initialise();
        }

        [Fact]
        public void GetEngine_Returns_Engine_From_GetEngineInternal()
        {
            var result = _Bootstrapper.GetEngine();

            result.ShouldBeSameAs(_Bootstrapper.FakeNancyEngine);
        }

        [Fact]
        public void GetEngine_Calls_ConfigureApplicationContainer_With_Container_From_GetContainer()
        {
            _Bootstrapper.GetEngine();

            _Bootstrapper.AppContainer.ShouldBeSameAs(_Bootstrapper.FakeContainer);
        }

        [Fact]
        public void GetEngine_Calls_RegisterModules_With_Assembly_Modules()
        {
            _Bootstrapper.GetEngine();

            _Bootstrapper.PassedModules.ShouldNotBeNull();
            _Bootstrapper.PassedModules.Where(mr => mr.ModuleType == typeof(Fakes.FakeNancyModuleWithBasePath)).FirstOrDefault().ShouldNotBeNull();
            _Bootstrapper.PassedModules.Where(mr => mr.ModuleType == typeof(Fakes.FakeNancyModuleWithoutBasePath)).FirstOrDefault().ShouldNotBeNull();
        }

        [Fact]
        public void GetEngine_Gets_ModuleRegistration_Keys_For_Each_Module_From_IModuleKeyGenerator_From_GetModuleKeyGenerator()
        {
            _Bootstrapper.GetEngine();

            var totalKeyEntries = _Bootstrapper.PassedModules.Count();
            var called = (_Bootstrapper.Generator as Fakes.FakeModuleKeyGenerator).CallCount;

            called.ShouldEqual(totalKeyEntries);
        }

        [Fact]
        public void Overridden_Modules_Is_Used_For_Getting_ModuleTypes()
        {
            var bootstrapper = new FakeBootstrapperBaseGetModulesOverride();
            bootstrapper.Initialise();
            bootstrapper.GetEngine();

            bootstrapper.RegisterModulesRegistrationTypes.ShouldBeSameAs(bootstrapper.ModuleRegistrations);
        }

        [Fact]
        public void RegisterTypes_Passes_In_User_Types_If_Custom_Config_Set()
        {
            _Bootstrapper.GetEngine();

            var moduleKeyGeneratorEntry = _Bootstrapper.TypeRegistrations.Where(tr => tr.RegistrationType == typeof(IModuleKeyGenerator)).FirstOrDefault();

            moduleKeyGeneratorEntry.ImplementationType.ShouldEqual(typeof(Fakes.FakeModuleKeyGenerator));
        }

        [Fact]
        public void GetEngine_sets_pre_request_hook()
        {
            _Bootstrapper.PreRequest += ctx => null;

            var result = _Bootstrapper.GetEngine();

            result.PreRequestHook.ShouldNotBeNull();
        }

        [Fact]
        public void GetEngine_sets_post_request_hook()
        {
            _Bootstrapper.PostRequest += ctx => { };

            var result = _Bootstrapper.GetEngine();

            result.PostRequestHook.ShouldNotBeNull();
        }
    }
}
