# Toimik.SitemapsProtocol

.NET 5 C# [Sitemap](https://en.wikipedia.org/wiki/Sitemaps) parser and Sitemap Index parser.

## Quick Start

### Installation

#### Package Manager

```command
PM> Install-Package Toimik.SitemapsProtocol
```

#### .NET CLI

```command
> dotnet add package Toimik.SitemapsProtocol
```

### Usage

This example shows how to use the `Sitemap` class.

Usage of the `SitemapIndex` class is similar.

```c# 
// The prefix that every sitemap entry's location must start with
// in order for that entry to be considered as valid
var urlPrefix = "http://www.example.com";

// Create an instance of the Sitemap class
var sitemap = new Sitemap(urlPrefix);

// Load the data from a Stream
using var stream = File.OpenRead(@"sitemap.xml");
await sitemap.Load(stream);

// Or load the data from a string
// var data = "...";
// sitemap.Load(data);

...

var entries = sitemap.Entries;

Console.WriteLine($"{entries.EntryCount} entries parsed:");

// Enumerate the sitemap entries
while (entries.MoveNext())
{
    var entry = entries.Current;
    Console.WriteLine(entry.Location);
    ...
}
```