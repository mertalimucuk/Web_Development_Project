using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Week5Lab.Models;

namespace Week5Lab.Pages
{
    public class IndexModel : PageModel
    {
        public static List<ClassInformationModel> ClassList { get; set; } = new();
        public List<ClassInformationTable> FilteredList { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        [BindProperty]
        public ClassInformationModel NewClass { get; set; } = new();

        public bool IsEditing { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedId { get; set; }

        private static bool isSeeded = false;

        public void OnGet()
        {
            SeedData();

            if (SelectedId.HasValue)
            {
                var item = ClassList.FirstOrDefault(c => c.Id == SelectedId.Value);
                if (item != null)
                {
                    NewClass = new ClassInformationModel
                    {
                        Id = item.Id,
                        ClassName = item.ClassName,
                        StudentCount = item.StudentCount,
                        Description = item.Description
                    };
                    IsEditing = true;
                }
            }

            // AI PROMPT : veri listem üzerinde filtreleme yapmamın kısa yolu nedir ?
            var query = ClassList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(c =>
                    c.ClassName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.StudentCount.ToString().Contains(SearchTerm)
                );
            }

            // AI PROMPT: Arama sonrası sayfa numarası sınırları kontrolünü nasıl sağlarım
            TotalPages = (int)Math.Ceiling(query.Count() / (double)PageSize);
            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > TotalPages) PageNumber = TotalPages;

            
            var paged = query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            FilteredList = paged.Select(c => new ClassInformationTable
            {
                ClassName = c.ClassName,
                StudentCount = c.StudentCount,
                Description = c.Description
            }).ToList();
        }

        public IActionResult OnPostAdd()
        {
            NewClass.Id = ClassList.Count > 0 ? ClassList.Max(c => c.Id) + 1 : 1;
            ClassList.Add(NewClass);
            return RedirectToPage();
        }

        public IActionResult OnPostUpdate()
        {
            var existing = ClassList.FirstOrDefault(c => c.Id == NewClass.Id);
            if (existing != null)
            {
                existing.ClassName = NewClass.ClassName;
                existing.StudentCount = NewClass.StudentCount;
                existing.Description = NewClass.Description;
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            var item = ClassList.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                ClassList.Remove(item);
            }

            // AI PROMPT: ID ve ClassName’leri yeniden nasıl sıralarım
            for (int i = 0; i < ClassList.Count; i++)
            {
                ClassList[i].Id = i + 1;
                ClassList[i].ClassName = $"Class {i + 1}";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostEdit(int id)
        {
            return RedirectToPage(new { SelectedId = id });
        }

        public void SeedData()
        {
            if (isSeeded || ClassList.Any()) return;

            var rnd = new Random();
            for (int i = 1; i <= 100; i++)
            {
                ClassList.Add(new ClassInformationModel
                {
                    Id = i,
                    ClassName = $"Class {i}",
                    StudentCount = rnd.Next(10, 100),
                    Description = $"Description for Class {i}"
                });
            }

            isSeeded = true;
        }
    }
}
