
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Denomica.OData
{
    /// <summary>
    /// Provides functionality to build an Entity Data Model (EDM) for use with OData services.
    /// </summary>
    /// <remarks>The <see cref="EdmModelBuilder"/> class allows you to define entity types, entity sets, and
    /// entity keys to construct an EDM model. This model can then be used to configure OData endpoints or other
    /// services that rely on EDM-based metadata.  Use the provided methods to add entity types, specify keys, and
    /// define entity sets. Once the model is fully configured, call <see cref="Build"/> to generate the final <see
    /// cref="IEdmModel"/>.</remarks>
    public class EdmModelBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmModelBuilder"/> class with the specified options.
        /// </summary>
        /// <param name="options">The options to configure the behavior of the <see cref="EdmModelBuilder"/>. If null, default options are
        /// used.</param>
        public EdmModelBuilder(EdmModelBuilderOptions? options = null)
        {
            this.Options = options ?? new EdmModelBuilderOptions();
        }

        private EdmModelBuilderOptions Options = new EdmModelBuilderOptions();
        private List<Type> EntityTypes = new List<Type>();
        private Dictionary<Type, string> UriSegments = new Dictionary<Type, string>();
        private Dictionary<Type, List<string>> EntityKeys = new Dictionary<Type, List<string>>();


        /// <summary>
        /// Adds a key property to the entity type specified by <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to which the key property belongs.</typeparam>
        /// <param name="keyPropertyName">The name of the property to designate as the key for the entity type.</param>
        public EdmModelBuilder AddEntityKey<TEntity>(string keyPropertyName)
        {
            return this.AddEntityKey(typeof(TEntity), keyPropertyName);
        }

        /// <summary>
        /// Adds a key property to the specified entity type in the model.
        /// </summary>
        /// <remarks>If the specified entity type does not already have keys defined, a new key collection
        /// is created.</remarks>
        /// <param name="entityType">The type of the entity to which the key property belongs.</param>
        /// <param name="keyPropertyName">The name of the property to be added as a key for the entity.</param>
        public EdmModelBuilder AddEntityKey(Type entityType, string keyPropertyName)
        {
            if(!this.EntityKeys.ContainsKey(entityType))
            {
                this.EntityKeys[entityType] = new List<string>();
            }

            this.EntityKeys[entityType].Add(keyPropertyName);
            return this;
        }

        /// <summary>
        /// Adds a URI segment associated with the specified entity type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The URI segment is the part of the URI that represents the set of entities that an OData URI targets.
        /// </para>
        /// <para>
        /// So for instance, if you have an entity of type <c>Employee</c>, that entity would most likely be
        /// represneted by the URI segment <c>employees</c>.
        /// </para>
        /// </remarks>
        /// <typeparam name="TEntity">The type of the entity to associate with the URI segment.</typeparam>
        /// <param name="segment">The URI segment to add. Cannot be null or empty.</param>
        public EdmModelBuilder AddUriSegment<TEntity>(string segment)
        {
            return this.AddUriSegment(typeof(TEntity), segment);
        }

        /// <summary>
        /// Adds a URI segment associated the specified entity type.
        /// </summary>
        /// <param name="entityType">The type of the entity for which the URI segment is being defined.</param>
        /// <param name="segment">The URI segment to associate with the specified entity type. Cannot be null or empty.</param>
        public EdmModelBuilder AddUriSegment(Type entityType, string segment)
        {
            this.UriSegments[entityType] = segment;
            return this;
        }

        /// <summary>
        /// Adds an entity type to the model and optionally configures its key property and URI segment.
        /// </summary>
        /// <remarks>This method adds the specified entity type to the model. If a key property name is
        /// provided, it is configured as the entity's key. If a URI segment is provided, it is associated with the
        /// entity for use in URI generation.</remarks>
        /// <typeparam name="TEntity">The type of the entity to add to the model.</typeparam>
        /// <param name="keyPropertyName">The name of the property to use as the key for the entity. If null or empty, no key property is configured.</param>
        /// <param name="uriSegment">The URI segment to associate with the entity. If null or empty, no URI segment is configured.</param>
        /// <returns>The current <see cref="EdmModelBuilder"/> instance, allowing for method chaining.</returns>
        public EdmModelBuilder AddEntity<TEntity>(string? keyPropertyName = null, string? uriSegment = null)
        {
            this.AddEntity(typeof(TEntity));
            if(keyPropertyName?.Length > 0)
            {
                this.AddEntityKey<TEntity>(keyPropertyName);
            }
            if(uriSegment?.Length > 0)
            {
                this.AddUriSegment<TEntity>(uriSegment);
            }

            return this;
        }

        /// <summary>
        /// Adds an entity type to the model and optionally specifies its key property and URI segment.
        /// </summary>
        /// <remarks>If the specified entity type is not already part of the model, it is added.  If a key
        /// property name is provided, it is set as the key for the entity type.  Similarly, if a URI segment is
        /// provided, it is associated with the entity type.</remarks>
        /// <param name="entityType">The <see cref="Type"/> representing the entity to add to the model.</param>
        /// <param name="keyPropertyName">The name of the property to use as the key for the entity.  If null or empty, no key property is explicitly
        /// set.</param>
        /// <param name="uriSegment">The URI segment to associate with the entity type.  If null or empty, no URI segment is explicitly set.</param>
        /// <returns>The current <see cref="EdmModelBuilder"/> instance, allowing for method chaining.</returns>
        public EdmModelBuilder AddEntity(Type entityType, string? keyPropertyName = null, string? uriSegment = null)
        {
            if (!this.EntityTypes.Contains(entityType))
            {
                this.EntityTypes.Add(entityType);
            }
            if(keyPropertyName?.Length > 0)
            {
                this.AddEntityKey(entityType, keyPropertyName);
            }
            if (uriSegment?.Length > 0)
            {
                this.AddUriSegment(entityType, uriSegment);
            }

            return this;
        }

        /// <summary>
        /// Builds and returns an Entity Data Model (EDM) based on the configured entity types and URI segments.
        /// </summary>
        /// <remarks>This method iterates through the configured entity types, adds them to the OData
        /// model builder,  and associates them with entity sets if corresponding URI segments are defined. The
        /// resulting  EDM model can be used to define the structure of an OData service.</remarks>
        /// <returns>An <see cref="IEdmModel"/> representing the constructed Entity Data Model.</returns>
        public IEdmModel Build()
        {
            var builder = new ODataModelBuilder();


            foreach (var t in this.EntityTypes)
            {
                var typeConfig = this.AddEntityType(builder, t);
                if(this.UriSegments.ContainsKey(t))
                {
                    builder.AddEntitySet(this.UriSegments[t], typeConfig);
                }
            }

            return builder.GetEdmModel();
        }



        private EntityTypeConfiguration AddEntityType(ODataModelBuilder builder, Type entityType)
        {
            var config = builder.AddEntityType(entityType);
            var baseType = this.EntityTypes.FirstOrDefault(x => x == entityType.BaseType);
            if(null != baseType)
            {
                var baseConfig = this.AddEntityType(builder, baseType);
                config.BaseType = baseConfig;
            }
            this.AddEntityProperties(builder, config, entityType);

            if(this.EntityKeys.ContainsKey(entityType))
            {
                foreach(var propertyName in this.EntityKeys[entityType])
                {
                    var prop = entityType.GetProperty(propertyName);
                    if(null != prop)
                    {
                        config.HasKey(prop);
                    }
                }
            }
            return config;
        }

        private IEnumerable<PropertyConfiguration> AddEntityProperties(ODataModelBuilder builder, EntityTypeConfiguration entityConfig, Type entityType)
        {
            var configs = new List<PropertyConfiguration>();
            var flags = BindingFlags.Public | BindingFlags.Instance;
            if(null != entityConfig.BaseType)
            {
                flags |= BindingFlags.DeclaredOnly;
            }

            var simpleProps = from x in entityType.GetProperties(flags)
                              where
                                    (!x.PropertyType.IsClass || x.PropertyType == typeof(string))
                                    && !x.PropertyType.IsArray
                                    && !x.PropertyType.IsEnum
                              select x;
            foreach(var p in simpleProps)
            {
                var propertyConfig = entityConfig.AddProperty(p);
                propertyConfig.Name = this.ModifyPropertyName(p.Name);
                configs.Add(propertyConfig);
            }

            var complexProps = from x in entityType.GetProperties(flags)
                               where this.EntityTypes.Contains(x.PropertyType)
                               select x;
            foreach(var p in complexProps)
            {
                var propertyConfig = entityConfig.AddNavigationProperty(p, EdmMultiplicity.ZeroOrOne);
                propertyConfig.Name = this.ModifyPropertyName(p.Name);
                configs.Add(propertyConfig);
            }

            return configs;
        }

        private string? ModifyPropertyName(string name)
        {
            if(name?.Length > 0 && this.Options.PropertyNaming.NamingPolicy == PropertyNamingPolicy.CamelCase)
            {
                return $"{name.Substring(0, 1).ToLower()}{name.Substring(1)}";
            }

            return name;
        }
    }
}
