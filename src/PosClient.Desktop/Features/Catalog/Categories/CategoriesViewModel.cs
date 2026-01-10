using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace PosClient.Desktop.Features.Catalog.Categories
{
    public partial class CategoriesViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;

        [ObservableProperty]
        private ObservableCollection<Category> _categoryTree = new();

        [ObservableProperty]
        private Category? _selectedCategory = null;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isLoadingDetails;

        [ObservableProperty]
        private bool _isEmptyResult;

        // Populate parent dropdown
        [ObservableProperty]
        private ObservableCollection<Category> _flatCategories = new();

        public CategoriesViewModel(IApiClient apiClient, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _apiClient = apiClient;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
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
            var result = await _apiClient.GetAsync<List<Category>>("api/categories/tree");

            if (result.IsSuccess)
            {
                var treeData = result.Data;
                if(treeData?.Count > 0)
                {
                    foreach (var category in treeData)
                    {
                        CategoryTree.Add(category);
                        Flatten(category);
                    }
                }
            }

            // Add a "None" option for the dropdown (Root category)
            FlatCategories.Insert(0, new Category { Id = Guid.Empty, NamePath = "-- Root Category --" });

            // Create a category to bind with the form
            //CreateNew();
            SelectedCategory = null;
            IsEmptyResult = !CategoryTree.Any();
            IsLoading = false;
        }

        public void Flatten(Category category)
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
            if (selectedItem is not Category category) return;

            IsLoadingDetails = true;

            var details = await _apiClient.GetAsync<Category>($"api/categories/{category.Id}");
            if(details.IsSuccess)
            {
                SelectedCategory = details.Data;
            }

            IsLoadingDetails = false;
        }

        [RelayCommand]
        public void CreateNew()
        {
            // Create a blank category
            var newCat = new Category
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

                _snackbarService.Show(
                    "Success!",
                    isNew ? "Category saved succesfully." : "Category updated succesfully.",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                    TimeSpan.FromSeconds(5)
                );
            }
        }

        [RelayCommand]
        public async Task ToggleStatus(Category? category)
        {
            if(category == null) return;
            if (category.Id == Guid.Empty) return;

            bool newStatus = category.IsActive;

            if (!newStatus)
            {
                var confirm = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions
                {
                    Title = "Deactivate Category?",
                    Content = "This might affect sub-categories or products.",
                    PrimaryButtonText = "Deactivate",
                    CloseButtonText = "Cancel"
                });

                if (confirm != ContentDialogResult.Primary)
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
                _snackbarService.Show(
                    "Success!", 
                    $"Category is now {(newStatus ? "Active" : "Inactive")}", 
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                    TimeSpan.FromSeconds(5));

                await LoadData();
            }
        }

        [RelayCommand]
        public async Task ToggleStatusFromMenu(Category category)
        {
            category.IsActive = !category.IsActive;
            await ToggleStatus(category);
        }

        [RelayCommand]
        public async Task Delete()
        {
            if (SelectedCategory == null) return;

            // 1. Show Confirmation Dialog
            var result = await _contentDialogService.ShowSimpleDialogAsync(
                new SimpleContentDialogCreateOptions
                {
                    Title = "Delete Category?",
                    Content = "Are you sure you want to delete this category? This action cannot be undone.",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                }
            );

            // 2. Check the user's choice
            // If they clicked "Cancel" or clicked away, we stop.
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            // 3. User said YES -> Call API
            // (Assuming you added a DeleteAsync method to your ApiClient)
            var apiResult = await _apiClient.DeleteAsync($"api/categories/{SelectedCategory.Id}");

            // 4. Handle Success
            if (apiResult.IsSuccess)
            {
                _snackbarService.Show(
                    "Deleted!",
                    "Category removed successfully.",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.Delete24),
                    TimeSpan.FromSeconds(5)
                );

                SelectedCategory = null;
                await LoadData();

            }
        }
    }
}
