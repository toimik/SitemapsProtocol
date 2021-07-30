namespace Toimik.SitemapsProtocol.Tests
{
    using System;
    using System.IO;
    using Xunit;

    public class SitemapTest
    {
        const string Location = "http://www.example.com";

        [Fact]
        public void AllFieldsSpecified()
        {
            var sitemap = new Sitemap(Location);
            var expectedLocation = $"{Location}/sitemap.xml";
            var expectedLastModified = "2005-01-01";
            const ChangeFrequency ExpectedChangeFrequency = ChangeFrequency.Monthly;
            const double ExpectedPriority = 0.8;
            var data = @$"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{expectedLocation}</loc>
                        <lastmod>{expectedLastModified}</lastmod>
                        <changefreq>{ExpectedChangeFrequency.ToString().ToLower()}</changefreq>
                        <priority>{ExpectedPriority}</priority>
                    </url>
                </urlset>".TrimStart();

            sitemap.Load(data);

            var entries = sitemap.Entries;
            entries.MoveNext();
            var entry = entries.Current;
            Assert.Equal(expectedLocation, entry.Location);
            Assert.Equal(ExpectedChangeFrequency, entry.ChangeFrequency);
            Assert.Equal(ExpectedPriority, entry.Priority);
            Assert.Equal(DateTime.Parse(expectedLastModified), entry.LastModified);
        }

        [Fact]
        public void CustomSchema()
        {
            var extendedSitemap = new ExtendedSitemap(Location);
            const string ExpectedValue = "foobar";
            var data = @$"
                <?xml version='1.0' encoding='UTF-8'?>
                <urlset
                    xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9""
                    xmlns:example=""http://www.example.com/schemas/0.9"">
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                        <example:title>{ExpectedValue}</example:title>
                    </url>
                </urlset>".TrimStart();
            using var stream = File.OpenRead(@$"Resources{Path.DirectorySeparatorChar}example.xsd");
            extendedSitemap.Load(data, stream);

            var entries = extendedSitemap.Entries;
            entries.MoveNext();
            var entry = (ExtendedSitemapEntry)entries.Current;
            Assert.Equal(ExpectedValue, entry.Title);
        }

        [Fact]
        public void DuplicateLocation()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                        <priority>0</priority>
                    </url>
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                        <priority>0</priority>
                    </url>
                </urlset>".TrimStart();

            sitemap.Load(data);

            Assert.Equal(1, sitemap.EntryCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Empty(string data)
        {
            var sitemap = new Sitemap(Location);

            Assert.Throws<ArgumentException>(() => sitemap.Load(data));
        }

        [Fact]
        public void EntryMaxCount()
        {
            var sitemap = new Sitemap(Location, entryMaxCount: 2);
            var data = @$"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                    <url>
                        <loc>{Location}/sitemap2.xml</loc>
                    </url>
                    <url>
                        <loc>{Location}/sitemap3.xml</loc>
                    </url>
                </urlset>".TrimStart();

            sitemap.Load(data);

            Assert.Equal(sitemap.EntryMaxCount, sitemap.EntryCount);
        }

        [Fact]
        public void InvalidChangeFrequency()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <changefreq>invalid</changefreq>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                </urlset>".TrimStart();

            Assert.Throws<ArgumentException>(() => sitemap.Load(data));
        }

        [Fact]
        public void InvalidConstructorParameter()
        {
            Assert.Throws<ArgumentException>(() => new Sitemap("www.example.com"));
        }

        [Fact]
        public void InvalidLastModified()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <lastmod>invalid</lastmod>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                </urlset>".TrimStart();

            Assert.Throws<ArgumentException>(() => sitemap.Load(data));
        }

        [Fact]
        public void InvalidLocation()
        {
            var sitemap = new Sitemap(Location);
            var data = @"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>invalid</loc>
                    </url>
                </urlset>".TrimStart();

            Assert.Throws<ArgumentException>(() => sitemap.Load(data));
        }

        [Fact]
        public void InvalidPriority()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <priority>invalid</priority>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                </urlset>".TrimStart();

            Assert.Throws<ArgumentException>(() => sitemap.Load(data));
        }

        [Fact]
        public void InvalidRootTag()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <invalid xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                </invalid>".TrimStart();

            Assert.Throws<ArgumentException>(() => sitemap.Load(data));
        }

        [Fact]
        public void InvalidUrlTag()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <invalid>
                        <loc>{Location}/sitemap.xml</loc>
                    </invalid>
                </urlset>".TrimStart();

            Assert.Throws<ArgumentException>(() => sitemap.Load(data));
        }

        [Fact]
        public void LoadStartsAfresh()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                    <url>
                        <loc>{Location}/sitemap2.xml</loc>
                    </url>
                </urlset>".TrimStart();
            sitemap.Load(data);
            var url = $"{Location}/sitemap3.xml";
            data = @$"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{url}</loc>
                    </url>
                </urlset>".TrimStart();
            sitemap.Load(data);

            var entries = sitemap.Entries;
            entries.MoveNext();
            var entry = entries.Current;
            Assert.Equal(url, entry.Location);
            Assert.False(entries.MoveNext());
        }

        [Theory]
        [InlineData("http://www.example.com", false)]
        [InlineData("http://www.example.com/", false)]
        [InlineData("http://www.example.com:80", false)]
        [InlineData("http://www.example.com:80/", false)]
        [InlineData("http://www.example.com:8080/", false)]
        [InlineData("http://WWW.EXAMPLE.COM:8080/path", false)]
        [InlineData("HTTP://WWW.EXAMPLE.COM/sitemap.xml", true)]
        [InlineData("http://www.example.com:80/sitemap.xml", true)]
        [InlineData("http://username:password@www.example.com/sitemap.xml", true)]
        public void LocationValidity(string location, bool isEqual)
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{location}</loc>
                    </url>
                </urlset>".TrimStart();

            sitemap.Load(data);

            Assert.Equal(isEqual, sitemap.Entries.MoveNext());
        }

        [Fact]
        public void MissingAllFields()
        {
            var sitemap = new Sitemap(Location);
            var data = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            Assert.Throws<ArgumentException>(() => sitemap.Load(data));
        }

        [Fact]
        public void MissingDeclaration()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                </urlset>".TrimStart();

            sitemap.Load(data);

            Assert.True(sitemap.Entries.MoveNext());
        }

        [Fact]
        public void MissingNamespace()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <urlset>
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                </urlset>".TrimStart();

            Assert.Throws<ArgumentException>(() => sitemap.Load(data));
        }

        [Fact]
        public void MultipleInstantiations()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                </urlset>".TrimStart();
            sitemap.Load(data);
            sitemap = new Sitemap(Location);
            sitemap.Load(data);

            Assert.Equal(1, sitemap.EntryCount);
        }

        [Fact]
        public void NonUtf8Encoding()
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <?xml version=""1.0"" encoding=""UTF-16""?>
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{Location}/sitemap.xml</loc>
                    </url>
                </urlset>".TrimStart();

            Assert.Throws<ArgumentException>(() => sitemap.Load(data));
        }

        [Theory]
        [InlineData("http://example.com")]
        [InlineData("https://www.example.com")]
        [InlineData("https://www.example.com:8080")]
        public void SupersetLocation(string location)
        {
            var sitemap = new Sitemap(Location);
            var data = @$"
                <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <url>
                        <loc>{location}/sitemap.xml</loc>
                    </url>
                </urlset>".TrimStart();

            sitemap.Load(data);

            Assert.False(sitemap.Entries.MoveNext());
        }

        private class ExtendedSitemap : Sitemap
        {
            public ExtendedSitemap(string location)
                : base(location)
            {
            }

            protected override SitemapEntry CreateEntry()
            {
                return new ExtendedSitemapEntry();
            }

            protected override void HandleExtendedTag(
                SitemapEntry entry,
                string name,
                string value)
            {
                if (name.Equals("title"))
                {
                    ((ExtendedSitemapEntry)entry).Title = value;
                }
            }
        }

        private class ExtendedSitemapEntry : SitemapEntry
        {
            public ExtendedSitemapEntry()
                : base()
            {
            }

            public string Title { get; internal set; }
        }
    }
}