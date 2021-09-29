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

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Toimik.SitemapsProtocol.Tests")]

namespace Toimik.SitemapsProtocol
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;

    public static class Utils
    {
        public static string AddDefaultPortIfMissing(string url)
        {
            // NOTE: Parameter is guaranteed to be in a valid URL format
            const string ColonSlashSlash = "://";
            var index = url.IndexOf(ColonSlashSlash);
            var startIndex = index + ColonSlashSlash.Length;
            var colonIndex = url.IndexOf(':', startIndex);
            if (colonIndex == -1)
            {
                // There will always be a slash
                var slashIndex = url.IndexOf('/', startIndex);

                // e.g. http://www.example.com/

                // e.g. http://www.example.com/path
                var prefix = url.Substring(0, slashIndex);
                var suffix = url[slashIndex..];
                url = $"{prefix}:80{suffix}";
            }

            return url;
        }

        public static string NormalizeLocation(string location)
        {
            string temp = null;
            try
            {
                var url = new Uri(location);
                if (url.UserInfo != null)
                {
                    // The standard has deprecate username:password in URL
                    location = $"{url.Scheme}://{url.Authority}{url.PathAndQuery}";
                }

                temp = AddDefaultPortIfMissing(location);
            }
            catch (UriFormatException)
            {
                // Do nothing
            }

            return temp;
        }

        internal static XmlSchemaSet CreateSchemaSet(string schema)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(schema);
            var schemaSet = CreateSchemaSet(stream);
            return schemaSet;
        }

        internal static XmlSchemaSet CreateSchemaSet(Stream stream)
        {
            var schemaSet = new XmlSchemaSet();
            var settings = new XmlReaderSettings
            {
                CloseInput = true,
            };
            using var reader = XmlReader.Create(stream, settings);
            schemaSet.Add(null, reader);
            schemaSet.Compile();
            return schemaSet;
        }

        internal static void ValidateNamespace(XmlReader reader)
        {
            if (reader.NamespaceURI == string.Empty)
            {
                throw new XmlException($"Root element's namespace must be http://www.sitemaps.org/schemas/sitemap/0.9.");
            }
        }
    }
}