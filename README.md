![Code Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/nurhafiz/8b8e2599175b077c2ffede14a92b8d75/raw/SitemapsProtocol-coverage.json)
![Nuget](https://img.shields.io/nuget/v/Toimik.SitemapsProtocol)

# Toimik.SitemapsProtocol

.NET 8 C# [Sitemap](https://en.wikipedia.org/wiki/Sitemaps) parser and Sitemap Index parser.

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

#### Sitemap.cs / SitemapIndex.cs

This example shows how to use the `Sitemap` class.

Usage of the `SitemapIndex` class is similar.

```c# 
// The value that every sitemap entry's location must start with
// in order for that entry to be considered as valid
var location = new Uri("http://www.example.com");

// Create an instance of the Sitemap class
var sitemap = new Sitemap(location);

// Load the data from a Stream
using var stream = // Create a local or remote stream
await sitemap.Load(stream);

// Or load the data from a string
// var data = "...";
// sitemap.Load(data);

...

var entries = sitemap.Entries;

Console.WriteLine($"{sitemap.EntryCount} entries parsed:");

// Enumerate the sitemap entries
while (entries.MoveNext())
{
    var entry = entries.Current;
    Console.WriteLine(entry.Location);
    ...
}
```

#### SitemapParser.cs / SitemapIndexParser.cs

This example shows how to use the `SitemapParser` class.

Usage of the `SitemapIndexParser` class is similar.

```c# 
var location = new Uri("http://www.example.com");
var parser = new SitemapParser(location);
using var stream = // Create a local or remote stream
await foreach (SitemapEntry entry in parser.Parse(stream))
{
    ...
}
```

Internally, `Sitemap` and `SitemapIndex` use `SitemapParser` and `SitemapIndexParser`, respectively.