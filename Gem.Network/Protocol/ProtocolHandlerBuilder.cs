﻿using System.Text.RegularExpressions;
using System;
using System.Text;
using System.Collections.Generic;
using CSScriptLibrary;
using System.Linq;
using Gem.Network.Handlers;
using Gem.Network.Builders;

namespace Gem.Network.Protocol
{
    /// <summary>
    /// Creates a type that handles the server incoming messages
    /// </summary>
    public class ProtocolHandlerBuilder
    {
        private string GetMapper(List<DynamicPropertyInfo> propertyFields)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < propertyFields.Count; i++)
            {
                sb.Append(string.Format("{0} =({1})props[{2}],",propertyFields[i].PropertyName, DynamicPropertyInfo.GetPrimitiveTypeAlias(propertyFields[i].PropertyType),i));
            }
            sb.Length--;
            return sb.ToString();
        }

        public Type Build<TPackage>(string assembly, string classname, string packageType)
            where TPackage:new()
        {
            var str = String.Format(@"using System; using Gem.Network.Extensions; using {3};
                                            namespace Gem.Network.Builders
                                            {{
                                            public class {0} : Gem.Network.Handlers.IMessageHandler
                                             {{ private readonly Action<{2}> action;
                                              public {0}() {{}}                                                                                               
                                                 public {0}(Action<{2}> action)
                                                 {{
                                                     this.action = action;
                                                 }}
                                                  public void Handle(object args)
                                                 {{
                                                      var props = args.ReadAllProperties();
                                                      {2} package = new {2} {{ {1} }};
                                                      action(package);
                                                 }}                                           
                                                }}                                                                                        
                                             }}", classname,
                                                GetMapper(DynamicPropertyInfo.GetPropertyTypesAndNames<TPackage>().ToList()),
                                                packageType,
                                                assembly);

            return CSScript.LoadCode(str)
                           .CreateObject("*")
                           .AlignToInterface<IMessageHandler>()
                           .GetType();
        }          

    }
}