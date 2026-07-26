// =============================================================================
// THE EXISTING APP - no AI in this folder
// =============================================================================
// Everything under Store/ represents the application you already have in
// production: a product catalog, a record of what customers searched for, and
// operational log entries. None of it references an AI package.
//
// That separation is the point of Part 11. The AI features in Ai/ are additive:
// they read the signals this app already produces and call the actions it
// already exposes. Deleting the Ai/ folder would leave a working store.
// =============================================================================

namespace StoreApp.Store;

/// <summary>A product in the store catalog.</summary>
public record Product(int Id, string Name, string Category, decimal Price, int InStock, string Description)
{
    /// <summary>The text that gets embedded for semantic search.</summary>
    public string ToEmbeddingText() => $"{Name}. Category: {Category}. {Description}";

    public override string ToString() => $"#{Id} {Name} ({Category}) - ${Price} - {InStock} in stock";
}

/// <summary>
/// The catalog service. In a real app this would be backed by a database;
/// the shape of the API is what matters here.
/// </summary>
public class Catalog
{
    private readonly List<Product> _products =
    [
        new(1, "TrailBlazer 3000 Hiking Boots", "Footwear", 189.99m, 42,
            "Waterproof full-grain leather boots with a Vibram sole. Ankle support for uneven terrain and multi-day treks."),
        new(2, "Summit Down Jacket", "Outerwear", 249.99m, 18,
            "800-fill down insulation, packs into its own pocket. Rated to -15C. Not waterproof on its own."),
        new(3, "Cascade Rain Shell", "Outerwear", 159.99m, 7,
            "Three-layer waterproof breathable shell with taped seams and pit zips. Layers over an insulating mid-layer."),
        new(4, "Riverbend Fleece Pullover", "Outerwear", 79.99m, 55,
            "Midweight grid fleece mid-layer. Warm when damp, dries fast, works under a shell."),
        new(5, "Alpine 65L Backpack", "Packs", 219.99m, 12,
            "65 litre internal frame pack with adjustable torso, hip belt, and a rain cover for multi-day trips."),
        new(6, "Daybreak 22L Daypack", "Packs", 89.99m, 64,
            "Lightweight 22 litre daypack with a hydration sleeve and a padded laptop pocket for commuting."),
        new(7, "Northstar 2-Person Tent", "Shelter", 329.99m, 9,
            "Freestanding three-season tent, 2.4kg packed, with a full-coverage fly and two vestibules."),
        new(8, "Basecamp Sleeping Bag", "Shelter", 179.99m, 21,
            "Synthetic-fill mummy bag comfort rated to -5C. Keeps loft when damp, machine washable."),
        new(9, "Meridian Trekking Poles", "Accessories", 69.99m, 38,
            "Collapsible aluminium poles with cork grips. Reduce knee load on steep descents."),
        new(10, "Lumen 400 Headlamp", "Accessories", 44.99m, 73,
            "400 lumen rechargeable headlamp with a red night mode and a 12 hour runtime."),
        new(11, "Dryflow Merino Base Layer", "Apparel", 64.99m, 47,
            "Merino wool long-sleeve base layer. Regulates temperature and resists odour on long trips."),
        new(12, "Canyon Water Filter", "Accessories", 54.99m, 29,
            "Inline hollow-fibre water filter, 0.1 micron, 2000 litre capacity. Attaches to a hydration bladder."),
    ];

    public IReadOnlyList<Product> All => _products;

    public Product? FindById(int id) => _products.FirstOrDefault(p => p.Id == id);

    /// <summary>
    /// The search the app has always had: literal substring matching.
    /// It works for "boots" and fails completely for "something warm for a rainy hike".
    /// </summary>
    public IReadOnlyList<Product> KeywordSearch(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return [];
        }

        return _products
            .Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || p.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || p.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>Products at or below a stock threshold - an existing operational action.</summary>
    public IReadOnlyList<Product> LowStock(int threshold = 10) =>
        _products.Where(p => p.InStock <= threshold).OrderBy(p => p.InStock).ToList();
}
