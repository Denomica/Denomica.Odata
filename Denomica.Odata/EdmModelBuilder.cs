
using Humanizer;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
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
        private Dictionary<Type, EntityTypeDefinition> EntityTypes = new Dictionary<Type, EntityTypeDefinition>();


        /// <summary>
        /// Adds a key property to the entity type specified by <typeparamref name="TEntity"/>.
        /// </summary>
        /// <remarks>
        /// If the entity type does not exist in the model, it will be added.
        /// </remarks>
        /// <typeparam name="TEntity">The type of the entity to which the key property belongs.</typeparam>
        /// <param name="keyPropertyName">The name of the property to designate as the key for the entity type.</param>
        public EdmModelBuilder AddEntityKey<TEntity>(string keyPropertyName)
        {
            return this.AddEntityKey(typeof(TEntity), keyPropertyName: keyPropertyName);
        }

        /// <summary>
        /// Adds a key property to the specified entity type in the model.
        /// </summary>
        /// <remarks>
        /// If the entity type does not exist in the model, it will be added.
        /// </remarks>
        /// <param name="entityType">The type of the entity to which the key property belongs.</param>
        /// <param name="keyPropertyName">The name of the property to be added as a key for the entity.</param>
        public EdmModelBuilder AddEntityKey(Type entityType, string keyPropertyName)
        {
            var et = this.EnsureEntityTypeDefinition(entityType);
            if (!et.KeyProperties.Contains(keyPropertyName))
            {
                et.KeyProperties.Add(keyPropertyName);
            }

            return this;
        }

        /// <summary>
        /// Configures the entity set name for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to associate with the entity set.</typeparam>
        /// <param name="entitySet">The name of the entity set to associate with the entity type. Cannot be null or empty.</param>
        public EdmModelBuilder SetEntitySet<TEntity>(string entitySet)
        {
            return this.SetEntitySet(typeof(TEntity), entitySet);
        }

        /// <summary>
        /// Configures the entity set name for the specified entity type.
        /// </summary>
        /// <param name="entityType">The <see cref="Type"/> representing the entity for which the entity set is being configured.</param>
        /// <param name="entitySet">The name of the entity set to associate with the specified entity type. Cannot be null or empty.</param>
        public EdmModelBuilder SetEntitySet(Type entityType, string entitySet)
        {
            var et = this.EnsureEntityTypeDefinition(entityType);
            et.EntitySet = entitySet;

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
        /// <param name="entitySet">The entity set name associated with the entity type.</param>
        public EdmModelBuilder AddEntity<TEntity>(string? keyPropertyName = null, string? entitySet = null)
        {
            return this.AddEntity(typeof(TEntity), keyPropertyName: keyPropertyName, uriSegment: entitySet);
        }

        /// <summary>
        /// Adds an entity type to the model and optionally specifies its key property and URI segment.
        /// </summary>
        /// <remarks>If the specified entity type is not already part of the model, it is added.  If a key
        /// property name is provided, it is set as the key for the entity type.  Similarly, if a URI segment is
        /// provided, it is associated with the entity type.</remarks>
        /// <param name="entityType">The <see cref="Type"/> representing the entity to add to the model.</param>
        /// <param name="keyPropertyName">
        /// The name of the property to use as the key for the entity.  If null or empty, no key 
        /// property is explicitly set.
        /// </param>
        /// <param name="entitySet">The entity set name associated with the entity type.</param>
        public EdmModelBuilder AddEntity(Type entityType, string? keyPropertyName = null, string? uriSegment = null)
        {
            var et = this.EnsureEntityTypeDefinition(entityType);

            if(keyPropertyName?.Length > 0)
            {
                this.AddEntityKey(entityType, keyPropertyName);
            }
            if (uriSegment?.Length > 0)
            {
                this.SetEntitySet(entityType, uriSegment);
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


            foreach (var t in this.EntityTypes.Keys)
            {
                var et = this.EntityTypes[t];
                var typeConfig = this.AddEntityType(builder, et);
                if(et.EntitySet?.Length > 0)
                {
                    builder.AddEntitySet(et.EntitySet, typeConfig);
                }
            }

            return builder.GetEdmModel();
        }



        private EntityTypeConfiguration AddEntityType(ODataModelBuilder builder, EntityTypeDefinition entityTypeDef)
        {
            var config = builder.AddEntityType(entityTypeDef.EntityType);

            var baseType = entityTypeDef.EntityType.BaseType;
            var baseTypeDef = null != baseType ? this.GetEntityTypeDefinitionOrCreateDefault(baseType) : null;

            if (null != baseTypeDef)
            {
                var baseConfig = this.AddEntityType(builder, baseTypeDef);
                config.BaseType = baseConfig;
            }

            this.AddEntityProperties(builder, config, entityTypeDef.EntityType);

            if(null != entityTypeDef)
            {
                if(entityTypeDef.KeyProperties.Count == 0)
                {
                    foreach (var keyProp in this.GetKeyProperties(entityTypeDef.EntityType))
                    {
                        entityTypeDef.KeyProperties.Add(keyProp.Name);
                    }
                }

                foreach (var key in entityTypeDef.KeyProperties)
                {
                    var prop = entityTypeDef.EntityType.GetProperty(key);
                    if (null != prop && (null == baseTypeDef || !baseTypeDef.KeyProperties.Contains(prop.Name)))
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
                               where this.EntityTypes.Keys.Contains(x.PropertyType)
                               select x;
            foreach(var p in complexProps)
            {
                var propertyConfig = entityConfig.AddNavigationProperty(p, EdmMultiplicity.ZeroOrOne);
                propertyConfig.Name = this.ModifyPropertyName(p.Name);
                configs.Add(propertyConfig);
            }

            return configs;
        }

        private EntityTypeDefinition CreateEntityTypeDefinition(Type entityType)
        {
            var et = new EntityTypeDefinition
            {
                EntityType = entityType,
                EntitySet = entityType.Name.Pluralize().ToLower()
            };

            return et;
        }

        private EntityTypeDefinition EnsureEntityTypeDefinition(Type entityType)
        {
            if (!this.EntityTypes.ContainsKey(entityType))
            {
                this.EntityTypes[entityType] = this.CreateEntityTypeDefinition(entityType);
            }

            return this.EntityTypes[entityType];
        }

        private EntityTypeDefinition GetEntityTypeDefinitionOrCreateDefault(Type entityType)
        {
            if(this.EntityTypes.ContainsKey(entityType))
            {
                return this.EntityTypes[entityType];
            }

            return this.CreateEntityTypeDefinition(entityType);
        }

        private IEnumerable<PropertyInfo> GetKeyProperties(Type entityType)
        {
            var keyProps = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => null != x.GetCustomAttribute<KeyAttribute>());
            return keyProps;
        }
        private string? ModifyPropertyName(string name)
        {
            if(name?.Length > 0 && this.Options.PropertyNaming.NamingPolicy == PropertyNamingPolicy.CamelCase)
            {
                return $"{name.Substring(0, 1).ToLower()}{name.Substring(1)}";
            }

            return name;
        }


        private class EntityTypeDefinition
        {
            public Type EntityType { get; set; } = null!;

            public string EntitySet { get; set; } = null!;

            public List<string> KeyProperties { get; set; } = new List<string>();

            public override string ToString()
            {
                return this.EntityType.FullName ?? base.ToString() ?? string.Empty;
            }
        }
    }
}
