/*
 * Copyright 2021 nurhafiz@hotmail.sg
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Toimik.SitemapsProtocol
{
    using System;

    public class SitemapIndexEntry : IEntry
    {
        private const string TagForLastModified = "lastmod";

        private const string TagForLocation = "loc";

        public SitemapIndexEntry(string baseLocation)
        {
            BaseLocation = baseLocation;
        }

        public string BaseLocation { get; }

        public DateTime? LastModified { get; internal set; }

        public string Location { get; internal set; }

        internal virtual void Set(string name, string value)
        {
            switch (name)
            {
                case TagForLastModified:
                    LastModified = DateTime.Parse(value);
                    break;

                case TagForLocation:
                    var location = Utils.NormalizeLocation(value.Trim());
                    if (location.Equals(BaseLocation, StringComparison.OrdinalIgnoreCase)
                        || !location.StartsWith(BaseLocation, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    Location = location;
                    break;
            }
        }
    }
}