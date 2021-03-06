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
using System.Linq;
using System.Web;
using Glass.Mapper.Sc.Configuration;
using Glass.Mapper.Sc.Configuration.Attributes;
using Glass.Mapper.Sites.Sc.Models.Content;
using Sitecore.Globalization;

namespace Glass.Mapper.Sites.Sc.Models.Landing
{
    [SitecoreType]
    public class EventsLanding
    {
        [SitecoreId]
        public virtual Guid Id { get; set; }

        [SitecoreInfo(SitecoreInfoType.Language)]
        public virtual Language Language { get; set; }

        [SitecoreField("Page Title")]
        public virtual string Title { get; set; }

        [SitecoreField]
        public virtual string MainBody { get; set; }

        [SitecoreQuery("./*/*/*[@@templatename='Event']", IsRelative = true)]
        public virtual IEnumerable<Event> Events { get; set; }
    }
}
