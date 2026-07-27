using DataEntities;

namespace Products.Data;

public static class SeedData
{
    public static void Initialize(ProductDataContext context)
    {
        if (context.Product.Any())
        {
            return;
        }

        context.Product.AddRange(
            new Product
            {
                Name = "Solar Powered Flashlight",
                Description = "A fantastic product for outdoor enthusiasts. Charges in daylight and " +
                              "runs for eight hours on a full charge, so you never need spare batteries.",
                Price = 19.99m
            },
            new Product
            {
                Name = "Hiking Poles",
                Description = "Adjustable aluminium poles that take the strain off your knees on steep " +
                              "descents. Cork grips stay comfortable on long days.",
                Price = 24.99m
            },
            new Product
            {
                Name = "Outdoor Rain Jacket",
                Description = "Fully waterproof shell with taped seams and a storm hood. Packs down " +
                              "small enough to live in the bottom of a daypack.",
                Price = 49.99m
            },
            new Product
            {
                Name = "Four Season Sleeping Bag",
                Description = "Rated to minus ten degrees. Down filled baffles trap heat through the " +
                              "coldest nights without adding bulk.",
                Price = 129.99m
            },
            new Product
            {
                Name = "Two Person Tent",
                Description = "Freestanding shelter that pitches in under five minutes. Survives gusts " +
                              "on exposed ridges and stays dry in sustained downpours.",
                Price = 179.99m
            },
            new Product
            {
                Name = "Camping Stove",
                Description = "Compact burner that boils a litre in three minutes. Runs on standard gas " +
                              "canisters and folds flat for packing.",
                Price = 39.99m
            },
            new Product
            {
                Name = "Insulated Water Bottle",
                Description = "Double walled steel keeps drinks cold for a full day or hot overnight. " +
                              "Fits standard cup holders and pack side pockets.",
                Price = 29.99m
            },
            new Product
            {
                Name = "Trail Running Shoes",
                Description = "Aggressive tread for loose gravel and wet rock. Drains quickly after " +
                              "stream crossings and dries on the move.",
                Price = 89.99m
            },
            new Product
            {
                Name = "Daypack",
                Description = "Twenty litre pack with a padded back panel and hip belt. Enough room for " +
                              "layers, lunch, and a litre of water.",
                Price = 59.99m
            },
            new Product
            {
                Name = "Merino Base Layer",
                Description = "Natural fibre top that regulates temperature and resists odour over " +
                              "several days of wear. Works as a mid layer under a shell.",
                Price = 44.99m
            },
            new Product
            {
                Name = "Head Torch",
                Description = "Hands free lighting with a red night mode that preserves your vision. " +
                              "Tilts down for reading maps and cooking after dark.",
                Price = 34.99m
            },
            new Product
            {
                Name = "Portable Power Bank",
                Description = "Twenty thousand milliamp hours, enough to recharge a phone four times " +
                              "on a long trip away from mains electricity.",
                Price = 54.99m
            });

        context.SaveChanges();
    }
}
