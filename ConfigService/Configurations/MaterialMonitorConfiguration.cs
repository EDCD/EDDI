using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage for configuration of material amounts</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\materialmonitor.json")]
    public class MaterialMonitorConfiguration : Config
    {
        private bool _useInvariantNames;
        private string _searchText = "";
        private string _categoryFilter;
        private int? _gradeFilter;
        private string _inventoryFilter = "All";

        public bool useInvariantNames
        {
            get => _useInvariantNames;
            set { if (_useInvariantNames == value) { return; } _useInvariantNames = value; OnPropertyChanged(); }
        }
        public string searchText
        {
            get => _searchText;
            set { if (_searchText == value) { return; } _searchText = value; OnPropertyChanged(); }
        }
        public string categoryFilter
        {
            get => _categoryFilter;
            set { if (_categoryFilter == value) { return; } _categoryFilter = value; OnPropertyChanged(); }
        }
        public int? gradeFilter
        {
            get => _gradeFilter;
            set { if (_gradeFilter == value) { return; } _gradeFilter = value; OnPropertyChanged(); }
        }
        public string inventoryFilter
        {
            get => _inventoryFilter;
            set { if (_inventoryFilter == value) { return; } _inventoryFilter = value; OnPropertyChanged(); }
        }

        private List<MaterialAmount> _materials = [];

        public List<MaterialAmount> materials
        {
            get => _materials;
            set
            {
                if ( Equals( value, _materials ) )
                {
                    return;
                }

                _materials = value;
                OnPropertyChanged();
            }
        }
    }
}
