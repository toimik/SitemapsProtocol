namespace Toimik.SitemapsProtocol.Tests
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class SitemapIndexTest
    {
        const string Location = "http://www.example.com";

        [Fact]
        public void AllFieldsSpecified()
        {
            var index = new SitemapIndex(Location);
            var expectedLocation = $"{Location}/sitemap-index.xml.gz";
            var expectedLastModified = "2005-01-01";
            var data = @$"
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{expectedLocation}</loc>
                        <lastmod>{expectedLastModified}</lastmod>
                    </sitemap>
                </sitemapindex>".TrimStart();

            index.Load(data);

            var entries = index.Entries;
            var entry = GetOnlyEntry(entries);
            Assert.Equal(Utils.NormalizeLocation(expectedLocation), entry.Location);
            Assert.Equal(DateTime.Parse(expectedLastModified), entry.LastModified);
        }

        [Fact]
        public void DuplicateLocation()
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                    <sitemap>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();

            index.Load(data);

            Assert.Equal(1, index.EntryCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Empty(string data)
        {
            var index = new SitemapIndex(Location);

            Assert.Throws<ArgumentException>(() => index.Load(data));
        }

        [Fact]
        public void EntryMaxCount()
        {
            var index = new SitemapIndex(Location, entryMaxCount: 2);
            var data = @$"
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                    <sitemap>
                        <loc>{Location}/sitemap2.xml.gz</loc>
                    </sitemap>
                    <sitemap>
                        <loc>{Location}/sitemap3.xml.gz</loc>
                    </sitemap>
                    <sitemap>
                        <loc>{Location}/sitemap4.xml.gz</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();

            index.Load(data);

            Assert.Equal(index.Parser.EntryMaxCount, index.EntryCount);
        }

        [Fact]
        public void InvalidConstructorParameter()
        {
            Assert.Throws<ArgumentException>(() => new SitemapIndex("www.example.com"));
        }

        [Fact]
        public void InvalidLastModified()
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <lastmod>invalid</lastmod>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();

            Assert.Throws<ArgumentException>(() => index.Load(data));
        }

        [Fact]
        public void InvalidLocation()
        {
            var index = new SitemapIndex(Location);
            var data = @"
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>invalid</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();

            Assert.Throws<ArgumentException>(() => index.Load(data));
        }

        [Fact]
        public void InvalidRootTag()
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <invalid xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                </invalid>".TrimStart();

            Assert.Throws<ArgumentException>(() => index.Load(data));
        }

        [Fact]
        public void InvalidUrlTag()
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <invalid>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </invalid>
                </sitemapindex>".TrimStart();

            Assert.Throws<ArgumentException>(() => index.Load(data));
        }

        [Fact]
        public void LoadStartsAfresh()
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                    <sitemap>
                        <loc>{Location}/sitemap2.xml.gz</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();
            index.Load(data);
            var url = $"{Location}/sitemap3.xml";
            data = @$"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{url}</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();
            index.Load(data);

            var entries = index.Entries;
            var entry = GetOnlyEntry(entries);
            Assert.Equal(Utils.NormalizeLocation(url), entry.Location);
            Assert.False(entries.MoveNext());
        }

        [Theory]
        [InlineData("http://www.example.com", false)]
        [InlineData("http://www.example.com/", false)]
        [InlineData("http://www.example.com:80", false)]
        [InlineData("http://www.example.com:80/", false)]
        [InlineData("http://www.example.com:8080/", false)]
        [InlineData("http://WWW.EXAMPLE.COM:8080/path", false)]
        [InlineData("HTTP://WWW.EXAMPLE.COM/sitemap-index.xml.gz", true)]
        [InlineData("http://www.example.com:80/sitemap-index.xml.gz", true)]
        [InlineData("http://username:password@www.example.com/sitemap-index.xml.gz", true)]
        public void LocationValidity(string location, bool isEqual)
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{location}</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();

            index.Load(data);

            Assert.Equal(isEqual, index.Entries.MoveNext());
        }

        [Fact]
        public void MissingAllFields()
        {
            var index = new SitemapIndex(Location);
            var data = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            Assert.Throws<ArgumentException>(() => index.Load(data));
        }

        [Fact]
        public void MissingDeclaration()
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();

            index.Load(data);

            Assert.True(index.Entries.MoveNext());
        }

        [Fact]
        public void MissingNamespace()
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <sitemapindex>
                    <sitemap>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();

            Assert.Throws<ArgumentException>(() => index.Load(data));
        }

        [Fact]
        public void MultipleInstantiations()
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();
            index.Load(data);
            index = new SitemapIndex(Location);
            index.Load(data);

            Assert.Equal(1, index.EntryCount);
        }

        [Fact]
        public void NonUtf8Encoding()
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <?xml version=""1.0"" encoding=""UTF-16""?>
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{Location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();

            Assert.Throws<ArgumentException>(() => index.Load(data));
        }

        [Theory]
        [InlineData("http://example.com")]
        [InlineData("https://www.example.com")]
        [InlineData("https://www.example.com:8080")]
        public void SupersetLocation(string location)
        {
            var index = new SitemapIndex(Location);
            var data = @$"
                <sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                    <sitemap>
                        <loc>{location}/sitemap-index.xml.gz</loc>
                    </sitemap>
                </sitemapindex>".TrimStart();

            index.Load(data);

            Assert.False(index.Entries.MoveNext());
        }

        private static SitemapIndexEntry GetOnlyEntry(IEnumerator<SitemapIndexEntry> entries)
        {
            entries.MoveNext();
            var entry = entries.Current;
            return entry;
        }
    }
}