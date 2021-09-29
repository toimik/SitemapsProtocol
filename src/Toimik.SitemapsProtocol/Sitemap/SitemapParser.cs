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
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;

    public class SitemapParser
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

        public SitemapParser(Uri location, int entryMaxCount = DefaultEntryMaxCount)
        {
            Location = Utils.NormalizeLocation(location) ?? throw new ArgumentException($"{nameof(location)} is not in a valid format.");
            EntryMaxCount = entryMaxCount;
        }

        public int EntryMaxCount { get; }

        public string Location { get; }

        public async IAsyncEnumerable<SitemapEntry> Parse(Stream dataStream, Stream schemaStream)
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
            reader.MoveToContent();
            Utils.ValidateNamespace(reader);
            var entryCount = 0;
            var entry = CreateEntry();
            while (await reader.ReadAsync())
            {
                if (!reader.IsStartElement())
                {
                    if (reader.Name.Equals(TagForUrlSet))
                    {
                        if (entry.Location != null)
                        {
                            var isWithinMaxCount = entryCount < EntryMaxCount;
                            if (isWithinMaxCount)
                            {
                                yield return entry;
                            }
                        }
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
                            var value = await reader.ReadElementContentAsStringAsync();
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

        protected virtual SitemapEntry CreateEntry()
        {
            return new SitemapEntry(Location);
        }
    }
}