using SqlSugar;
using System.ComponentModel;

namespace HanSongApp.Models
{
    [SugarTable("station")]
    public class Station : INotifyPropertyChanged
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int id { get; set; }
        public string? name { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

