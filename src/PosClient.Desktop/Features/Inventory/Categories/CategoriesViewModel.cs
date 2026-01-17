using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Inventory.Categories
{
    public partial class CategoriesViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;
        private readonly INotificationService _notificationService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<CategoryDetails> _categoryTree = new();

        [ObservableProperty]
        private CategoryDetails? _selectedCategory = null;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isLoadingDetails;

        [ObservableProperty]
        private bool _isEmptyResult;

        // Populate parent dropdown
        [ObservableProperty]
        private ObservableCollection<CategoryDetails> _flatCategories = new();

        public CategoriesViewModel(
            IApiClient apiClient, 
            INotificationService notificationService, 
            IDialogService dialogService)
        {
            _apiClient = apiClient;
            _notificationService = notificationService;
            _dialogService = dialogService;

            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        public async Task LoadData()
        {
            IsLoading = true;
            IsEmptyResult = false;
            CategoryTree.Clear();
            FlatCategories.Clear();

            // Fetch the tree
            var result = await _apiClient.GetAsync<List<CategoryDetails>>("api/categories/tree");

            if (result.IsSuccess)
            {
                var treeData = result.Data;
                if (treeData?.Count > 0)
                {
                    foreach (var category in treeData)
                    {
                        CategoryTree.Add(category);
                        Flatten(category);
                    }
                }
            }

            // Add a "None" option for the dropdown (Root category)
            FlatCategories.Insert(0, new CategoryDetails { Id = Guid.Empty, NamePath = "-- Root Category --" });

            // Create a category to bind with the form
            //CreateNew();
            SelectedCategory = null;
            IsEmptyResult = !CategoryTree.Any();
            IsLoading = false;
        }

        public void Flatten(CategoryDetails category)
        {
            FlatCategories.Add(category);
            if (category != null)
            {
                foreach (var child in category.Children)
                    Flatten(child);
            }
        }

        [RelayCommand]
        public async Task LoadCategoryDetails(object? selectedItem)
        {
            if (selectedItem is not CategoryDetails category) return;

            IsLoadingDetails = true;

            var details = await _apiClient.GetAsync<CategoryDetails>($"api/categories/{category.Id}");
            if (details.IsSuccess)
            {
                SelectedCategory = details.Data;
            }

            IsLoadingDetails = false;
        }

        [RelayCommand]
        public void CreateNew()
        {
            // Create a blank category
            var newCat = new CategoryDetails
            {
                Name = "",
                DisplayOrder = 1,
                IsActive = true
            };

            // If a parent is currently selected in the tree, auto-select it as parent
            if (SelectedCategory != null && SelectedCategory.Id != Guid.Empty)
            {
                newCat.ParentCategoryId = SelectedCategory.Id;
            }

            SelectedCategory = newCat;
        }

        [RelayCommand]
        public async Task Save()
        {
            if (SelectedCategory == null) return;

            bool isNew = SelectedCategory.Id == Guid.Empty;

            if (SelectedCategory.ParentCategoryId == Guid.Empty)
            {
                SelectedCategory.ParentCategoryId = null;
            }

            Result<Guid> result;
            if (isNew)
            {
                result = await _apiClient.PostAsync<Guid>("api/categories", SelectedCategory);
            }
            else
            {
                result = await _apiClient.PutAsync<Guid>("api/categories", SelectedCategory);
            }

            // 4. On Success, Refresh
            if (result.IsSuccess)
            {
                SelectedCategory = null; // Clear form
                await LoadData(); // Reload tree to show new position

                _notificationService.ShowSuccess(isNew ? "Category saved succesfully." : "Category updated succesfully.");
            }
        }

        [RelayCommand]
        public async Task ToggleStatus(CategoryDetails? category)
        {
            if (category == null) return;
            if (category.Id == Guid.Empty) return;

            bool newStatus = category.IsActive;

            if (!newStatus)
            {
                var confirm = await _dialogService.ShowConfirmationAsync(
                    "Deactivate Category?",
                    "This might affect sub-categories or products.",
                    "Deactivate",
                    "Cancel");

                if (!confirm)
                {
                    // Revert the toggle visually without calling API
                    category.IsActive = true;
                    return;
                }
            }

            var endpoint = newStatus ? $"api/categories/{category.Id}/activate" : $"api/categories/{category.Id}/deactivate";
            var result = await _apiClient.PostAsync(endpoint, null!);

            if (!result.IsSuccess)
            {
                // Revert UI on failure
                category.IsActive = !newStatus;
            }
            else
            {
                _notificationService.ShowSuccess($"Category is now {(newStatus ? "Active" : "Inactive")}");

                await LoadData();
            }
        }

        [RelayCommand]
        public async Task ToggleStatusFromMenu(CategoryDetails category)
        {
            category.IsActive = !category.IsActive;
            await ToggleStatus(category);
        }

        [RelayCommand]
        public async Task Delete()
        {
            if (SelectedCategory == null) return;

            var confirm = await _dialogService.ShowConfirmationAsync(
                    "Delete Category?",
                    "Are you sure you want to delete this category? This action cannot be undone.",
                    "Delete",
                    "Cancel");

            if (!confirm)
            {
                return;
            }

            var apiResult = await _apiClient.DeleteAsync($"api/categories/{SelectedCategory.Id}");

            if (apiResult.IsSuccess)
            {
                _notificationService.ShowSuccess("Category removed successfully.");

                SelectedCategory = null;
                await LoadData();

            }
        }
    }
}
