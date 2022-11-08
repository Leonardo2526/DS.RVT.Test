using System.Collections.ObjectModel;

namespace DS.RevitApp.SwitchSolution
{
    public class NamesCollection : ObservableCollection<MyObject>
    {
        public void Add(string name)
        {
            this.Add(new MyObject { Name = name });
        }
    }
}
