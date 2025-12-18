using FurnitureShop.Models.Entities;

namespace FurnitureShop.Helpers;

public static class CategoryTreeHelper
{
    private const int ROOT = 0; // sentinel cho ParentId = null (Id identity bắt đầu từ 1 nên 0 là an toàn)

    public sealed class FlatCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public int? ParentId { get; set; }
        public int Level { get; set; }
    }

    public static List<FlatCategory> Flatten(IEnumerable<Category> categories)
    {
        var list = categories.ToList();

        // Key = ParentId ?? ROOT  => không bao giờ null
        var byParent = list
            .GroupBy(c => c.ParentId ?? ROOT)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Name).ToList());

        var result = new List<FlatCategory>();

        void Dfs(int parentKey, int level)
        {
            if (!byParent.TryGetValue(parentKey, out var children)) return;

            foreach (var c in children)
            {
                result.Add(new FlatCategory
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    ParentId = c.ParentId,
                    Level = level
                });

                Dfs(c.Id, level + 1);
            }
        }

        Dfs(ROOT, 0);
        return result;
    }

    public static HashSet<int> GetDescendantIds(int rootId, IEnumerable<Category> categories)
    {
        var list = categories.ToList();

        // Chỉ group những cái có ParentId (tránh null key)
        var childrenByParent = list
            .Where(c => c.ParentId.HasValue)
            .GroupBy(c => c.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

        var ids = new HashSet<int> { rootId };
        var q = new Queue<int>();
        q.Enqueue(rootId);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (!childrenByParent.TryGetValue(cur, out var children)) continue;

            foreach (var childId in children)
            {
                if (ids.Add(childId))
                    q.Enqueue(childId);
            }
        }

        return ids;
    }
}
