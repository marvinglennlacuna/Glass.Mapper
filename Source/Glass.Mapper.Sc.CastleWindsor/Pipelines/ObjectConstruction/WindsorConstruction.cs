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
using System.Linq;
using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Glass.Mapper.Pipelines.ObjectConstruction;

namespace Glass.Mapper.Sc.CastleWindsor.Pipelines.ObjectConstruction
{
    /// <summary>
    /// WindsorConstruction
    /// </summary>
    public class WindsorConstruction : IObjectConstructionTask
    {
        

        /// <summary>
        /// Initializes static members of the <see cref="CreateConcreteTask"/> class.
        /// </summary>
        static WindsorConstruction()
        {
            
        }

        /// <summary>
        /// Executes the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        public void Execute(ObjectConstructionArgs args)
        {
            if (args.Result != null)
            {
                return;
            }
            var resolver = args.Context.DependencyResolver as DependencyResolver;
            if (resolver == null)
            {
                return;
            }

            if (args.AbstractTypeCreationContext.ConstructorParameters == null ||
                !args.AbstractTypeCreationContext.ConstructorParameters.Any())
            {
                var type = args.Configuration.Type;
                var container = resolver.Container;

                if (type.IsClass)
                {
                    if (!container.Kernel.HasComponent(typeof (LazyObjectInterceptor)))
                    {
                        container.Kernel.Register(Component.For<LazyObjectInterceptor>().LifestyleTransient());
                    }
                    if (!container.Kernel.HasComponent(type))
                    {
                        container.Kernel.Register(
                            Component.For(type).Named(type.FullName).LifeStyle.Is(LifestyleType.Transient)
                            );
                        container.Kernel.Register(
                            Component.For(type).Named(type.FullName+"lazy").LifeStyle.Is(LifestyleType.Transient)
                                .Interceptors<LazyObjectInterceptor>()
                            );
                    }

                    Action<object> mappingAction = (target) => 
                        args.Configuration.MapPropertiesToObject(target, args.Service, args.AbstractTypeCreationContext);


                    object result = null;
                    if (args.AbstractTypeCreationContext.IsLazy)
                    {
                        using (new UsingLazyInterceptor())
                        {
                            result = container.Resolve(type.FullName+"lazy", type);
                            var proxy = result as IProxyTargetAccessor;
                            var interceptor = proxy.GetInterceptors().First(x=>x is LazyObjectInterceptor) as LazyObjectInterceptor;
                            interceptor.MappingAction = mappingAction;
                            interceptor.Actual = result;
                        }
                    }
                    else
                    {
                        result = container.Resolve(type);
                        if (result != null)
                        {
                            mappingAction(result);
                        }
                    }
                    
                   

                    args.Result = result;

                }//if (type.IsClass)
            }


        }
    }
}


