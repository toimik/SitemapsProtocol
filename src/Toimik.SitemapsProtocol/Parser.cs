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

public class Parser(
    Uri location,
    string tagNameForRoot,
    string tagNameForEntry,
    string schemaLocation,
    IEntryFactory entryFactory,
    int entryMaxCount = Parser.DefaultEntryMaxCount)
{
    // As per standard
    internal const int DefaultEntryMaxCount = 50000;

    public IEntryFactory EntryFactory { get; } = entryFactory;

    public int EntryMaxCount { get; } = entryMaxCount;

    public string Location { get; } = Utils.NormalizeLocation(location)
        ?? throw new ArgumentException($"{nameof(location)} is not in a valid format.");

    public string SchemaLocation { get; } = schemaLocation;

    public string TagNameForEntry { get; } = tagNameForEntry;

    public string TagNameForRoot { get; } = tagNameForRoot;

    protected XmlSchemaSet SchemaSet { get; } = Utils.CreateSchemaSet($"{typeof(SitemapIndex).Namespace}.{schemaLocation}");

    public async IAsyncEnumerable<IEntry> Parse(Stream dataStream, Stream? schemaStream = null)
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
        var entry = EntryFactory.Create(Location);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            if (!reader.IsStartElement())
            {
                if (reader.Name.Equals(TagNameForRoot)
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
                if (!name.Equals(TagNameForEntry))
                {
                    var value = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    entry.Set(name, value);
                }
                else
                {
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

                    entry = EntryFactory.Create(Location);
                }

                if (!isWithinMaxCount)
                {
                    break;
                }
            }
        }
    }
}