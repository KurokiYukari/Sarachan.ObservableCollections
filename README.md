# Sarachan.ObservableCollections

![GitHub License](https://img.shields.io/github/license/KurokiYukari/Sarachan.ObservableCollections?style=for-the-badge)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge)

A high-performance C# observable collection implementation with Linq to it.

- **PERFORMANCE** > Alloc free CollectionChanged event!
- **LINQ** > Use Linq to create collection view for observable collection!
- **SUPPORT** > Convert to dotnet standard observable collection, and therefore you may use it with WPF or MAUI!

# Code Sample

``` cs
var list = new ObservableList<int>();
var view = list.BuildView(emitter =>
{
    return emitter.Where(i => i % 2 == 0)
        .Select(i => i * 2)
        .OrderBy();
});

view.CollectionChanged += (sender, e) =>
{
    // Only queried items will raise view's CollectionChanged event
};

// create a standard view to support mvvm binding. (for exp: WPF)
var standardView = view.CreateStandardView(false);
```
