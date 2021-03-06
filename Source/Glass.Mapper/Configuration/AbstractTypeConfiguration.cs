/*
   Copyright 2012 Michael Edwards
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
*/ 
//-CRE-


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace Glass.Mapper.Configuration
{
    /// <summary>
    /// Represents the configuration for a .Net type
    /// </summary>
    [DebuggerDisplay("Type: {Type}")]
    public abstract class AbstractTypeConfiguration
    {
        private List<AbstractPropertyConfiguration> _properties;

        /// <summary>
        /// The type this configuration represents
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get;  set; }

        /// <summary>
        /// A list of the properties configured on a type
        /// </summary>
        /// <value>The properties.</value>
        public IEnumerable<AbstractPropertyConfiguration> Properties { get { return _properties; } }

        /// <summary>
        /// A list of the constructors on a type
        /// </summary>
        /// <value>The constructor methods.</value>
        public IDictionary<ConstructorInfo, Delegate> ConstructorMethods { get; set; }

        /// <summary>
        /// Indicates properties should be automatically mapped
        /// </summary>
        public bool AutoMap { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractTypeConfiguration"/> class.
        /// </summary>
        public AbstractTypeConfiguration()
        {
            _properties = new List<AbstractPropertyConfiguration>();
        }



        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="property">The property.</param>
        public virtual void AddProperty(AbstractPropertyConfiguration property)
        {
            if(_properties.Any(x=>x.PropertyInfo.Name == property.PropertyInfo.Name))
                throw new MapperException("You can not have duplicate mappings for properties. Property Name: {0}  Type: {0}".Formatted(property.PropertyInfo.Name, Type.Name));

            if(property != null)
                _properties.Add(property);
        }


        /// <summary>
        /// Maps the properties to object.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="service">The service.</param>
        /// <param name="context">The context.</param>
        public void MapPropertiesToObject( object obj, IAbstractService service, AbstractTypeCreationContext context)
        {
            //create properties 
            AbstractDataMappingContext dataMappingContext = service.CreateDataMappingContext(context, obj);

            foreach (var prop in Properties)
            {
                try
                {
                    prop.Mapper.MapCmsToProperty(dataMappingContext);
                }
                catch (Exception e)
                {
                    throw new MapperException("Failed to map property {0} on {1}".Formatted(prop.PropertyInfo.Name, prop.PropertyInfo.DeclaringType.FullName), e);
                }
            }
        }

        /// <summary>
        /// Called when the AutoMap property is true. Automatically maps un-specified properties.
        /// </summary>
        public void PerformAutoMap()
        {
            //we now run the auto-mapping after all the static configuration is loaded
            if (AutoMap)
            {
                //TODO: ME - probably need some binding flags.
                foreach (var propConfig in AutoMapProperties(Type))
                {
                    AddProperty(propConfig);
                }
            }
        
        }

        /// <summary>
        /// Autoes the map properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public virtual IEnumerable<AbstractPropertyConfiguration> AutoMapProperties(Type type)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                    BindingFlags.FlattenHierarchy;
            IEnumerable<PropertyInfo> properties = type.GetProperties(flags);

            if (type.IsInterface)
            {
                foreach (var inter in type.GetInterfaces())
                {
                    properties = properties.Union(inter.GetProperties(flags));
                }
            }

            foreach (var property in properties)
            {
                if (Properties.All(x => x.PropertyInfo != property))
                {
                    //skipped already mapped properties
                    if(_properties.Any(x=>x.PropertyInfo.Name == property.Name))
                        continue;

                    var propConfig = AutoMapProperty(property);
                    if (propConfig != null)
                        yield return propConfig;
                }
            }
        }

        /// <summary>
        /// Called to map each property automatically
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual AbstractPropertyConfiguration AutoMapProperty(PropertyInfo property)
        {
            return null;
        }
    }
}





