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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

public class Sitemap(SitemapParser parser)
{
    private readonly ISet<SitemapEntry> entries = new HashSet<SitemapEntry>(new EntryComparer());

    public Sitemap(Uri location, int entryMaxCount = SitemapParser.DefaultEntryMaxCount)
        : this(new SitemapParser(location, entryMaxCount))
    {
    }

    public IEnumerator<SitemapEntry> Entries => entries.GetEnumerator();

    public int EntryCount => entries.Count;

    public SitemapParser Parser { get; } = parser;

    public bool AddEntry(SitemapEntry entry)
    {
        var isWithinMaxCount = entries.Count < Parser.EntryMaxCount;
        var isAdded = isWithinMaxCount
            && entries.Add(entry);
        return isAdded;
    }

    /// <summary>
    /// Loads, to this instance, the data of a sitemap from a <see cref="string"/>.
    /// </summary>
    /// <param name="data">Data of a sitemap.</param>
    /// <param name="schemaStream">
    /// <see cref="Stream"/> of schema, which is used to validate the sitemap index against. If
    /// <c>null</c>, the default one is used.
    /// </param>
    /// <remarks>All existing entries, if any, are cleared when this method is called.</remarks>
    public void Load(string data, Stream? schemaStream = null)
    {
        var byteArray = Encoding.UTF8.GetBytes(data);
        using var dataStream = new MemoryStream(byteArray);
        try
        {
            Load(dataStream, schemaStream).Wait();
        }
        catch (AggregateException ex)
        {
            throw ex.InnerException!;
        }
    }

    /// <summary>
    /// Loads, to this instance, the data of a sitemap from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="dataStream">
    /// A stream containing the data of a sitemap. This is left opened after processing.
    /// </param>
    /// <param name="schemaStream">
    /// <see cref="Stream"/> of schema, which is used to validate the sitemap index against. If
    /// <c>null</c>, the default one is used.
    /// </param>
    /// <returns>A <see cref="Task"/>.</returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when <paramref name="dataStream"/> is manually closed.
    /// </exception>
    /// <remarks>
    /// All existing entries, if any, are cleared when this method is called.
    /// <para>Call <see cref="Stream.Close()"/> on <paramref name="dataStream"/> to cancel loading.</para>
    /// </remarks>
    public async Task Load(Stream dataStream, Stream? schemaStream = null)
    {
        entries.Clear();
        try
        {
            await DoLoad(dataStream, schemaStream).ConfigureAwait(false);
        }
        catch (XmlException ex)
        {
            throw new ArgumentException(ex.Message);
        }
        catch (XmlSchemaValidationException ex)
        {
            throw new ArgumentException(ex.Message);
        }
    }

    private async Task DoLoad(Stream dataStream, Stream? schemaStream)
    {
        await foreach (SitemapEntry entry in Parser.Parse(dataStream, schemaStream).ConfigureAwait(false))
        {
            AddEntry(entry);
        }
    }
}