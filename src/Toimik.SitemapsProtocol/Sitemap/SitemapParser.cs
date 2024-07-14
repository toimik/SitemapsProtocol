/*
 * Copyright 2021-2024 nurhafiz@hotmail.sg
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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

public class SitemapParser(Uri location, int entryMaxCount = SitemapParser.DefaultEntryMaxCount)
{
    // As per standard
    internal const int DefaultEntryMaxCount = 50000;

    private const string TagForUrl = "url";

    private const string TagForUrlSet = "urlset";

    private static readonly XmlSchemaSet SchemaSet;

    static SitemapParser()
    {
        SchemaSet = Utils.CreateSchemaSet($"{typeof(Sitemap).Namespace}.Resources.sitemap.xsd");
    }

    public int EntryMaxCount { get; } = entryMaxCount;

    public string Location { get; } = Utils.NormalizeLocation(location) ?? throw new ArgumentException($"{nameof(location)} is not in a valid format.");

    public async IAsyncEnumerable<SitemapEntry> Parse(Stream dataStream, Stream? schemaStream = null)
    {
        var settings = new XmlReaderSettings
        {
            Async = true,
        };

        var inner = XmlReader.Create(dataStream, settings);
        settings.Schemas = schemaStream == null
            ? SchemaSet
            : Utils.CreateSchemaSet(schemaStream);
        settings.ValidationType = ValidationType.Schema;
        var reader = XmlReader.Create(inner, settings);
        await reader.MoveToContentAsync();
        Utils.ValidateNamespace(reader);
        var entryCount = 0;
        var entry = CreateEntry();
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            if (!reader.IsStartElement())
            {
                if (reader.Name.Equals(TagForUrlSet)
                    && entry.Location != null
                        && entryCount < EntryMaxCount)
                {
                    yield return entry;
                }
            }
            else
            {
                var isWithinMaxCount = true;
                var name = reader.Name;
                switch (name)
                {
                    case TagForUrl:
                        if (entry.Location != null)
                        {
                            isWithinMaxCount = entryCount < EntryMaxCount;
                            if (!isWithinMaxCount)
                            {
                                break;
                            }

                            yield return entry;
                            entryCount++;
                        }

                        entry = CreateEntry();
                        break;

                    default:
                        var value = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        entry.Set(name, value);
                        break;
                }

                if (!isWithinMaxCount)
                {
                    break;
                }
            }
        }
    }

    protected virtual SitemapEntry CreateEntry() => new(Location);
}