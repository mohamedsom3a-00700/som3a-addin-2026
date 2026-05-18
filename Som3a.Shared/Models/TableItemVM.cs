using System.Collections.Generic;
using System.ComponentModel;

public class TableItemVM : INotifyPropertyChanged
{
    public string Name { get; set; } // الاسم الحقيقي (TASK, PROJWBS)

    public string DisplayName { get; set; } // الاسم المفهوم

    public int Count { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            PropertyChanged?.Invoke(this, new(nameof(IsSelected)));
        }
    }

    private string _status = "Not Updated";
    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            PropertyChanged?.Invoke(this, new(nameof(Status)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

}
public class XerTable
{
    public string Name { get; set; }
    public List<string> Headers { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
}

