/*
 * Copyright 2021-2022 nurhafiz@hotmail.sg
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

namespace Toimik.SitemapsProtocol;

using System;

public class SitemapEntry : IEntry
{
    private const string TagForChangeFrequency = "changefreq";

    private const string TagForLastModified = "lastmod";

    private const string TagForLocation = "loc";

    private const string TagForPriority = "priority";

    public SitemapEntry(string baseLocation)
    {
        BaseLocation = baseLocation;
    }

    public string BaseLocation { get; }

    public ChangeFrequency? ChangeFrequency { get; private set; }

    public DateTime? LastModified { get; private set; }

    public string? Location { get; private set; }

    public double? Priority { get; private set; }

    internal virtual void Set(string name, string value)
    {
        switch (name)
        {
            case TagForChangeFrequency:
                ChangeFrequency = (ChangeFrequency)Enum.Parse(typeof(ChangeFrequency), value, ignoreCase: true);
                break;

            case TagForLastModified:
                LastModified = DateTime.Parse(value);
                break;

            case TagForLocation:
                var location = Utils.NormalizeLocation(new Uri(value.Trim()));
                var root = BaseLocation.ToString();
                if (location.Equals(root, StringComparison.OrdinalIgnoreCase)
                    || !location.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                Location = location;
                break;

            case TagForPriority:
                Priority = double.Parse(value);
                break;
        }
    }
}